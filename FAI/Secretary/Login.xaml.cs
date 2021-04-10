using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Security.Cryptography;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace Secretary
{
    /**
     * <summary>
     * Class for storing encrypted DB login information.
     * </summary>
     */
    public class DBLoginEncrypted
    {
        private byte[] username, password, ip, port;
        public byte[] Username { get; set; }
        public byte[] Password { get; set; }
        public byte[] IP { get; set; }
        public byte[] Port { get; set; }
    }

    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        private string credentialsFilepath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "credential.xml");
        // Additional salt
        static byte[] s_additionalEntropy = { 3, 8, 10, 98, 128, 245, 2, 0};

        public Login()
        {
            InitializeComponent();
            if (File.Exists(credentialsFilepath))
            {
                var login = this.ReadXML();
                this.dbUsername.Text = this.Unprotect(login.Username);
                this.dbPassword.Password = this.Unprotect(login.Password);
                this.dbIP.Text = this.Unprotect(login.IP);
                this.dbPort.Text = this.Unprotect(login.Port);
            }
        }

        /** <summary> Write credentials into an XML file. </summary> */
        public void WriteXML( DBLoginEncrypted login)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(DBLoginEncrypted));
            System.IO.FileStream file = System.IO.File.Create(
                this.credentialsFilepath);
            writer.Serialize(file, login);
            file.Close();
        }

        /** <summary> Read credentials from an XML file. </summary> */
        public DBLoginEncrypted ReadXML()
        {
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(DBLoginEncrypted));
            StreamReader file = new System.IO.StreamReader(
                this.credentialsFilepath);
            DBLoginEncrypted credentials = (DBLoginEncrypted)reader.Deserialize(file);
            file.Close();
            return credentials;
        }

        /** <summary> Delete XML file with credentials. </summary> */
        public void DeleteXML()
        {
            File.Delete(this.credentialsFilepath);
        }

        /** <summary> Encrypt credentials before saving. </summary> */
        public byte[] Protect(string data)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                return ProtectedData.Protect( Encoding.Unicode.GetBytes(data),
                    s_additionalEntropy, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                MessageBox.Show("Credentials encryption failed.",
                    "FAI Secretary", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }
        }

        /** <summary> Decrypt credentials after reading them. </summary> */
        public string Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return Encoding.Unicode.GetString(ProtectedData.Unprotect(data, 
                    s_additionalEntropy, DataProtectionScope.CurrentUser));
            }
            catch (CryptographicException e)
            {
                return "";
            }
        }

        /** <summary> Event handler for the "Connect" button. </summary> */
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cs = "server=" + this.dbIP.Text + ";" +
                    "port=" + this.dbPort.Text + ";" +
                    "uid=" + this.dbUsername.Text + ";" +
                    "pwd=" + this.dbPassword.Password;
                var con = new MySqlConnection(cs);
                con.Open();
                con.Close();
                DBLoginEncrypted login = new DBLoginEncrypted();
                login.Username = Protect(this.dbUsername.Text);
                login.Password = Protect(this.dbPassword.Password);
                login.IP = Protect(this.dbIP.Text);
                login.Port = Protect(this.dbPort.Text);
                WriteXML(login);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Unable to connect to a DB.\n" +
                    ex.Number.ToString() + " - " + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Control window = new Control(this.dbUsername.Text,
                this.dbPassword.Password, this.dbIP.Text, this.dbPort.Text);
            App.Current.MainWindow = window;
            this.Close();
            window.Show();
        }

        /** <summary> Event handler for the "Forget" button. </summary> */
        private void Forget_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dbUsername.Text = "";
                this.dbPassword.Password = "";
                this.dbIP.Text = "";
                this.dbPort.Text = "";
                DeleteXML();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Unable to delete login to a DB.\n" +
                    ex.Number.ToString() + " - " + ex.Message,
                    "FAI Secretary", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
