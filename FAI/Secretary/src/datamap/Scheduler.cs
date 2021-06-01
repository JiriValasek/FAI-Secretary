using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Secretary
{
    /** <summary> Schedule for an Academic Year. </summary> */
    class Scheduler
    {
        private Dictionary<UInt32, Subject> subjects;
        private Dictionary<UInt32, StudentGroup> studentGroups;
        private Dictionary<UInt32, Employee> employees;
        private Dictionary<UInt32, Label> labels;
        private Weights weights;
        private Database db;

        /** <summary> Subjects guaranteed by the departement. </summary> */
        public List<Subject> Subjects
        {
            get
            {
                return this.subjects.Select(x => x.Value).OrderBy(x => x.Name).ToList();
            }
        }

        /** <summary> Student group guaranteed by the departement. </summary> */
        public List<StudentGroup> StudentGroups
        { 
            get
            {
                return this.studentGroups.Select(x => x.Value).OrderBy(x => x.Name).ToList();
            }
        }

        /** <summary> Employees working during the academic year. </summary> */
        public List<Employee> Employees 
        {
            get
            {
                return this.employees.Select(x => x.Value).OrderBy(x => x.Name).ToList();
            }
        }

        /** <summary> Weights to compute work points. </summary> */
        public Weights Weights 
        { 
            get
            {
                if (weights == null)
                {
                    db.InsertWeights(new Weights(0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0));
                }
                weights = db.GetWeights();
                return weights;
            }
            set
            {
                db.UpdateWeights(value);
                weights = value;
            }
        }

        /** <summary> Labels managed by the departement. </summary> */
        public List<Label> Labels
        {
            get
            {
                return this.labels.Select(x => x.Value).OrderBy(x => x.Name).ToList();
            }
        }

        /** 
         * <summary> Constructor for scheduler. </summary>
         * <param name="db"> Initialized database. </param>
         */
        public Scheduler(Database db)
        {
            this.db = db;
            SyncWithDatabase();
        }

        /**
         * <summary> Generates all labels for a subject. </summary>
         * <param name="s"> Subject for which to generate labels. </param>
         */
        public void GenerateLabels(Subject s)
        {
            List<LabelType> unlimitedTypes = new List<LabelType>(); // types of labels for all students
            List<LabelType> limitedTypes = new List<LabelType>(); // types of labels for limited number of students
            if (s.LectureLength > 0)
            {
                unlimitedTypes.Add(LabelType.Lecture);
            }
            if (s.PracticeLength > 0)
            {
                limitedTypes.Add(LabelType.Practice);
            }
            if (s.SeminarLength > 0)
            {
                limitedTypes.Add(LabelType.Seminar);
            }
            switch (s.Conditions)
            {
                case SubjectConditions.Assessment:
                    limitedTypes.Add(LabelType.Assesment);
                    break;
                case SubjectConditions.ClassifiedAssesment:
                    limitedTypes.Add(LabelType.ClassifiedAssesment);
                    break;
                case SubjectConditions.AssesmentAndExam:
                    limitedTypes.Add(LabelType.Assesment);
                    unlimitedTypes.Add(LabelType.Exam);
                    break;
            }
            // zero student count in all labels, so that unused labels don't have points anymore
            foreach(Label l in s.Labels.Values)
            {
                l.StudentCount = 0;
                db.UpdateLabel(l);
            }
            GenerateUnlimitedLabels(s, unlimitedTypes);
            GenerateLimitedLabels(s, limitedTypes);
            SyncWithDatabase();
        }

        /** 
         * <summary> Removes a label and connections to a subject/employee. </summary> 
         * <param name="l">Label to remove.</param>
         */
        public void RemoveLabel(Label l)
        {
            if (l.LabelEmployee != null)
            {
                db.DeleteEmployeeLabel(l.LabelEmployee, l);
            }
            if (l.LabelSubject != null)
            {
                db.DeleteSubjectLabel(l.LabelSubject, l);
            }
            db.DeleteLabel(l);
            SyncWithDatabase();
        }

        /**
         * <summary> 
         * Generates lecture and exam/classified asessment labels.
         * Labels for all subject's students.
         * </summary>
         * <param name="s"> Subject for which to generate labels. </param>
         * <param name="types"> Types of labels for all student in a subject</param>
         */
        private void GenerateUnlimitedLabels(Subject s, List<LabelType> types)
        {
            UInt16 subjectStudentCount = (UInt16) s.StudentGroups.Values.Sum(x => x.StudentCount); // count students attending the subject
            UInt16 labelStudentCount;
            foreach ( LabelType type in types)
            {
                labelStudentCount = (UInt16) s.Labels.Values.Where(x => x.Type == type)
                    .Sum(x => Convert.ToInt32(x.StudentCount)); // count students in labels with given type
                if (subjectStudentCount != labelStudentCount)
                {
                    if (s.Labels.Values.Where(x => x.Type == type).Count() == 0) // no such label exists
                    {
                        AddLabel(s.Abbreviation + " " + type.ToString(), s, type, subjectStudentCount);
                    }
                    else
                    {
                        var l = s.Labels.Values.Where(x => x.Type == type).ToList()[0];
                        l.StudentCount = Convert.ToByte(subjectStudentCount);
                        db.UpdateLabel(l); // update student count
                    }
                }
            }
        }

        /**
         * <summary> 
         * Generates seminar, practice and asessment labels.
         * Labels that can have only MaxGroupSize students.
         * </summary>
         * <param name="s"> Subject for which to generate labels. </param>
         * <param name="types"> Types of labels for limited number of students. </param>
         */
        private void GenerateLimitedLabels(Subject s, List<LabelType> types)
        {
            UInt16 subjectStudentCount = (UInt16)s.StudentGroups.Values.Sum(x => x.StudentCount); // count students attending the subject
            foreach (LabelType type in types)
            {
                var oldTypeLabels = s.Labels.Values.Where(x => x.Type == type).ToList(); // get all labels that are for limited students 
                var labelStudentCount = oldTypeLabels.Sum(x => Convert.ToInt32(x.StudentCount)); // get current student count
                var newTypeLabelCount = (int)Math.Ceiling(((float) subjectStudentCount) / s.MaxGroupSize);

                // minimal number of groups necessary
                int numberOfGroups = (int)Math.Ceiling(Convert.ToDouble(subjectStudentCount) / s.MaxGroupSize);
                int leftOutStudentCount = subjectStudentCount; // left out students to put into labels
                UInt16 newStudentCount;

                foreach (Label l in s.Labels.Where(x => x.Value.Type == type).Select(x => x.Value).ToList())
                {
                    newStudentCount = (UInt16)Math.Ceiling(Convert.ToDouble(leftOutStudentCount) / numberOfGroups);
                    newStudentCount = (UInt16)(newStudentCount <= leftOutStudentCount ? newStudentCount : leftOutStudentCount);
                    l.StudentCount = Convert.ToByte(newStudentCount);
                    db.UpdateLabel(l);
                    leftOutStudentCount = Math.Max(0, leftOutStudentCount - newStudentCount);
                    numberOfGroups--;
                    numberOfGroups = Math.Max(1, numberOfGroups);
                }

                for (var i = 0; i < (newTypeLabelCount - oldTypeLabels.Count); i++)
                {
                    newStudentCount = (UInt16)Math.Ceiling(Convert.ToDouble(leftOutStudentCount) / numberOfGroups);
                    newStudentCount = (UInt16)(newStudentCount <= leftOutStudentCount ? newStudentCount : leftOutStudentCount);
                    AddLabel(s.Abbreviation + " " + type.ToString() + " " + (i + oldTypeLabels.Count + 1), s, type, newStudentCount);
                    leftOutStudentCount = Math.Max(0, leftOutStudentCount - newStudentCount);
                    numberOfGroups--;
                    numberOfGroups = Math.Max(1, numberOfGroups);
                }

                StudentsIntoSeparateGroupLabels(s, type, subjectStudentCount);
            }
        }

        /**
         * <summary> Updates a label in the database. </summary>
         * <param name="l"> Edited label. </param>
         */
        public void UpdateLabel(Label l)
        {
            var lOld = this.labels[l.Id];
            db.UpdateLabel(l);
            if (l.LabelEmployee != lOld.LabelEmployee)
            {
                if (lOld.LabelEmployee is Employee)
                {
                    db.DeleteEmployeeLabel(lOld.LabelEmployee, lOld);
                }
                if (l.LabelEmployee is Employee)
                {
                    db.InsertEmployeeLabel(l.LabelEmployee, l);
                }
            }
            if (l.LabelSubject != lOld.LabelSubject)
            {
                db.DeleteSubjectLabel(lOld.LabelSubject, lOld);
                db.InsertSubjectLabel(l.LabelSubject, l);
            }
            SyncWithDatabase();
        }

        /**
         * <summary> Adds a label to the database. </summary>
         * <param name="name"> Name of the label. </param>
         * <param name="s"> Subject to which the label should be added. </param>
         * <param name="t"> Type of the label. </param>
         * <param name="sc"> Number of students in the label. </param>
         */
        private void AddLabel(string name, Subject s, LabelType t, UInt16 sc)
        {
            var l = new Label(0, name, null, s, t, s.Language, sc);
            var id = db.InsertLabel(l); // add label
            if (id == 0)
            {
                throw new Exception("Adding a label failed");
            }
            l.Id = id;
            if (s != null)
            {
                db.InsertSubjectLabel(s, l); // add connection to a label
            }
        }

        /**
         * <summary> Adds a label to the database. </summary>
         * <param name="l"> Label to add. </param>
         */
        public void AddLabel(Label l)
        {
            var id = db.InsertLabel(l); // add label
            if (id == 0)
            {
                throw new Exception("Adding label failed");
            }
            l.Id = id;
            if (l.LabelEmployee != null)
            {
                db.InsertEmployeeLabel(l.LabelEmployee, l);
            }
            SyncWithDatabase();
        }

        /**
         * <summary> Fills labels for practice/seminar etc. </summary>
         * <param name="s"> Subject to which the label should be added. </param>
         * <param name="t"> Type of the label. </param>
         * <param name="studentCount"> Number of students to sort into labels. </param>
         */
        private void StudentsIntoSeparateGroupLabels(Subject s, LabelType t, UInt16 studentCount)
        {
            // minimal number of groups necessary
            int numberOfGroups = (int) Math.Ceiling(Convert.ToDouble(studentCount) / s.MaxGroupSize);
            int leftOutStudentCount = studentCount; // left out students to put into labels
            int newStudentCount;
            foreach (Label l in s.Labels.Where(x => x.Value.Type == t).Select(x => x.Value).ToList())
            {
                newStudentCount = (int)Math.Ceiling(Convert.ToDouble(leftOutStudentCount) / numberOfGroups);
                newStudentCount = newStudentCount <= leftOutStudentCount ? 
                    newStudentCount : leftOutStudentCount;
                l.StudentCount = Convert.ToByte(newStudentCount);
                db.UpdateLabel(l);
                leftOutStudentCount = Math.Max(0, leftOutStudentCount - newStudentCount);
                numberOfGroups--;
                numberOfGroups = Math.Max(1,numberOfGroups);
            }
        }

        /**
         * <summary> Adds a subject to the database. </summary>
         * <param name="s"> Subject to add. </param>
         */
        public void AddSubject(Subject s)
        {
            if (s.Labels.Count != 0)
            {
                MessageBox.Show("Only subject without labels can be added.", "FAI Secretary", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UInt32 id = db.InsertSubject(s);
            if (id != 0)
            {
                s.Id = id;
                foreach (StudentGroup sg in s.StudentGroups.Values)
                {
                    db.InsertSubjectStudentGroup(s, sg);
                }
                SyncWithDatabase();
            }
            else
            {
                throw new Exception("Adding a subject failed.");
            }
        }

        /**
         * <summary> Updates a subject in the database. </summary>
         * <param name="s"> Edited subject. </param>
         */
        public void UpdateSubject(Subject s)
        {
            var sOld = this.subjects[s.Id];
            db.UpdateSubject(s);
            SyncWithDatabase();
        }

        /**
         * <summary> Removes a subject from the database. </summary>
         * <param name="s"> Subject to remove. </param>
         */
        public void RemoveSubject(Subject s)
        {
            foreach (Label l in s.Labels.Values)
            {
                db.DeleteSubjectLabel(s, l);
            }
            foreach (StudentGroup sg in s.StudentGroups.Values)
            {
                db.DeleteSubjectStudentGroup(s, sg);
            }
            db.DeleteSubject(s);
            SyncWithDatabase();
        }

        /**
         * <summary> Adds an employee to the database. </summary>
         * <param name="e"> Employee to add. </param>
         */
        public void AddEmployee(Employee e)
        {
            UInt32 id = db.InsertEmployee(e);
            if (id != 0)
            {
                e.Id = id;
                foreach (Label l in e.Labels.Values)
                {
                    db.InsertEmployeeLabel(e, l);
                }
                SyncWithDatabase();
            } 
            else
            {
                throw new Exception("Adding an employee failed.");
            }
        }

        /**
         * <summary> Updates an employee in the database. </summary>
         * <param name="e"> Edited Employee. </param>
         */
        public void UpdateEmployee(Employee e)
        {
            var eOld = this.employees[e.Id];
            foreach(Label l in eOld.Labels.Values)
            {
                db.DeleteEmployeeLabel(eOld, l);
            }
            foreach(Label l in e.Labels.Values)
            {
                db.InsertEmployeeLabel(e, l);
            }
            db.UpdateEmployee(e);
            SyncWithDatabase();
        }

        /**
         * <summary> Removes an employee from the database. </summary>
         * <param name="e"> Employee to remove. </param>
         */
        public void RemoveEmployee(Employee e)
        {
            foreach(Label l in e.Labels.Values)
            {
                db.DeleteEmployeeLabel(e, l);
            }
            db.DeleteEmployee(e);
            SyncWithDatabase();
        }

        /**
         * <summary> Adds a student group to the database. </summary>
         * <param name="sg"> Student group to add. </param>
         */
        public void AddStudentGroup(StudentGroup sg)
        {
            
            UInt32 id = db.InsertStudentGroup(sg);
            if (id != 0)
            {
                sg.Id = id;
                foreach (Subject s in sg.Subjects.Values)
                {
                    db.InsertSubjectStudentGroup(s, sg);
                }
                SyncWithDatabase();
            }
            else
            {
                throw new Exception("Adding a student group failed.");
            }
        }

        /**
         * <summary> Updates a student group in the database. </summary>
         * <param name="sg"> Student group to edit. </param>
         */
        public void UpdateStudentGroup(StudentGroup sg)
        {
            var sgOld = this.studentGroups[sg.Id];
            foreach (Subject s in sgOld.Subjects.Values)
            {
                db.DeleteSubjectStudentGroup(s, sgOld);
            }
            foreach (Subject s in sg.Subjects.Values)
            {
                db.InsertSubjectStudentGroup(s, sg);
            }
            db.UpdateStudentGroup(sg);
            SyncWithDatabase();
        }

        /**
         * <summary> Removes a student group in the database. </summary>
         * <param name="sg"> Student group to remove. </param>
         */
        public void RemoveStudentGroup(StudentGroup sg)
        {
            foreach (Subject s in sg.Subjects.Values)
            {
                db.DeleteSubjectStudentGroup(s, sg);
            }
            db.DeleteStudentGroup(sg);
            SyncWithDatabase();
        }

        /**
         * <summary> Loads objects according to the database. </summary>
         */
        public void SyncWithDatabase()
        {

            // connect subject with labels/student groups
            this.studentGroups = db.GetStudentGroups();
            this.subjects = db.GetSubjects();
            this.labels = db.GetLabels();
            foreach (var s in this.subjects)
            {
                var ids = db.GetSubjectsLabels(s.Value);
                foreach (UInt32 id in ids)
                {                    
                    this.labels[id].LabelSubject = s.Value; // assign subject to a label
                    s.Value.Labels.Add(id, this.labels[id]); // assign label to a subject
                }
                ids = db.GetSubjectsStudentGroups(s.Value);
                foreach (UInt32 id in ids)
                {
                    this.studentGroups[id].Subjects.Add(s.Key, s.Value); // assign subject to a student group
                    s.Value.StudentGroups.Add(id, this.studentGroups[id]); // assign student group to a subject
                }
            }
            this.employees = db.GetEmployees();
            // connect employees with labels
            foreach (var e in this.employees)
            {
                var ids = db.GetEmployeesLabels(e.Value);
                foreach (UInt32 id in ids)
                {
                    this.labels[id].LabelEmployee = e.Value; // assign employee to a label
                    e.Value.Labels.Add(id, this.labels[id]); // assign label to an employee
                }
            }
            AddEmployeePoints();
        }

        /**
         * <summary> Adds point to emloyees according to the labels assigned. </summary>
         */
        private void AddEmployeePoints()
        {
            foreach(Employee e in employees.Select(x => x.Value))
            {
                e.WorkPoints = Convert.ToUInt16(
                    Math.Floor(e.Labels.Sum(x => x.Value.GetPoints(Weights))));
                e.WorkPointsWithoutEnglish = Convert.ToUInt16(
                    Math.Floor(e.Labels
                    .Where(x => x.Value.Language == StudyLanguage.Czech)
                    .Sum(x => x.Value.GetPoints(Weights))));
            }
        }
    }
}
