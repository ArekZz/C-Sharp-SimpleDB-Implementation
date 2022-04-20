using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace LoginForm
{
    public delegate void LoginFunc(string usr,string pswd,LoadDataBase db);
    public delegate void LoadDataBase();
    public delegate void addToDb(string login, string password, bool admin);
    public static class  deserializeUser
    {

        public static Users deserialize(string serialized)
        {
            try
            {
                return new JavaScriptSerializer().Deserialize<Users>(serialized);
           
            }
            catch(Exception e)
            {

                MessageBox.Show("Blad podczas deserializacji :C");
                throw;
            }

          
        }
    }

    public partial class Form1 : Form
    {
        DataBase db = null;


        public Form1()
        {
        
            InitializeComponent();
            
            
        }
       
        private void ShowAdminForm(Users u)
        {
       
            db.loggedUser(u);
            this.Hide();
            MessageBox.Show("Poprawnie Zalogowano!");
            db.adminForm.FormClosed += Form_FormClosed;
     
            db.adminForm.Show();
        }

        private void ShowUserForm(Users u)
        {
           
          
            db.loggedUser(u);
            this.Hide();
            MessageBox.Show("Poprawnie Zalogowano!");
            db.userForm.FormClosed += Form_FormClosed;
            db.userForm.label1.Text = u.login;
            db.userForm.label2.Text = u.isAdmin.ToString();
            db.userForm.Show();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string usr = textBox1.Text;
            string pswd = textBox2.Text;
            LoginFunc LoginDelg = loginUser;
            LoadDataBase loaddb = loadDb;
            LoginDelg(usr,pswd,loaddb);

        }
        public void loadDb()
        {
            db = new DataBase();
        }
        public void loginUser(string usr,string pswd,LoadDataBase ldb)
        {
            try
            {
                //wczytanie bazy danych
                ldb();

                if (!db.DBDefinied() || db.getUsersCount() == 0)
                {

                    Users admin = db.Admin();

                    if (admin.comparePasswords(pswd) && admin.login == usr && admin.isAdmin)
                    {
                        ShowAdminForm(admin);


                    }
                    else if (admin.login != usr)
                    {
                        MessageBox.Show("Baza danych nie jest zdefiniowana,zaloguj sie przy pomocy konta Administratora!");
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzono niepoprawne haslo dla konta Administratora!");
                    }
                }
                else
                {
                    Users f = db.findUser(usr);
                    if (f != null)
                    {

                        if (f.comparePasswords(pswd))
                        {
                            if (f.isAdmin)
                            {

                                ShowAdminForm(f);
                            }
                            else
                            {
                                ShowUserForm(f);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Haslo jest nieprawidlowe :(");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie znaleziono uzytkownika o takim loginie!");
                    }

                }

            }
            catch
            {
                MessageBox.Show("Wystapil Blad :(");
            }
        }
        private void text_Click(object sender, EventArgs e)
        {
            TextBox text = sender as TextBox;
            text.Text = "";
        }
        private void Form_FormClosed(Object sender, FormClosedEventArgs e)
        {
            db = new DataBase();
          

          
            this.Show();
        }

    }
   

    public class DataBase
    {
       
        private List<Users> db = new List<Users>();
        readonly private string path = "AP8987.bin";
        public Form3 adminForm = new Form3();
        public Form2 userForm = new Form2();
        public bool selectedUser = false;
        private Users loggedUsr;
        public DataBase()
        {
            if (DBDefinied()){
                this.deserializeDB();
            }
            adminForm.updateUserList(db);
            adminForm.button2.Click += (sender, e) => button2_Click(sender, e, adminForm.textBox1.Text, adminForm.textBox2.Text, adminForm.checkBox1.Checked);
            adminForm.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            adminForm.button1.Click += new System.EventHandler(this.button1_Click);
        }
       public int getUsersCount()
        {
            return db.Count;
        }
       public Users findUser(string login)
        {
            foreach(Users usr in db)
            {
                if(usr.login == login)
                {
                    return usr;
                }
            }
            return null;
        }
       public  void deserializeDB()
        {
            db.Clear();
            
            using(StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    db.Add(deserializeUser.deserialize(sr.ReadLine()));
                   
                }
            }
            adminForm.updateUserList(db);
        }
       public void addToDB(Users user)
        {
            try
            {
            
                if (authUser(loggedUsr))
                {
                    if (findUser(user.login) != null)
                    {
                        MessageBox.Show("Uzytkownik o takim loginie juz istieje!");
                        return;
                    }
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(user.serializeUser());
                    }
                    deserializeDB();
                }
                else
                {
                    MessageBox.Show("Blad podczas autoryzacji!");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Blad podczas autoryzacji!");
                return;
            }
          
        }
        private  void button1_Click(object sender, EventArgs e)
        {
            if (this.selectedUser)
            {
                removeFromDB(findUser(adminForm.label5.Text).uuid);
                MessageBox.Show("Poprawnie usunieto uzytkownika!");
            }
            else
            {
                MessageBox.Show("Wybierz uzytkownika!");
            }
          

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.selectedUser = true;
            ListBox lb = sender as ListBox;
            try
            {
                Users selectedUser = findUser(lb.SelectedItem.ToString());
                adminForm.label5.Text = selectedUser.login;
                adminForm.label6.Text = selectedUser.password;
                adminForm.label7.Text = selectedUser.isAdmin.ToString();
            }
            catch
            {
                selectedUser = false;
            }
        }
        public void loggedUser(Users user)
        {
            loggedUsr = user;
        }
        public void removeFromDB(int uuid)
        {
            List<Users> db_zast = new List<Users>(db);
            db.Clear();
            adminForm.listBox1.ClearSelected();
            File.WriteAllText(path, String.Empty);
            foreach (Users u in db_zast)
            {
                if (u.uuid != uuid)
                {
                    addToDB(u);
                }
            }
            adminForm.updateUserList(db);
            selectedUser = false;
        }
       public bool DBDefinied()
        {
        
            if (!File.Exists(path)) {
               
                return false;
            }
            else
            {
                return true;
            }
        }
       public int getNextId()
        {
            if (!DBDefinied() || getUsersCount() == 0)
            {
                return 0;
            }
            else
            {
                return db.Last().uuid +1;
            }
        }
        public Users Admin()
        {
            return new Users(0,"Arkadiusz", "Piersa", true);
        }
        private void button2_Click(object sender, EventArgs e, string login, string password, bool admin)
        {

            /*  Debug.WriteLine("{0} {1} {2}", login, password, admin);*/
            //dodawanie do bazy
            addToDb atb = tryAdd;
            atb(login, password, admin);
        
        }
        private void tryAdd(string login,string password,bool admin)
        {
            if (login != "" && password != "" && login.Length > 1 && password.Length > 1)
            {
                addToDB(new Users(getNextId(), login, password, admin));
            }
            else
            {
                MessageBox.Show("Wprowadz poprawne dane");
            }
        }
        private bool authUser(Users u)
        {
            try
            {
                if (u.isAdmin)
                {

                    Debug.WriteLine(u.isAdmin);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

    }

    public class Users
    {
        public int uuid { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public bool isAdmin { get; set; }
        public Users(int uuid,string login, string pswd, bool Admin)
        {
            this.uuid = uuid;
            this.login = login;
            this.password = hashPassword(pswd);
            this.isAdmin = Admin;
            
        }
        public Users()
        {

        }
        public string serializeUser()
        {
            var json = new JavaScriptSerializer().Serialize(this);
            return json;
        }
    
        private string hashPassword(string toHash)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(toHash, Encoding.ASCII.GetBytes(String.Concat(new object[] { this.uuid.ToString(),"8997","Piersa"})));
            return Encoding.ASCII.GetString(pbkdf2.GetBytes(20));
        }
        public bool comparePasswords(string newPassword)
        {
            if (hashPassword(newPassword) == this.password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
     
    
    }
   
}
