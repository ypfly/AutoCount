using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCountDemo
{
    class MyDelegate
    {
        /// <summary>
        /// 设置TextWeight的背景颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="str"></param>
        delegate void DoSetTextWeightBackColor(object sender,Color c);
       
        /// <summary>
        /// 设置RichTextBox显示的文本
        /// </summary>
        /// <param name="shender">控件对象</param>
        /// <param name="message">显示的消息</param>
        /// <param name="IsWarning">是否警告</param>
        delegate void DoSetRichTextBoxText(object shender, string message, bool IsWarning);
        public void SetTextBoxColor(object sender, Color c)
        {           
            if (sender == null)
                return;
            TextBox tb = (TextBox)sender;
            if (tb == null)
                return;
            if (tb.InvokeRequired)
            {
                DoSetTextWeightBackColor dw = new DoSetTextWeightBackColor(setColor);
                tb.Invoke(dw, new object[] { tb, c });
            }
            else
                tb.BackColor = c;
        }

        void setColor(object sender, Color c)
        {
            if (sender == null)
                return;
            TextBox tb = (TextBox)sender;
            tb.BackColor = c;
        }

        public void SetRichTextBoxText(object sender, string message, bool IsWarning)
        {
            string msg = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss ") + message+"\n";
            if (sender == null)
                return;
            RichTextBox rb = (RichTextBox)sender;
            if (rb.InvokeRequired)
            {
                DoSetRichTextBoxText dstb = new DoSetRichTextBoxText(SetShowText);
                rb.Invoke(dstb, new object[] { rb, msg, IsWarning });
            }
            else
            {
                rb.AppendText(msg);
                if (IsWarning)
                    rb.ForeColor = System.Drawing.Color.Red;
                else
                    rb.ForeColor = System.Drawing.Color.Yellow;
                rb.Select(rb.TextLength, 0);
                rb.ScrollToCaret();
            }
        }

        void SetShowText(object sender, string message, bool IsWarning)
        {
            if (sender == null)
                return;
            RichTextBox rb = (RichTextBox)sender;
            rb.AppendText(message);
            if (IsWarning)
                rb.ForeColor = System.Drawing.Color.Red;
            else
                rb.ForeColor = System.Drawing.Color.Yellow;

            rb.Select(rb.TextLength, 0);
            rb.ScrollToCaret();
        }
    }
}
