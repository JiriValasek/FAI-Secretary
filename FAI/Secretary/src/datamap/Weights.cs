using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secretary
{
    /** <summary> Weights to compute work points. </summary> */
    public class Weights
    {
        /** <summary> Weight's ID in the DB. </summary> */
        public UInt32 Id { get; set; }
        /** <summary> Weight for lectures - 1 lesson tought. </summary> */
        public double Lecture { get; set; }
        /** <summary> Weight for practices - 1 lesson tought. </summary> */
        public double Practice { get; set; }
        /** <summary> Weight for seminars - 1 lesson tought. </summary> */
        public double Seminar { get; set; }
        /** <summary> Weight for assessments - 1 assessment given. </summary> */
        public double Assessment { get; set; }
        /** <summary> Weight for classified assessments - 1 assessment given. </summary> */
        public double ClassifiedAssessment { get; set; }
        /** <summary> Weight for exams - 1 exam graded. </summary> */
        public double Exam { get; set; }
        /** <summary> Weight for lectures - 1 lesson tought in english. </summary> */
        public double EnglishLecture { get; set; }
        /** <summary> Weight for practices - 1 lesson tought in english. </summary> */
        public double EnglishPractice { get; set; }
        /** <summary> Weight for seminars - 1 lesson tought in english. </summary> */
        public double EnglishSeminar { get; set; }
        /** <summary> Weight for assessments - 1 assessment given for a class in english. </summary> */
        public double EnglishAssessment { get; set; }
        /** <summary> Weight for classified assessments - 1 assessment given for a class in english. </summary> */
        public double EnglishClassifiedAssessment { get; set; }
        /** <summary> Weight for exams - 1 exam graded for a class in english. </summary> */
        public double EnglishExam { get; set; }

        /** 
         * <summary> Constructor from known parameters. </summary> 
         * <param name="id"> Weight's ID in the DB.  </param>
         * <param name="lecture"> Weight for lectures - 1 lesson tought. </param>
         * <param name="practise"> Weight for practices - 1 lesson tought. </param>
         * <param name="seminar"> Weight for seminars - 1 lesson tought. </param>
         * <param name="assessment"> Weight for assessments - 1 assessment given. </param>
         * <param name="classifiedAssessment"> Weight for classified assessments - 1 assessment given. </param>
         * <param name="exam"> Weight for exams - 1 exam graded. </param>
         * <param name="englishLecture"> Weight for lectures - 1 lesson tought in english. </param>
         * <param name="englishPractise"> Weight for practices - 1 lesson tought in english. </param>
         * <param name="englishSeminar"> Weight for seminars - 1 lesson tought in english. </param>
         * <param name="englishAssessment"> Weight for assessments - 1 assessment given for a class in english. </param>
         * <param name="englishClassifiedAssessment"> Weight for classified assessments - 1 assessment given for a class in english. </param>
         * <param name="englishExam"> Weight for exams - 1 exam graded for a class in english. </param>
         */
        public Weights(UInt32 id, double lecture, double practise,
            double seminar, double assessment,
            double classifiedAssessment, double exam,
            double englishLecture, double englishPractise,
            double englishSeminar, double englishAssessment,
            double englishClassifiedAssessment,
            double englishExam)
        {
            this.Id = id;
            this.Lecture = lecture;
            this.Practice = practise;
            this.Seminar = seminar;
            this.Assessment = assessment;
            this.ClassifiedAssessment = classifiedAssessment;
            this.Exam = exam;
            this.EnglishLecture = englishLecture;
            this.EnglishPractice = englishPractise;
            this.EnglishSeminar = englishSeminar;
            this.EnglishAssessment = englishAssessment;
            this.EnglishClassifiedAssessment = englishClassifiedAssessment;
            this.EnglishExam = englishExam;
        }

        /** <summary> Constructor for filling the parameters later. </summary> */
        public Weights()
        {
            this.Id = 0;
            this.Lecture = 1;
            this.Practice = 1;
            this.Seminar = 1;
            this.Assessment = 1;
            this.ClassifiedAssessment = 1;
            this.Exam = 1;
            this.EnglishLecture = 1;
            this.EnglishPractice = 1;
            this.EnglishSeminar = 1;
            this.EnglishAssessment = 1;
            this.EnglishClassifiedAssessment = 1;
            this.EnglishExam = 1;
        }

    }
}
