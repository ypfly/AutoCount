using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCountDemo
{
    public partial class FormSetmCount : System.Windows.Forms.Form
    {
        public FormSetmCount()
        {
            InitializeComponent();
        }

        public int SetValue = 0;
      
        public FormSetmCount(int value)
        {
            InitializeComponent();
            SetValue = value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SetValue=  int.Parse(textBox1.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正整数！");
            }          
            this.DialogResult = DialogResult.OK;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = SetValue + "";
        }
    }
}
