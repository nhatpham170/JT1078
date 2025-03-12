namespace JT1078ServerWF
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStart = new Button();
            btnHttpInit = new Button();
            label1 = new Label();
            txtTCPPort = new TextBox();
            txtPortWs = new TextBox();
            label2 = new Label();
            txtHostAPI = new TextBox();
            label3 = new Label();
            txtPortAPI = new TextBox();
            label4 = new Label();
            txtLogPath = new TextBox();
            label5 = new Label();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            btnStart.Location = new Point(378, 27);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(163, 146);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start Service";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += button1_Click;
            // 
            // btnHttpInit
            // 
            btnHttpInit.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            btnHttpInit.Location = new Point(685, 12);
            btnHttpInit.Name = "btnHttpInit";
            btnHttpInit.Size = new Size(103, 44);
            btnHttpInit.TabIndex = 1;
            btnHttpInit.Text = "HTTP Init";
            btnHttpInit.UseVisualStyleBackColor = true;
            btnHttpInit.Visible = false;
            btnHttpInit.Click += btnHttpInit_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(12, 27);
            label1.Name = "label1";
            label1.Size = new Size(67, 19);
            label1.TabIndex = 2;
            label1.Text = "TCP Port";
            // 
            // txtTCPPort
            // 
            txtTCPPort.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtTCPPort.Location = new Point(85, 27);
            txtTCPPort.Name = "txtTCPPort";
            txtTCPPort.Size = new Size(213, 26);
            txtTCPPort.TabIndex = 3;
            txtTCPPort.Text = "2202";
            // 
            // txtPortWs
            // 
            txtPortWs.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtPortWs.Location = new Point(85, 147);
            txtPortWs.Name = "txtPortWs";
            txtPortWs.Size = new Size(213, 26);
            txtPortWs.TabIndex = 5;
            txtPortWs.Text = "6602";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(12, 147);
            label2.Name = "label2";
            label2.Size = new Size(60, 19);
            label2.TabIndex = 4;
            label2.Text = "Ws Port";
            // 
            // txtHostAPI
            // 
            txtHostAPI.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtHostAPI.Location = new Point(85, 65);
            txtHostAPI.Name = "txtHostAPI";
            txtHostAPI.Size = new Size(213, 26);
            txtHostAPI.TabIndex = 7;
            txtHostAPI.Text = "127.0.0.1";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label3.Location = new Point(12, 65);
            label3.Name = "label3";
            label3.Size = new Size(66, 19);
            label3.TabIndex = 6;
            label3.Text = "Host API";
            // 
            // txtPortAPI
            // 
            txtPortAPI.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtPortAPI.Location = new Point(85, 108);
            txtPortAPI.Name = "txtPortAPI";
            txtPortAPI.Size = new Size(213, 26);
            txtPortAPI.TabIndex = 9;
            txtPortAPI.Text = "8080";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label4.Location = new Point(12, 108);
            label4.Name = "label4";
            label4.Size = new Size(63, 19);
            label4.TabIndex = 8;
            label4.Text = "Port API";
            // 
            // txtLogPath
            // 
            txtLogPath.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtLogPath.Location = new Point(85, 246);
            txtLogPath.Name = "txtLogPath";
            txtLogPath.Size = new Size(213, 26);
            txtLogPath.TabIndex = 11;
            txtLogPath.Text = "C:/Logs/MediaServer/";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Times New Roman", 12F, FontStyle.Regular, GraphicsUnit.Point);
            label5.Location = new Point(12, 246);
            label5.Name = "label5";
            label5.Size = new Size(64, 19);
            label5.TabIndex = 10;
            label5.Text = "Log Path";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(txtLogPath);
            Controls.Add(label5);
            Controls.Add(txtPortAPI);
            Controls.Add(label4);
            Controls.Add(txtHostAPI);
            Controls.Add(label3);
            Controls.Add(txtPortWs);
            Controls.Add(label2);
            Controls.Add(txtTCPPort);
            Controls.Add(label1);
            Controls.Add(btnHttpInit);
            Controls.Add(btnStart);
            Name = "Form1";
            Text = "Media Server";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStart;
        private Button btnHttpInit;
        private Label label1;
        private TextBox txtTCPPort;
        private TextBox txtPortWs;
        private Label label2;
        private TextBox txtHostAPI;
        private Label label3;
        private TextBox txtPortAPI;
        private Label label4;
        private TextBox txtLogPath;
        private Label label5;
    }
}
