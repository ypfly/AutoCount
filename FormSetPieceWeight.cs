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
    public partial class FormSetPieceWeight : Form
    {
        double D_wieght = 0;
        string str_wieght = "";
       public  double D_PieceWeight = 0;
        delegate void DoSetD_wieght(double i, string str);

        public void Do_SetD_wieght(double i, string str)
        {
            if (labWeight.InvokeRequired)
            {
                DoSetD_wieght dw = new DoSetD_wieght(SetD_wieght);
                labWeight.Invoke(dw, new object[] { i, str });
            }
            else
            {
                D_wieght = i;
                str_wieght = labWeight.Text = str;
            }
        }

        void SetD_wieght(double i, string str)
        {
            D_wieght = i;
            str_wieght = labWeight.Text = labWeight.Text = str;

        }
        public FormSetPieceWeight()
        {
            InitializeComponent();

        }

        public FormSetPieceWeight(double D_w, string str)
        {
            InitializeComponent();
            Do_SetD_wieght(D_w, str);
        }

        private void butSubmit_Click(object sender, EventArgs e)
        {
            if (D_PieceWeight <= 0)
            {
                MessageBox.Show("单重不能小于0");
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        void GetPieceWeight(int pcs)
        {
            bool IsKG = false;
            if (str_wieght.ToLower().Contains("kg"))
            {
                IsKG = true;
            }
            if (IsKG)
            {
                D_PieceWeight = Convert.ToDouble(D_wieght / pcs*1000 );
            }
            else
            {
                D_PieceWeight = Convert.ToDouble(D_wieght / pcs );
            }         
        }
        private void radioBut_CheckedChanged(object sender, EventArgs e)
        {
            //D_PieceWeight
            if (sender == null)
                return;
            RadioButton rb = (RadioButton)sender;
         
            switch (rb.Name)
            {
                case "radioBut50":
                    numericUpDown1.Visible = false;
                    GetPieceWeight(50);                  
                    break;
                case "radioBut100":
                    numericUpDown1.Visible = false;
                    GetPieceWeight(100);
                    break;
                case "radioButCustomization":
                    numericUpDown1.Visible = true;
                    break;
            }
            labPieceWeight.Text = D_PieceWeight.ToString("0.0000g");
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            GetPieceWeight(int.Parse(numericUpDown1.Value.ToString()));
            labPieceWeight.Text = D_PieceWeight.ToString("0.0000g");
        }

        private void butClear_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void labWeight_TextChanged(object sender, EventArgs e)
        {
           
                if (radioBut50.Checked)
               {
                numericUpDown1.Visible = false;
                GetPieceWeight(50);
                return;
                 }
            if (radioBut100.Checked)
            {
                numericUpDown1.Visible = false;
                GetPieceWeight(100);
            }
            if (radioButCustomization.Checked)
            {
                numericUpDown1.Visible = true;
                GetPieceWeight(int.Parse(numericUpDown1.Value.ToString()));
                labPieceWeight.Text = D_PieceWeight.ToString("0.0000g");
            }
          
           
        }
    }
}
