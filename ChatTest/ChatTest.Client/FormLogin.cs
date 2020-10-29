using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatTest.Client
{
    public partial class FormLogin : Form
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Cancel { get; set; }
        public bool IsRegister { get; set; }
        public FormLogin()
        {
            InitializeComponent();
            Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Login = textBoxLogin.Text;
            Password = textBoxPassword.Text;
            Cancel = false;
            this.Close();
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            IsRegister = true;
            Cancel = false;
            this.Close();
        }
    }
}
