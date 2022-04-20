using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginForm
{

    public partial class Form3 : Form
    {
      
        public Form3()
        {
            InitializeComponent();
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(280, 72);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(100, 30);
            this.listBox1.TabIndex = 6;
           
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }



        public void updateUserList(List<Users> users)
        {
           listBox1.Items.Clear();
            if (users.Count > 0)
            {
                foreach (Users usr in users)
                {
                    this.listBox1.Items.Add(usr.login);
                }
            }
        }

 

 
    }
}
