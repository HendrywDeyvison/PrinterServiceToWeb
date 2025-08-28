using System;
using System.Drawing;
using System.IO;
using System.Printing;
using System.Windows.Forms;
using MaterialDesign.Icons;

namespace PrinterServiceToWeb
{
    public class PrinterGUI : Form
    {
        private WebSocketServer _wsServer;
        private ComboBox _printerComboBox;
        private Button _saveButton;
        private NotifyIcon _trayIcon;
        private WriterAndReaderConfigs _writerAndReaderConfigs;
        private Updater _updater;

        static string localPath = @"C:\Quality\PrinterServiceToWeb\service";
        static string versionFile = "versao.txt";

        static string localVersion = File.Exists(Path.Combine(localPath, versionFile))
                ? File.ReadAllText(Path.Combine(localPath, versionFile)).Trim()
                : "0.0.0";

        public PrinterGUI(WebSocketServer wsServer)
        {
            _writerAndReaderConfigs = new WriterAndReaderConfigs();
            _wsServer = wsServer;

            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;

            InitializeComponents();
            LoadPrinters();
            SetupTrayIcon();

            if (!string.IsNullOrEmpty(_wsServer.LabelPrinter))
            {
                this.Hide();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }

        }

        private void InitializeComponents()
        {

            this.Icon = new Icon("quality_32x32.ico");
            this.Text = $"ZPL/EPL Printer Configuration  Version:{localVersion}";
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            _printerComboBox = new ComboBox
            {
                Location = new Point(20, 40),
                Size = new Size(350, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _saveButton = new Button
            {
                Text = "Save Printer",
                Location = new Point(20, 80),
                Size = new Size(100, 30)
            };
            _saveButton.Click += (sender, e) => SavePrinter();

            this.Controls.Add(_printerComboBox);
            this.Controls.Add(_saveButton);
        }

        private void SetupTrayIcon()
        {

            var contextMenu = new ContextMenuStrip();
            var settingsIcon = Properties.Resources.settings.ToBitmap();
            var updateIcon = Properties.Resources.update.ToBitmap();
            var exitIcon = Properties.Resources.exit.ToBitmap();

            _updater = new Updater();

            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("quality_32x32.ico"),
                Text = $"ZPL Printer Service Version:{localVersion}",
                Visible = true
            };

            _trayIcon.DoubleClick += ((sender, e) => ShowForm());


            contextMenu.Items.Add($"ZPL Printer Service Version:{localVersion}", new Icon("quality_32x32.ico").ToBitmap()).Enabled=false;
            
            contextMenu.Items.Add(new ToolStripSeparator());

            contextMenu.Items.Add("Configurações", settingsIcon, (sender, e) => ShowForm());
            contextMenu.Items.Add("Atualização", updateIcon, (sender, e) => _updater.QuestionCheckUpdateService());
            contextMenu.Items.Add("Sair/Parar", exitIcon, (sender, e) => Application.Exit());

            _trayIcon.ContextMenuStrip = contextMenu;
        }

        private void LoadPrinters()
        {
            var printServer = new LocalPrintServer();

            foreach (var queue in printServer.GetPrintQueues())
            {
                if (IsZplOrEplPrinter(queue))
                {
                    _printerComboBox.Items.Add(queue.Name);
                }
            }

            if (!string.IsNullOrEmpty(_wsServer.LabelPrinter))
            {
                _printerComboBox.SelectedItem = _wsServer.LabelPrinter;
            }
            else if (_printerComboBox.Items.Count > 0)
            {
                _printerComboBox.SelectedIndex = 0;
            }
        }

        private bool IsZplOrEplPrinter(PrintQueue queue)
        {
            string name = queue.Name.ToUpper();
            return name.Contains("ZDESIGNER") || name.Contains("ZEBRA") || name.Contains("ZPL") ||
                   name.Contains("ELGIN") || name.Contains("EPL");
        }

        private void SavePrinter()
        {
            if (_printerComboBox.SelectedItem != null)
            {
                try
                {
                    _wsServer.LabelPrinter = _printerComboBox.SelectedItem.ToString();

                    _writerAndReaderConfigs.WriterConfig(_wsServer.LabelPrinter);

                    this.Close();
                }
                catch (Exception e)
                {

                    MessageBox.Show($"Erro ao salvar impressora: {e.Message}", "Error");
                }
            }
        }

        public void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }

            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PrinterGUI
            // 
            this.ClientSize = new System.Drawing.Size(282, 238);
            this.Name = "PrinterGUI";
            this.ResumeLayout(false);

        }
    }
}