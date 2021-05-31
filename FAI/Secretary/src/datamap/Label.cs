using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secretary
{
    /** <summary> Type of a label. </summary> */
    public enum LabelType : Byte
    {
        Unknown,
        Practice,
        Seminar,
        Lecture,
        ClassifiedAssesment,
        Assesment,
        Exam
    }

    /** <summary> Label for a group of students signed up for a subject. </summary> */
    public class Label
    {
        /** <summary> Number of hours the label is for, when no LabelSubject specified. </summary> */
        private double hourCount = 0;

        /** <summary> Number of weeks the label is for, when no LabelSubject specified. </summary> */
        private byte weekCount = 0;

        /** <summary> Label's ID in the DB. </summary> */
        public UInt32 Id { get; set; }

        /** <summary> Label's Name. </summary> */
        public string Name { get; set; }

        /** <summary> Employee teaching the subject. </summary> */
        public Employee LabelEmployee { get; set; }

        /** <summary> Subject this class belong to. </summary> */
        public Subject LabelSubject { get; set; }

        /** <summary> Type of the label. </summary> */
        public LabelType Type { get; set; }

        /** <summary> Language of the label. </summary> */
        public StudyLanguage Language { get; set; }

        /** <summary> Number of students belonging to the label/group. </summary> */
        public UInt16 StudentCount { get; set; }

        /** <summary> Number of hours the label is for. </summary> */
        public double HourCount
        {
            get
            {
                if (LabelSubject == null)
                {
                    return hourCount;
                }
                double hours;
                switch (Type)
                {
                    case LabelType.Lecture:
                        hours = LabelSubject.WeekCount * LabelSubject.LectureLength;
                        break;
                    case LabelType.Practice:
                        hours = LabelSubject.WeekCount * LabelSubject.PracticeLength;
                        break;
                    case LabelType.Seminar:
                        hours = LabelSubject.WeekCount * LabelSubject.SeminarLength;
                        break;
                    default:
                        hours = 0;
                        break;
                }
                return hours;
            }
            set
            {
                hourCount = value;
            }
        }

        /** <summary> Number of weeks the label is for. </summary> */
        public byte WeekCount
        {
            get
            {
                byte weeks;
                if (LabelSubject == null)
                {
                    return weekCount;
                }
                else
                {
                    weeks = LabelSubject.WeekCount;
                    return weeks;
                }
            }
            set
            {
                weekCount = value;
            }
        }

        /**
         * <summary> Computes label points according to the given weights. </summary>
         * <param name="w"> Weights to use for computing. </param>
         */
        public double GetPoints(Weights w)
        {
            double coef = 0;
            switch (Type)
            {
                case LabelType.Lecture:
                    if (Language == StudyLanguage.Czech)
                    {
                        coef = w.Lecture;
                    }
                    else if (Language == StudyLanguage.English)
                    {
                        coef = w.EnglishLecture;
                    }
                    return WeekCount * HourCount * coef;
                case LabelType.Practice:
                    if (Language == StudyLanguage.Czech)
                    {
                        coef = w.Practice;
                    }
                    else if (Language == StudyLanguage.English)
                    {
                        coef = w.EnglishPractice;
                    }
                    return WeekCount * HourCount * coef;
                case LabelType.Seminar:
                    if (Language == StudyLanguage.Czech)
                    {
                        coef = w.Seminar;
                    }
                    else if (Language == StudyLanguage.English)
                    {
                        coef = w.EnglishSeminar;
                    }
                    return WeekCount * HourCount * coef;
                case LabelType.Assesment:
                    if (Language == StudyLanguage.Czech)
                    {
                        coef = w.Assessment;
                    }
                    else if (Language == StudyLanguage.English)
                    {
                        coef = w.EnglishAssessment;
                    }
                    return StudentCount * coef;
                case LabelType.ClassifiedAssesment:
                    if (Language == StudyLanguage.Czech)
                    {
                        coef = w.ClassifiedAssessment;
                    }
                    else if (Language == StudyLanguage.English)
                    {
                        coef = w.EnglishClassifiedAssessment;
                    }
                    return StudentCount * coef;
                case LabelType.Exam:
                    if (Language == StudyLanguage.Czech)
                    {
                        coef = w.Exam;
                    }
                    else if (Language == StudyLanguage.English)
                    {
                        coef = w.EnglishExam;
                    }
                    return StudentCount * coef;
                default:
                    return double.NaN;
            }
        }

        /** 
         * <summary> Constructor from known parameters. </summary>
         * <param name="id"> Label's ID in the DB. </param>
         * <param name="name"> Label's name. </param>
         * <param name="employee"> Employee teaching the subject. </param>
         * <param name="subject"> Subject this class belong to. </param>
         * <param name="type"> Type of the label. </param>
         * <param name="language"> Language of the label. </param>
         * <param name="studentCount"> Number of students belonging to the label/group. </param>
         * <param name="hourCount"> Number of hours the label is for. </param>
         * <param name="weekCount"> Number of weeks the label is for. </param>
         */
        public Label(UInt32 id, string name, Employee employee, Subject subject, LabelType type, StudyLanguage language, UInt16 studentCount, double hourCount = 0, byte weekCount = 0)
        {
            this.Id = id;
            this.Name = name;
            this.LabelEmployee = employee;
            this.LabelSubject = subject;
            this.Type = type;
            this.Language = language;
            this.StudentCount = studentCount;
            this.hourCount = hourCount;
            this.weekCount = weekCount;
        }

        /** 
         * <summary> Constructor for filling the parameters later. </summary> 
         */
        public Label()
        {
            this.Id = 0;
            this.LabelEmployee = null;
            this.LabelSubject = null;
            this.Type = LabelType.Unknown;
            this.Language = StudyLanguage.Unknown;
            this.StudentCount = 0;
        }
    }
}
