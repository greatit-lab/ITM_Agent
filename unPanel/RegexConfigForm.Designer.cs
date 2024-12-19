namespace ITM_Agent.ucPanel
{
    partial class REgexConfigForm
    {
        private System.ComponentModel.IContainer component = null;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (component != null))
            {
                component.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private System.Windowns.Forms.Button btn_RegSelectFolder;
        private System.Windowns.Forms.Button btn_RegApply;
        private System.Windowns.Forms.Label label1;
        private System.Windowns.Forms.TextBox tb_RegInput;
        private System.Windowns.Forms.TextBox tb_RegFolder;
        private System.Windowns.Forms.Button btn_RegCancel;
        private System.Windowns.Forms.Label label2;
        private System.Windowns.Forms.Label label3;
        
        #region 구성 요소 디자이너에서 생성한 코드
        
        private void InitializeComponent()
        {
            this.btn_RegSelectFolder = new System.Windowns.Forms.Button();
            this.btn_RegApply = new System.Windowns.Forms.Button();
            this.label1 = new System.Windowns.Forms.Label();
            this.tb_RegInput = new System.Windowns.Forms.TextBox();
            this.tb_RegFolder = new System.Windowns.Forms.TextBox();
            this.btn_RegCancel = new System.Windowns.Forms.Button();
            this.label2 = new System.Windowns.Forms.Label();
            this.label3 = new System.Windowns.Forms.Label();
            this.SuspendLayout();
            //
            // btn_RegSelectFolder
            //
            this.btn_RegSelectFolder.Location = new System.Drawing.Point(260, 78);
            this.btn_RegSelectFolder.Name = "btn_RegSelectFolder";
            this.btn_RegSelectFolder.Size = new System.Drawing.Size(64, 23);
            this.btn_RegSelectFolder.TabIndex = 0;
            this.btn_RegSelectFolder.Text = "Select";
            this.btn_RegSelectFolder.UseVisualStyleBackColor = true;
            //
            // btn_RegApply
            //
            this.btn_RegApply.Location = new System.Drawing.Point(68, 117);
            this.btn_RegApply.Name = "btn_RegApply";
            this.btn_RegApply.Size = new System.Drawing.Size(95, 23);
            this.btn_RegApply.TabIndex = 1;
            this.btn_RegApply.Text = "Apply";
            this.btn_RegApply.UseVisualStyleBackColor = true;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Regex";
            //
            // tb_RegInput
            //
            this.tb_RegInput.Location = new System.Drawing.Point(104, 25);
            this.tb_RegInput.Name = "tb_RegInput";
            this.tb_RegInput.Size = new System.Drawing.Size(220, 21);
            this.tb_RegInput.TabIndex = 3;
            //
            // tb_RegFolder
            //
            this.tb_RegFolder.Location = new System.Drawing.Point(104, 79);
            this.tb_RegFolder.Name = "tb_RegFolder";
            this.tb_RegFolder.Size = new System.Drawing.Size(150, 21);
            this.tb_RegFolder.TabIndex = 4;
            //
            // btn_RegCancel
            //
            this.btn_RegCancel.Location = new System.Drawing.Point(191, 117);
            this.btn_RegCancel.Name = "btn_RegCancel";
            this.btn_RegCancel.Size = new System.Drawing.Size(95, 23);
            this.btn_RegCancel.TabIndex = 5;
            this.btn_RegCancel.Text = "Cancel";
            this.btn_RegCancel.UseVisualStyleBackColor = true;
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Target Folder";
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(189, 57);
            this.label3.Name = "label4";
            this.label3.Size = new System.Drawing.Size(43, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "¿¿¿";
            //
            // ucScreen1_Reg
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windowns.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 154);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_RegCancel);
            this.Controls.Add(this.tb_RegFolder);
            this.Controls.Add(this.tb_RegInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_RegApply);
            this.Controls.Add(this.btn_RegSelectFolder);
            this.Name = "ucScreen1_Reg";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}