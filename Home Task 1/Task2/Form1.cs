using System;
using System.Windows.Forms;
using MyConcatenateLib;

namespace Task2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = usernametxt.Text;
            if (string.IsNullOrEmpty(username))
            {
                lblUsername.Text = "Please enter a user name!!";
            }
            else
            {
                lblUsername.Text = $"{ConCatLib.ConcatenateLogic(username)}";
            }
        }

        private void hello_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
