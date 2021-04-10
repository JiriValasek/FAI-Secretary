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
                return this.subjects.Select(x => x.Value).ToList();
            }
        }

        /** <summary> Student group guaranteed by the departement. </summary> */
        public List<StudentGroup> StudentGroups
        { 
            get
            {
                return this.studentGroups.Select(x => x.Value).ToList();
            }
        }

        /** <summary> Employees working during the academic year. </summary> */
        public List<Employee> Employees 
        {
            get
            {
                return this.employees.Select(x => x.Value).ToList();
            }
        }

        /** <summary> Weights to compute work points. </summary> */
        public Weights Weights 
        { 
            get
            {
                if (weights == null)
                {
                    db.InsertWeights(new Weights(0,1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0));
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
                return this.labels.Select(x => x.Value).ToList();
            }
        }

        /** 
         * <summary> Constructor for scheduler. </summary>
         * <param name="db"> Initialized database. </param>
         */
        public Scheduler(Database db)
        {
            this.db = db;
            this.subjects = db.GetSubjects();
            this.studentGroups = db.GetStudentGroups();
            this.employees = db.GetEmployees();
            this.weights = db.GetWeights();
            this.labels = db.GetLabels();
            Interconnect();
        }

        /**
         * <summary> Adds a subject without labels or groups. </summary>
         */
        public void AddSubject(Subject s)
        {
            if (s.Labels.Count != 0)
            {
                MessageBox.Show("Only subject without labels can be added.", "FAI Secretary", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (s.StudentGroups.Count != 0)
            {
                MessageBox.Show("Only subject without student groups can be added.", "FAI Secretary", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            db.InsertSubject(s);
        }

        /**
         *  <summary> Changes a subject in the db and scheduler's lists. </summary>
         */
        public void ChangeSubject(Subject sNew)
        {
            Subject sOld = subjects[sNew.Id];
            db.UpdateSubject(sNew);
            if ( sNew.Labels != sOld.Labels)
            {
                foreach (var lNew in sNew.Labels)
                {
                    if (sOld.Labels.ContainsKey(lNew.Key))
                    {
                        this.labels[lNew.Key].LabelSubject = sNew;
                        db.UpdateLabelsSubject(sNew, lNew.Value);
                        sOld.Labels.Remove(lNew.Key);
                    } else
                    {
                        db.InsertLabel(lNew.Value);
                    }
                }
                foreach(var lOldNotInNew in sOld.Labels)
                {
                    db.DeleteSubjectLabel(sOld,lOldNotInNew.Value);
                }
            }
        }

        public void AddEmployee(Employee e)
        {
            db.InsertEmployee(e);
            employees = db.GetEmployees();
        }

        public void UpdateEmployee(Employee eNew)
        {
            var eOld = this.employees[eNew.Id];
            foreach(Label l in eOld.Labels.Values)
            {
                db.DeleteEmployeeLabel(eOld, l);
                l.LabelEmployee = null;
            }
            foreach(Label l in eNew.Labels.Values)
            {
                db.InsertEmployeeLabel(eNew, l);
                l.LabelEmployee = eNew;
            }
            db.UpdateEmployee(eNew);
            employees = db.GetEmployees();
        }

        public void RemoveEmployee(Employee e)
        {
            foreach(Label l in e.Labels.Values)
            {
                db.DeleteEmployeeLabel(e, l);
                l.LabelEmployee = null;
            }
            db.DeleteEmployee(e);
            employees = db.GetEmployees();
        }

        /**
         * <summary> Interconnects data with objects. </summary>
         */
        public void Interconnect()
        {
            // connect subject with labels/student groups
            foreach(var s in this.subjects)
            {
                var ids = db.GetSubjectsLabels(s.Value);
                foreach(UInt32 id in ids)
                {
                    this.labels[id].LabelSubject = s.Value;
                    this.subjects[s.Key].Labels.Add(id, this.labels[id]);
                }
                ids = db.GetSubjectsStudentGroups(s.Value);
                foreach (UInt32 id in ids)
                {
                    this.subjects[s.Key].StudentGroups.Add(id, this.studentGroups[id]);
                    this.studentGroups[id].Subjects.Add(s.Key, this.subjects[s.Key]);
                }
            }
            // connect employees with labels
            foreach(var e in this.employees)
            {
                var ids = db.GetEmployeesLabels(e.Value);
                foreach (UInt32 id in ids)
                {
                    this.labels[id].LabelEmployee = this.employees[e.Key];
                    this.employees[e.Key].Labels.Add(id, this.labels[id]);
                }
            }
        }

    }
}
