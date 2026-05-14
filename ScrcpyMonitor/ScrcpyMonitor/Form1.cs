using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrcpyMonitor
{
    public partial class Form1 : Form
    {
        private class DeviceData
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string Name { get; set; }
        }
        private ManagementEventWatcher watcher;
        private Dictionary<string, Process> activeProcesses = new Dictionary<string, Process>();
        private Dictionary<string, string> deviceNames = new Dictionary<string, string>();
        private readonly string devicesFilePath;
        private readonly string wifiFilePath;
        private readonly string bridgeFilePath;

        private string ScrcpyDir;
        private string GetBridgeIp() => txtBridgeIp?.Text.Trim() ?? "";

        public Form1()
        {
            InitializeComponent();
            
            // Tạo thư mục lưu trữ trong AppData
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MirrorFlux");
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            
            devicesFilePath = Path.Combine(appDataPath, "devices.txt");
            wifiFilePath = Path.Combine(appDataPath, "wifi.txt");
            bridgeFilePath = Path.Combine(appDataPath, "bridge.txt");

            // Migration: Di cư dữ liệu từ thư mục gốc cũ sang AppData nếu AppData chưa có
            try
            {
                string localDevices = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.txt");
                if (!File.Exists(devicesFilePath) && File.Exists(localDevices))
                {
                    File.Copy(localDevices, devicesFilePath);
                }

                string localWifi = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wifi.txt");
                if (!File.Exists(wifiFilePath) && File.Exists(localWifi))
                {
                    File.Copy(localWifi, wifiFilePath);
                }

                string localBridge = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bridge.txt");
                if (!File.Exists(bridgeFilePath) && File.Exists(localBridge))
                {
                    File.Copy(localBridge, bridgeFilePath);
                }
            }
            catch { /* Bỏ qua nếu có lỗi quyền truy cập */ }

            ScrcpyDir = FindScrcpyDir();
        }

        private string FindScrcpyDir()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string checkDir = currentDir;

            // Thử tìm tại thư mục hiện hành và ngược lên 5 cấp thư mục cha
            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(Path.Combine(checkDir, "adb.exe")))
                {
                    return checkDir;
                }
                
                checkDir = Path.GetDirectoryName(checkDir.TrimEnd(Path.DirectorySeparatorChar));
                if (checkDir == null) break;
            }

            return currentDir;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDeviceNames();
            LoadWifiSettings();
            if (File.Exists(bridgeFilePath)) txtBridgeIp.Text = File.ReadAllText(bridgeFilePath);
            if (cmbSort.SelectedIndex == -1) cmbSort.SelectedIndex = 0;
            StartWatcher();
            RefreshDeviceList();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => RefreshDeviceList();
        private void cmbSort_SelectedIndexChanged(object sender, EventArgs e) => RefreshDeviceList();


        private void LoadWifiSettings()
        {
            if (File.Exists(wifiFilePath))
            {
                var lines = File.ReadAllLines(wifiFilePath);
                if (lines.Length >= 2)
                {
                    txtIp.Text = lines[0];
                    txtPort.Text = lines[1];
                }
            }
        }

        private void SaveWifiSettings()
        {
            File.WriteAllLines(wifiFilePath, new[] { txtIp.Text.Trim(), txtPort.Text.Trim() });
            File.WriteAllText(bridgeFilePath, txtBridgeIp.Text.Trim());
        }

        private void LoadDeviceNames()
        {
            deviceNames.Clear();
            if (File.Exists(devicesFilePath))
            {
                var lines = File.ReadAllLines(devicesFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        deviceNames[parts[0]] = parts[1];
                    }
                }
            }
        }

        private void SaveDeviceNames()
        {
            var lines = new List<string>();
            foreach (var kvp in deviceNames)
            {
                lines.Add($"{kvp.Key}={kvp.Value}");
            }
            File.WriteAllLines(devicesFilePath, lines);
        }

        private void StartWatcher()
        {
            try
            {
                var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
                watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += Watcher_EventArrived;
                watcher.Start();
            }
            catch { }
        }

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            // Update UI on main thread
            this.Invoke(new Action(() => RefreshDeviceList()));
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDeviceList();
        }

        private async void btnConnectWifi_Click(object sender, EventArgs e)
        {
            string ip = txtIp.Text.Trim();
            string port = txtPort.Text.Trim();
            
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Please enter IP address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(port)) port = "5555";

            SaveWifiSettings();

            btnConnectWifi.Enabled = false;
            btnConnectWifi.Text = "Connecting...";

            string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Could not find adb.exe!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConnectWifi.Enabled = true;
                btnConnectWifi.Text = "Connect";
                return;
            }

            // Ngắt kết nối cũ trước để tránh xung đột
            await Task.Run(() => {
                var psiDisc = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = $"disconnect {ip}:{port}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                string bridgeIpForDisconnect = GetBridgeIp();
                if (!string.IsNullOrEmpty(bridgeIpForDisconnect))
                    psiDisc.EnvironmentVariables["ADBHOST"] = bridgeIpForDisconnect;

                Process.Start(psiDisc)?.WaitForExit();
            });

            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = $"connect {ip}:{port}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            string currentBridgeIp = GetBridgeIp();
            if (!string.IsNullOrEmpty(currentBridgeIp)) psi.EnvironmentVariables["ADBHOST"] = currentBridgeIp;

            string output = "";
            await Task.Run(() =>
            {
                using (var proc = Process.Start(psi))
                {
                    output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                }
            });

            MessageBox.Show(output, "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Thông báo cho Bridge nếu có
            string bIpManual = GetBridgeIp();
            if (!string.IsNullOrEmpty(bIpManual) && output.Contains("connected to"))
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        await client.PostAsync($"http://{bIpManual}:5000/adb/connect?address={ip}:{port}", null);
                    }
                }
                catch { }
            }

            // Lưu ngay khi kết nối thành công/thất bại để tránh mất data khi restart
            SaveWifiSettings();

            btnConnectWifi.Enabled = true;
            btnConnectWifi.Text = "Connect";
            
            RefreshDeviceList();
        }

        private async Task<string> GetTailscaleIp(string deviceId)
        {
            string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = $"-s {deviceId} shell ip addr",
                WorkingDirectory = ScrcpyDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            string bIp = GetBridgeIp();
            if (!string.IsNullOrEmpty(bIp)) psi.EnvironmentVariables["ADBHOST"] = bIp;

            string output = "";
            await Task.Run(() => {
                try {
                    using (var proc = Process.Start(psi)) {
                        output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();
                    }
                } catch { }
            });

            // Tìm dải IP của Tailscale (thường bắt đầu bằng 100.64.0.0/10)
            foreach (string line in output.Split('\n'))
            {
                if (line.Contains("inet 100."))
                {
                    var parts = line.Split(new[] { "inet " }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        return parts[1].Trim().Split('/')[0].Split(' ')[0];
                    }
                }
            }
            return null;
        }

        private async Task<string> GetDeviceIp(string deviceId)
        {
            // Ưu tiên lấy IP Tailscale để có thể kết nối khác mạng LAN
            string tsIp = await GetTailscaleIp(deviceId);
            if (!string.IsNullOrEmpty(tsIp)) return tsIp;

            string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = $"-s {deviceId} shell ip route",
                WorkingDirectory = ScrcpyDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            string deviceBridgeIp = GetBridgeIp();
            if (!string.IsNullOrEmpty(deviceBridgeIp)) psi.EnvironmentVariables["ADBHOST"] = deviceBridgeIp;
            string output = "";
            await Task.Run(() =>
            {
                using (var proc = Process.Start(psi))
                {
                    output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                }
            });

            foreach (string line in output.Split('\n'))
            {
                if (line.Contains("wlan0") && line.Contains("src "))
                {
                    var parts = line.Split(new[] { "src " }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var ipPart = parts[1].Trim().Split(' ')[0];
                        return ipPart;
                    }
                }
            }
            return null;
        }

        private async void AutoSetup(string deviceId, bool forceVpn)
        {
            string portStr = txtPort.Text.Trim();
            if (string.IsNullOrEmpty(portStr)) portStr = "5555";
            
            string ip = "";
            if (forceVpn)
            {
                ip = await GetTailscaleIp(deviceId);
                if (string.IsNullOrEmpty(ip))
                {
                    using (var frm = new VpnSetupForm())
                    {
                        if (frm.ShowDialog() == DialogResult.Retry)
                        {
                            AutoSetup(deviceId, forceVpn);
                        }
                    }
                    return;
                }
            }
            else
            {
                ip = await GetWiFiIpOnly(deviceId); // Lấy riêng IP WiFi
                if (string.IsNullOrEmpty(ip))
                {
                    MessageBox.Show("Could not find WiFi IP on this device.", "WiFi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Đồng bộ tên từ thiết bị USB sang thiết bị Wireless/VPN mới
            if (deviceNames.ContainsKey(deviceId))
            {
                deviceNames[$"{ip}:{portStr}"] = deviceNames[deviceId];
                SaveDeviceNames();
            }

            string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
            var psiTcp = new ProcessStartInfo { FileName = adbPath, Arguments = $"-s {deviceId} tcpip {portStr}", CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden };
            Process.Start(psiTcp)?.WaitForExit();

            await Task.Delay(2000);

            var psiConn = new ProcessStartInfo { FileName = adbPath, Arguments = $"connect {ip}:{portStr}", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
            string outConn = "";
            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo { FileName = adbPath, Arguments = $"disconnect {ip}:{portStr}", CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden })?.WaitForExit();
                using (var p = Process.Start(psiConn))
                {
                    outConn = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                }
            });

            MessageBox.Show(outConn, forceVpn ? "Auto VPN Result" : "Auto WiFi Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            string bridgeIp = GetBridgeIp();
            if (!string.IsNullOrEmpty(bridgeIp))
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        await client.PostAsync($"http://{bridgeIp}:5000/adb/connect?address={ip}:{portStr}", null);
                    }
                }
                catch { }
            }

            txtIp.Text = ip;
            txtPort.Text = portStr;
            SaveWifiSettings();
            RefreshDeviceList();
        }

        private async Task<string> GetWiFiIpOnly(string deviceId)
        {
            string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = $"-s {deviceId} shell ip route",
                WorkingDirectory = ScrcpyDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            string bIp = GetBridgeIp();
            if (!string.IsNullOrEmpty(bIp)) psi.EnvironmentVariables["ADBHOST"] = bIp;
            string output = "";
            await Task.Run(() => {
                using (var proc = Process.Start(psi)) {
                    output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                }
            });

            foreach (string line in output.Split('\n'))
            {
                if (line.Contains("wlan0") && line.Contains("src "))
                {
                    var parts = line.Split(new[] { "src " }, StringSplitOptions.None);
                    if (parts.Length > 1) return parts[1].Trim().Split(' ')[0];
                }
            }
            return null;
        }

        private async void RefreshDeviceList()
        {
            btnRefresh.Enabled = false;
            btnRefresh.Text = "Scanning...";

            string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
            if (!File.Exists(adbPath))
            {
                MessageBox.Show("Could not find adb.exe at " + adbPath);
                btnRefresh.Enabled = true;
                btnRefresh.Text = "Refresh";
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = "devices",
                WorkingDirectory = ScrcpyDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            string refreshBridgeIp = GetBridgeIp();
            if (!string.IsNullOrEmpty(refreshBridgeIp)) psi.EnvironmentVariables["ADBHOST"] = refreshBridgeIp;

            string output = "";
            await Task.Run(() =>
            {
                using (var proc = Process.Start(psi))
                {
                    output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                }
            });

            List<DeviceData> allDevices = new List<DeviceData>();
            HashSet<string> foundDevices = new HashSet<string>();

            using (var reader = new StringReader(output))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("List of devices"))
                        continue;

                    var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string id = parts[0];
                        string status = parts[1];
                        foundDevices.Add(id);
                        
                        // Lấy tên đã lưu hoặc dùng ID nếu chưa có tên
                        string displayName = deviceNames.ContainsKey(id) ? deviceNames[id] : id;
                        allDevices.Add(new DeviceData { Id = id, Status = status, Name = displayName });
                    }
                }
            }

            foreach (var savedId in deviceNames.Keys)
            {
                if (!foundDevices.Contains(savedId))
                {
                    allDevices.Add(new DeviceData { Id = savedId, Status = "offline", Name = deviceNames[savedId] });
                }
            }

            // Filtering
            string searchText = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                allDevices = allDevices.Where(d => d.Name.ToLower().Contains(searchText) || d.Id.ToLower().Contains(searchText)).ToList();
            }

            // Sorting
            string sortOption = cmbSort.SelectedItem?.ToString() ?? "Default";
            switch (sortOption)
            {
                case "Name (A-Z)":
                    allDevices = allDevices.OrderBy(d => d.Name).ToList();
                    break;
                case "WiFi First":
                    allDevices = allDevices.OrderByDescending(d => d.Id.Contains(":")).ThenBy(d => d.Name).ToList();
                    break;
                case "Cable First":
                    allDevices = allDevices.OrderBy(d => d.Id.Contains(":")).ThenBy(d => d.Name).ToList();
                    break;
                case "Default":
                default:
                    allDevices = allDevices.OrderBy(d => d.Status == "offline").ThenBy(d => d.Name).ToList();
                    break;
            }

            flpDevices.SuspendLayout();
            flpDevices.Controls.Clear();

            foreach (var d in allDevices)
            {
                AddDeviceUI(d.Id, d.Status);
            }

            if (flpDevices.Controls.Count == 0)
            {
                Label noDev = new Label
                {
                    Text = "No devices connected.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10),
                    Margin = new Padding(10)
                };
                flpDevices.Controls.Add(noDev);
            }

            flpDevices.ResumeLayout();
            btnRefresh.Enabled = true;
            btnRefresh.Text = "Refresh";
        }

        private void flpDevices_Resize(object sender, EventArgs e)
        {
            foreach (Control ctrl in flpDevices.Controls)
            {
                if (ctrl is Panel)
                {
                    ctrl.Width = flpDevices.ClientSize.Width - 25;
                }
            }
        }

        private void AddDeviceUI(string deviceId, string status)
        {
            Panel pnl = new Panel
            {
                Width = flpDevices.ClientSize.Width - 25,
                Height = 70,
                BackColor = Color.FromArgb(250, 250, 250),
                Margin = new Padding(10)
            };

            string displayName = deviceNames.ContainsKey(deviceId) ? deviceNames[deviceId] : deviceId;

            Label lblId = new Label
            {
                Text = displayName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true,
                MaximumSize = new Size(pnl.Width - 365, 0),
                AutoEllipsis = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Button btnEdit = new Button
            {
                Text = "Rename",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(65, 35),
                Location = new Point(pnl.Width - 265, 17),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnEdit.FlatAppearance.BorderColor = Color.LightGray;
            btnEdit.Click += (s, e) => {
                string newName = Prompt.ShowDialog("Enter new name for device:", "Rename Device", displayName);
                if (newName != null && newName.Trim() != "")
                {
                    deviceNames[deviceId] = newName.Trim();
                    SaveDeviceNames();
                    RefreshDeviceList();
                }
            };

            Button btnDelete = new Button
            {
                Text = "Delete",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(200, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(55, 35),
                Location = new Point(pnl.Width - 195, 17),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnDelete.FlatAppearance.BorderColor = Color.LightGray;
            btnDelete.Click += (s, e) => {
                if (MessageBox.Show($"Are you sure you want to remove device '{displayName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (deviceId.Contains(":"))
                    {
                        var psi = new ProcessStartInfo { FileName = Path.Combine(ScrcpyDir, "adb.exe"), Arguments = $"disconnect {deviceId}", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
                        try { Process.Start(psi); } catch { }
                    }
                    deviceNames.Remove(deviceId);
                    SaveDeviceNames();
                    RefreshDeviceList();
                }
            };

            bool isRemote = deviceId.Contains(":");
            string connectionType = "(Cable)";
            if (isRemote)
            {
                // Nếu IP thuộc dải 100. thì coi là VPN (Tailscale)
                connectionType = deviceId.StartsWith("100.") ? "(VPN)" : "(WiFi)";
            }

            if (!isRemote)
            {
                // Nút Auto VPN
                Button btnAutoVpn = new Button
                {
                    Text = "Auto VPN",
                    Font = new Font("Segoe UI", 8, FontStyle.Regular),
                    BackColor = Color.White,
                    ForeColor = Color.RoyalBlue,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(70, 35),
                    Location = new Point(pnl.Width - 340, 17),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnAutoVpn.FlatAppearance.BorderColor = Color.LightGray;
                btnAutoVpn.Click += (s, e) => AutoSetup(deviceId, true);
                pnl.Controls.Add(btnAutoVpn);

                // Nút Auto WiFi
                Button btnAutoWifi = new Button
                {
                    Text = "Auto WiFi",
                    Font = new Font("Segoe UI", 8, FontStyle.Regular),
                    BackColor = Color.White,
                    ForeColor = Color.DarkMagenta,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(70, 35),
                    Location = new Point(pnl.Width - 415, 17),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnAutoWifi.FlatAppearance.BorderColor = Color.LightGray;
                btnAutoWifi.Click += (s, e) => AutoSetup(deviceId, false);
                pnl.Controls.Add(btnAutoWifi);
            }
            else
            {
                Button btnEditIp = new Button
                {
                    Text = "Edit IP/Port",
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    BackColor = Color.White,
                    ForeColor = Color.DarkOrange,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(85, 35),
                    Location = new Point(pnl.Width - 355, 17),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnEditIp.FlatAppearance.BorderColor = Color.LightGray;
                btnEditIp.Click += async (s, e) => {
                    string newIpPort = Prompt.ShowDialog("Enter new IP:Port for this device:", "Edit IP/Port", deviceId);
                    if (!string.IsNullOrWhiteSpace(newIpPort) && newIpPort.Trim() != deviceId)
                    {
                        newIpPort = newIpPort.Trim();
                        string oldName = deviceNames.ContainsKey(deviceId) ? deviceNames[deviceId] : deviceId;
                        
                        if (deviceNames.ContainsKey(deviceId)) deviceNames.Remove(deviceId);
                        deviceNames[newIpPort] = oldName;
                        SaveDeviceNames();

                        string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
                        Process.Start(new ProcessStartInfo { FileName = adbPath, Arguments = $"disconnect {deviceId}", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden })?.WaitForExit();
                        
                        await Task.Run(() => {
                            var p = Process.Start(new ProcessStartInfo { FileName = adbPath, Arguments = $"connect {newIpPort}", CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
                            p?.WaitForExit();
                        });

                        RefreshDeviceList();
                    }
                };
                pnl.Controls.Add(btnEditIp);
            }

            bool isConnected = activeProcesses.ContainsKey(deviceId) && !activeProcesses[deviceId].HasExited;
            string displayStatus = isConnected ? "Connected" : (status.ToLower() == "device" ? "Ready" : status);
            displayStatus += $" {connectionType}";

            Label lblStatus = new Label
            {
                Text = "Status: " + displayStatus,
                Font = new Font("Segoe UI", 9),
                ForeColor = isConnected ? Color.RoyalBlue : (status.ToLower() == "device" ? Color.Green : Color.Gray),
                Location = new Point(15, 40),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Button btnConnect = new Button
            {
                Text = isConnected ? "Disconnect" : "Connect",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = isConnected ? Color.IndianRed : Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 35),
                Location = new Point(pnl.Width - 130, 17),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Enabled = status.ToLower() == "device" || isConnected || (isRemote && status.ToLower() == "offline");
            btnConnect.Click += async (s, e) => {
                if (status.ToLower() == "offline" && isRemote)
                {
                    btnConnect.Enabled = false;
                    btnConnect.Text = "Connecting...";
                    
                    string adbPath = Path.Combine(ScrcpyDir, "adb.exe");
                    await Task.Run(() => {
                        var p = Process.Start(new ProcessStartInfo { 
                            FileName = adbPath, 
                            Arguments = $"connect {deviceId}", 
                            CreateNoWindow = true, 
                            WindowStyle = ProcessWindowStyle.Hidden 
                        });
                        p?.WaitForExit();
                    });
                    
                    RefreshDeviceList();
                }
                else
                {
                    ConnectDevice(deviceId);
                }
            };

            pnl.Controls.Add(lblId);
            pnl.Controls.Add(btnEdit);
            pnl.Controls.Add(btnDelete);
            pnl.Controls.Add(lblStatus);
            pnl.Controls.Add(btnConnect);
            flpDevices.Controls.Add(pnl);
        }

        private void ConnectDevice(string deviceId)
        {
            try
            {
                if (activeProcesses.ContainsKey(deviceId) && !activeProcesses[deviceId].HasExited)
                {
                    // Đang chạy -> Ngắt kết nối
                    activeProcesses[deviceId].Kill();
                    RefreshDeviceList();
                    return;
                }

                string scrcpyPath = Path.Combine(ScrcpyDir, "scrcpy.exe");
                if (!File.Exists(scrcpyPath))
                {
                    MessageBox.Show("Could not find scrcpy.exe!");
                    return;
                }

                string args = $"-s {deviceId}";
                if (chkTurnOffScreen.Checked)
                {
                    args += " --turn-screen-off";
                }

                var psi = new ProcessStartInfo
                {
                    FileName = scrcpyPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                string bridgeIp = txtBridgeIp.Text.Trim();
                if (!string.IsNullOrEmpty(bridgeIp))
                {
                    psi.EnvironmentVariables["ADBHOST"] = bridgeIp;
                }

                Process proc = Process.Start(psi);
                if (proc != null)
                {
                    proc.EnableRaisingEvents = true;
                    proc.Exited += (s, e) => {
                        this.Invoke(new Action(() => RefreshDeviceList()));
                    };
                    activeProcesses[deviceId] = proc;
                    RefreshDeviceList(); // Gọi lại để đổi trạng thái nút
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error launching scrcpy: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWifiSettings();
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
            }
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string helpText = "A. CONNECT VIA USB CABLE:\n" +
                              "1. Open Settings on your phone.\n" +
                              "2. Go to 'About phone'.\n" +
                              "3. Find 'Build number' and tap it 7 times to enable Developer options.\n" +
                              "4. Go back to Settings > System > Developer options.\n" +
                              "5. Enable 'USB Debugging'.\n" +
                              "6. Connect the cable to PC, tap 'Allow' on your phone when prompted.\n\n" +
                              "B. CONNECT VIA WIFI:\n" +
                              "1. Ensure your phone and PC are on the same WiFi network.\n" +
                              "2. For Android 11+: Enable 'Wireless debugging' in Developer options, view the IP and Port, then enter them in the app.\n" +
                              "3. For older Android: Connect USB cable first, enable USB Debugging, then open CMD and type 'adb tcpip 5555'. Disconnect the cable, enter your phone's WiFi IP and port 5555 in the app, then click 'Connect'.";
            MessageBox.Show(helpText, "Connection Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class ToggleSwitch : CheckBox
    {
        public ToggleSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            this.OnPaintBackground(pevent);
            pevent.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int d = this.Height - 1;
            int width = this.Width;
            if (width < d * 2) width = d * 2;

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(0, 0, d, d, 90, 180);
                path.AddArc(width - d - 1, 0, d, d, -90, 180);
                path.CloseFigure();
                pevent.Graphics.FillPath(Checked ? Brushes.LimeGreen : Brushes.LightGray, path);
            }

            int r = this.Height - 5;
            if (Checked)
            {
                pevent.Graphics.FillEllipse(Brushes.White, width - r - 3, 2, r, r);
            }
            else
            {
                pevent.Graphics.FillEllipse(Brushes.White, 2, 2, r, r);
            }
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true, Font = new Font("Segoe UI", 10) };
            TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 340, Text = defaultValue, Font = new Font("Segoe UI", 10) };
            Button confirmation = new Button() { Text = "Confirm", Left = 260, Width = 100, Top = 75, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}
