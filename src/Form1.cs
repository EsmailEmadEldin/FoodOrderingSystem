using System;
using System.Windows.Forms;

namespace FoodOrderingSystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Button "Page 1" - Opens Feedback Table (Form2)
        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        // Button "Page 2" - Opens Customer Table (Form3)
        private void button3_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
        }
    }
}
