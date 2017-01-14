namespace WarshipGirlsFinalTool
{
    partial class Form5
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
            this.btnFleet1 = new System.Windows.Forms.Button();
            this.btnFleet2 = new System.Windows.Forms.Button();
            this.btnFleet3 = new System.Windows.Forms.Button();
            this.btnFleet4 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnGO = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnFleet1
            // 
            this.btnFleet1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnFleet1.Location = new System.Drawing.Point(48, 196);
            this.btnFleet1.Name = "btnFleet1";
            this.btnFleet1.Size = new System.Drawing.Size(160, 130);
            this.btnFleet1.TabIndex = 0;
            this.btnFleet1.Text = "第一舰队";
            this.btnFleet1.UseVisualStyleBackColor = true;
            // 
            // btnFleet2
            // 
            this.btnFleet2.Location = new System.Drawing.Point(241, 196);
            this.btnFleet2.Name = "btnFleet2";
            this.btnFleet2.Size = new System.Drawing.Size(160, 130);
            this.btnFleet2.TabIndex = 1;
            this.btnFleet2.Text = "第二舰队";
            this.btnFleet2.UseVisualStyleBackColor = true;
            // 
            // btnFleet3
            // 
            this.btnFleet3.Location = new System.Drawing.Point(451, 196);
            this.btnFleet3.Name = "btnFleet3";
            this.btnFleet3.Size = new System.Drawing.Size(160, 130);
            this.btnFleet3.TabIndex = 2;
            this.btnFleet3.Text = "第三舰队";
            this.btnFleet3.UseVisualStyleBackColor = true;
            // 
            // btnFleet4
            // 
            this.btnFleet4.Location = new System.Drawing.Point(652, 196);
            this.btnFleet4.Name = "btnFleet4";
            this.btnFleet4.Size = new System.Drawing.Size(160, 130);
            this.btnFleet4.TabIndex = 3;
            this.btnFleet4.Text = "第四舰队";
            this.btnFleet4.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(48, 30);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(764, 151);
            this.textBox1.TabIndex = 4;
            // 
            // btnGO
            // 
            this.btnGO.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnGO.Location = new System.Drawing.Point(517, 381);
            this.btnGO.Name = "btnGO";
            this.btnGO.Size = new System.Drawing.Size(120, 73);
            this.btnGO.TabIndex = 5;
            this.btnGO.Text = "出发";
            this.btnGO.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(693, 381);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(119, 73);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // Form5
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 506);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGO);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnFleet4);
            this.Controls.Add(this.btnFleet3);
            this.Controls.Add(this.btnFleet2);
            this.Controls.Add(this.btnFleet1);
            this.Name = "Form5";
            this.Text = "Form5";
            this.Load += new System.EventHandler(this.Form5_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnGO;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFleet1;
        private System.Windows.Forms.Button btnFleet2;
        private System.Windows.Forms.Button btnFleet3;
        private System.Windows.Forms.Button btnFleet4;
        private System.Windows.Forms.TextBox textBox1;
    }
}