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
        /** <summary> Label's ID in the DB. </summary> */
        public UInt32 Id { get; set; }
        /** <summary> Label's Name. </summary> */
        public string Name { get; set; }
        /** <summary> Employee teaching the subject. </summary> */
        public Employee LabelEmployee { get; set; }
        /** <summary> Subject this class belong to. </summary> */
        public Subject LabelSubject { get; set; }
        /** <summary> Type of the label. </summary> */
        public LabelType Type { get; set;  }
        /** <summary> Number of students belonging to the label/group. </summary> */
        public Byte StudentCount { get; set; }
        /** <summary> Number of hours the label is for. </summary> */
        public double HourCount
        {
            get
            {
                double hours;
                switch (Type)
                {
                    case LabelType.Lecture:
                        hours = LabelSubject.LectureCount * LabelSubject.LectureLength;
                        break;
                    case LabelType.Practice:
                        hours = LabelSubject.PracticeCount * LabelSubject.PracticeLength;
                        break;
                    case LabelType.Seminar:
                        hours = LabelSubject.SeminarCount * LabelSubject.SeminarLength;
                        break;
                    default:
                        hours = 0;
                        break;
                }
                return hours;
            }
        }
        /** <summary> Number of weeks the label is for. </summary> */
        public byte WeekCount
        {
            get
            {
                byte weeks;
                switch (Type)
                {
                    case LabelType.Lecture:
                        weeks = LabelSubject.LectureCount;
                        break;
                    case LabelType.Practice:
                        weeks = LabelSubject.PracticeCount;
                        break;
                    case LabelType.Seminar:
                        weeks = LabelSubject.SeminarCount;
                        break;
                    default:
                        weeks = 0;
                        break;
                }
                return weeks;
            }
        }
        /** <summary> Points the label is for. </summary> */
        public double Points { get; set; }

        /** 
         * <summary> Constructor from known parameters. </summary>
         * <param name="id"> Label's ID in the DB. </param>
         * <param name="employee"> Employee teaching the subject. </param>
         * <param name="subject"> Subject this class belong to. </param>
         * <param name="type"> Type of the label. </param>
         * <param name="studentCount"> Number of students belonging to the label/group. </param>
         */
        public Label(UInt32 id, string name, Employee employee, Subject subject, LabelType type, Byte studentCount)
        {
            this.Id = id;
            this.Name = name;
            this.LabelEmployee = employee;
            this.LabelSubject = subject;
            this.Type = type;
            this.StudentCount = studentCount;
        }

        /** 
         * <summary> Constructor for filling the parameters later. </summary> 
         */
        public Label()
        {
            this.Id = 0;
            this.LabelEmployee = null;
            this.LabelSubject = null;
            this.Type = LabelType.Unknown; ;
            this.StudentCount = 0;
        }
    }
}
