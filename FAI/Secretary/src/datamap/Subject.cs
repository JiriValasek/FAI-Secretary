using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secretary
{

    /** <summary> Subject guaranteed by the departement. </summary>*/
    public class Subject
    {
        /** <summary> Subject ID from the DB. </summary> */
        public UInt32 Id { get; set; }
        /** <summary> Subject abbreviation such as AK8PO. </summary> */
        public string Abbreviation { get; set; }
        /** <summary> Subject's full name. </summary> */
        public string Name { get; set; }
        /** <summary> Number of credits the subject have. </summary> */
        public Byte Credits { get; set; }
        /** <summary> Maximal group size for the subject. </summary> */
        public UInt16 MaxGroupSize { get; set; }
        /** <summary> Number of weeks the subject will have. </summary> */
        public Byte WeekCount { get; set; }
        /** <summary> Number of lectures the subject will have. </summary> */
        public Byte LectureCount { get; set; }
        /** <summary> Length of a lecture in hours. </summary> */
        public double LectureLength { get; set; }
        /** <summary> Number of seminars the subject will have. </summary> */
        public Byte SeminarCount { get; set; }
        /** <summary> Length of a seminar in hours. </summary> */
        public double SeminarLength { get; set; }
        /** <summary> Number of practice lessons the subject will have. </summary> */
        public Byte PracticeCount { get; set; }
        /** <summary> Length of a practice in hours. </summary> */
        public double PracticeLength { get; set; }
        /** <summary> Conditions required to finish the subject. </summary> */
        public SubjectConditions Conditions { get; set; }
        /** <summary> Labels assigned to the subject. </summary> */
        public Dictionary<UInt32,Label> Labels { get; set; }
        /** <summary> Student groups attending the subject. </summary> */
        public Dictionary<UInt32,StudentGroup> StudentGroups { get; set; }

        /** 
         * <summary> Constructor from known parameters. </summary> 
         * <param name="id"> Subject ID from the DB. </param>
         * <param name="abbreviation"> Subject abbreviation such as AK8PO. </param>
         * <param name="name"> Subject's full name. </param>
         * <param name="credits"> Number of credits the subject have. </param>
         * <param name="maxGroupSize"> Maximal group size for the subject. </param>
         * <param name="weekCount"> Number of weeks the subject will have. </param>
         * <param name="lectureCount"> Number of lectures the subject will have. </param>
         * <param name="lectureLength"> Length of a lecture in hours. </param>
         * <param name="seminarCount"> Number of seminars the subject will have. </param>
         * <param name="seminarLength"> Length of a seminar in hours. </param>
         * <param name="practiceCount"> Number of practice lessons the subject will have. </param>
         * <param name="practiceLength"> Length of a practice in hours. </param>
         * <param name="conditions"> Conditions required to finish the subject. </param>
         */
        public Subject(UInt32 id, string abbreviation, string name, Byte credits, UInt16 maxGroupSize,
            Byte weekCount, Byte lectureCount, double lectureLength, Byte seminarCount, double seminarLength,
            Byte practiceCount, double practiceLength, SubjectConditions conditions)
        {
            this.Id = id;
            this.Abbreviation = abbreviation;
            this.Name = name;
            this.Credits = credits;
            this.MaxGroupSize = maxGroupSize;
            this.WeekCount = weekCount;
            this.LectureCount = lectureCount;
            this.LectureLength = lectureLength;
            this.SeminarCount = seminarCount;
            this.SeminarLength = seminarLength;
            this.PracticeCount = practiceCount;
            this.PracticeLength = practiceLength;
            this.Conditions = conditions;
            this.Labels = new Dictionary<UInt32,Label>();
            this.StudentGroups = new Dictionary<UInt32,StudentGroup>();
        }

        /** 
         * <summary> Constructor for filling the parameters later. </summary> 
         */
        public Subject()
        {
            this.Id = 0;
            this.Abbreviation = "";
            this.Name = "";
            this.Credits = 0;
            this.MaxGroupSize = 0;
            this.WeekCount = 0;
            this.LectureCount = 0;
            this.SeminarCount = 0;
            this.PracticeCount = 0;
            this.Conditions = SubjectConditions.Unknown;
            this.Labels = new Dictionary<UInt32,Label>();
            this.StudentGroups = new Dictionary<UInt32,StudentGroup>();
        }

        /** 
         * <summary> Constructor for searching. </summary>
         */
        public Subject(UInt32 id)
        {
            this.Id = id;
        }
    } 
}
