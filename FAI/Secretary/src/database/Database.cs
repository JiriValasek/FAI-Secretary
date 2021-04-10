using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Secretary
{
    /** <summary> Class controlling FAI Secretary database. </summary> */
    class Database
    {
        private MySqlConnection connection;
        const string dbName = "fai_secretary";
        private string cs;

        /**
         * <summary> Constructor for database object. </summary>
         * <param name="cs"> MySQL connection string. </param>
         */
        public Database(string cs)
        {
            this.cs = cs;
            this.connection = new MySqlConnection(cs);
            SetupDatabase();
            //InitialDatabaseTest();
        }

        /**
         * <summary> Opens a connection before a query. </summary>
         */
        private bool OpenConnection()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Opening DB: Unable to connect to the server.",
                            "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case 1042:
                        MessageBox.Show("Opening DB: Database offline.",
                            "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case 1045:
                        MessageBox.Show("Opening DB: Invalid username or password.",
                            "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show("Opening DB: " + ex.Number.ToString() + " - " + ex.Message,
                            "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
                return false;
            }
        }

        /**
         * <summary> Closes a connection after a query. </summary>
         */
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Closing DB: " + ex.Number.ToString() + " - " + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /**
         * <summary> Setups a database if it's not already created. </summary>
         */
        private bool SetupDatabase()
        {
            bool allNew = false;
            // create db
            string query = "CREATE DATABASE IF NOT EXISTS " + dbName + ";";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
                connection.ConnectionString = cs + ";database=" + dbName;
            }
            string[] setupQueries = {
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".subjects (
                    subject_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    abbreviation VARCHAR(31) UNIQUE NOT NULL,
                    name VARCHAR(127),
                    credits TINYINT UNSIGNED NOT NULL,
                    max_group_size SMALLINT UNSIGNED NOT NULL,
                    week_count TINYINT UNSIGNED NOT NULL,
                    lecture_count TINYINT UNSIGNED NOT NULL,
                    lecture_length TINYINT UNSIGNED NOT NULL,
                    seminar_count TINYINT UNSIGNED NOT NULL,
                    seminar_length TINYINT UNSIGNED NOT NULL,
                    practice_count TINYINT UNSIGNED NOT NULL,
                    practice_length TINYINT UNSIGNED NOT NULL,
                    conditions TINYINT UNSIGNED NOT NULL
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".labels (
                    label_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(127) UNIQUE NOT NULL,
                    type TINYINT UNSIGNED NOT NULL,
                    student_count TINYINT UNSIGNED NOT NULL
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".employees (
                    employee_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(127) NOT NULL,
                    workmail VARCHAR(127),
                    privatemail VARCHAR(127),
                    workpoints SMALLINT UNSIGNED NOT NULL,
                    workpoints_en SMALLINT UNSIGNED NOT NULL,
                    workload DOUBLE NOT NULL,
                    status TINYINT UNSIGNED NOT NULL
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".student_groups (
                    group_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    abbreviation VARCHAR(31) UNIQUE NOT NULL,
                    name VARCHAR(127) NOT NULL,
                    year TINYINT UNSIGNED NOT NULL,
                    semester TINYINT UNSIGNED NOT NULL,
                    form TINYINT UNSIGNED NOT NULL,
                    type TINYINT UNSIGNED NOT NULL,
                    language TINYINT UNSIGNED NOT NULL,
                    student_count SMALLINT UNSIGNED NOT NULL
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".weights (
                    weights_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    lecture DOUBLE NOT NULL,
                    practice DOUBLE NOT NULL,
                    seminar DOUBLE NOT NULL,
                    assessment DOUBLE NOT NULL,
                    classified_assessment DOUBLE NOT NULL,
                    exam DOUBLE NOT NULL,
                    lecture_en DOUBLE NOT NULL,
                    practice_en DOUBLE NOT NULL,
                    seminar_en DOUBLE NOT NULL,
                    assessment_en DOUBLE NOT NULL,
                    classified_assessment_en DOUBLE NOT NULL,
                    exam_en DOUBLE NOT NULL
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".subject_labels (
                    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    subject_id INT UNSIGNED NOT NULL,
                    label_id INT UNSIGNED UNIQUE NOT NULL,
                    FOREIGN KEY (subject_id) REFERENCES  " + dbName + @".subjects (subject_id) on delete cascade,
                    FOREIGN KEY (label_id) REFERENCES  " + dbName + @".labels (label_id) on delete cascade
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".subject_student_groups (
                    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    subject_id INT UNSIGNED NOT NULL,
                    group_id INT UNSIGNED NOT NULL,
                    FOREIGN KEY (subject_id) REFERENCES  " + dbName + @".subjects (subject_id) on delete cascade,
                    FOREIGN KEY (group_id) REFERENCES  " + dbName + @".student_groups (group_id) on delete cascade
                )  ENGINE=INNODB;",
                @"CREATE TABLE IF NOT EXISTS " + dbName + @".employee_labels (
                    id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
                    employee_id INT UNSIGNED NOT NULL,
                    label_id INT UNSIGNED UNIQUE NOT NULL,
                    FOREIGN KEY (employee_id) REFERENCES  " + dbName + @".employees (employee_id) on delete cascade,
                    FOREIGN KEY (label_id) REFERENCES  " + dbName + @".labels (label_id) on delete cascade
                )  ENGINE=INNODB;"};
            if (this.OpenConnection() == true)
            {
                foreach(string q in setupQueries)
                {
                    MySqlCommand cmd = new MySqlCommand(q, connection);
                    //cmd.ExecuteNonQuery();
                    cmd.ExecuteNonQuery();
                }
                this.CloseConnection();
            }
            return allNew;
        }

        /**
         * <summary> Deletes a database if it's already created. </summary>
         */
        private void DeleteDatabase()
        {
            // delate db
            string query = "DROP DATABASE IF EXISTS " + dbName + ";";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
                connection.ConnectionString = cs + ";database=" + dbName;
            }

        }

        /**
         * <summary> Inserts a subject into the database. Checks for validity of parameters. </summary>
         */
        public void InsertSubject(Subject s)
        {
            string query = @"INSERT INTO 
                subjects (abbreviation, name, credits, max_group_size, week_count, lecture_count, lecture_length, seminar_count, seminar_length, practice_count, practice_length, conditions)
                VALUES (@abbreviation, @name, @credits, @max_group_size, @week_count, @lecture_count, @lecture_length, @seminar_count, @seminar_length, @practice_count, @practice_length, @conditions)";

            // check string lengths
            if (s.Abbreviation.Length > 31)
            {
                MessageBox.Show("Abbreviation is too long. Subject not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (s.Name.Length > 127)
            {
                MessageBox.Show("Name is too long. Subject not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@abbreviation", s.Abbreviation);
                cmd.Parameters.AddWithValue("@name", s.Name);
                cmd.Parameters.AddWithValue("@credits", s.Credits);
                cmd.Parameters.AddWithValue("@max_group_size", s.MaxGroupSize);
                cmd.Parameters.AddWithValue("@week_count", s.WeekCount);
                cmd.Parameters.AddWithValue("@lecture_count", s.LectureCount);
                cmd.Parameters.AddWithValue("@lecture_length", s.LectureLength);
                cmd.Parameters.AddWithValue("@seminar_count", s.SeminarCount);
                cmd.Parameters.AddWithValue("@seminar_length", s.SeminarLength);
                cmd.Parameters.AddWithValue("@practice_count", s.PracticeCount);
                cmd.Parameters.AddWithValue("@practice_length", s.PracticeLength);
                cmd.Parameters.AddWithValue("@conditions", s.Conditions);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a subject in the database. Checks for validity of parameters. </summary>
         */
        public void UpdateSubject(Subject s)
        {
            string query = @"UPDATE subjects SET 
                abbreviation= @abbreviation,
                name= @name, 
                credits= @credits, 
                max_group_size= @max_group_size, 
                week_count= @week_count, 
                lecture_count= @lecture_count, 
                lecture_length= @lecture_length, 
                seminar_count= @seminar_count, 
                seminar_length= @seminar_length, 
                practice_count= @practice_count, 
                practice_length= @practice_length, 
                conditions= @conditions
                WHERE (subject_id= @subject_id)";

            // check string lengths
            if (s.Abbreviation.Length > 31)
            {
                MessageBox.Show("Abbreviation is too long. Subject not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (s.Name.Length > 127)
            {
                MessageBox.Show("Name is too long. Subject not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.Parameters.AddWithValue("@abbreviation", s.Abbreviation);
                cmd.Parameters.AddWithValue("@name", s.Name);
                cmd.Parameters.AddWithValue("@credits", s.Credits);
                cmd.Parameters.AddWithValue("@max_group_size", s.MaxGroupSize);
                cmd.Parameters.AddWithValue("@week_count", s.WeekCount);
                cmd.Parameters.AddWithValue("@lecture_count", s.LectureCount);
                cmd.Parameters.AddWithValue("@lecture_length", s.LectureLength);
                cmd.Parameters.AddWithValue("@seminar_count", s.SeminarCount);
                cmd.Parameters.AddWithValue("@seminar_length", s.SeminarLength);
                cmd.Parameters.AddWithValue("@practice_count", s.PracticeCount);
                cmd.Parameters.AddWithValue("@practice_length", s.PracticeLength);
                cmd.Parameters.AddWithValue("@conditions", s.Conditions);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes a subject from the database. </summary>
         */
        public void DeleteSubject(Subject s)
        {
            string query = "DELETE FROM subjects WHERE (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all subjects from the database. </summary>
         */
        public Dictionary<UInt32,Subject> GetSubjects()
        {
            string query = "SELECT * FROM subjects";
            Dictionary<UInt32,Subject> subjects = new Dictionary<UInt32,Subject>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                Subject s;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return subjects;
                }
                while (dataReader.Read())
                {
                    s = new Subject(
                        Convert.ToUInt32(dataReader["subject_id"]),
                        Convert.ToString(dataReader["abbreviation"]),
                        Convert.ToString(dataReader["name"]),
                        Convert.ToByte(dataReader["credits"]),
                        Convert.ToUInt16(dataReader["max_group_size"]),
                        Convert.ToByte(dataReader["week_count"]),
                        Convert.ToByte(dataReader["lecture_count"]),
                        Convert.ToByte(dataReader["lecture_length"]),
                        Convert.ToByte(dataReader["seminar_count"]),
                        Convert.ToByte(dataReader["seminar_length"]),
                        Convert.ToByte(dataReader["practice_count"]),
                        Convert.ToByte(dataReader["practice_length"]),
                        (SubjectConditions) Convert.ToByte(dataReader["conditions"])
                        );
                    subjects.Add(s.Id, s);
                }
                dataReader.Close();
                this.CloseConnection();
                return subjects;
            }
            else
            {
                return subjects;
            }
        }

        /**
         * <summary> Inserts a label into the database. </summary>
         */
        public void InsertLabel(Label l)
        {
            string query = @"INSERT INTO 
                labels (name, type, student_count)
                VALUES (@name, @type, @student_count)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", l.Name);
                cmd.Parameters.AddWithValue("@type", l.Type);
                cmd.Parameters.AddWithValue("@student_count", l.StudentCount);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a label in the database. </summary>
         */
        public void UpdateLabel(Label l)
        {
            string query = @"UPDATE labels SET 
                name= @name,
                type= @type,
                student_count= @student_count
                WHERE (label_id= @label_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.Parameters.AddWithValue("@name", l.Name);
                cmd.Parameters.AddWithValue("@type", l.Type);
                cmd.Parameters.AddWithValue("@student_count", l.StudentCount);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes a label from the database. </summary>
         */
        public void DeleteLabel(Label l)
        {
            string query = "DELETE FROM labels WHERE (label_id= @label_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all labels from the database. </summary>
         */
        public Dictionary<UInt32,Label> GetLabels()
        {
            string query = "SELECT * FROM labels";
            Dictionary<UInt32,Label> labels = new Dictionary<UInt32,Label>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                Label l;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return labels;
                }
                while (dataReader.Read())
                {
                    l = new Label(
                        Convert.ToUInt32(dataReader["label_id"]),
                        Convert.ToString(dataReader["name"]),
                        null,
                        null,
                        (LabelType)Convert.ToByte(dataReader["type"]),
                        Convert.ToByte(dataReader["student_count"])
                        );
                    labels.Add(l.Id, l);
                }
                dataReader.Close();
                this.CloseConnection();
                return labels;
            }
            else
            {
                return labels;
            }
        }

        /**
         * <summary> Inserts an employee into the database. Checks for validity of parameters. </summary>
         */
        public void InsertEmployee(Employee e)
        {
            string query = @"INSERT INTO 
                employees (name, workmail, privatemail, workpoints, workpoints_en, workload, status)
                VALUES (@name, @workmail, @privatemail, @workpoints, @workpoints_en, @workload, @status)";
            
            // check string lengths
            if (e.Name.Length > 127)
            {
                MessageBox.Show("Name is too long. Employee not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.WorkMail.Length > 127)
            {
                MessageBox.Show("Work mail is too long. Employee not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.PrivateMail.Length > 127)
            {
                MessageBox.Show("Private mail is too long. Employee not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", e.Name);
                cmd.Parameters.AddWithValue("@workmail", e.WorkMail);
                cmd.Parameters.AddWithValue("@privatemail", e.PrivateMail);
                cmd.Parameters.AddWithValue("@workpoints", e.WorkPoints);
                cmd.Parameters.AddWithValue("@workpoints_en", e.WorkPointsWithoutEnglish);
                cmd.Parameters.AddWithValue("@workload", e.WorkLoad);
                cmd.Parameters.AddWithValue("@status", e.Status);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates an employee in the database. Checks for validity of parameters. </summary>
         */
        public void UpdateEmployee(Employee e)
        {
            string query = @"UPDATE employees SET 
                name= @name,
                workmail= @workmail, 
                privatemail= @privatemail, 
                workpoints= @workpoints,
                workpoints_en= @workpoints_en,
                workload= @workload,
                status= @status
                WHERE (employee_id= @employee_id)";
            // check string lengths
            if (e.Name.Length > 127)
            {
                MessageBox.Show("Name is too long. Employee not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.WorkMail.Length > 127)
            {
                MessageBox.Show("Work mail is too long. Employee not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (e.PrivateMail.Length > 127)
            {
                MessageBox.Show("Private mail is too long. Employee not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.Parameters.AddWithValue("@name", e.Name);
                cmd.Parameters.AddWithValue("@workmail", e.WorkMail);
                cmd.Parameters.AddWithValue("@privatemail", e.PrivateMail);
                cmd.Parameters.AddWithValue("@workpoints", e.WorkPoints);
                cmd.Parameters.AddWithValue("@workpoints_en", e.WorkPointsWithoutEnglish);
                cmd.Parameters.AddWithValue("@workload", e.WorkLoad);
                cmd.Parameters.AddWithValue("@status", e.Status);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes an employee from the database. </summary>
         */
        public void DeleteEmployee(Employee e)
        {
            string query = "DELETE FROM employees WHERE (employee_id= @employee_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all employees from the database. </summary>
         */
        public Dictionary<UInt32,Employee> GetEmployees()
        {
            string query = "SELECT * FROM employees";
            Dictionary<UInt32,Employee> employees = new Dictionary<UInt32,Employee>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                Employee e;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return employees;
                }
                while (dataReader.Read())
                {
                    e = new Employee(
                        Convert.ToUInt32(dataReader["employee_id"]),
                        Convert.ToString(dataReader["name"]),
                        Convert.ToString(dataReader["workmail"]),
                        Convert.ToString(dataReader["privatemail"]),
                        Convert.ToUInt16(dataReader["workpoints"]),
                        Convert.ToUInt16(dataReader["workpoints_en"]),
                        Convert.ToSingle(dataReader["workload"]),
                        (EmployeeStatus)Convert.ToByte(dataReader["status"])
                        );
                    employees.Add(e.Id, e);
                }
                dataReader.Close();
                this.CloseConnection();
                return employees;
            }
            else
            {
                return employees;
            }
        }

        /**
         * <summary> Inserts a student group into the database. Checks for validity of parameters. </summary>
         */
        public void InsertStudentGroup(StudentGroup sg)
        {
            string query = @"INSERT INTO 
                student_groups (abbreviation, name, year, semester, form, type, language, student_count)
                VALUES (@abbreviation, @name, @year, @semester, @form, @type, @language, @student_count)";
            // check string lengths
            if (sg.Abbreviation.Length > 31)
            {
                MessageBox.Show("Abbreviation is too long. Group not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (sg.Name.Length > 127)
            {
                MessageBox.Show("Name is too long. Group not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@abbreviation", sg.Abbreviation);
                cmd.Parameters.AddWithValue("@name", sg.Name);
                cmd.Parameters.AddWithValue("@year", sg.Year);
                cmd.Parameters.AddWithValue("@semester", sg.Semester);
                cmd.Parameters.AddWithValue("@form", sg.Form);
                cmd.Parameters.AddWithValue("@type", sg.Type);
                cmd.Parameters.AddWithValue("@language", sg.Language);
                cmd.Parameters.AddWithValue("@student_count", sg.StudentCount);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a student group in the database. Checks for validity of parameters. </summary>
         */
        public void UpdateStudentGroup(StudentGroup sg)
        {
            string query = @"UPDATE student_groups SET 
                abbreviation= @abbreviation, 
                name= @name, 
                year= @year, 
                semester= @semester, 
                form= @form, 
                type= @type,
                language= @language, 
                student_count= @student_count 
                WHERE (group_id= @group_id)";
            // check string lengths
            if (sg.Abbreviation.Length > 31)
            {
                MessageBox.Show("Abbreviation is too long. Group not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (sg.Name.Length > 127)
            {
                MessageBox.Show("Name is too long. Group not inserted into the database.",
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (this.OpenConnection() == true)
            {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@group_id", sg.Id);
                    cmd.Parameters.AddWithValue("@abbreviation", sg.Abbreviation);
                    cmd.Parameters.AddWithValue("@name", sg.Name);
                    cmd.Parameters.AddWithValue("@year", sg.Year);
                    cmd.Parameters.AddWithValue("@semester", sg.Semester);
                    cmd.Parameters.AddWithValue("@form", sg.Form);
                    cmd.Parameters.AddWithValue("@type", sg.Type);
                    cmd.Parameters.AddWithValue("@language", sg.Language);
                    cmd.Parameters.AddWithValue("@student_count", sg.StudentCount);
                    cmd.ExecuteNonQuery();
                    this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes a student group from the database. </summary>
         */
        public void DeleteStudentGroup(StudentGroup sg)
        {
            string query = "DELETE FROM student_groups WHERE (group_id= @group_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@group_id", sg.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all student groups from the database. </summary>
         */
        public Dictionary<UInt32,StudentGroup> GetStudentGroups()
        {
            string query = "SELECT * FROM student_groups";
            Dictionary<UInt32,StudentGroup> studentGroups = new Dictionary<UInt32,StudentGroup>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                StudentGroup sg;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return studentGroups;
                }
                while (dataReader.Read())
                {
                    sg = new StudentGroup(
                        Convert.ToUInt32(dataReader["group_id"]),
                        Convert.ToString(dataReader["abbreviation"]),
                        Convert.ToString(dataReader["name"]),
                        (StudyYear) Convert.ToByte(dataReader["year"]),
                        (StudySemester) Convert.ToByte(dataReader["semester"]),
                        (StudyForm) Convert.ToByte(dataReader["form"]),
                        (StudyType) Convert.ToByte(dataReader["type"]),
                        (StudyLanguage) Convert.ToByte(dataReader["language"]),
                        Convert.ToUInt16(dataReader["student_count"])
                        );
                    studentGroups.Add(sg.Id, sg);
                }
                dataReader.Close();
                this.CloseConnection();
                return studentGroups;
            }
            else
            {
                return studentGroups;
            }
        }

        /**
         * <summary> Inserts weights into the database. </summary>
         */
        public void InsertWeights(Weights w)
        {
            string query = @"INSERT INTO 
                weights (lecture, practice, seminar, assessment, classified_assessment, exam, 
                lecture_en, practice_en, seminar_en, assessment_en, classified_assessment_en, exam_en)
                VALUES (@lecture, @practice, @seminar, @assessment, @classified_assessment, @exam, 
                @lecture_en, @practice_en, @seminar_en, @assessment_en, @classified_assessment_en, @exam_en)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@lecture", w.Lecture);
                cmd.Parameters.AddWithValue("@practice", w.Practice);
                cmd.Parameters.AddWithValue("@seminar", w.Seminar);
                cmd.Parameters.AddWithValue("@assessment", w.Assessment);
                cmd.Parameters.AddWithValue("@classified_assessment", w.ClassifiedAssessment);
                cmd.Parameters.AddWithValue("@exam", w.Exam);
                cmd.Parameters.AddWithValue("@lecture_en", w.EnglishLecture);
                cmd.Parameters.AddWithValue("@practice_en", w.EnglishPractice);
                cmd.Parameters.AddWithValue("@seminar_en", w.EnglishSeminar);
                cmd.Parameters.AddWithValue("@assessment_en", w.EnglishAssessment);
                cmd.Parameters.AddWithValue("@classified_assessment_en", w.EnglishClassifiedAssessment);
                cmd.Parameters.AddWithValue("@exam_en", w.EnglishExam);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates weights in the database. </summary>
         */
        public void UpdateWeights(Weights w)
        {
            string query = @"UPDATE weights SET 
                lecture= @lecture,
                practice= @practice,
                seminar= @seminar,
                assessment= @assessment,
                classified_assessment= @classified_assessment,
                exam= @exam, 
                lecture_en= @lecture_en,
                practice_en= @practice_en,
                seminar_en= @seminar_en,
                assessment_en= @assessment_en,
                classified_assessment_en= @classified_assessment_en,
                exam_en= @exam_en
                WHERE (weights_id= @weights_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@weights_id", w.Id);
                cmd.Parameters.AddWithValue("@lecture", w.Lecture);
                cmd.Parameters.AddWithValue("@practice", w.Practice);
                cmd.Parameters.AddWithValue("@seminar", w.Seminar);
                cmd.Parameters.AddWithValue("@assessment", w.Assessment);
                cmd.Parameters.AddWithValue("@classified_assessment", w.ClassifiedAssessment);
                cmd.Parameters.AddWithValue("@exam", w.Exam);
                cmd.Parameters.AddWithValue("@lecture_en", w.EnglishLecture);
                cmd.Parameters.AddWithValue("@practice_en", w.EnglishPractice);
                cmd.Parameters.AddWithValue("@seminar_en", w.EnglishSeminar);
                cmd.Parameters.AddWithValue("@assessment_en", w.EnglishAssessment);
                cmd.Parameters.AddWithValue("@classified_assessment_en", w.EnglishClassifiedAssessment);
                cmd.Parameters.AddWithValue("@exam_en", w.EnglishExam);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all weights from the database. </summary>
         */
        public Weights GetWeights()
        {
            string query = "SELECT * FROM weights";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                Weights w;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return null;
                }
                dataReader.Read();
                w = new Weights(
                        Convert.ToUInt32(dataReader["weights_id"]),
                        Convert.ToSingle(dataReader["lecture"]),
                        Convert.ToSingle(dataReader["practice"]),
                        Convert.ToSingle(dataReader["seminar"]),
                        Convert.ToSingle(dataReader["assessment"]),
                        Convert.ToSingle(dataReader["classified_assessment"]),
                        Convert.ToSingle(dataReader["exam"]),
                        Convert.ToSingle(dataReader["lecture_en"]),
                        Convert.ToSingle(dataReader["practice_en"]),
                        Convert.ToSingle(dataReader["seminar_en"]),
                        Convert.ToSingle(dataReader["assessment_en"]),
                        Convert.ToSingle(dataReader["classified_assessment_en"]),
                        Convert.ToSingle(dataReader["exam_en"])
                        );
                dataReader.Close();
                this.CloseConnection();
                return w;
            }
            else
            {
                return null;
            }
        }

        /**
         * <summary> Inserts a subject-label relation into the database. </summary>
         */
        public void InsertSubjectLabel(Subject s, Label l)
        {
            string query = @"INSERT INTO 
                subject_labels (subject_id, label_id)
                VALUES (@subject_id, @label_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a subject-label relation in the database. Changes a subject of a label. </summary>
         */
        public void UpdateLabelsSubject(Subject s, Label l)
        {
            string query = @"UPDATE subject_labels SET 
                subject_id= @subject_id
                WHERE (label_id= @label_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a subject-label relation in the database. Changes a label of a subject. </summary>
         *//*
        public void UpdateSubjectsLabel(Subject s, Label l)
        {
            string query = @"UPDATE subject_labels SET 
                label_id= @label_id
                WHERE (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }*/

        /**
         * <summary> Deletes a subject-label relation from the database. </summary>
         */
        public void DeleteSubjectLabel(Subject s, Label l)
        {
            string query = "DELETE FROM subject_labels WHERE (label_id= @label_id) AND (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes all subject's label relations from the database. </summary>
         */
        public void DeleteAllSubjectsLabels(Subject s)
        {
            string query = "DELETE FROM subject_labels WHERE (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all subject's subject-label relations from the database. </summary>
         */
        public List<UInt32> GetSubjectsLabels(Subject s)
        {
            string query = "SELECT * FROM subject_labels WHERE (subject_id= @subject_id)";
            List<UInt32> labels = new List<UInt32>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                UInt32 l;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return labels;
                }
                while (dataReader.Read())
                {
                    l = Convert.ToUInt32(dataReader["label_id"]);
                    labels.Add(l);
                }
                dataReader.Close();
                this.CloseConnection();
                return labels;
            }
            else
            {
                return labels;
            }
        }

        /**
         * <summary> Inserts a subject-student group relation into the database. </summary>
         */
        public void InsertSubjectStudentGroup(Subject s, StudentGroup sg)
        {
            string query = @"INSERT INTO 
                subject_student_groups (subject_id, group_id)
                VALUES (@subject_id, @group_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.Parameters.AddWithValue("@group_id", sg.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a subject-student group relation in the database. Changes a subject of a student group. </summary>
         */
        public void UpdateStudentGroupsSubject(Subject s, StudentGroup sg)
        {
            string query = @"UPDATE subject_student_groups SET 
                subject_id= @subject_id
                WHERE (group_id= @group_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.Parameters.AddWithValue("@group_id", sg.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates a subject-student group relation in the database. Changes a student group of a subject. </summary>
         */
        public void UpdateSubjectsStudentGroup(Subject s, StudentGroup sg)
        {
            string query = @"UPDATE subject_student_groups SET 
                group_id= @group_id
                WHERE (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.Parameters.AddWithValue("@group_id", sg.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes a subject-student group relation from the database. </summary>
         */
        public void DeleteSubjectStudentGroup(Subject s, StudentGroup sg)
        {
            string query = "DELETE FROM subject_student_groups WHERE (group_id= @group_id) AND (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.Parameters.AddWithValue("@group_id", sg.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes all subject's student group relations from the database. </summary>
         */
        public void DeleteAllSubjectsStudentGroups(Subject s)
        {
            string query = "DELETE FROM subject_student_groups WHERE (subject_id= @subject_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all subject's subject-student group relations from the database. </summary>
         */
        public List<UInt32> GetSubjectsStudentGroups(Subject s)
        {
            string query = "SELECT * FROM subject_student_groups WHERE (subject_id= @subject_id)";
            List<UInt32> labels = new List<UInt32>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@subject_id", s.Id);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                UInt32 l;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return labels;
                }
                while (dataReader.Read())
                {
                    l = Convert.ToUInt32(dataReader["group_id"]);
                    labels.Add(l);
                }
                dataReader.Close();
                this.CloseConnection();
                return labels;
            }
            else
            {
                return labels;
            }
        }

        /**
         * <summary> Inserts an employee-label relation into the database. </summary>
         */
        public void InsertEmployeeLabel(Employee e, Label l)
        {
            string query = @"INSERT INTO 
                employee_labels (employee_id, label_id)
                VALUES (@employee_id, @label_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates an employee-label relation in the database. Changes an employee of a label. </summary>
         */
        public void UpdateLabelsEmployee(Employee e, Label l)
        {
            string query = @"UPDATE employee_labels SET 
                employee_id= @employee_id
                WHERE (label_id= @label_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Updates an employee-label relation in the database. Changes a label of an employee. </summary>
         *//*
        public void UpdateEmployeesLabel(Employee e, Label l)
        {
            string query = @"UPDATE employee_labels SET 
                label_id= @label_id
                WHERE (employee_id= @employee_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }*/

        /**
         * <summary> Deletes an employee-label relation from the database. </summary>
         */
        public void DeleteEmployeeLabel(Employee e, Label l)
        {
            string query = "DELETE FROM employee_labels WHERE (label_id= @label_id) AND (employee_id= @employee_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.Parameters.AddWithValue("@label_id", l.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Deletes all employee label relations from the database. </summary>
         */
        public void DeleteAllEmployeeLabels(Employee e)
        {
            string query = "DELETE FROM employee_labels WHERE (employee_id= @employee_id)";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        /**
         * <summary> Get all employee's employee-label relations from the database. </summary>
         */
        public List<UInt32> GetEmployeesLabels(Employee e)
        {
            string query = "SELECT * FROM employee_labels WHERE (employee_id= @employee_id)";
            List<UInt32> labels = new List<UInt32>();
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@employee_id", e.Id);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                UInt32 l;
                if (!dataReader.HasRows)
                {
                    this.CloseConnection();
                    return labels;
                }
                while (dataReader.Read())
                {
                    l = Convert.ToUInt32(dataReader["label_id"]);
                    labels.Add(l);
                }
                dataReader.Close();
                this.CloseConnection();
                return labels;
            }
            else
            {
                return labels;
            }
        }

        /**
         * <summary> Initial test of the database. </summary>
         */
        private void InitialDatabaseTest()
        {
            /* Setup */
            SetupDatabase();
            /* Test of subject methods. */
            InsertSubject(new Subject(0, "TT6TT", "Test subject", 0, 256, 14, 10, 1, 11, 1, 12, 1, SubjectConditions.Unknown));
            InsertSubject(new Subject(0, "TT7TT", "Test subject", 0, 256, 14, 10, 1, 11, 1, 12, 1, SubjectConditions.Unknown));
            InsertSubject(new Subject(0, "TT8TT", "Test subject", 0, 256, 14, 10, 1, 11, 1, 12, 1, SubjectConditions.Unknown));
            InsertSubject(new Subject(0, "TT9TT", "Test subject", 0, 256, 14, 10, 1, 11, 1, 12, 1, SubjectConditions.Unknown));
            List<Subject> s = GetSubjects().Select(x => x.Value).ToList();
            //s.ForEach(x => Console.WriteLine(x));
            if (s.Count > 0)
            {
                UpdateSubject(new Subject(s.Last().Id, "TTXTT", "Test subject", 1, 128, 14, 10, 2, 11, 2, 12, 2, SubjectConditions.Unknown));
                DeleteSubject(s[s.Count - 2]);
            }
            s = GetSubjects().Select(x => x.Value).ToList();
            //s.ForEach(x => Console.WriteLine(x));
            /* Test of employee methods. */
            InsertEmployee(new Employee(0, "John Doe1", "johnDoe@test_work.uni", "johnDoe@test_private.com", 1000, 500, 1.0, EmployeeStatus.Unknown));
            InsertEmployee(new Employee(0, "John Doe2", "johnDoe@test_work.uni", "johnDoe@test_private.com", 1000, 500, 1.0, EmployeeStatus.Unknown));
            InsertEmployee(new Employee(0, "John Doe3", "johnDoe@test_work.uni", "johnDoe@test_private.com", 1000, 500, 1.0, EmployeeStatus.Unknown));
            InsertEmployee(new Employee(0, "John Doe4", "johnDoe@test_work.uni", "johnDoe@test_private.com", 1000, 500, 1.0, EmployeeStatus.Unknown));
            List<Employee> e = GetEmployees().Select(x => x.Value).ToList();
            //e.ForEach(x => Console.WriteLine(x.ToString()));
            if (e.Count > 0)
            {
                UpdateEmployee(new Employee(e[e.Count - 1].Id, "Jane Doe", "janeDoe@test_work.uni", "janeDoe@test_private.com", 500, 250, 0.5, EmployeeStatus.Doctorate));
                DeleteEmployee(e[e.Count - 2]);
            }
            e = GetEmployees().Select(x => x.Value).ToList();
            //e.ForEach(x => Console.WriteLine(x.ToString()));
            /* Test of group methods. */
            InsertStudentGroup(new StudentGroup(0, "TE1", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 20));
            InsertStudentGroup(new StudentGroup(0, "TE2", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 20));
            InsertStudentGroup(new StudentGroup(0, "TE3", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 20));
            InsertStudentGroup(new StudentGroup(0, "TE4", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 20));
            InsertStudentGroup(new StudentGroup(0, "TE5", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 20));
            InsertStudentGroup(new StudentGroup(0, "TE6", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 20));
            List<StudentGroup> g = GetStudentGroups().Select(x => x.Value).ToList();
            //g.ForEach(x => Console.WriteLine(x));
            if (g.Count > 0)
            {
                UpdateStudentGroup(new StudentGroup(g.Last().Id, "TEX", "TestGroup", StudyYear.Unknown, StudySemester.Unknown, StudyForm.Unknown, StudyType.Unknown, StudyLanguage.Unknown, 10));
                DeleteStudentGroup(g[g.Count - 2]);
            }
            g = GetStudentGroups().Select(x => x.Value).ToList();
            //g.ForEach(x => Console.WriteLine(x));
            /* Test of label methods. */
            InsertLabel(new Label(0, "Test 1", null, null, LabelType.Unknown, 20));
            InsertLabel(new Label(0, "Test 2", null, null, LabelType.Unknown, 20));
            InsertLabel(new Label(0, "Test 3", null, null, LabelType.Unknown, 20));
            InsertLabel(new Label(0, "Test 4", null, null, LabelType.Unknown, 20));
            InsertLabel(new Label(0, "Test 5", null, null, LabelType.Unknown, 20));
            InsertLabel(new Label(0, "Test 6", null, null, LabelType.Unknown, 20));
            List<Label> l = GetLabels().Select(x => x.Value).ToList();
            //l.ForEach(x => Console.WriteLine(x));
            if (l.Count > 0)
            {
                UpdateLabel(new Label(l[l.Count - 1].Id, "Test X", null, null, LabelType.Unknown, 10));
                DeleteLabel(l[l.Count - 2]);
            }
            l = GetLabels().Select(x => x.Value).ToList();
            //l.ForEach(x => Console.WriteLine(x));
            /* Test of weights methods. */
            InsertWeights(new Weights(0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0));
            Weights w = GetWeights();
            //Console.WriteLine(w);
            UpdateWeights(new Weights(w.Id, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5));
            /* Test of subject-label methods. */
            InsertSubjectLabel( s[s.Count - 1], l[l.Count - 1]);
            InsertSubjectLabel( s[s.Count - 2], l[l.Count - 2]);
            InsertSubjectLabel( s[s.Count - 1], l[l.Count - 3]);
            InsertSubjectLabel( s[s.Count - 2], l[l.Count - 4]);
            //GetSubjectsLabels(  s[s.Count - 1]).ForEach(x => Console.WriteLine(x));
            UpdateLabelsSubject(s[s.Count - 3], l[l.Count - 4]);
            //UpdateSubjectsLabel(s[s.Count - 3], l[l.Count - 5]);
            DeleteSubjectLabel( s[s.Count - 1], l[l.Count - 1]);
            //GetSubjectsLabels(  s[s.Count - 1]).ForEach(x => Console.WriteLine(x));
            /* Test of subject-student group methods. */
            InsertSubjectStudentGroup( s[s.Count - 1], g[g.Count - 1]);
            InsertSubjectStudentGroup( s[s.Count - 2], g[g.Count - 2]);
            InsertSubjectStudentGroup( s[s.Count - 1], g[g.Count - 3]);
            InsertSubjectStudentGroup( s[s.Count - 2], g[g.Count - 4]);
            //GetSubjectsStudentGroups(  s[s.Count - 1]).ForEach(x => Console.WriteLine(x));
            UpdateStudentGroupsSubject(s[s.Count - 3], g[g.Count - 4]);
            UpdateSubjectsStudentGroup(s[s.Count - 3], g[g.Count - 5]);
            DeleteSubjectStudentGroup( s[s.Count - 1], g[g.Count - 1]);
            //GetSubjectsStudentGroups(  s[s.Count - 1]).ForEach(x => Console.WriteLine(x));
            /* Test of employee-label methods. */
            InsertEmployeeLabel( e[e.Count - 1], l[l.Count - 1]);
            InsertEmployeeLabel( e[e.Count - 2], l[l.Count - 2]);
            InsertEmployeeLabel( e[e.Count - 1], l[l.Count - 3]);
            InsertEmployeeLabel( e[e.Count - 2], l[l.Count - 4]);
            //GetEmployeesLabels(  e[e.Count - 1]).ForEach(x => Console.WriteLine(x));
            UpdateLabelsEmployee(e[e.Count - 3], l[l.Count - 4]);
            //UpdateEmployeesLabel(e[e.Count - 3], l[l.Count - 5]);
            DeleteEmployeeLabel( e[e.Count - 1], l[l.Count - 1]);
            //GetEmployeesLabels(  e[e.Count - 1]).ForEach(x => Console.WriteLine(x));
            /* Delete database. */
            //DeleteDatabase();
        }
    }
}
