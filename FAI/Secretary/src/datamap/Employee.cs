using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secretary
{
    /** <summary> Status of an employee - used to mark doctorate students. </summary> */
    public enum EmployeeStatus : Byte
    {
        Unknown = 0,
        Regular = 1,
        Doctorate = 2
    }

    /** 
     * <summary> Class for an employee of the departement. </summary>
     */
    public class Employee
    {
        /** <summary> Employee's id in the DB. </summary> */
        public UInt32 Id { get; set; }
        /** <summary> Emplyee's name with titles etc. </summary> */
        public string Name { get; set; }
        /** <summary> E-mail address to a work mail. </summary> */
        public string WorkMail { get; set; }
        /** <summary> E-mail address to a private mail. </summary> */
        public string PrivateMail { get; set; }
        /** <summary> Total work points, 1000 points = work load 1. </summary> */
        public UInt16 WorkPoints { get; set; }
        /** <summary> Total work points without subjects in english. </summary> */
        public UInt16 WorkPointsWithoutEnglish { get; set; }
        /** <summary> Workload, 1 equals to full-time job. </summary> */
        public double WorkLoad { get; set; }
        /** <summary> Is employee a doctorate student. </summary> */
        public EmployeeStatus Status { get; set; }
        /** <summary> Labels the employee is responsible for. </summary> */
        public Dictionary<UInt32,Label> Labels { get; set; }

        /**
         * <summary> Constructor from known parameters. </summary>
         * <param name="id"> Employee's ID in the DB. </param>
         * <param name="name"> Emplyee's name with titles etc. </param>
         * <param name="workMail"> E-mail address to a work mail. </param>
         * <param name="privateMail"> E-mail address to a private mail. </param>
         * <param name="workPoints"> Total work points without subjects in english. </param>
         * <param name="workPointsWithoutEnglish"> Total work points without subjects in english. </param>
         * <param name="workload"> Workload, 1 equals to full-time job. </param>
         * <param name="status"> Is employee a doctorate student. </param>
         */
        public Employee(UInt32 id, string name, string workMail, string privateMail, UInt16 workPoints,
            UInt16 workPointsWithoutEnglish, double workload, EmployeeStatus status)
        {
            this.Id = id;
            this.Name = name;
            this.WorkMail = workMail;
            this.PrivateMail = privateMail;
            this.WorkPoints = workPoints;
            this.WorkPointsWithoutEnglish = workPointsWithoutEnglish;
            this.WorkLoad = workload;
            this.Status = status;
            this.Labels = new Dictionary<UInt32,Label>();
        }

        /** 
         * <summary> Constructor for filling the parameters later. </summary>
         */
        public Employee()
        {
            this.Id = 0;
            this.Name = "";
            this.WorkMail = "";
            this.PrivateMail = "";
            this.WorkPoints = 0;
            this.WorkPointsWithoutEnglish = 0;
            this.WorkLoad = 0;
            this.Status = EmployeeStatus.Unknown;
            this.Labels = new Dictionary<UInt32,Label>();
        }

        /** 
         * <summary> Constructor for searching. </summary>
         */
        public Employee(UInt32 id)
        {
            this.Id = id;
        }

        /**
         * <summary> Assign a label to the employee. </summary>
         * <param name="l"> Label to be assigned. </param>
         */
        public void assignLabel(Label l)
        {
            this.Labels.Add(l.Id, l);
        }

        /**
         * <summary> Remove a label from the employee. </summary> 
         * <param name="l"> Label to be assigned. </param>
         */
        public void removeLabel(Label l)
        {
            this.Labels.Remove(l.Id);
        }
    }
}
