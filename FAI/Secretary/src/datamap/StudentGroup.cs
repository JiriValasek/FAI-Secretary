using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Secretary
{
    /** <summary> Semester for which is the class planned. </summary> */
    public enum StudySemester : Byte
    {
        Unknown = 0,
        Spring = 1,
        Fall = 2
    }

    /** <summary> Form of study the subject is tought in. </summary> */
    public enum StudyForm : Byte
    {
        Unknown = 0,
        FullTime = 1,
        PartTime = 2
    }

    /** <summary> Language in which the subject is tought. </summary> */
    public enum StudyLanguage : Byte
    {
        Unknown = 0,
        Czech = 1,
        English = 2
    }

    /** <summary> Year in which the subject is tought. </summary> */
    public enum StudyYear : Byte
    {
        Unknown = 0,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Fifth = 5
    }

    /** <summary> Conditions required to finish the Subject. </summary> */
    public enum SubjectConditions : Byte
    {
        Unknown = 0,
        Assessment = 1,
        ClassifiedAssesment = 2,
        AssesmentAndExam = 3
    }

    /** <summary> Type of study the students are in. </summary> */
    public enum StudyType : Byte
    {
        Unknown = 0,
        Undergraduate = 1,
        Graduate = 2
    }

    /** <summary> A Class of students in a given program, year etc. </summary> */
    public class StudentGroup
    {
        /** <summary> Group's ID in the DB. </summary> */
        public UInt32 Id { get; set; }
        /** <summary> Abbreaviation of the program the students are in. </summary> */
        public string Abbreviation { get; set; }
        /** <summary> Name of the program the students are in. </summary> */
        public string Name { get; set; }
        /** <summary> Year/grade to which students belong to. </summary> */
        public StudyYear Year { get; set; }
        /** <summary> Semester in which the students are. </summary> */
        public StudySemester Semester { get; set; }
        /** <summary> Form of study the students attend. </summary> */
        public StudyForm Form { get; set; }
        /** <summary> Type of study the students are in. </summary> */
        public StudyType Type { get; set; }
        /** <summary> Language of study which the students use. </summary> */
        public StudyLanguage Language { get; set; }
        /** <summary> Number of student's in a study group. </summary> */
        public UInt16 StudentCount { get; set; }
        /** <summary> List of subjects the group is taking. </summary> */
        public Dictionary<UInt32,Subject> Subjects { get; set; }

        /** 
         * <summary> Constructor from known parameters. </summary>
         * <param name="id"> Group's ID in the DB. </param>
         * <param name="abbreviation"> Abbreaviation of the program the students are in. </param>
         * <param name="name"> Name of the program the students are in. </param>
         * <param name="year"> Year/grade to which students belong to. </param>
         * <param name="semester"> Semester in which the students are. </param>
         * <param name="form"> Form of study the students attend. </param>
         * <param name="type"> Type of study the students are in. </param>
         * <param name="language"> Language of study in which the student use. </param>
         * <param name="studentCount"> Number of student's in a study group. </param>
         */
        public StudentGroup(UInt32 id, string abbreviation, string name, StudyYear year, 
            StudySemester semester, StudyForm form, StudyType type, StudyLanguage language,
            UInt16 studentCount)
        {
            this.Id = id;
            this.Abbreviation = abbreviation;
            this.Name = name;
            this.Year = year;
            this.Semester = semester;
            this.Form = form;
            this.Type = type;
            this.Language = language;
            this.StudentCount = studentCount;
            this.Subjects = new Dictionary<UInt32,Subject>();
        }

        /** 
         * <summary> Constructor for filling the parameters later. </summary> 
         */
        public StudentGroup()
        {
            this.Id = 0;
            this.Abbreviation = "";
            this.Name = "";
            this.Year = StudyYear.Unknown;
            this.Semester = StudySemester.Unknown;
            this.Form = StudyForm.Unknown;
            this.Type = StudyType.Unknown;
            this.Language = StudyLanguage.Unknown;
            this.StudentCount = 0;
            this.Subjects = new Dictionary<UInt32,Subject>();
        }

        /** 
         * <summary> Constructor for searching. </summary> 
         */
        public StudentGroup(UInt32 id)
        {
            this.Id = id;
        }

        /**
         * <summary> Assign a subject to the student group. </summary>
         * <param name="s"> Subject to be assigned. </param>
         */
        public void assignSubject(Subject s)
        {
            this.Subjects.Add(s.Id, s);
        }

        /**
         * <summary> Remove a subject from the student group. </summary> 
         * <param name="s"> Subject to be assigned. </param>
         */
        public void removeSubject(Subject s)
        {
            this.Subjects.Remove(s.Id);
        }
    }
}
