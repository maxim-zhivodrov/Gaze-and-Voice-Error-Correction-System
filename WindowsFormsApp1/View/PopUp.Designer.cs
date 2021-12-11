namespace EyeGaze
{
    partial class PopUp
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
            this.waveformPainter1 = new NAudio.Gui.WaveformPainter();
            this.popUpmessageLable = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // waveformPainter1
            // 
            this.waveformPainter1.Location = new System.Drawing.Point(272, 79);
            this.waveformPainter1.Name = "waveformPainter1";
            this.waveformPainter1.Size = new System.Drawing.Size(141, 31);
            this.waveformPainter1.TabIndex = 0;
            this.waveformPainter1.Text = "waveformPainter1";
            // 
            // popUpmessageLable
            // 
            this.popUpmessageLable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(241)))), ((int)(((byte)(237)))));
            this.popUpmessageLable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.popUpmessageLable.Font = new System.Drawing.Font("Century Gothic", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.popUpmessageLable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(75)))), ((int)(((byte)(115)))));
            this.popUpmessageLable.Location = new System.Drawing.Point(22, 25);
            this.popUpmessageLable.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.popUpmessageLable.Multiline = true;
            this.popUpmessageLable.Name = "popUpmessageLable";
            this.popUpmessageLable.Size = new System.Drawing.Size(359, 75);
            this.popUpmessageLable.TabIndex = 6;
            this.popUpmessageLable.Text = "THIS IS MESSAGE BLA BLA BLA BLA BLA BLA BLA BLA";
            // 
            // OKButton
            // 
            this.OKButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(189)))), ((int)(((byte)(179)))));
            this.OKButton.FlatAppearance.BorderSize = 0;
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OKButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(75)))), ((int)(((byte)(115)))));
            this.OKButton.Location = new System.Drawing.Point(133, 116);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(149, 39);
            this.OKButton.TabIndex = 7;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = false;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // PopUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(241)))), ((int)(((byte)(237)))));
            this.ClientSize = new System.Drawing.Size(402, 167);
            this.ControlBox = false;
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.popUpmessageLable);
            this.Controls.Add(this.waveformPainter1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Location = new System.Drawing.Point(750, 800);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PopUp";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NAudio.Gui.WaveformPainter waveformPainter1;
        private System.Windows.Forms.TextBox popUpmessageLable;
        private System.Windows.Forms.Button OKButton;
    }
}