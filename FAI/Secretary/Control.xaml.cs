using Secretary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Secretary
{

    enum Tabs
    {
        Overview = 0,
        Employees = 1,
        Subjects = 2,
        StudentGroups = 3,
        Labels = 4,
        Weights = 5
    }

    enum EditMode
    {
        New = 0,
        Existing = 1
    }

    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : Window
    {
        private Scheduler scheduler;
        private EditMode edit;
        private static readonly Regex doubleNumber = new Regex(@"^[0-9]*(?:\.[0-9]*)?$", RegexOptions.Compiled);


        /**
         * <summary> Constructor for database object. </summary>
         * <param name="pwd"> Database password. </param>
         * <param name="uid"> Database username. </param>
         * <param name="ip"> Database ip or domain. </param>
         * <param name="port"> Database port. </param>
         */
        public Control(string uid, string pwd, string ip, string port)
        {
            InitializeComponent();
            var db = new Database("server=" + ip + ";port=" + port + ";uid=" + uid + ";pwd=" + pwd);
            scheduler = new Scheduler(db);
            tabs.SelectedIndex = 0;
            oEmployees.Content = scheduler.Employees.Count + " employees";
            oSubjects.Content = scheduler.Subjects.Count + " subjects";
            oStudentGroups.Content = scheduler.StudentGroups.Count + " student groups";
            oStudentGroupKinds.Content = scheduler.StudentGroups.Select(s => s.Abbreviation).Distinct().Count() + " group kinds";
        }

        private void TabsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabControl tc = (TabControl)e.Source;
                switch ((Tabs) tc.SelectedIndex)
                {
                    case Tabs.Employees:
                        eEmployees.ItemsSource = scheduler.Employees;
                        if (eEmployees.Items.Count > 1)
                        {
                            eEmployees.SelectedIndex = 0;
                        }
                        EmployeeEnableEdit(false);
                        break;
                    case Tabs.Subjects:
                        sSubjects.ItemsSource = scheduler.Subjects;
                        if (sSubjects.Items.Count > 1)
                        {
                            sSubjects.SelectedIndex = 0;
                        }
                        SubjectEnableEdit(false);
                        break;
                    case Tabs.StudentGroups:
                        sgGroups.ItemsSource = scheduler.StudentGroups;
                        if (sgGroups.Items.Count > 1)
                        {
                            sgGroups.SelectedIndex = 0;
                        }
                        StudentGroupEnableEdit(false);
                        break;
                    case Tabs.Labels:
                        lLabels.ItemsSource = scheduler.Labels;
                        if (lLabels.Items.Count > 1)
                        {
                            lLabels.SelectedIndex = 0;
                        }
                        LabelEnableEdit(false);
                        break;
                    case Tabs.Weights:
                        WeightsShow(scheduler.Weights);
                        WeightsEnableEdit(false);
                        break;
                }
            }
        }

        private void DisableTabChange(Tabs t)
        {
            tabOverview.IsEnabled = t == Tabs.Overview;
            tabEmployees.IsEnabled = t == Tabs.Employees;
            tabStudentGroups.IsEnabled = t == Tabs.StudentGroups;
            tabSubjects.IsEnabled = t == Tabs.Subjects;
            tabLabels.IsEnabled = t == Tabs.Labels;
            tabWeights.IsEnabled = t == Tabs.Weights;
        }

        private void EnableTabChange()
    {
            tabOverview.IsEnabled = true;
            tabEmployees.IsEnabled = true;
            tabStudentGroups.IsEnabled = true;
            tabSubjects.IsEnabled = true;
            tabLabels.IsEnabled = true;
            tabWeights.IsEnabled = true;
        }

        private void SubjectEnableEdit(bool enable)
        {
            sSubjects.IsEnabled = !enable;
            sNew.IsEnabled = !enable;
            sEdit.IsEnabled = !enable;
            sRemove.IsEnabled = !enable;
            sAbbreviation.IsEnabled = enable;
            sName.IsEnabled = enable;
            sCredits.IsEnabled = enable;
            sWeekCount.IsEnabled = enable;
            sMaxGroupSize.IsEnabled = enable;
            sLectureCount.IsEnabled = enable;
            sSeminarCount.IsEnabled = enable;
            sPracticeCount.IsEnabled = enable;
            sLectureLength.IsEnabled = enable;
            sSeminarLength.IsEnabled = enable;
            sPracticeLength.IsEnabled = enable;
            sConditions.IsEnabled = enable;
            sSave.IsEnabled = enable;
            sSaveNew.IsEnabled = enable;
            sCancel.IsEnabled = enable;
            sGenerate.IsEnabled = enable;
            // Do not disable these to allow showing whole list
            // sLabels.IsEnabled = enable;
            // sStudentGroups.IsEnabled = enable;
        }

        private void SubjectClearValues()
        {
            sAbbreviation.Text = "";
            sName.Text = "";
            sCredits.Text = "";
            sWeekCount.Text = "";
            sMaxGroupSize.Text = "";
            sLectureCount.Text = "";
            sSeminarCount.Text = "";
            sPracticeCount.Text = "";
            sLectureLength.Text = "";
            sSeminarLength.Text = "";
            sPracticeLength.Text = "";
            sConditions.SelectedIndex = 0; // Unkonwn
            sLabels.ItemsSource = new List<string>();
            sStudentGroups.ItemsSource = new List<string>();
        }

        private void SubjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Subject)
            {
                Subject s = (Subject)e.AddedItems[0];
                SubjectShow(s);
            }
        }

        private void SubjectShow(Subject s)
        {
            sAbbreviation.Text = s.Abbreviation;
            sName.Text = s.Name;
            sCredits.Text = s.Credits.ToString();
            sWeekCount.Text = s.WeekCount.ToString();
            sMaxGroupSize.Text = s.MaxGroupSize.ToString();
            sLectureCount.Text = s.LectureCount.ToString();
            sSeminarCount.Text = s.SeminarCount.ToString();
            sPracticeCount.Text = s.PracticeCount.ToString();
            sLectureLength.Text = s.LectureLength.ToString();
            sSeminarLength.Text = s.SeminarLength.ToString();
            sPracticeLength.Text = s.PracticeLength.ToString();
            sConditions.ItemsSource = Enum.GetNames(typeof(SubjectConditions))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sConditions.SelectedIndex = (int) s.Conditions;
            sLabels.ItemsSource = s.Labels;
            sLabels.SelectedIndex = 0;
            sStudentGroups.ItemsSource = s.StudentGroups;
            sStudentGroups.SelectedIndex = 0;
            SubjectEnableEdit(false);
        }

        private void SubjectNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            SubjectClearValues();
            SubjectEnableEdit(true);
            DisableTabChange(Tabs.Subjects);
            sSaveNew.IsEnabled = false;
        }

        private void SubjectEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            SubjectEnableEdit(true);
            DisableTabChange(Tabs.Subjects);
        }

        private void SubjectRemoveClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this subject?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                //this.scheduler.RemoveSubject(sSubjects.SelectedValue);
            }
        }

        private void SubjectCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                SubjectShow((Subject)sSubjects.SelectedItem);
                EnableTabChange();
            }

        }

        private void SubjectSaveClicked(object sender, RoutedEventArgs e)
        {
            if ( edit == EditMode.New)
            {
                try
                {
                    scheduler.AddSubject(new Subject(
                        0,
                        sAbbreviation.Text,
                        sName.Text,
                        Convert.ToByte(sCredits.Text),
                        Convert.ToUInt16(sMaxGroupSize.Text),
                        Convert.ToByte(sWeekCount.Text),
                        Convert.ToByte(sLectureCount.Text),
                        Convert.ToDouble(sLectureLength.Text),
                        Convert.ToByte(sSeminarCount.Text),
                        Convert.ToDouble(sSeminarLength.Text),
                        Convert.ToByte(sPracticeCount.Text),
                        Convert.ToDouble(sPracticeLength.Text),
                        (SubjectConditions) Convert.ToByte(sConditions.SelectedIndex)
                        ));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid values in edit boxes.",
                        "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Console.WriteLine(ex);
                }
            } 
            else if ( edit == EditMode.Existing)
            {

            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void SubjectSaveNewClicked(object sender, RoutedEventArgs e)
        {

            
        }

        private void SubjectGenerateClicked(object sender, RoutedEventArgs e)
        {


        }

        private void StudentGroupEnableEdit(bool enable)
        {
            sgGroups.IsEnabled = !enable;
            sgNew.IsEnabled = !enable;
            sgEdit.IsEnabled = !enable;
            sgRemove.IsEnabled = !enable;
            sgAbbreviation.IsEnabled = enable;
            sgName.IsEnabled = enable;
            sgYear.IsEnabled = enable;
            sgSemester.IsEnabled = enable;
            sgStudyForm.IsEnabled = enable;
            sgStudyType.IsEnabled = enable;
            sgStudyLanguage.IsEnabled = enable;
            sgStudentCount.IsEnabled = enable;
            sgSave.IsEnabled = enable;
            sgSaveNew.IsEnabled = enable;
            sgCancel.IsEnabled = enable;
            sgSubjects.IsEnabled = enable;
        }

        private void StudentGroupClearValues()
        {
            sgAbbreviation.Text = "";
            sgName.Text = "";
            sgYear.SelectedIndex = 0;
            sgSemester.SelectedIndex = 0;
            sgStudyForm.SelectedIndex = 0;
            sgStudyType.SelectedIndex = 0;
            sgStudyLanguage.SelectedIndex = 0;
            sgStudentCount.Text = "";
            sgSubjects.ItemsSource = scheduler.Subjects.Select(x => new SelectableObject<Subject>(x, false));
        }

        private void StudentGroupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is StudentGroup)
            {
                StudentGroup sg = (StudentGroup)e.AddedItems[0];
                StudentGroupShow(sg);
            }
        }

        private void StudentGroupShow(StudentGroup sg)
        {
            sgAbbreviation.Text = sg.Abbreviation;
            sgName.Text = sg.Name;
            sgYear.ItemsSource = Enum.GetNames(typeof(StudyYear));
            sgYear.SelectedIndex = (int)sg.Year;
            sgSemester.ItemsSource = Enum.GetNames(typeof(StudySemester));
            sgSemester.SelectedIndex = (int)sg.Semester;
            sgStudyForm.ItemsSource = Enum.GetNames(typeof(StudyForm))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sgStudyForm.SelectedIndex = (int)sg.Form;
            sgStudyType.ItemsSource = Enum.GetNames(typeof(StudyType));
            sgStudyType.SelectedIndex = (int)sg.Type;
            sgStudyLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage));
            sgStudyLanguage.SelectedIndex = (int)sg.Language;
            sgStudentCount.Text = sg.StudentCount.ToString();
            sgSubjects.ItemsSource = scheduler.Subjects.Select(x => new SelectableObject<Subject>(x, sg.Subjects.Keys.Contains(x.Id)));
            StudentGroupEnableEdit(false);
        }

        private void StudentGroupNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            StudentGroupClearValues();
            StudentGroupEnableEdit(true);
            DisableTabChange(Tabs.StudentGroups);
            sgSaveNew.IsEnabled = false;
        }

        private void StudentGroupEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            StudentGroupEnableEdit(true);
            DisableTabChange(Tabs.StudentGroups);
        }

        private void StudentGroupRemoveClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this student group?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                //this.scheduler.RemoveStudentGroup(sStudentGroups.SelectedValue);
            }
        }

        private void StudentGroupCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                StudentGroupShow((StudentGroup)sgGroups.SelectedItem);
                EnableTabChange();
            }

        }

        private void StudentGroupSaveClicked(object sender, RoutedEventArgs e)
        {


        }

        private void StudentGroupSaveNewClicked(object sender, RoutedEventArgs e)
        {


        }

        private void StudentGroupGenerateClicked(object sender, RoutedEventArgs e)
        {


        }

        private void EmployeeEnableEdit(bool enable)
        {
            eEmployees.IsEnabled = !enable;
            eNew.IsEnabled = !enable;
            eEdit.IsEnabled = !enable;
            eRemove.IsEnabled = !enable;
            eName.IsEnabled = enable;
            eWorkLoad.IsEnabled = enable;
            ePrivateMail.IsEnabled = enable;
            eWorkMail.IsEnabled = enable;
            eWorkPoints.IsEnabled = false; // computed
            eWorkPointsWithoutEnglish.IsEnabled = false; // computed
            eSave.IsEnabled = enable;
            eSaveNew.IsEnabled = enable;
            eCancel.IsEnabled = enable;
            eLabels.IsEnabled = enable;
            eStatus.IsEnabled = enable;
        }

        private void EmployeeClearValues()
        {
            eName.Text = "";
            eWorkLoad.Text = "";
            ePrivateMail.Text = "";
            eWorkMail.Text = "";
            eWorkPoints.Text = "";
            eWorkPointsWithoutEnglish.Text = "";
            eLabels.ItemsSource = scheduler.Labels.Where(x => x.LabelEmployee == null)
                .Select(x => new SelectableObject<Label>(x, false));
            eStatus.SelectedIndex = (int)EmployeeStatus.Unknown;
        }

        private void EmployeeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Employee)
            {
                Employee ee = (Employee)e.AddedItems[0];
                EmployeeShow(ee);
            }
        }

        private void EmployeeShow(Employee e)
        {
            eName.Text = e.Name;
            eWorkLoad.Text = e.WorkLoad.ToString();
            ePrivateMail.Text = e.PrivateMail;
            eWorkMail.Text = e.WorkMail;
            eWorkPoints.Text = e.WorkPoints.ToString();
            eWorkPointsWithoutEnglish.Text = e.WorkPointsWithoutEnglish.ToString();
            eLabels.ItemsSource = scheduler.Labels.Where(x => x.LabelEmployee == null || e.Labels.Values.Contains(x))
                .Select(x => new SelectableObject<Label>(x, e.Labels.Values.Contains(x)));
            eStatus.ItemsSource = Enum.GetNames(typeof(EmployeeStatus));
            eStatus.SelectedIndex = (int)e.Status;
        }

        private void EmployeeNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            EmployeeClearValues();
            EmployeeEnableEdit(true);
            DisableTabChange(Tabs.Employees);
            eSaveNew.IsEnabled = false;
        }

        private void EmployeeEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            EmployeeEnableEdit(true);
            DisableTabChange(Tabs.Employees);
        }

        private void EmployeeRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (eEmployees.SelectedItem.GetType() == typeof(Employee))
            {
                Employee ee = (Employee)eEmployees.SelectedItem;
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this student group?",
                    "FAI Secretary", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    scheduler.RemoveEmployee(ee);
                }
            }
        }

        private void EmployeeCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                EmployeeShow((Employee)eEmployees.SelectedItem);
                EmployeeEnableEdit(false);
                EnableTabChange();
            }

        }

        private void EmployeeSaveClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.New)
            {
                AddEmployee();
            }
            else if (edit == EditMode.Existing)
            {
                EditEmployee();
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        private void AddEmployee()
        {
            if (CheckEmail(eWorkMail.Text) && CheckEmail(ePrivateMail.Text) &&
               eEmployees.SelectedItem.GetType() == typeof(Employee))
            {
                try
                {
                    Employee ee = new Employee(
                            0,
                            eName.Text,
                            eWorkMail.Text,
                            ePrivateMail.Text,
                            0,
                            0,
                            Convert.ToDouble(eWorkLoad.Text),
                            (EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex)
                        );
                    foreach(object o in eLabels.Items)
                    {
                        if( o.GetType() == typeof(SelectableObject<Label>))
                        {
                            SelectableObject<Label> sol = (SelectableObject<Label>)o;
                            if (sol.IsSelected)
                            {
                                ee.assignLabel(sol.ObjectData);
                            }
                        }
                    }
                    scheduler.AddEmployee(ee);
                    eEmployees.ItemsSource = scheduler.Employees;
                    eEmployees.SelectedIndex = eEmployees.Items.Count - 1;
                    EmployeeShow((Employee)eEmployees.SelectedItem);
                    EmployeeEnableEdit(false);
                    EnableTabChange();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid values in edit boxes.",
                        "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(ex);

                }
            }
            else
            {
                MessageBox.Show("Invalid values in edit boxes.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditEmployee()
        {
            if (CheckEmail(eWorkMail.Text) && CheckEmail(ePrivateMail.Text) &&
                eEmployees.SelectedItem.GetType() == typeof(Employee))
            {
                try
                {
                    Employee ee = (Employee)eEmployees.SelectedItem;
                    Console.WriteLine(ee.Id.ToString());
                    Console.WriteLine(eName.Text);
                    Console.WriteLine(eWorkMail.Text);
                    Console.WriteLine(ePrivateMail.Text);
                    Console.WriteLine((Convert.ToUInt16(eWorkPoints.Text)).ToString());
                    Console.WriteLine((Convert.ToUInt16(eWorkPointsWithoutEnglish.Text)).ToString());
                    Console.WriteLine((Convert.ToDouble(eWorkLoad.Text)).ToString());
                    Console.WriteLine(((EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex)).ToString());
                    ee = new Employee(
                            ee.Id,
                            eName.Text,
                            eWorkMail.Text,
                            ePrivateMail.Text,
                            Convert.ToUInt16(eWorkPoints.Text),
                            Convert.ToUInt16(eWorkPointsWithoutEnglish.Text),
                            Convert.ToDouble(eWorkLoad.Text),
                            (EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex)
                        );
                    foreach (object o in eLabels.Items)
                    {
                        if (o.GetType() == typeof(SelectableObject<Label>))
                        {
                            SelectableObject<Label> sol = (SelectableObject<Label>)o;
                            if (sol.IsSelected)
                            {
                                ee.assignLabel(sol.ObjectData);
                            }
                        }
                    }
                    scheduler.UpdateEmployee(ee);
                    List<Employee> employees = scheduler.Employees;
                    var ind = employees.IndexOf(ee);
                    eEmployees.ItemsSource = employees;
                    eEmployees.SelectedIndex = ind;
                    EmployeeShow((Employee)eEmployees.SelectedItem);
                    EmployeeEnableEdit(false);
                    EnableTabChange();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid values in edit boxes.",
                        "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(ex);
                }
            }
            else
            {
                MessageBox.Show("Invalid values in edit boxes.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EmployeeSaveNewClicked(object sender, RoutedEventArgs e)
        {


        }

        private void EmployeeGenerateClicked(object sender, RoutedEventArgs e)
        {


        }

        private void LabelEnableEdit(bool enable)
        {
            lLabels.IsEnabled = !enable;
            lEdit.IsEnabled = !enable;
            lName.IsEnabled = enable;
            lType.IsEnabled = false;
            lStudentCount.IsEnabled = enable;
            lPoints.IsEnabled = false;
            lHourCount.IsEnabled = false;
            lWeekCount.IsEnabled = false;
            lLabelEmployee.IsEnabled = enable;
            lLabelSubject.IsEnabled = false;
            lSave.IsEnabled = enable;
            lSaveNew.IsEnabled = enable;
            lCancel.IsEnabled = enable;
        }

        private void LabelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Label)
            {
                Label l = (Label)e.AddedItems[0];
                LabelShow(l);
            }
        }

        private void LabelShow(Label l)
        {
            lName.Text = l.Name;
            lType.Text = l.Type.ToString();
            lStudentCount.Text = l.StudentCount.ToString();
            lPoints.Text = l.Points.ToString();
            lHourCount.Text = l.HourCount.ToString();
            lWeekCount.Text = l.WeekCount.ToString();
            lLabelEmployee.ItemsSource = scheduler.Employees;
            lLabelEmployee.SelectedIndex = scheduler.Employees.IndexOf(l.LabelEmployee);
            lLabelSubject.Text = l.LabelSubject != null ? l.LabelSubject.Name : "";
        }

        private void LabelEditClicked(object sender, RoutedEventArgs e)
        {

            edit = EditMode.Existing;
            LabelEnableEdit(true);
            DisableTabChange(Tabs.Labels);
        }

        private void LabelCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                LabelShow((Label)lLabels.SelectedItem);
                LabelEnableEdit(false);
                EnableTabChange();
            }

        }

        private void LabelSaveClicked(object sender, RoutedEventArgs e)
        {


        }

        private void LabelSaveNewClicked(object sender, RoutedEventArgs e)
        {


        }

        private void LabelGenerateClicked(object sender, RoutedEventArgs e)
        {


        }

        private void WeightsEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            WeightsEnableEdit(true);
            DisableTabChange(Tabs.Weights);
        }

        private void WeightsEnableEdit(bool enable)
        {
            wEdit.IsEnabled = !enable;
            wLecture.IsEnabled = enable;
            wPractice.IsEnabled = enable;
            wSeminar.IsEnabled = enable;
            wAssessment.IsEnabled = enable;
            wClassifiedAssessment.IsEnabled = enable;
            wExam.IsEnabled = enable;
            wEnglishLecture.IsEnabled = enable;
            wEnglishPractice.IsEnabled = enable;
            wEnglishSeminar.IsEnabled = enable;
            wEnglishAssessment.IsEnabled = enable;
            wEnglishClassifiedAssessment.IsEnabled = enable;
            wEnglishExam.IsEnabled = enable;
            wSave.IsEnabled = enable;
            wCancel.IsEnabled = enable;
        }

        private void WeightsShow(Weights w)
        {
            wLecture.Text = scheduler.Weights.Lecture.ToString();
            wPractice.Text = scheduler.Weights.Practice.ToString();
            wSeminar.Text = scheduler.Weights.Seminar.ToString();
            wAssessment.Text = scheduler.Weights.Assessment.ToString();
            wClassifiedAssessment.Text = scheduler.Weights.ClassifiedAssessment.ToString();
            wExam.Text = scheduler.Weights.Exam.ToString();
            wEnglishLecture.Text = scheduler.Weights.EnglishLecture.ToString();
            wEnglishPractice.Text = scheduler.Weights.EnglishPractice.ToString();
            wEnglishSeminar.Text = scheduler.Weights.EnglishSeminar.ToString();
            wEnglishAssessment.Text = scheduler.Weights.EnglishAssessment.ToString();
            wEnglishClassifiedAssessment.Text = scheduler.Weights.EnglishClassifiedAssessment.ToString();
            wEnglishExam.Text = scheduler.Weights.EnglishExam.ToString();
        }

        private void WeightsCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                WeightsShow(scheduler.Weights);
                WeightsEnableEdit(false);
                EnableTabChange();
            }
        }

        private void WeightsSaveClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                scheduler.Weights = new Weights(
                    scheduler.Weights.Id,
                    Convert.ToDouble(wLecture.Text),
                    Convert.ToDouble(wPractice.Text),
                    Convert.ToDouble(wSeminar.Text),
                    Convert.ToDouble(wAssessment.Text),
                    Convert.ToDouble(wClassifiedAssessment.Text),
                    Convert.ToDouble(wExam.Text),
                    Convert.ToDouble(wEnglishLecture.Text),
                    Convert.ToDouble(wEnglishPractice.Text),
                    Convert.ToDouble(wEnglishSeminar.Text),
                    Convert.ToDouble(wEnglishAssessment.Text),
                    Convert.ToDouble(wEnglishClassifiedAssessment.Text),
                    Convert.ToDouble(wEnglishExam.Text)
                    );
                WeightsShow(scheduler.Weights);
                WeightsEnableEdit(false);
                EnableTabChange();
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show("Saving weights failed.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex);
            }
        }

        private bool CheckEmail(string text)
        {
            try
            {
                MailAddress m = new MailAddress(text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void CheckDoubleTyping(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !doubleNumber.IsMatch(e.Text);
        }

        private void CheckDoublePasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!doubleNumber.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void CheckIntTyping(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !doubleNumber.IsMatch(e.Text);
        }

        private void CheckIntPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!doubleNumber.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
