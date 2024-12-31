namespace ITM_Agent.ucPanel
{
    partial class ucImageTransPanel
    {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_FolderClear;
        private System.Windows.Forms.ComboBox cb_TargetImageFolder;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_SelectOutputFolder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_TimeClear;
        private System.Windows.Forms.ComboBox cb_WaitTime;
        private System.Windows.Forms.Button btn_SetTime;
        private System.Windows.Forms.Button btn_SetFolder;
        private System.Windows.Forms.Label label2;

        #region 구성 요소 디자이너에서 생성한 코드
        
        private void InitializeComponent()
        {
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_SetTime = new System.Windows.Forms.Button();
            this.btn_SetFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_TimeClear = new System.Windows.Forms.Button();
            this.cb_WaitTime = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_FolderClear = new System.Windows.Forms.Button();
            this.cb_TargetImageFolder = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_SelectOutputFolder = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer2
            //
            this.splitContainer2.Location = new System.Drawing.Point(22, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainer2.Panel1
            //
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            //
            // splitContainer2.Panel2
            //
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            //this.splitContainer2.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel2_Paint);
            this.splitContainer2.Size = new System.Drawing.Size(632, 304);
            this.splitContainer2.SplitterDistance = 128;
            this.splitContainer2.TabIndex = 3;
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.btn_SetTime);
            this.groupBox1.Controls.Add(this.btn_SetFolder);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btn_TimeClear);
            this.groupBox1.Controls.Add(this.cb_WaitTime);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btn_FolderClear);
            this.groupBox1.Controls.Add(this.cb_TargetImageFolder);
            this.groupBox1.Location = new System.Drawing.Point(3, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(624, 102);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "● Set a Condition";
            //
            // btn_SetTime
            //
            this.btn_SetTime.Location = new System.Drawing.Point(373, 65);
            this.btn_SetTime.Name = "btn_SetTime";
            this.btn_SetTime.Size = new System.Drawing.Size(127, 20);
            this.btn_SetTime.TabIndex = 12;
            this.btn_SetTime.Text = "Set Time";
            this.btn_SetTime.UseVisualStyleBackColor = true;
            //
            // btn_SetFolder
            //
            this.btn_SetFolder.Location = new System.Drawing.Point(373, 22);
            this.btn_SetFolder.Name = "btn_SetFolder";
            this.btn_SetFolder.Size = new System.Drawing.Size(127, 20);
            this.btn_SetFolder.TabIndex = 11;
            this.btn_SetFolder.Text = "Set Folder";
            this.btn_SetFolder.UseVisualStyleBackColor = true;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(18, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 22);
            this.label1.TabIndex = 10;
            this.label1.Text = "Wait Time";
            //
            // btn_TimeClear
            //
            this.btn_TimeClear.Location = new System.Drawing.Point(506, 65);
            this.btn_TimeClear.Name = "btn_TimeClear";
            this.btn_TimeClear.Size = new System.Drawing.Size(112, 20);
            this.btn_TimeClear.TabIndex = 9;
            this.btn_TimeClear.Text = "Clear";
            this.btn_TimeClear.UseVisualStyleBackColor = true;
            //
            // cb_WaitTime
            //
            this.cb_WaitTime.FormattingEnabled = true;
            this.cb_WaitTime.Location = new System.Drawing.Point(170, 65);
            this.cb_WaitTime.Name = "cb_WaitTime";
            this.cb_WaitTime.Size = new System.Drawing.Size(184, 20);
            this.cb_WaitTime.TabIndex = 8;
            //
            // label3
            //
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label3.Location = new System.Drawing.Point(18, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 22);
            this.label3.TabIndex = 7;
            this.label3.Text = "Target Image Folder";
            //
            // btn_FolderClear
            //
            this.btn_FolderClear.Location = new System.Drawing.Point(506, 23);
            this.btn_FolderClear.Name = "btn_FolderClear";
            this.btn_FolderClear.Size = new System.Drawing.Size(112, 20);
            this.btn_FolderClear.TabIndex = 6;
            this.btn_FolderClear.Text = "Clear";
            this.btn_FolderClear.UseVisualStyleBackColor = true;
            //
            // cb_TargetImageFolder
            //
            this.cb_TargetImageFolder.FormattingEnabled = true;
            this.cb_TargetImageFolder.Location = new System.Drawing.Point(170, 23);
            this.cb_TargetImageFolder.Name = "cb_TargetImageFolder";
            this.cb_TargetImageFolder.Size = new System.Drawing.Size(184, 20);
            this.cb_TargetImageFolder.TabIndex = 0;
            //
            // groupBox2
            //
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btn_SelectOutputFolder);
            this.groupBox2.Location = new System.Drawing.Point(3, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(624, 156);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "● Image Save Folder";
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Location = new System.Drawing.Point(18, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(336, 22);
            this.label2.TabIndex = 13;
            this.label2.Text = "label2";
            //
            // btn_SelectOutputFolder
            //
            this.btn_SelectOutputFolder.Location = new System.Drawing.Point(373, 65);
            this.btn_SelectOutputFolder.Name = "btn_SelectOutputFolder";
            this.btn_SelectOutputFolder.Size = new System.Drawing.Size(245, 28);
            this.btn_SelectOutputFolder.TabIndex = 5;
            this.btn_SelectOutputFolder.Text = "Select Folder";
            this.btn_SelectOutputFolder.UseVisualStyleBackColor = true;
            //
            // ucImageTransPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Name = "ucImageTransPanel";
            this.Size = new System.Drawing.Size(676, 310);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}