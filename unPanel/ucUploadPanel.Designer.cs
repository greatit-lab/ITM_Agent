namespace ITM_Agent.ucPanel
{
    partial class ucUploadPanel
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
        
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_FlatSet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb_PreAlign_Path;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_WaferFlat_Path;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_ImgPath;
        private System.Windows.Forms.Button btn_ImgSet;
        private System.Windows.Forms.ComboBox cb_FlatPlugin;
        private System.Windows.Forms.Button btn_PreAlignSet;
        private System.Windows.Forms.ComboBox cb_PreAlignPlugin;
        private System.Windows.Forms.ComboBox cb_ImagePlugin;
        
        #region 구성 요소 디자이너에서 생성한 코드
        
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_ImagePlugin = new System.Windows.Forms.ComboBox();
            this.btn_PreAlignSet = new System.Windows.Forms.Button();
            this.cb_PreAlignPlugin = new System.Windows.Forms.ComboBox();
            this.cb_ImgPath = new System.Windows.Forms.ComboBox();
            this.cb_PreAlign_Path = new System.Windows.Forms.ComboBox();
            this.cb_FlatPlugin = new System.Windows.Forms.ComboBox();
            this.cb_WaferFlat_Path = new System.Windows.Forms.ComboBox();
            this.btn_ImgSet = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_FlatSet = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.cb_ImagePlugin);
            this.groupBox1.Controls.Add(this.btn_PreAlignSet);
            this.groupBox1.Controls.Add(this.cb_PreAlignPlugin);
            this.groupBox1.Controls.Add(this.cb_ImgPath);
            this.groupBox1.Controls.Add(this.cb_PreAlign_Path);
            this.groupBox1.Controls.Add(this.cb_FlatPlugin);
            this.groupBox1.Controls.Add(this.cb_WaferFlat_Path);
            this.groupBox1.Controls.Add(this.btn_ImgSet);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btn_FlatSet);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(25, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(624, 276);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "● Database Uploading";
            //
            // cb_ImagePlugin
            //
            this.cb_ImagePlugin.FormattingEnabled = true;
            this.cb_ImagePlugin.Location = new System.Drawing.Point(331, 93);
            this.cb_ImagePlugin.Name = "cb_ImagePlugin";
            this.cb_ImagePlugin.Size = new System.Drawing.Size(158, 20);
            this.cb_ImagePlugin.TabIndex = 32;
            //
            // btn_PreAlignSet
            //
            this.btn_PreAlignSet.Location = new System.Drawing.Point(506, 57);
            this.btn_PreAlignSet.Name = "btn_PreAlignSet";
            this.btn_PreAlignSet.Size = new System.Drawing.Size(112, 22);
            this.btn_PreAlignSet.TabIndex = 31;
            this.btn_PreAlignSet.Text = "Set";
            this.btn_PreAlignSet.UseVisualStyleBackColor = true;
            //
            // cb_PreAlignPlugin
            //
            this.cb_PreAlignPlugin.FormattingEnabled = true;
            this.cb_PreAlignPlugin.Location = new System.Drawing.Point(331, 58);
            this.cb_PreAlignPlugin.Name = "cb_PreAlignPlugin";
            this.cb_PreAlignPlugin.Size = new System.Drawing.Size(158, 20);
            this.cb_PreAlignPlugin.TabIndex = 30;
            //
            // cb_ImgPath
            //
            this.cb_ImgPath.FormattingEnabled = true;
            this.cb_ImgPath.Location = new System.Drawing.Point(156, 93);
            this.cb_ImgPath.Name = "cb_ImgPath";
            this.cb_ImgPath.Size = new System.Drawing.Size(158, 20);
            this.cb_ImgPath.TabIndex = 13;
            //
            // cb_PreAlign_Path
            //
            this.cb_PreAlign_Path.FormattingEnabled = true;
            this.cb_PreAlign_Path.Location = new System.Drawing.Point(156, 58);
            this.cb_PreAlign_Path.Name = "cb_PreAlign_Path";
            this.cb_PreAlign_Path.Size = new System.Drawing.Size(158, 20);
            this.cb_PreAlign_Path.TabIndex = 8;
            //
            // cb_FlatPlugin
            //
            this.cb_FlatPlugin.FormattingEnabled = true;
            this.cb_FlatPlugin.Location = new System.Drawing.Point(331, 23);
            this.cb_FlatPlugin.Name = "cb_FlatPlugin";
            this.cb_FlatPlugin.Size = new System.Drawing.Size(158, 20);
            this.cb_FlatPlugin.TabIndex = 29;
            //
            // cb_WaferFlat_Path
            //
            this.cb_WaferFlat_Path.FormattingEnabled = true;
            this.cb_WaferFlat_Path.Location = new System.Drawing.Point(156, 23);
            this.cb_WaferFlat_Path.Name = "cb_WaferFlat_Path";
            this.cb_WaferFlat_Path.Size = new System.Drawing.Size(158, 20);
            this.cb_WaferFlat_Path.TabIndex = 0;
            //
            // btn_ImgSet
            //
            this.btn_ImgSet.Location = new System.Drawing.Point(506, 92);
            this.btn_ImgSet.Name = "btn_ImgSet";
            this.btn_ImgSet.Size = new System.Drawing.Size(112, 22);
            this.btn_ImgSet.TabIndex = 28;
            this.btn_ImgSet.Text = "Set";
            this.btn_ImgSet.UseVisualStyleBackColor = true;
            //
            // label7
            //
            this.label7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label7.Location = new System.Drawing.Point(18, 202);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(146, 22);
            this.label7.TabIndex = 27;
            this.label7.Text = "Wave Data Path";
            //
            // label6
            //
            this.label6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label6.Location = new System.Drawing.Point(18, 167);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 22);
            this.label6.TabIndex = 23;
            this.label6.Text = "Event Data Path";
            //
            // label5
            //
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label5.Location = new System.Drawing.Point(18, 132);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(146, 22);
            this.label5.TabIndex = 19;
            this.label5.Text = "Error Data Path";
            //
            // label2
            //
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Location = new System.Drawing.Point(18, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 22);
            this.label2.TabIndex = 15;
            this.label2.Text = "Image Data Path";
            //
            // btn_FlatSet
            //
            this.btn_FlatSet.Location = new System.Drawing.Point(506, 23);
            this.btn_FlatSet.Name = "btn_FlatSet";
            this.btn_FlatSet.Size = new System.Drawing.Size(112, 22);
            this.btn_FlatSet.TabIndex = 11;
            this.btn_FlatSet.Text = "Set";
            this.btn_FlatSet.UseVisualStyleBackColor = true;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(18, 62);
            this.label1.Name = "label2";
            this.label1.Size = new System.Drawing.Size(146, 22);
            this.label1.TabIndex = 10;
            this.label1.Text = "PreAlign Data Path";
            //
            // label3
            //
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label3.Location = new System.Drawing.Point(18, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 22);
            this.label3.TabIndex = 7;
            this.label3.Text = "Wafer Flat Data Path";
            //
            // ucUploadPanel
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "ucUploadPanel";
            this.Size = new System.Drawing.Size(676, 310);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}
