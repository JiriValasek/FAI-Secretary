using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Secretary
{

    /// <summary>
    /// Global variables for the whole project.
    /// </summary>
    public static class MyGlobals
    {
        public static readonly float WORKLOAD_TO_WORKPOINTS_COEF = 1000;
    }

    /// <summary>
    /// Selectable object for list boxes storing representing object inside.
    /// </summary>
    /// <typeparam name="T"> Type of the object to store. </typeparam>
    public class SelectableObject<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Variable for marking if the object is enabled.
        /// </summary>
        private bool isEnabled;

        /// <summary>
        /// Variable for marking if the object is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Variable for marking if the object is allowed to be selected.
        /// </summary>
        public bool IsEnabled 
        { 
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Stored data.
        /// </summary>
        public T ObjectData { get; set; }

        /// <summary>
        /// Declared event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="objectData"> Object to store. </param>
        /// <param name="isSelected"> Is the item selected. </param>
        /// <param name="isEnabled"> Is the item enabled. </param>
        public SelectableObject(T objectData, bool isSelected, bool isEnabled)
        {
            IsEnabled = isEnabled;
            IsSelected = isSelected;
            ObjectData = objectData;
        }

        /// <summary>
        /// OnPropertyChanged method to raise the event.
        /// </summary>
        /// <param name="name"> The calling member's name. </param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Colored object for label combo box storing representing label inside.
    /// </summary>
    public class ColoredLabelObject
    {

        /// <summary>
        /// Foreground color for the item.
        /// </summary>
        public object Foreground 
        {
            get
            {
                if (Object.StudentCount == 0)
                {
                    return "Red";
                }
                else if (this.Object.LabelEmployee == null)
                {
                    return "Orange";
                }
                else if (Object.LabelSubject == null)
                {
                    return "Magenta";
                }
                else
                {
                    return "Green";
                }
            }
        }

        /// <summary>
        /// Stored label.
        /// </summary>
        public Label Object { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="l"> Label to store. </param>
        public ColoredLabelObject(Label l)
        {
            Object = l;
        }
    }


    /// <summary>
    /// Colored object for subject combo box storing representing subject inside.
    /// </summary>
    public class ColoredSubjectObject
    {

        /// <summary>
        /// Foreground color for the item.
        /// </summary>
        public object Foreground
        {
            get
            {
                if (checkLabelsAreOK()) // students in groups are equal to students in labels
                {
                    return "Green";
                }
                else if (Object.StudentGroups != null && Object.StudentGroups.Count > 0)
                {                    
                    return "Orange";
                }
                else
                {
                    return "Red";
                }
            }
        }

        /// <summary>
        /// Method for checking labels are OK.
        /// </summary>
        /// <returns> True, if labels are all ok, false otherwise. </returns>
        private bool checkLabelsAreOK()
        {
            if (Object.StudentGroups == null || Object.StudentGroups.Count == 0)
            {
                return false;
            }
            var studentCount = Object.StudentGroups.Sum(x => x.Value.StudentCount);
            if (Object.LectureLength > 0 && 
                Object.Labels.Where(x => x.Value.Type == LabelType.Lecture).Sum(x => x.Value.StudentCount) != studentCount)
            {
                return false;
            }
            if (Object.SeminarLength > 0 &&
                Object.Labels.Where(x => x.Value.Type == LabelType.Seminar).Sum(x => x.Value.StudentCount) != studentCount)
            {
                return false;
            }
            if (Object.PracticeLength > 0 &&
                Object.Labels.Where(x => x.Value.Type == LabelType.Practice).Sum(x => x.Value.StudentCount) != studentCount)
            {
                return false;
            }
            if (Object.Conditions == SubjectConditions.Assessment &&
                Object.Labels.Where(x => x.Value.Type == LabelType.Assesment).Sum(x => x.Value.StudentCount) != studentCount)
            {
                return false;
            }
            if (Object.Conditions == SubjectConditions.ClassifiedAssesment &&
                Object.Labels.Where(x => x.Value.Type == LabelType.ClassifiedAssesment).Sum(x => x.Value.StudentCount) != studentCount)
            {
                return false;
            }
            if (Object.Conditions == SubjectConditions.AssesmentAndExam &&
                Object.Labels.Where(x => x.Value.Type == LabelType.Assesment).Sum(x => x.Value.StudentCount) != studentCount &&
                Object.Labels.Where(x => x.Value.Type == LabelType.Exam).Sum(x => x.Value.StudentCount) != studentCount)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stored subject.
        /// </summary>
        public Subject Object { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="s"> Subject to store. </param>
        public ColoredSubjectObject(Subject s)
        {
            Object = s;
        }
    }


    /// <summary>
    /// Colored object for student group combo box storing representing student group inside.
    /// </summary>
    public class ColoredStudentGroupObject
    {

        /// <summary>
        /// Foreground color for the item.
        /// </summary>
        public object Foreground
        {
            get
            {
                if (Object.StudentCount == 0)
                {
                    return "Red";
                }
                else if (Object.Subjects == null)
                {
                    return "Orange";
                }
                else
                {
                    return "Green";
                }
            }
        }

        /// <summary>
        /// Stored student group.
        /// </summary>
        public StudentGroup Object { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sg"> Student group to store. </param>
        public ColoredStudentGroupObject(StudentGroup sg)
        {
            Object = sg;
        }
    }


    /// <summary>
    /// Colored object for employee combo box storing representing employee inside.
    /// </summary>
    public class ColoredEmployeeObject
    {

        /// <summary>
        /// Foreground color for the item.
        /// </summary>
        public object Foreground
        {
            get
            {
                if (Object.WorkPoints < Object.WorkLoad * MyGlobals.WORKLOAD_TO_WORKPOINTS_COEF)
                {
                    return "Red";
                }
                else if (Object.WorkPoints > Object.WorkLoad * MyGlobals.WORKLOAD_TO_WORKPOINTS_COEF)
                {
                    return "Orange";
                }
                else
                {
                    return "Green";
                }
            }
        }

        /// <summary>
        /// Stored employee.
        /// </summary>
        public Employee Object { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="e"> Employee to store. </param>
        public ColoredEmployeeObject(Employee e)
        {
            Object = e;
        }
    }
}
