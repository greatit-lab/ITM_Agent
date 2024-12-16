// EqpidInputForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ITM_Agent
{
    /// <summary>
    /// 신규 Eqpid 등록을 위한 입력 폼입니다.
    /// 사용자가 장비명을 입력하면 OK를 누를 때 Eqpid 속성에 해당 값이 저장되며,
    /// Cancel 시 애플리케이션이 종료됩니다.
    /// </summary>
    public class EqpidInputForm : Form
    {
        public string Eqpid { get; private set; }

        private TextBox textBox;
        private Button submitButton;
        private Button cancelButton;
        private Label instructionLabel;
        private Label warningLabel;

        public EqpidInputForm()
        {
            this.Text = "New EQPID Registry";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = false; // Close 버튼 비활성화

            instructionLabel = new Label()
            {
                Text = "신규로 등록 필요한 장비명을 입력하세요.",
                Top = 20,
                Left = 25,
                Width = 300
            };

            textBox = new TextBox()
            {
                Top = 50,
                Left = 90,
                Width = 110
            };

            warningLabel = new Label()
            {
                Text = "장비명을 입력해주세요.",
                Top = 80,
                Left = 80,
                ForeColor = Color.Red,
                AutoSize = true,
                Visible = false
            };

            submitButton = new Button()
            {
                Text = "Submit",
                Top = 120,
                Left = 50,
                Width = 90
            };

            cancelButton = new Button()
            {
                Text = "Cancel",
                Top = 120,
                Left = 150,
                Width = 90
            };

            submitButton.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    warningLabel.Visible = true;
                    return; // 빈 값이면 폼 닫지 않음
                }
                this.Eqpid = textBox.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            cancelButton.Click += (sender, e) =>
            {
                Application.Exit();
            };

            this.Controls.Add(instructionLabel);
            this.Controls.Add(textBox);
            this.Controls.Add(warningLabel);
            this.Controls.Add(submitButton);
            this.Controls.Add(cancelButton);
        }
    }
}
