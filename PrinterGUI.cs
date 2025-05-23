using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Printing;
using System.Windows.Forms;

namespace PrinterServiceToWeb
{
    public class PrinterGUI : Form
    {
        private WebSocketServer _wsServer;
        private ComboBox _printerComboBox;
        private Button _saveButton;
        private NotifyIcon _trayIcon;
        private WriterAndReaderConfigs _writerAndReaderConfigs;
        private const string RegistryKeyPath = @"SOFTWARE\ZPLPrinterService";
        private const string ValueName = "SelectedPrinter";

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
            this.Text = "ZPL/EPL Printer Configuration";
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
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

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "ZPL Printer Service",
                Visible = true
            };

            _trayIcon.DoubleClick += ((sender, e) => ShowForm());

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open Config", null, (sender, e) => ShowForm());
            contextMenu.Items.Add("Exit", null, (sender, e) => Application.Exit());

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

                    //Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\ZPLPrinterService"); //Apagar, só para teste!

                    /*Registry.SetValue(
                        @"HKEY_CURRENT_USER\SOFTWARE\ZPLPrinterService",
                        "SelectedPrinter",
                        _wsServer.LabelPrinter
                    );*/

                    //MessageBox("Success", "Title", 5000);

                    // MessageBox.Show($"Printer saved: {_wsServer.LabelPrinter}", "Success");

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
    }
}