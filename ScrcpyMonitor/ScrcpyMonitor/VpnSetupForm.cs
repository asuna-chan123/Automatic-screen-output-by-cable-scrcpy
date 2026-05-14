using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace ScrcpyMonitor
{
    public class VpnSetupForm : Form
    {
        private Label lblTitle;
        private Label lblStep1, lblStep2, lblStep3;
        private Button btnDownload, btnAdminConsole;
        private Label lblLinkTitle;
        private TextBox txtLink;
        private Button btnSaveKey;
        private Button btnRetry;
        private Button btnCancel;
        private Label lblStatus;

        public VpnSetupForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Configure Tailscale VPN";
            this.Size = new Size(500, 520); // Tăng chiều cao để đủ chỗ cho hướng dẫn
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // 1. Tiêu đề chính
            lblTitle = new Label
            {
                Text = "Tailscale connection guide",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 2. Bước 1: Tải ứng dụng
            lblStep1 = new Label
            {
                Text = "Step 1: Install Tailscale on your computer and phone.",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(25, 60),
                AutoSize = true
            };

            btnDownload = new Button
            {
                Text = "Download Tailscale now",
                Location = new Point(30, 85),
                Size = new Size(130, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand
            };
            btnDownload.Click += (s, e) => Process.Start("https://tailscale.com/download");

            // 3. Bước 2: Đăng nhập
            lblStep2 = new Label
            {
                Text = "Step 2: Log in with the same account on both devices.",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(25, 130),
                AutoSize = true
            };

            var lblStep2Sub = new Label
            {
                Text = "After logging in, the device will automatically appear on the local network.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(30, 150),
                Size = new Size(430, 20)
            };

            // 4. Bước 3: Lấy Auth Key
            lblStep3 = new Label
            {
                Text = "Step 3: Obtain the Auth Key so the application can automatically authenticate you.",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(25, 185),
                AutoSize = true
            };

            var lblStep3Sub = new Label
            {
                Text = "Go to Settings -> Keys -> Generate Auth Key (Select 'Reusable').",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(30, 205),
                Size = new Size(430, 20)
            };

            btnAdminConsole = new Button
            {
                Text = "Open the Key retrieval page",
                Location = new Point(30, 230),
                Size = new Size(130, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand
            };
            btnAdminConsole.Click += (s, e) => Process.Start("https://login.tailscale.com/admin/settings/keys");

            // --- Khu vực nhập Key ---
            var panelKey = new Panel
            {
                Location = new Point(20, 280),
                Size = new Size(445, 100),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            lblLinkTitle = new Label
            {
                Text = "Enter Tailscale Auth Key here:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MirrorFlux");
            string keyPath = System.IO.Path.Combine(appDataPath, "tailscale_key.txt");
            string savedKey = System.IO.File.Exists(keyPath) ? System.IO.File.ReadAllText(keyPath) : "";

            txtLink = new TextBox
            {
                Text = savedKey,
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 35),
                Size = new Size(335, 25)
            };

            btnSaveKey = new Button
            {
                Text = "Save Key",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(350, 34),
                Size = new Size(85, 28),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSaveKey.FlatAppearance.BorderSize = 0;
            btnSaveKey.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtLink.Text) || !txtLink.Text.StartsWith("tskey-"))
                {
                    lblStatus.Text = "Note: Auth Key usually starts with 'tskey-'.";
                    lblStatus.ForeColor = Color.IndianRed;
                }
                else
                {
                    try {
                        if (!System.IO.Directory.Exists(appDataPath)) System.IO.Directory.CreateDirectory(appDataPath);
                        System.IO.File.WriteAllText(keyPath, txtLink.Text.Trim());
                        lblStatus.Text = "Save Auth Key successfully!";
                        lblStatus.ForeColor = Color.Green;
                    } catch (Exception ex) {
                        lblStatus.Text = "Error saving file: " + ex.Message;
                    }
                }
            };

            lblStatus = new Label
            {
                Text = "Ready to connect.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(10, 70),
                Size = new Size(420, 20),
                ForeColor = Color.DimGray
            };

            panelKey.Controls.Add(lblLinkTitle);
            panelKey.Controls.Add(txtLink);
            panelKey.Controls.Add(btnSaveKey);
            panelKey.Controls.Add(lblStatus);

            // --- Nút điều hướng dưới cùng ---
            btnRetry = new Button
            {
                Text = "Start Connection",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(20, 410),
                Size = new Size(160, 45),
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Retry
            };
            btnRetry.FlatAppearance.BorderSize = 0;

            btnCancel = new Button
            {
                Text = "Skip",
                Font = new Font("Segoe UI", 10),
                Location = new Point(190, 410),
                Size = new Size(100, 45),
                BackColor = Color.White,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Color.LightGray;

            // Thêm vào Form
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblStep1);
            this.Controls.Add(btnDownload);
            this.Controls.Add(lblStep2);
            this.Controls.Add(lblStep2Sub);
            this.Controls.Add(lblStep3);
            this.Controls.Add(lblStep3Sub);
            this.Controls.Add(btnAdminConsole);
            this.Controls.Add(panelKey);
            this.Controls.Add(btnRetry);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnRetry;
            this.CancelButton = btnCancel;
        }
    }
}