namespace AutoCountDemo
{
    partial class FormSetPieceWeight
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labPieceWeight = new System.Windows.Forms.Label();
            this.labWeight = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.radioBut100 = new System.Windows.Forms.RadioButton();
            this.radioButCustomization = new System.Windows.Forms.RadioButton();
            this.radioBut50 = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.butSubmit = new System.Windows.Forms.Button();
            this.butClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labPieceWeight);
            this.groupBox1.Controls.Add(this.labWeight);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.radioBut100);
            this.groupBox1.Controls.Add(this.radioButCustomization);
            this.groupBox1.Controls.Add(this.radioBut50);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.ForeColor = System.Drawing.Color.Black;
            this.groupBox1.Location = new System.Drawing.Point(19, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(514, 554);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "请选择当前称重基数:";
            // 
            // labPieceWeight
            // 
            this.labPieceWeight.AutoSize = true;
            this.labPieceWeight.Font = new System.Drawing.Font("微软雅黑", 39.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labPieceWeight.Location = new System.Drawing.Point(228, 108);
            this.labPieceWeight.Name = "labPieceWeight";
            this.labPieceWeight.Size = new System.Drawing.Size(60, 68);
            this.labPieceWeight.TabIndex = 3;
            this.labPieceWeight.Text = "0";
            // 
            // labWeight
            // 
            this.labWeight.AutoSize = true;
            this.labWeight.Font = new System.Drawing.Font("微软雅黑", 39.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labWeight.Location = new System.Drawing.Point(228, 32);
            this.labWeight.Name = "labWeight";
            this.labWeight.Size = new System.Drawing.Size(60, 68);
            this.labWeight.TabIndex = 3;
            this.labWeight.Text = "0";
            this.labWeight.TextChanged += new System.EventHandler(this.labWeight_TextChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Font = new System.Drawing.Font("微软雅黑", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDown1.Location = new System.Drawing.Point(115, 461);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(296, 70);
            this.numericUpDown1.TabIndex = 2;
            this.numericUpDown1.Visible = false;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // radioBut100
            // 
            this.radioBut100.AutoSize = true;
            this.radioBut100.Font = new System.Drawing.Font("微软雅黑", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioBut100.Location = new System.Drawing.Point(106, 279);
            this.radioBut100.Name = "radioBut100";
            this.radioBut100.Size = new System.Drawing.Size(174, 64);
            this.radioBut100.TabIndex = 1;
            this.radioBut100.Text = "100个";
            this.radioBut100.UseVisualStyleBackColor = true;
            this.radioBut100.CheckedChanged += new System.EventHandler(this.radioBut_CheckedChanged);
            // 
            // radioButCustomization
            // 
            this.radioButCustomization.AutoSize = true;
            this.radioButCustomization.Font = new System.Drawing.Font("微软雅黑", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButCustomization.Location = new System.Drawing.Point(106, 362);
            this.radioButCustomization.Name = "radioButCustomization";
            this.radioButCustomization.Size = new System.Drawing.Size(184, 64);
            this.radioButCustomization.TabIndex = 1;
            this.radioButCustomization.Text = "自定义";
            this.radioButCustomization.UseVisualStyleBackColor = true;
            this.radioButCustomization.CheckedChanged += new System.EventHandler(this.radioBut_CheckedChanged);
            // 
            // radioBut50
            // 
            this.radioBut50.AutoSize = true;
            this.radioBut50.Font = new System.Drawing.Font("微软雅黑", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioBut50.Location = new System.Drawing.Point(106, 196);
            this.radioBut50.Name = "radioBut50";
            this.radioBut50.Size = new System.Drawing.Size(146, 64);
            this.radioBut50.TabIndex = 1;
            this.radioBut50.Text = "50个";
            this.radioBut50.UseVisualStyleBackColor = true;
            this.radioBut50.CheckedChanged += new System.EventHandler(this.radioBut_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(122, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 52);
            this.label2.TabIndex = 0;
            this.label2.Text = "单重:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(42, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 52);
            this.label1.TabIndex = 0;
            this.label1.Text = "当前重量:";
            // 
            // butSubmit
            // 
            this.butSubmit.Font = new System.Drawing.Font("微软雅黑", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butSubmit.Location = new System.Drawing.Point(19, 593);
            this.butSubmit.Name = "butSubmit";
            this.butSubmit.Size = new System.Drawing.Size(158, 66);
            this.butSubmit.TabIndex = 1;
            this.butSubmit.Text = "提交";
            this.butSubmit.UseVisualStyleBackColor = true;
            this.butSubmit.Click += new System.EventHandler(this.butSubmit_Click);
            // 
            // butClear
            // 
            this.butClear.Font = new System.Drawing.Font("微软雅黑", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butClear.Location = new System.Drawing.Point(358, 593);
            this.butClear.Name = "butClear";
            this.butClear.Size = new System.Drawing.Size(158, 66);
            this.butClear.TabIndex = 1;
            this.butClear.Text = "取消";
            this.butClear.UseVisualStyleBackColor = true;
            this.butClear.Click += new System.EventHandler(this.butClear_Click);
            // 
            // FormSetPieceWeight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 671);
            this.Controls.Add(this.butClear);
            this.Controls.Add(this.butSubmit);
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(235, 349);
            this.Name = "FormSetPieceWeight";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "自动计算单重";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.RadioButton radioBut100;
        private System.Windows.Forms.RadioButton radioButCustomization;
        private System.Windows.Forms.RadioButton radioBut50;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button butSubmit;
        private System.Windows.Forms.Label labWeight;
        private System.Windows.Forms.Button butClear;
        private System.Windows.Forms.Label labPieceWeight;
        private System.Windows.Forms.Label label2;
    }
}