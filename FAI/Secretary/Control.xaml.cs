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
using MySql.Data.MySqlClient;

namespace Secretary
{
    /// <summary>
    /// Tab names and coresponding indexes.
    /// </summary>
    enum Tabs : Byte
    {
        Overview = 0,
        Employees = 1,
        Subjects = 2,
        StudentGroups = 3,
        Labels = 4,
        Weights = 5
    }

    /// <summary>
    /// Edit mode for distinguishing between editing a new item or one already existing in the DB.
    /// </summary>
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
        /// <summary>
        /// Scheduler that maintains the database to be consistent with shown values.
        /// </summary>
        private Scheduler scheduler;

        /// <summary>
        /// State variable for editing.
        /// </summary>
        private EditMode edit;

        /// <summary>
        /// Regex for allowing only floating point value (or empty) strings in certain text boxes.
        /// </summary>
        private static readonly Regex doubleNumber = new Regex(@"^[0-9]*(?:\.[0-9]*)?$", RegexOptions.Compiled);

        /// <summary>
        /// Regex for allowing only int value (or empty) strings in certain text boxes.
        /// </summary>
        private static readonly Regex intNumber = new Regex(@"^[0-9]*$", RegexOptions.Compiled);


        /// <summary> 
        /// Variable used to remember last viewed subject, employee, student group or label between tab changes.
        /// </summary>
        private Dictionary<Tabs, int> lastInd;

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
            lastInd = new Dictionary<Tabs, int>();
            lastInd.Add(Tabs.Employees, 0);
            lastInd.Add(Tabs.Subjects, 0);
            lastInd.Add(Tabs.StudentGroups, 0);
            lastInd.Add(Tabs.Labels, 0);
        }

        /// <summary>
        /// Method used to handle loading of tabs.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void TabsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabControl tc = (TabControl)e.Source;
                switch ((Tabs) tc.SelectedIndex)
                {
                    case Tabs.Overview:
                        ShowOverview();
                        break;
                    case Tabs.Employees:
                        eEmployees.ItemsSource = scheduler.Employees
                            .Select(x => new ColoredEmployeeObject(x)).ToList();
                        if (eEmployees.Items.Count > 0)
                        {
                            eEmployees.SelectedIndex = eEmployees.Items.Count >= lastInd[Tabs.Employees]? 
                                Math.Max(lastInd[Tabs.Employees],0) : 0;
                        }
                        EmployeeEnableEdit(false);
                        break;
                    case Tabs.Subjects:
                        sSubjects.ItemsSource = scheduler.Subjects
                            .Select(x => new ColoredSubjectObject(x)).ToList();
                        if (sSubjects.Items.Count > 0)
                        {
                            sSubjects.SelectedIndex = sSubjects.Items.Count >= lastInd[Tabs.Subjects]?
                                Math.Max(lastInd[Tabs.Subjects],0) : 0;
                        }
                        SubjectEnableEdit(false);
                        break;
                    case Tabs.StudentGroups:
                        sgGroups.ItemsSource = scheduler.StudentGroups
                            .Select(x => new ColoredStudentGroupObject(x)).ToList();
                        if (sgGroups.Items.Count > 0)
                        {
                            sgGroups.SelectedIndex = sgGroups.Items.Count >= lastInd[Tabs.StudentGroups]?
                                Math.Max(lastInd[Tabs.StudentGroups],0) : 0;
                        }
                        StudentGroupEnableEdit(false);
                        break;
                    case Tabs.Labels:
                        lLabels.ItemsSource = scheduler.Labels.Select(x => new ColoredLabelObject(x)).ToList();
                        if (lLabels.Items.Count > 0)
                        {
                            lLabels.SelectedIndex = lLabels.Items.Count  >= lastInd[Tabs.Labels]?
                                Math.Max(lastInd[Tabs.Labels],0) : 0;
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

        /// <summary>
        /// Method for showing summary of current databese status on the overview tab.
        /// </summary>
        private void ShowOverview()
        {
            oSubjectTotal.Text = scheduler.Subjects.Count.ToString();
            var csos = scheduler.Subjects.Select(x => new ColoredSubjectObject(x)).ToList();
            oSubjectWithWrongLabels.Text = csos.Where(x => x.Foreground.ToString() == "Orange").Count().ToString();
            oSubjectWithoutStudents.Text = csos.Where(x => x.Foreground.ToString() == "Red").Count().ToString();
            oStudentGroupsTotal.Text = scheduler.StudentGroups.Count.ToString();
            oStudentGroupsEmpty.Text = scheduler.StudentGroups.Where(x => x.StudentCount == 0).Count().ToString();
            oStudentGroupsWithoutSubject.Text = scheduler.StudentGroups.Where(x => x.Subjects.Count == 0).Count().ToString();
            oEmployeesTotal.Text = scheduler.Employees.Count.ToString();
            oEmployeesLittleWorkpoints.Text = scheduler.Employees.Where(x => x.WorkPoints < x.WorkLoad * MyGlobals.WORKLOAD_TO_WORKPOINTS_COEF).Count().ToString();
            oEmployeesManyWorkpoints.Text = scheduler.Employees.Where(x => x.WorkPoints > x.WorkLoad * MyGlobals.WORKLOAD_TO_WORKPOINTS_COEF).Count().ToString();
            oLabelsTotal.Text = scheduler.Labels.Count.ToString();
            oLabelsEmpty.Text = scheduler.Labels.Where(x => x.StudentCount == 0).Count().ToString();
            oLabelsUnassigned.Text = scheduler.Labels.Where(x => x.LabelEmployee == null).Count().ToString();
            oLabelsSpecial.Text = scheduler.Labels.Where(x => x.LabelSubject == null).Count().ToString();
        }

        /// <summary>
        /// Method to disable all tabs except a given tab.
        /// </summary>
        /// <param name="t"> Tab to leave allowed. </param>
        private void DisableTabChange(Tabs t)
        {
            tabOverview.IsEnabled = t == Tabs.Overview;
            tabEmployees.IsEnabled = t == Tabs.Employees;
            tabStudentGroups.IsEnabled = t == Tabs.StudentGroups;
            tabSubjects.IsEnabled = t == Tabs.Subjects;
            tabLabels.IsEnabled = t == Tabs.Labels;
            tabWeights.IsEnabled = t == Tabs.Weights;
        }

        /// <summary>
        /// Method to enable all tabs.
        /// </summary>
        private void EnableTabChange()
    {
            tabOverview.IsEnabled = true;
            tabEmployees.IsEnabled = true;
            tabStudentGroups.IsEnabled = true;
            tabSubjects.IsEnabled = true;
            tabLabels.IsEnabled = true;
            tabWeights.IsEnabled = true;
        }

        /// <summary>
        /// Method to enable and disable editing of certain text boxes, combo boxes, etc. on the subjects tab.
        /// </summary>
        /// <param name="enable"> New IsEnabled value for the components. </param>
        private void SubjectEnableEdit(bool enable)
        {
            /*sLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sConditions.ItemsSource = Enum.GetNames(typeof(SubjectConditions))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));*/
            sSubjects.IsEnabled = !enable;
            sNew.IsEnabled = !enable;
            sEdit.IsEnabled = !enable && sSubjects.Items.Count > 0;
            sRemove.IsEnabled = !enable && sSubjects.Items.Count > 0;
            sName.IsEnabled = enable;
            sAbbreviation.IsEnabled = enable;
            sLanguage.IsEnabled = enable;
            sCredits.IsEnabled = enable;
            sWeekCount.IsEnabled = enable;
            sMaxGroupSize.IsEnabled = enable;
            sLectureLength.IsEnabled = enable;
            sSeminarLength.IsEnabled = enable;
            sPracticeLength.IsEnabled = enable;
            sConditions.IsEnabled = enable;
            sSave.IsEnabled = enable;
            sSaveNew.IsEnabled = enable;
            sCancel.IsEnabled = enable;
            sGenerate.IsEnabled = !enable;
            sLabels.IsEnabled = !enable;
            sStudentGroups.IsEnabled = !enable;
        }

        /// <summary>
        /// Method clearing text boxes and combo boxes for adding a new subject.
        /// </summary>
        private void SubjectClearValues()
        {
            sName.Text = "";
            sAbbreviation.Text = "";
            sLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sLanguage.SelectedIndex = 0; // Unkonwn
            sCredits.Text = "";
            sWeekCount.Text = "";
            sLectureLength.Text = "";
            sSeminarLength.Text = "";
            sPracticeLength.Text = "";
            sMaxGroupSize.Text = "";
            sConditions.ItemsSource = Enum.GetNames(typeof(SubjectConditions))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sConditions.SelectedIndex = 0; // Unkonwn
            sLabels.ItemsSource = new List<string>();
            sStudentGroups.ItemsSource = new List<string>();
        }

        /// <summary>
        /// Method for handling change of selected subject - shows it's values and saves the index.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ColoredSubjectObject)
            {
                ColoredSubjectObject cso = (ColoredSubjectObject)e.AddedItems[0];
                SubjectShow(cso.Object);
                lastInd[Tabs.Subjects] = sSubjects.SelectedIndex;
            }
        }

        /// <summary>
        /// Method for showing selected subject.
        /// </summary>
        /// <param name="s"> Subject to show. </param>
        private void SubjectShow(Subject s)
        {
            sName.Text = s.Name;
            sAbbreviation.Text = s.Abbreviation;
            sLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sLanguage.SelectedIndex = (int) s.Language;
            sCredits.Text = s.Credits.ToString();
            sWeekCount.Text = s.WeekCount.ToString();
            sMaxGroupSize.Text = s.MaxGroupSize.ToString();
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

        /// <summary>
        /// Method for preparing the subject form for adding a new one.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            SubjectClearValues();
            SubjectEnableEdit(true);
            DisableTabChange(Tabs.Subjects);
            sSaveNew.IsEnabled = false;
        }

        /// <summary>
        /// Method for preparing the subject form for editing of an existing subject.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            SubjectEnableEdit(true);
            DisableTabChange(Tabs.Subjects);
        }

        /// <summary>
        /// Method for removing of a selected subject.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (sSubjects.SelectedValue.GetType() == typeof(ColoredSubjectObject))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this subject?",
                    "FAI Secretary", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    ColoredSubjectObject cso = (ColoredSubjectObject)sSubjects.SelectedValue;
                    this.scheduler.RemoveSubject(cso.Object);
                    sSubjects.ItemsSource = scheduler.Subjects
                            .Select(x => new ColoredSubjectObject(x)).ToList();
                    if (sSubjects.Items.Count > 0)
                    {
                        sSubjects.SelectedIndex = 0;
                        SubjectShow(((ColoredSubjectObject)sSubjects.SelectedItem).Object);
                    }
                    else
                    {
                        SubjectClearValues();
                    }
                    SubjectEnableEdit(false);
                    EnableTabChange();
                }
            }
        }

        /// <summary>
        /// Method canceling editing/adding a new subject.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                if (sSubjects.SelectedItem != null && sSubjects.SelectedItem.GetType() == typeof(ColoredSubjectObject))
                {
                    SubjectShow(((ColoredSubjectObject)sSubjects.SelectedItem).Object);
                }
                else
                {
                    SubjectClearValues();
                }
                SubjectEnableEdit(false);
                EnableTabChange();
            }

        }

        /// <summary>
        /// Method handling saving of an updated subject form.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectSaveClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.New)
            {
                AddSubject();
            }
            else if (edit == EditMode.Existing)
            {
                EditSubject();
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Method for saving edited subject form as a new subject.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectSaveNewClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.Existing)
            {
                AddEditedSubject();
            }
            else if (edit == EditMode.New)
            {
                MessageBox.Show("Cannot add new employee as edited existing.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Method for generating of subject labels.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void SubjectGenerateClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Subject s = ((ColoredSubjectObject)sSubjects.SelectedItem).Object;
                scheduler.GenerateLabels(s);
                int ind = sSubjects.SelectedIndex;
                sSubjects.ItemsSource = scheduler.Subjects
                    .Select(x => new ColoredSubjectObject(x)).ToList();
                sSubjects.SelectedIndex = ind;
                SubjectShow(((ColoredSubjectObject)sSubjects.SelectedItem).Object);
                MessageBox.Show("Labels generated.", "FAI Secretary", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method checking validity of inputed values for new or edited subject.
        /// </summary>
        /// <param name="edit"> Parameter for distinguishing between edited and new subject. </param>
        /// <returns> True, if text boxes are valied. </returns>
        private bool SubjectTextBoxesValid(bool edit = false)
        {
            if (string.IsNullOrWhiteSpace(sName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(sAbbreviation.Text))
            {
                MessageBox.Show("Abbreviation cannot be empty.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sAbbreviation.Focus();
                return false;
            }
            if (sLanguage.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid language.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgYear.Focus();
                return false;
            }
            try
            {
                Convert.ToByte(sCredits.Text);
            }
            catch
            {
                MessageBox.Show("Invalid credits.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sCredits.Focus();
                return false;
            }
            try
            {
                Convert.ToUInt16(sMaxGroupSize.Text);
            }
            catch
            {
                MessageBox.Show("Invalid class size.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sMaxGroupSize.Focus();
                return false;
            }
            try
            {
                Convert.ToByte(sWeekCount.Text);
            }
            catch
            {
                MessageBox.Show("Invalid week count.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sWeekCount.Focus();
                return false;
            }
            try
            {
                Convert.ToDouble(sLectureLength.Text);
            }
            catch
            {
                MessageBox.Show("Invalid lecture length.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sLectureLength.Focus();
                return false;
            }
            try
            {
                Convert.ToDouble(sSeminarLength.Text);
            }
            catch
            {
                MessageBox.Show("Invalid seminar length.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sSeminarLength.Focus();
                return false;
            }
            try
            {
                Convert.ToDouble(sPracticeLength.Text);
            }
            catch
            {
                MessageBox.Show("Invalid practice length.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sPracticeLength.Focus();
                return false;
            }
            if (sConditions.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid conditions.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sConditions.Focus();
                return false;
            }
            if (edit)
            {
                return true;
            }
            foreach (object o in sSubjects.Items)
            {
                if (o.GetType() == typeof(ColoredSubjectObject))
                {
                    Subject s = ((ColoredSubjectObject)o).Object;
                    if (s.Abbreviation == sAbbreviation.Text)
                    {
                        MessageBox.Show("Dublicate abbreviation.", "FAI Secretary",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        sAbbreviation.Focus();
                        return false;
                    }
                    if (s.Name == sName.Text)
                    {
                        MessageBox.Show("Dublicate name.", "FAI Secretary",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        sName.Focus();
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Method checking and adding a new subject.
        /// </summary>
        private void AddSubject() 
        {
            if (!SubjectTextBoxesValid())
            {
                return;
            }
            try
            {
                Subject s = new Subject(0, sAbbreviation.Text, sName.Text, Convert.ToByte(sCredits.Text),
                    Convert.ToUInt16(sMaxGroupSize.Text), Convert.ToByte(sWeekCount.Text),
                     Convert.ToDouble(sLectureLength.Text), Convert.ToDouble(sSeminarLength.Text),
                     Convert.ToDouble(sPracticeLength.Text), (SubjectConditions)sConditions.SelectedIndex, 
                     (StudyLanguage)sLanguage.SelectedIndex);
                scheduler.AddSubject(s);
                sSubjects.ItemsSource = scheduler.Subjects
                    .Select(x => new ColoredSubjectObject(x)).ToList();
                sSubjects.SelectedIndex = sSubjects.Items.Count - 1;
                SubjectShow(((ColoredSubjectObject)sSubjects.SelectedItem).Object);
                SubjectEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method checking and editing an existing subject.
        /// </summary>
        private void EditSubject()
        {
            if (!SubjectTextBoxesValid(true))
            {
                return;
            }
            try
            {
                Subject s = ((ColoredSubjectObject)sSubjects.SelectedItem).Object;
                s = new Subject(s.Id, sAbbreviation.Text, sName.Text, Convert.ToByte(sCredits.Text),
                    Convert.ToUInt16(sMaxGroupSize.Text), Convert.ToByte(sWeekCount.Text),
                    Convert.ToDouble(sLectureLength.Text), Convert.ToDouble(sSeminarLength.Text),
                    Convert.ToDouble(sPracticeLength.Text), (SubjectConditions)sConditions.SelectedIndex,
                    (StudyLanguage)sLanguage.SelectedIndex);
                scheduler.UpdateSubject(s);
                sSubjects.ItemsSource = scheduler.Subjects.Select(x => new ColoredSubjectObject(x)).ToList();
                sSubjects.SelectedIndex = scheduler.Subjects.FindIndex(x => x.Id == s.Id);
                SubjectShow(((ColoredSubjectObject)sSubjects.SelectedItem).Object);
                SubjectEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method for adding an edited subject as a new one.
        /// </summary>
        private void AddEditedSubject()
        {
            if (!SubjectTextBoxesValid())
            {
                return;
            }
            try
            {
                Subject s = new Subject(0, sAbbreviation.Text, sName.Text, Convert.ToByte(sCredits.Text),
                    Convert.ToUInt16(sMaxGroupSize.Text), Convert.ToByte(sWeekCount.Text),
                    Convert.ToDouble(sLectureLength.Text), Convert.ToDouble(sSeminarLength.Text),
                     Convert.ToDouble(sPracticeLength.Text), (SubjectConditions)sConditions.SelectedIndex, (StudyLanguage)sLanguage.SelectedIndex);
                scheduler.AddSubject(s);
                sSubjects.ItemsSource = scheduler.Subjects
                    .Select(x => new ColoredSubjectObject(x)).ToList();
                sSubjects.SelectedIndex = sSubjects.Items.Count - 1;
                SubjectShow(((ColoredSubjectObject)sSubjects.SelectedItem).Object);
                SubjectEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method to enable and disable editing of certain text boxes, combo boxes, etc. on the student group tab.
        /// </summary>
        /// <param name="enable"> New IsEnabled value for the components. </param>
        private void StudentGroupEnableEdit(bool enable)
        {
            /*sgYear.ItemsSource = Enum.GetNames(typeof(StudyYear));
            sgSemester.ItemsSource = Enum.GetNames(typeof(StudySemester));
            sgStudyForm.ItemsSource = Enum.GetNames(typeof(StudyForm))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sgStudyType.ItemsSource = Enum.GetNames(typeof(StudyType));
            sgStudyLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage));*/
            sgGroups.IsEnabled = !enable;
            sgNew.IsEnabled = !enable;
            sgEdit.IsEnabled = !enable && sgGroups.Items.Count > 0;
            sgRemove.IsEnabled = !enable && sgGroups.Items.Count > 0;
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
            foreach( Object o in sgSubjects.Items )
            {
                if (o.GetType() == typeof(SelectableObject<Subject>))
                {
                    ((SelectableObject<Subject>)o).IsEnabled = enable;
                }
            }
            
        }

        /// <summary>
        /// Method clearing text boxes and combo boxes for adding a new student group.
        /// </summary>
        private void StudentGroupClearValues()
        {
            sgAbbreviation.Text = "";
            sgName.Text = "";
            sgYear.ItemsSource = Enum.GetNames(typeof(StudyYear));
            sgYear.SelectedIndex = 0;
            sgSemester.ItemsSource = Enum.GetNames(typeof(StudySemester));
            sgSemester.SelectedIndex = 0;
            sgStudyForm.ItemsSource = Enum.GetNames(typeof(StudyForm))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            sgStudyForm.SelectedIndex = 0;
            sgStudyType.ItemsSource = Enum.GetNames(typeof(StudyType));
            sgStudyType.SelectedIndex = 0;
            sgStudyLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage));
            sgStudyLanguage.SelectedIndex = 0;
            sgStudentCount.Text = "";
            sgSubjects.ItemsSource = scheduler.Subjects
                .Where(x => x.Language == (StudyLanguage)sgStudyLanguage.SelectedIndex)
                .Select(x => new SelectableObject<Subject>(x, false, true));
        }

        /// <summary>
        /// Method for handling change of selected student group - shows it's values and saves the index.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ColoredStudentGroupObject)
            {
                ColoredStudentGroupObject csgo = (ColoredStudentGroupObject)e.AddedItems[0];
                StudentGroupShow(csgo.Object);
                lastInd[Tabs.StudentGroups] = sgGroups.SelectedIndex;
            }
        }

        /// <summary>
        /// Method for showing selected student group.
        /// </summary>
        /// <param name="s"> Student group to show. </param>
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
            sgSubjects.ItemsSource = scheduler.Subjects
                .Where(x => x.Language == (StudyLanguage)sgStudyLanguage.SelectedIndex)
                .Select(x => new SelectableObject<Subject>(x, sg.Subjects.Keys.Contains(x.Id), false));
            StudentGroupEnableEdit(false);
        }

        /// <summary>
        /// Method for showing only subjects that have the same language as the student group.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sgSubjects.ItemsSource = scheduler.Subjects
                .Where(x => x.Language == (StudyLanguage)sgStudyLanguage.SelectedIndex)
                .Select(x => new SelectableObject<Subject>(x, false, true));
        }

        /// <summary>
        /// Method for preparing the student group form for adding a new one.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            StudentGroupClearValues();
            StudentGroupEnableEdit(true);
            DisableTabChange(Tabs.StudentGroups);
            sgSaveNew.IsEnabled = false;
        }

        /// <summary>
        /// Method for preparing the student group form for editing of an existing student group.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            StudentGroupEnableEdit(true);
            DisableTabChange(Tabs.StudentGroups);
        }

        /// <summary>
        /// Method for removing of a selected student group.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (sgGroups.SelectedItem.GetType() == typeof(ColoredStudentGroupObject))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this student group?",
                    "FAI Secretary", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    ColoredStudentGroupObject csgo = (ColoredStudentGroupObject)sgGroups.SelectedItem;
                    scheduler.RemoveStudentGroup(csgo.Object);
                    sgGroups.ItemsSource = scheduler.StudentGroups
                        .Select(x => new ColoredStudentGroupObject(x)).ToList();
                    if (sgGroups.Items.Count > 0)
                    {
                        sgGroups.SelectedIndex = 0;
                        StudentGroupShow(((ColoredStudentGroupObject)sgGroups.SelectedItem).Object);
                    }
                    else
                    {
                        StudentGroupClearValues();
                    }
                    EmployeeEnableEdit(false);
                    EnableTabChange();
                }
            }
        }

        /// <summary>
        /// Method canceling editing/adding a new student group.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                if (sgGroups.SelectedItem != null && sgGroups.SelectedItem.GetType() == typeof(ColoredStudentGroupObject))
                {
                    StudentGroupShow(((ColoredStudentGroupObject)sgGroups.SelectedItem).Object);
                }
                else
                {
                    StudentGroupClearValues();
                }
                StudentGroupEnableEdit(false);
                EnableTabChange();
            }
        }

        /// <summary>
        /// Method handling saving of an updated student group form.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupSaveClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.New)
            {
                AddStudentGroup();
            }
            else if (edit == EditMode.Existing)
            {
                EditStudentGroup();
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Method for saving edited student group form as a new student group.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void StudentGroupSaveNewClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.Existing)
            {
                AddEditedStudentGroup();
            }
            else if (edit == EditMode.New)
            {
                MessageBox.Show("Cannot add new employee as edited existing.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Method checking validity of inputed values for new or edited student group.
        /// </summary>
        /// <param name="edit"> Parameter for distinguishing between edited and new student group. </param>
        /// <returns> True, if text boxes are valied. </returns>
        private bool StudentGroupTextBoxesValid(bool edit=false)
        {
            if (string.IsNullOrWhiteSpace(sgName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(sgAbbreviation.Text))
            {
                MessageBox.Show("Abbreviation cannot be empty.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgAbbreviation.Focus();
                return false;
            }
            try
            {
                Convert.ToUInt16(sgStudentCount.Text);
            }
            catch
            {
                MessageBox.Show("Invalid student group.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgStudentCount.Focus();
                return false;
            }
            if (sgYear.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid year.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgYear.Focus();
                return false;
            }
            if (sgSemester.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid semester.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgSemester.Focus();
                return false;
            }
            if (sgStudyForm.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid study form.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgStudyForm.Focus();
                return false;
            }
            if (sgStudyType.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid study form.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgStudyType.Focus();
                return false;
            }
            if (sgStudyLanguage.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid study language.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                sgStudyType.Focus();
                return false;
            }
            if (edit)
            {
                return true;
            }
            foreach(object o in sgGroups.Items)
            {
                if ( o.GetType() == typeof(ColoredStudentGroupObject))
                {
                    StudentGroup sg = ((ColoredStudentGroupObject)o).Object;
                    if (sg.Abbreviation == sgAbbreviation.Text)
                    {
                        MessageBox.Show("Dublicate abbreviation.", "FAI Secretary",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        sgAbbreviation.Focus();
                        return false;
                    }
                    if (sg.Name == sgName.Text)
                    {
                        MessageBox.Show("Dublicate name.", "FAI Secretary",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        sgName.Focus();
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Method checking and adding a new student group.
        /// </summary>
        private void AddStudentGroup()
        {
            if (!StudentGroupTextBoxesValid())
            {
                return;
            }
            try
            {
                StudentGroup sg = new StudentGroup(0, sgAbbreviation.Text, sgName.Text,
                    (StudyYear)sgYear.SelectedIndex, (StudySemester)sgSemester.SelectedIndex,
                    (StudyForm)sgStudyForm.SelectedIndex, (StudyType)sgStudyType.SelectedIndex,
                    (StudyLanguage)sgStudyLanguage.SelectedIndex, Convert.ToUInt16(sgStudentCount.Text));
                foreach(object o in sgSubjects.Items)
                {
                    if ( o.GetType() == typeof(SelectableObject<Subject>))
                    {
                        SelectableObject<Subject> sos = (SelectableObject<Subject>)o;
                        if (sos.IsSelected)
                        {
                            sg.assignSubject(sos.ObjectData);
                        }
                    }
                }
                scheduler.AddStudentGroup(sg);
                sgGroups.ItemsSource = scheduler.StudentGroups
                    .Select(x => new ColoredStudentGroupObject(x)).ToList();
                sgGroups.SelectedIndex = sgGroups.Items.Count - 1;
                StudentGroupShow(((ColoredStudentGroupObject)sgGroups.SelectedItem).Object);
                StudentGroupEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method checking and editing an existing student group.
        /// </summary>
        private void EditStudentGroup()
        {
            if (!StudentGroupTextBoxesValid(true))
            {
                return;
            }
            try
            {
                StudentGroup sg = ((ColoredStudentGroupObject)sgGroups.SelectedItem).Object;
                    sg = new StudentGroup(sg.Id, sgAbbreviation.Text, sgName.Text,
                    (StudyYear)sgYear.SelectedIndex, (StudySemester)sgSemester.SelectedIndex,
                    (StudyForm)sgStudyForm.SelectedIndex, (StudyType)sgStudyType.SelectedIndex,
                    (StudyLanguage)sgStudyLanguage.SelectedIndex, Convert.ToUInt16(sgStudentCount.Text));
                foreach (object o in sgSubjects.Items)
                {
                    if (o.GetType() == typeof(SelectableObject<Subject>))
                    {
                        SelectableObject<Subject> sos = (SelectableObject<Subject>)o;
                        if (sos.IsSelected)
                        {
                            sg.assignSubject(sos.ObjectData);
                        }
                    }
                }
                scheduler.UpdateStudentGroup(sg);
                sgGroups.ItemsSource = scheduler.StudentGroups.Select(x => new ColoredStudentGroupObject(x)).ToList();
                sgGroups.SelectedIndex = scheduler.StudentGroups.FindIndex(x => x.Id == sg.Id);
                StudentGroupShow(((ColoredStudentGroupObject)sgGroups.SelectedItem).Object);
                StudentGroupEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method for adding an edited student group as a new one.
        /// </summary>
        private void AddEditedStudentGroup()
        {
            if (!StudentGroupTextBoxesValid())
            {
                return;
            }
            try
            {
                StudentGroup sg = new StudentGroup(0, sgAbbreviation.Text, sgName.Text,
                (StudyYear)sgYear.SelectedIndex, (StudySemester)sgSemester.SelectedIndex,
                (StudyForm)sgStudyForm.SelectedIndex, (StudyType)sgStudyType.SelectedIndex,
                (StudyLanguage)sgStudyLanguage.SelectedIndex, Convert.ToUInt16(sgStudentCount.Text));
                foreach (object o in sgSubjects.Items)
                {
                    if (o.GetType() == typeof(SelectableObject<Subject>))
                    {
                        SelectableObject<Subject> sos = (SelectableObject<Subject>)o;
                        if (sos.IsSelected)
                        {
                            sg.assignSubject(sos.ObjectData);
                        }
                    }
                }
                scheduler.AddStudentGroup(sg);
                sgGroups.ItemsSource = scheduler.StudentGroups
                    .Select(x => new ColoredStudentGroupObject(x)).ToList();
                sgGroups.SelectedIndex = sgGroups.Items.Count - 1;
                StudentGroupShow(((ColoredStudentGroupObject)sgGroups.SelectedItem).Object);
                StudentGroupEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method to enable and disable editing of certain text boxes, combo boxes, etc. on the employee tab.
        /// </summary>
        /// <param name="enable"> New IsEnabled value for the components. </param>
        private void EmployeeEnableEdit(bool enable)
        {
            //eStatus.ItemsSource = Enum.GetNames(typeof(EmployeeStatus));
            eEmployees.IsEnabled = !enable;
            eNew.IsEnabled = !enable;
            eEdit.IsEnabled = !enable && eEmployees.Items.Count > 0;
            eRemove.IsEnabled = !enable && eEmployees.Items.Count > 0;
            eName.IsEnabled = enable;
            eWorkLoad.IsEnabled = enable;
            ePrivateMail.IsEnabled = enable;
            eWorkMail.IsEnabled = enable;
            eWorkPoints.IsEnabled = false; // computed
            eWorkPointsWithoutEnglish.IsEnabled = false; // computed
            eSave.IsEnabled = enable;
            eSaveNew.IsEnabled = enable;
            eCancel.IsEnabled = enable;
            eStatus.IsEnabled = enable;
            foreach (Object o in eLabels.Items)
            {
                if (o.GetType() == typeof(SelectableObject<Label>))
                {
                    ((SelectableObject<Label>)o).IsEnabled = enable;
                }
            }
        }

        /// <summary>
        /// Method clearing text boxes and combo boxes for adding a new employee.
        /// </summary>
        private void EmployeeClearValues()
        {
            eName.Text = "";
            eWorkLoad.Text = "";
            ePrivateMail.Text = "";
            eWorkMail.Text = "";
            eWorkPoints.Text = "";
            eWorkPointsWithoutEnglish.Text = "";
            eLabels.ItemsSource = scheduler.Labels.Where(x => x.LabelEmployee == null)
                .Select(x => new SelectableObject<Label>(x, false, true));
            eStatus.ItemsSource = Enum.GetNames(typeof(EmployeeStatus));
            eStatus.SelectedIndex = (int)EmployeeStatus.Unknown;
        }

        /// <summary>
        /// Method for handling change of selected employee - shows it's values and saves the index.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ColoredEmployeeObject)
            {
                ColoredEmployeeObject ceo = (ColoredEmployeeObject)e.AddedItems[0];
                EmployeeShow(ceo.Object);
                lastInd[Tabs.Employees] = eEmployees.SelectedIndex;
            }
        }

        /// <summary>
        /// Method for showing selected employee.
        /// </summary>
        /// <param name="e"> Employee to show. </param>
        private void EmployeeShow(Employee e)
        {
            eName.Text = e.Name;
            eWorkLoad.Text = e.WorkLoad.ToString();
            ePrivateMail.Text = e.PrivateMail;
            eWorkMail.Text = e.WorkMail;
            eWorkPoints.Text = e.WorkPoints.ToString();
            eWorkPointsWithoutEnglish.Text = e.WorkPointsWithoutEnglish.ToString();
            eLabels.ItemsSource = scheduler.Labels
                .Where(x => x.LabelEmployee == null || e.Labels.Values.Contains(x))
                .Select(x => new SelectableObject<Label>(x, e.Labels.Values.Contains(x), false));
            eStatus.ItemsSource = Enum.GetNames(typeof(EmployeeStatus));
            eStatus.SelectedIndex = (int)e.Status;
        }

        /// <summary>
        /// Method for preparing the employee form for adding a new one.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            EmployeeClearValues();
            EmployeeEnableEdit(true);
            DisableTabChange(Tabs.Employees);
            eSaveNew.IsEnabled = false;
        }

        /// <summary>
        /// Method for preparing the employee form for editing of an existing employee.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            EmployeeEnableEdit(true);
            DisableTabChange(Tabs.Employees);
        }

        /// <summary>
        /// Method for removing of a selected employee.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (eEmployees.SelectedItem.GetType() == typeof(ColoredEmployeeObject))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this student group?",
                    "FAI Secretary", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    ColoredEmployeeObject ceo = (ColoredEmployeeObject)eEmployees.SelectedItem;
                    scheduler.RemoveEmployee(ceo.Object);
                    eEmployees.ItemsSource = scheduler.Employees
                            .Select(x => new ColoredEmployeeObject(x)).ToList();
                    if (eEmployees.Items.Count > 0)
                    {
                        eEmployees.SelectedIndex = 0;
                        EmployeeShow(((ColoredEmployeeObject)eEmployees.SelectedItem).Object);
                    }
                    else
                    {
                        EmployeeClearValues();
                    }
                    EmployeeEnableEdit(false);
                    EnableTabChange();
                }
            }
        }

        /// <summary>
        /// Method canceling editing/adding a new employee.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                if (eEmployees.SelectedItem != null && eEmployees.SelectedItem.GetType() == typeof(ColoredEmployeeObject))
                {
                    EmployeeShow(((ColoredEmployeeObject)eEmployees.SelectedItem).Object);
                }
                else
                {
                    EmployeeClearValues();
                }
                EmployeeEnableEdit(false);
                EnableTabChange();
            }
        }

        /// <summary>
        /// Method handling saving of an updated employee form.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
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

        /// <summary>
        /// Method for saving edited employee form as a new employee.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeSaveNewClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.Existing)
            {
                AddEditedEmployee();
            }
            else if (edit == EditMode.New)
            {
                MessageBox.Show("Cannot add new employee as edited existing.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Method for recomputing employee's points.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void EmployeeRecomputePoints(object sender, RoutedEventArgs e)
        {
            var points = 0d;
            var pointsCZ = 0d;
            foreach(Object o in eLabels.Items)
            {
                if ( o is SelectableObject<Label> && ((SelectableObject<Label>)o).IsSelected)
                {
                    points += ((SelectableObject<Label>)o).ObjectData.GetPoints(scheduler.Weights);
                    pointsCZ += ((SelectableObject<Label>)o).ObjectData.Language == StudyLanguage.Czech ?
                        ((SelectableObject<Label>)o).ObjectData.GetPoints(scheduler.Weights) : 0;
                }
            }
            eWorkPoints.Text = points.ToString();
            eWorkPointsWithoutEnglish.Text = pointsCZ.ToString();
        }

        /// <summary>
        /// Method checking validity of inputed values for new or edited employee.
        /// </summary>
        /// <param name="edit"> Parameter for distinguishing between edited and new employee. </param>
        /// <returns> True, if text boxes are valied. </returns>
        private bool EmployeeTextBoxesValid(bool edit=false)
        {
            if (!CheckEmail(eWorkMail.Text))
            {
                MessageBox.Show("Invalid work email format.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                eWorkMail.Focus();
                return false;
            }
            if (!CheckEmail(ePrivateMail.Text))
            {
                MessageBox.Show("Invalid work email format.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                ePrivateMail.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(eName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "FAI Secretary", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                eName.Focus();
                return false;
            }
            if (edit && !(eEmployees.SelectedItem.GetType() == typeof(ColoredEmployeeObject)))
            {
                MessageBox.Show("Select an employee.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                eEmployees.Focus();
                return false;
            }
            try
            {
                Convert.ToDouble(eWorkLoad.Text);
            }
            catch
            {
                MessageBox.Show("Invalid workload.", "FAI Secretary", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                eWorkLoad.Focus();
                return false;
            }
            try
            {
                var x = (EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex);
            }
            catch
            {
                MessageBox.Show("Invalid employee status.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                eStatus.Focus();
                return false;
            }
            if (edit)
            {
                return true;
            }
            foreach (object o in eEmployees.Items)
            {
                if (o.GetType() == typeof(ColoredEmployeeObject))
                {
                    Employee ee = ((ColoredEmployeeObject)o).Object;
                    if (ee.WorkMail == eWorkMail.Text)
                    {
                        MessageBox.Show("Dublicate workmail.", "FAI Secretary",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        eWorkMail.Focus();
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Method checking and adding a new employee.
        /// </summary>
        private void AddEmployee()
        {
            if (!EmployeeTextBoxesValid())
            {
                return;
            }
            try
            {
                Employee ee = new Employee(0, eName.Text, eWorkMail.Text, ePrivateMail.Text, 0, 0, 
                    Convert.ToDouble(eWorkLoad.Text), (EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex));
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
                eEmployees.ItemsSource = scheduler.Employees
                            .Select(x => new ColoredEmployeeObject(x)).ToList();
                eEmployees.SelectedIndex = eEmployees.Items.Count - 1;
                EmployeeShow(((ColoredEmployeeObject)eEmployees.SelectedItem).Object);
                EmployeeEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method checking and editing an existing employee.
        /// </summary>
        private void EditEmployee()
        {
            if (!EmployeeTextBoxesValid(true))
            {
                return;
            }
            try
            {
                Employee e = ((ColoredEmployeeObject)eEmployees.SelectedItem).Object;
                e = new Employee(e.Id, eName.Text, eWorkMail.Text, ePrivateMail.Text,
                        Convert.ToUInt16(eWorkPoints.Text), Convert.ToUInt16(eWorkPointsWithoutEnglish.Text),
                        Convert.ToDouble(eWorkLoad.Text), (EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex));
                foreach (object o in eLabels.Items)
                {
                    if (o.GetType() == typeof(SelectableObject<Label>))
                    {
                        SelectableObject<Label> sol = (SelectableObject<Label>)o;
                        if (sol.IsSelected)
                        {
                            e.assignLabel(sol.ObjectData);
                        }
                    }
                }
                scheduler.UpdateEmployee(e);
                eEmployees.ItemsSource = scheduler.Employees.Select(x => new ColoredEmployeeObject(x)).ToList();
                eEmployees.SelectedIndex = scheduler.Employees.FindIndex(x => x.Id == e.Id);
                EmployeeShow(((ColoredEmployeeObject)eEmployees.SelectedItem).Object);
                EmployeeEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method for adding an edited employee as a new one.
        /// </summary>
        private void AddEditedEmployee()
        {
            if (!EmployeeTextBoxesValid())
            {
                return;
            }
            Employee eOld = ((ColoredEmployeeObject)eEmployees.SelectedItem).Object;
            try
            {
                Employee eNew = new Employee(0, eName.Text, eWorkMail.Text, ePrivateMail.Text, 0, 0, 
                    Convert.ToDouble(eWorkLoad.Text), (EmployeeStatus)Convert.ToByte(eStatus.SelectedIndex));
                foreach (object o in eLabels.Items)
                {
                    if (o.GetType() == typeof(SelectableObject<Label>))
                    {
                        SelectableObject<Label> sol = (SelectableObject<Label>)o;
                        if (sol.IsSelected)
                        {
                            if (eOld.Labels.Values.Contains(sol.ObjectData))
                            {
                                MessageBox.Show("Labels cannot be assigned to two employees at once.",
                                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            eNew.assignLabel(sol.ObjectData);
                        }
                    }
                }
                scheduler.AddEmployee(eNew);
                eEmployees.ItemsSource = scheduler.Employees
                            .Select(x => new ColoredEmployeeObject(x)).ToList();
                eEmployees.SelectedIndex = eEmployees.Items.Count - 1;
                EmployeeShow(((ColoredEmployeeObject)eEmployees.SelectedItem).Object);
                EmployeeEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method to enable and disable editing of certain text boxes, combo boxes, etc. on the label tab.
        /// </summary>
        /// <param name="enable"> New IsEnabled value for the components. </param>
        /// <param name="special"> Enable text boxes for special label adding. </param>
        private void LabelEnableEdit(bool enable, bool special=false)
        {
            lLabels.IsEnabled = !enable;
            lNew.IsEnabled = !enable;
            lEdit.IsEnabled = !enable && lLabels.Items.Count > 0;
            lRemove.IsEnabled = !enable && lLabels.Items.Count > 0;
            lName.IsEnabled = enable;
            lType.IsEnabled = false || special;
            lLanguage.IsEnabled = false || special;
            lStudentCount.IsEnabled = enable;
            lPoints.IsEnabled = false;
            lHourCount.IsEnabled = false || special;
            lWeekCount.IsEnabled = false || special;
            lLabelEmployee.IsEnabled = enable;
            lLabelSubject.IsEnabled = false;
            lSave.IsEnabled = enable;
            lCancel.IsEnabled = enable;
        }

        /// <summary>
        /// Method clearing text boxes and combo boxes for adding a new label.
        /// </summary>
        private void LabelClearValues()
        {
            lName.Text = "";
            lType.ItemsSource = Enum.GetNames(typeof(LabelType))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            lType.SelectedIndex = 0;
            lLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            lLanguage.SelectedIndex = 0;
            lStudentCount.Text = "";
            lPoints.Text = "";
            lHourCount.Text = "";
            lWeekCount.Text = "";
            lLabelEmployee.ItemsSource = scheduler.Employees.Select(x => new ColoredEmployeeObject(x)).ToList();
            lLabelEmployee.SelectedIndex = -1;
            lLabelSubject.Text = "";
        }

        /// <summary>
        /// Method for handling change of selected label - shows it's values and saves the index.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void LabelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ColoredLabelObject)
            {
                ColoredLabelObject clo = (ColoredLabelObject)e.AddedItems[0];
                LabelShow(clo.Object);
                lastInd[Tabs.Labels] = lLabels.SelectedIndex;
            }
        }

        /// <summary>
        /// Method for showing selected label.
        /// </summary>
        /// <param name="l"> Label to show. </param>
        private void LabelShow(Label l)
        {
            lName.Text = l.Name;
            lType.ItemsSource = Enum.GetNames(typeof(LabelType))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            lType.SelectedIndex = (int)l.Type;
            lLanguage.ItemsSource = Enum.GetNames(typeof(StudyLanguage))
                .Select(x => Regex.Replace(x, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled));
            lLanguage.SelectedIndex = (int)l.Language;
            lStudentCount.Text = l.StudentCount.ToString();
            lPoints.Text = l.GetPoints(scheduler.Weights).ToString();
            lHourCount.Text = l.HourCount.ToString();
            lWeekCount.Text = l.WeekCount.ToString();
            if (l.LabelEmployee != null)
            {
                lLabelEmployee.ItemsSource = scheduler.Employees.Select(x => new ColoredEmployeeObject(x)).ToList();
                lLabelEmployee.SelectedIndex = scheduler.Employees.IndexOf(l.LabelEmployee);
            }
            else
            {
                lLabelEmployee.ItemsSource = null;
                lLabelEmployee.SelectedIndex = -1;
            }
            lLabelSubject.Text = l.LabelSubject != null ? l.LabelSubject.Name : "";
        }

        /// <summary>
        /// Method for preparing the label form for adding a new one.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void LabelNewClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.New;
            LabelClearValues();
            LabelEnableEdit(true, true);
            DisableTabChange(Tabs.Labels);
        }

        /// <summary>
        /// Method for preparing the label form for editing of an existing label.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void LabelEditClicked(object sender, RoutedEventArgs e)
        {
            var ind = lLabelEmployee.SelectedIndex;
            lLabelEmployee.ItemsSource = scheduler.Employees
                .Select(x => new ColoredEmployeeObject(x)).ToList();
            lLabelEmployee.SelectedIndex = ind;
            edit = EditMode.Existing;
            if (lLabels.SelectedItem is ColoredLabelObject)
            {
                if(((ColoredLabelObject)lLabels.SelectedItem).Object.LabelSubject == null)
                {
                    LabelEnableEdit(true,true);
                }
                else
                {
                    LabelEnableEdit(true);
                }
                DisableTabChange(Tabs.Labels);
            }
        }

        /// <summary>
        /// Method for removing of a selected label.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void LabelRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (lLabels.SelectedValue.GetType() == typeof(ColoredLabelObject))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this label?",
                    "FAI Secretary", MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    ColoredLabelObject clo = (ColoredLabelObject)lLabels.SelectedValue;
                    scheduler.RemoveLabel(clo.Object);
                    lLabels.ItemsSource = scheduler.Labels
                            .Select(x => new ColoredLabelObject(x)).ToList();
                    if (lLabels.Items.Count > 0)
                    {
                        lLabels.SelectedIndex = 0;
                        LabelShow(((ColoredLabelObject)lLabels.SelectedItem).Object);
                    }
                    else
                    {
                        LabelClearValues();
                    }
                    LabelEnableEdit(false);
                    EnableTabChange();
                }
            }
        }

        /// <summary>
        /// Method canceling editing/adding a new label.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void LabelCancelClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to throw away your edits?",
                "FAI Secretary", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.Yes)
            {
                if (lLabels.SelectedItem != null && lLabels.SelectedItem.GetType() == typeof(ColoredLabelObject))
                {
                    LabelShow(((ColoredLabelObject)lLabels.SelectedItem).Object);
                }
                else
                {
                    LabelClearValues();
                }
                LabelEnableEdit(false);
                EnableTabChange();
            }

        }

        /// <summary>
        /// Method handling saving of an updated label form.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void LabelSaveClicked(object sender, RoutedEventArgs e)
        {
            if (edit == EditMode.New)
            {
                AddLabel();
            }
            else if (edit == EditMode.Existing)
            {
                EditLabel();
            }
            else
            {
                MessageBox.Show("Unknown edit mode occured.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Method checking validity of inputed values for new or edited label.
        /// </summary>
        /// <param name="edit"> Parameter for distinguishing between edited and new label. </param>
        /// <returns> True, if text boxes are valied. </returns>
        private bool LabelTextBoxesValid(bool edit = false)
        {
            if (string.IsNullOrEmpty(lName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lName.Focus();
                return false;
            }
            if (lType.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid type.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lType.Focus();
                return false;
            }
            if (lLanguage.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid language.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lLanguage.Focus();
                return false;
            }
            try
            {
                Convert.ToUInt16(lStudentCount.Text);
            }
            catch
            {
                MessageBox.Show("Invalid student count.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lStudentCount.Focus();
                return false;
            }
            try
            {
                Convert.ToByte(lWeekCount.Text);
            }
            catch
            {
                MessageBox.Show("Invalid week count.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lWeekCount.Focus();
                return false;
            }
            try
            {
                Convert.ToDouble(lHourCount.Text);
            }
            catch
            {
                MessageBox.Show("Invalid hour count.", "FAI Secretary",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lHourCount.Focus();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method checking and adding a new label.
        /// </summary>
        private void AddLabel()
        {
            if (!LabelTextBoxesValid())
            {
                return;
            }
            try
            {
                Label l = new Label();
                if (lLabelEmployee.SelectedItem != null && lLabelEmployee.SelectedItem.GetType() == typeof(ColoredEmployeeObject))
                {
                    ColoredEmployeeObject ceo = (ColoredEmployeeObject)lLabelEmployee.SelectedItem;
                    l.LabelEmployee = ceo.Object;
                }
                l.Type = (LabelType)lType.SelectedIndex;
                l.Language = (StudyLanguage)lLanguage.SelectedIndex;
                l.Name = lName.Text;
                l.StudentCount = Convert.ToUInt16(lStudentCount.Text);
                l.HourCount = Convert.ToDouble(lHourCount.Text);
                l.WeekCount = Convert.ToByte(lWeekCount.Text);
                scheduler.AddLabel(l);
                lLabels.ItemsSource = scheduler.Labels
                    .Select(x => new ColoredLabelObject(x)).ToList();
                lLabels.SelectedIndex = lLabels.Items.Count - 1;
                LabelShow(((ColoredLabelObject)lLabels.SelectedItem).Object);
                LabelEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method checking and editing an existing label.
        /// </summary>
        private void EditLabel()
        {
            if (!LabelTextBoxesValid(true))
            {
                return;
            }
            try
            {
                Label l = ((ColoredLabelObject)lLabels.SelectedItem).Object;
                l = new Label(l.Id, l.Name, l.LabelEmployee, l.LabelSubject, l.Type, l.Language, l.StudentCount);
                if (lLabelEmployee.SelectedItem != null && lLabelEmployee.SelectedItem.GetType() == typeof(ColoredEmployeeObject))
                {
                    ColoredEmployeeObject ceo = (ColoredEmployeeObject)lLabelEmployee.SelectedItem;
                    l.LabelEmployee = ceo.Object;
                }
                l.Type = (LabelType)lType.SelectedIndex;
                l.Language = (StudyLanguage)lLanguage.SelectedIndex;
                l.Name = lName.Text;
                l.StudentCount = Convert.ToUInt16(lStudentCount.Text);
                l.HourCount = Convert.ToDouble(lHourCount.Text);
                l.WeekCount = Convert.ToByte(lWeekCount.Text);
                scheduler.UpdateLabel(l);
                lLabels.ItemsSource = scheduler.Labels.Select(x => new ColoredLabelObject(x)).ToList();
                lLabels.SelectedIndex = scheduler.Labels.FindIndex(x => x.Id == l.Id);
                LabelShow(((ColoredLabelObject)lLabels.SelectedItem).Object);
                LabelEnableEdit(false);
                EnableTabChange();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method for preparing the weights form for editing of weights.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void WeightsEditClicked(object sender, RoutedEventArgs e)
        {
            edit = EditMode.Existing;
            WeightsEnableEdit(true);
            DisableTabChange(Tabs.Weights);
        }

        /// <summary>
        /// Method to enable and disable editing of certain text boxes, combo boxes, etc. on the weights tab.
        /// </summary>
        /// <param name="enable"> New IsEnabled value for the components. </param>
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

        /// <summary>
        /// Method canceling of editing of weights.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
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

        /// <summary>
        /// Method handling saving of an updated weights form.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
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
            catch (MySqlException ex)
            {
                MessageBox.Show("Problem with MySQL database occured.\n" + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Method for checking if text is a valid email adress.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Method checking that typed text is a floating point number.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void CheckDoubleTyping(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !doubleNumber.IsMatch(e.Text);
        }

        /// <summary>
        /// Method checking that pasted text is a floating point number.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
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

        /// <summary>
        /// Method checking that typed text is an integer.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void CheckIntTyping(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !intNumber.IsMatch(e.Text);
        }

        /// <summary>
        /// Method checking that pasted text is an integer number.
        /// </summary>
        /// <param name="sender"> The object which initiated the event. </param>
        /// <param name="e"> Any event arguments. </param>
        private void CheckIntPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!intNumber.IsMatch(text))
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
