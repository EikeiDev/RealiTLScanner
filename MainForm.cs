using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private TextBox txtAddr;
    private NumericUpDown numPort;
    private NumericUpDown numTimeout;
    private NumericUpDown numThreads;
    private CheckBox chkVerbose;
    private CheckBox chkIPv6;
    private Button btnRun;
    private Label lblAddr;
    private Label lblPort;
    private Label lblTimeout;
    private Label lblThreads;
    private Process? runningProcess;

    public MainForm()
    {
        InitializeComponent();
        this.FormClosing += MainForm_FormClosing;
        _ = CheckAndDownloadGeoDB();
    }

    private void InitializeComponent()
    {
        lblAddr = new Label();
        lblPort = new Label();
        lblTimeout = new Label();
        lblThreads = new Label();
        txtAddr = new TextBox();
        numPort = new NumericUpDown();
        numTimeout = new NumericUpDown();
        numThreads = new NumericUpDown();
        chkVerbose = new CheckBox();
        chkIPv6 = new CheckBox();
        btnRun = new Button();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numTimeout).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numThreads).BeginInit();
        SuspendLayout();
        // 
        // lblAddr
        // 
        lblAddr.Location = new Point(12, 15);
        lblAddr.Name = "lblAddr";
        lblAddr.Size = new Size(71, 24);
        lblAddr.TabIndex = 0;
        lblAddr.Text = "Address:";
        // 
        // lblPort
        // 
        lblPort.Location = new Point(50, 51);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(40, 20);
        lblPort.TabIndex = 2;
        lblPort.Text = "Port:";
        // 
        // lblTimeout
        // 
        lblTimeout.Location = new Point(349, 49);
        lblTimeout.Name = "lblTimeout";
        lblTimeout.Size = new Size(71, 26);
        lblTimeout.TabIndex = 4;
        lblTimeout.Text = "Timeout:";
        // 
        // lblThreads
        // 
        lblThreads.Location = new Point(196, 52);
        lblThreads.Name = "lblThreads";
        lblThreads.Size = new Size(75, 20);
        lblThreads.TabIndex = 6;
        lblThreads.Text = "Threads:";
        // 
        // txtAddr
        // 
        txtAddr.Location = new Point(89, 12);
        txtAddr.Name = "txtAddr";
        txtAddr.PlaceholderText = "Single address (optional)";
        txtAddr.Size = new Size(383, 27);
        txtAddr.TabIndex = 1;
        // 
        // numPort
        // 
        numPort.Location = new Point(96, 47);
        numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        numPort.Name = "numPort";
        numPort.Size = new Size(52, 27);
        numPort.TabIndex = 3;
        numPort.Value = new decimal(new int[] { 443, 0, 0, 0 });
        // 
        // numTimeout
        // 
        numTimeout.Location = new Point(419, 47);
        numTimeout.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        numTimeout.Name = "numTimeout";
        numTimeout.Size = new Size(46, 27);
        numTimeout.TabIndex = 5;
        numTimeout.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // numThreads
        // 
        numThreads.Location = new Point(271, 48);
        numThreads.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
        numThreads.Name = "numThreads";
        numThreads.Size = new Size(46, 27);
        numThreads.TabIndex = 7;
        numThreads.Value = new decimal(new int[] { 10, 0, 0, 0 });
        // 
        // chkVerbose
        // 
        chkVerbose.Location = new Point(139, 86);
        chkVerbose.Name = "chkVerbose";
        chkVerbose.Size = new Size(86, 24);
        chkVerbose.TabIndex = 8;
        chkVerbose.Text = "Verbose";
        // 
        // chkIPv6
        // 
        chkIPv6.Location = new Point(12, 86);
        chkIPv6.Name = "chkIPv6";
        chkIPv6.Size = new Size(136, 24);
        chkIPv6.TabIndex = 9;
        chkIPv6.Text = "Enable IPv6";
        // 
        // btnRun
        // 
        btnRun.Location = new Point(267, 86);
        btnRun.Name = "btnRun";
        btnRun.Size = new Size(205, 27);
        btnRun.TabIndex = 10;
        btnRun.Text = "Scan (CLI)";
        btnRun.Click += btnRun_Click;
        // 
        // MainForm
        // 
        BackColor = Color.BurlyWood;
        ClientSize = new Size(484, 122);
        Controls.Add(lblAddr);
        Controls.Add(txtAddr);
        Controls.Add(lblPort);
        Controls.Add(numPort);
        Controls.Add(lblTimeout);
        Controls.Add(numTimeout);
        Controls.Add(lblThreads);
        Controls.Add(numThreads);
        Controls.Add(chkVerbose);
        Controls.Add(chkIPv6);
        Controls.Add(btnRun);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        Text = "RealiTLScanner GUI (CLI Wrapper)";
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)numTimeout).EndInit();
        ((System.ComponentModel.ISupportInitialize)numThreads).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private async void btnRun_Click(object sender, EventArgs e)
    {
        btnRun.Enabled = false;

        string exePath = "RealiTLScanner_Win.exe";
        if (!File.Exists(exePath))
        {
            MessageBox.Show("RealiTLScanner.exe не найден рядом с GUI", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRun.Enabled = true;
            return;
        }

        StringBuilder args = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(txtAddr.Text))
            args.Append($" -addr {txtAddr.Text}");

        args.Append($" -port {numPort.Value}");
        args.Append($" -thread {numThreads.Value}");
        args.Append($" -timeout {numTimeout.Value}");
        args.Append(" -out result.csv");

        if (chkIPv6.Checked) args.Append(" -46");
        if (chkVerbose.Checked) args.Append(" -v");

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args.ToString(),
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            runningProcess = new Process { StartInfo = psi };
            runningProcess.Start();

            await runningProcess.WaitForExitAsync();
            runningProcess = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске: {ex.Message}");
        }

        btnRun.Enabled = true;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (runningProcess != null && !runningProcess.HasExited)
        {
            try { runningProcess.Kill(true); } catch { }
        }
    }

    private async Task CheckAndDownloadGeoDB()
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Country.mmdb");
        if (File.Exists(dbPath)) return;

        try
        {
            using var http = new HttpClient();
            var data = await http.GetByteArrayAsync("https://git.io/GeoLite2-Country.mmdb");
            await File.WriteAllBytesAsync(dbPath, data);
            MessageBox.Show("Файл Country.mmdb был успешно загружен.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить базу GeoLite:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
