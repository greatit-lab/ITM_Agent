// EqpidInputForm.cs
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
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
        private PictureBox PictureBox;  // 이미지 표시를 위한 PictureBox

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
                Top = 60,
                Left = 125,
                Width = 110
            };
            
            warningLabel = new Label()
            {
                Text = "장비명을 입력해주세요.",
                Top = 90,
                Left = 115,
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
            
            // 흐림 처리된 이미지 생성
            pictureBox = new PictureBox()
            {
                Image = CreateTransparentImage("Resources\\Icons\\icon.png", 128), // 투명도 적용 (128은 50% 알파)
                Location = new Point(250, 20),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            submitButton.Click += (sender, e) =>
            {
                string trimmedInput = textBox.Text.TrimStart(); // 앞쪽 공백 제거
                if (string.IsNullOrWhiteSpace(trimmedInput))
                {
                    warningLabel.Visible = true;
                    return; // 빈 값이면 폼 닫지 않음
                }
                this.Eqpid = trimmedInput;  // Eqpid 에 공백 제거된 값 저장
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            cancelButton.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.Cancel;    // Cancel 반환
                this.Close();
            };

            this.Controls.Add(instructionLabel);
            this.Controls.Add(textBox);
            this.Controls.Add(warningLabel);
            this.Controls.Add(submitButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(pictureBox); // PictureBox 추가
        }
        
        /// <summary>
        /// 이미지에 Alpha(투명도) 값을 적용하는 메서드
        /// </summary>
        private Image CreateTransparentImage(string filePath, int alpha)
        {
            if (!File.Exists(filePath))
                return null;

            Bitmap original = new Bitmap(filePath);
            Bitmap transparentImage = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(transparentImage))
            {
                ColorMatrix colorMatrix = new ColorMatrix
                {
                    Matrix33 = alpha / 255f // Alpha 값 (0~255 사이의 값)
                };
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }

            return transparentImage;
        }
    }
}
