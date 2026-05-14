namespace ScrcpyMonitor
{
    partial class Form1
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.chkTurnOffScreen = new ScrcpyMonitor.ToggleSwitch();
            this.lblTurnOffScreen = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.flpDevices = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.lblWifi = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btnConnectWifi = new System.Windows.Forms.Button();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSort = new System.Windows.Forms.Label();
            this.cmbSort = new System.Windows.Forms.ComboBox();
            this.lblBridge = new System.Windows.Forms.Label();
            this.txtBridgeIp = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(137, 45);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Devices";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.Gray;
            this.lblSubtitle.Location = new System.Drawing.Point(24, 70);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(350, 21);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "Manage and connect to your previously paired devices.";
            // 
            // chkTurnOffScreen
            // 
            this.chkTurnOffScreen.Location = new System.Drawing.Point(28, 110);
            this.chkTurnOffScreen.Name = "chkTurnOffScreen";
            this.chkTurnOffScreen.Size = new System.Drawing.Size(45, 22);
            this.chkTurnOffScreen.TabIndex = 2;
            this.chkTurnOffScreen.Checked = true;
            // 
            // lblTurnOffScreen
            // 
            this.lblTurnOffScreen.AutoSize = true;
            this.lblTurnOffScreen.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblTurnOffScreen.Location = new System.Drawing.Point(80, 111);
            this.lblTurnOffScreen.Name = "lblTurnOffScreen";
            this.lblTurnOffScreen.Size = new System.Drawing.Size(242, 19);
            this.lblTurnOffScreen.TabIndex = 5;
            this.lblTurnOffScreen.Text = "Turn off screen on connect (-S)";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnRefresh.Location = new System.Drawing.Point(650, 105);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // flpDevices
            // 
            this.flpDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpDevices.AutoScroll = true;
            this.flpDevices.BackColor = System.Drawing.Color.White;
            this.flpDevices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpDevices.Location = new System.Drawing.Point(28, 190);
            this.flpDevices.Name = "flpDevices";
            this.flpDevices.Size = new System.Drawing.Size(722, 290);
            this.flpDevices.TabIndex = 4;
            this.flpDevices.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpDevices.WrapContents = false;
            this.flpDevices.Resize += new System.EventHandler(this.flpDevices_Resize);
            // 
            // lnkHelp
            // 
            this.lnkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lnkHelp.Location = new System.Drawing.Point(24, 495);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(262, 19);
            this.lnkHelp.TabIndex = 6;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "USB Debugging & WiFi Setup Guide";
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // lblWifi
            // 
            this.lblWifi.AutoSize = true;
            this.lblWifi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblWifi.Location = new System.Drawing.Point(340, 111);
            this.lblWifi.Name = "lblWifi";
            this.lblWifi.Size = new System.Drawing.Size(84, 19);
            this.lblWifi.TabIndex = 7;
            this.lblWifi.Text = "Wifi IP:Port:";
            // 
            // txtIp
            // 
            this.txtIp.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtIp.Location = new System.Drawing.Point(425, 108);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(100, 25);
            this.txtIp.TabIndex = 8;
            this.txtIp.Text = "192.168.68.";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPort.Location = new System.Drawing.Point(530, 108);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(50, 25);
            this.txtPort.TabIndex = 9;
            this.txtPort.Text = "5555";
            // 
            // btnConnectWifi
            // 
            this.btnConnectWifi.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnConnectWifi.Location = new System.Drawing.Point(585, 105);
            this.btnConnectWifi.Name = "btnConnectWifi";
            this.btnConnectWifi.Size = new System.Drawing.Size(60, 30);
            this.btnConnectWifi.TabIndex = 10;
            this.btnConnectWifi.Text = "Connect";
            this.btnConnectWifi.UseVisualStyleBackColor = true;
            this.btnConnectWifi.Click += new System.EventHandler(this.btnConnectWifi_Click);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSearch.Location = new System.Drawing.Point(28, 153);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(52, 19);
            this.lblSearch.TabIndex = 11;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(85, 150);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(200, 25);
            this.txtSearch.TabIndex = 12;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSort
            // 
            this.lblSort.AutoSize = true;
            this.lblSort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSort.Location = new System.Drawing.Point(300, 153);
            this.lblSort.Name = "lblSort";
            this.lblSort.Size = new System.Drawing.Size(56, 19);
            this.lblSort.TabIndex = 13;
            this.lblSort.Text = "Sort by:";
            // 
            // cmbSort
            // 
            this.cmbSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbSort.FormattingEnabled = true;
            this.cmbSort.Items.AddRange(new object[] {
            "Default",
            "Name (A-Z)",
            "WiFi First",
            "Cable First"});
            this.cmbSort.Location = new System.Drawing.Point(360, 150);
            this.cmbSort.Name = "cmbSort";
            this.cmbSort.Size = new System.Drawing.Size(150, 25);
            this.cmbSort.TabIndex = 14;
            this.cmbSort.SelectedIndexChanged += new System.EventHandler(this.cmbSort_SelectedIndexChanged);
            // 
            // lblBridge
            // 
            this.lblBridge.AutoSize = true;
            this.lblBridge.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblBridge.Location = new System.Drawing.Point(525, 153);
            this.lblBridge.Name = "lblBridge";
            this.lblBridge.Size = new System.Drawing.Size(51, 19);
            this.lblBridge.TabIndex = 15;
            this.lblBridge.Text = "Bridge:";
            // 
            // txtBridgeIp
            // 
            this.txtBridgeIp.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBridgeIp.Location = new System.Drawing.Point(580, 150);
            this.txtBridgeIp.Name = "txtBridgeIp";
            this.txtBridgeIp.Size = new System.Drawing.Size(120, 25);
            this.txtBridgeIp.TabIndex = 16;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(780, 530);
            this.Controls.Add(this.txtBridgeIp);
            this.Controls.Add(this.lblBridge);
            this.Controls.Add(this.cmbSort);
            this.Controls.Add(this.lblSort);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.btnConnectWifi);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.lblWifi);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.lblTurnOffScreen);
            this.Controls.Add(this.flpDevices);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.chkTurnOffScreen);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.Name = "Form1";
            this.Text = "MIRRORFLUX - Scrcpy Monitor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private ScrcpyMonitor.ToggleSwitch chkTurnOffScreen;
        private System.Windows.Forms.Label lblTurnOffScreen;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.FlowLayoutPanel flpDevices;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.Label lblWifi;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnConnectWifi;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSort;
        private System.Windows.Forms.ComboBox cmbSort;
        private System.Windows.Forms.Label lblBridge;
        private System.Windows.Forms.TextBox txtBridgeIp;
    }
}

