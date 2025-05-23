using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace PrinterServiceToWeb
{
    public class PrinterService : ServiceBase
    {
        private WebSocketServer _wsServer;
        private Thread _guiThread;

        public PrinterService()
        {
            ServiceName = "LabelPrinterService";
            CanStop = true;
            CanPauseAndContinue = false;
        }

        public void StartService()
        {
            this.OnStart(null); // Chama o método protegido
        }

        protected override void OnStart(string[] args)
        {
            _wsServer = new WebSocketServer();
            _wsServer.Start("http://localhost:9090/");

            _guiThread = new Thread(() =>
            {
                var gui = new PrinterGUI(_wsServer);
                Application.Run(gui);
            });
            _guiThread.SetApartmentState(ApartmentState.STA);
            _guiThread.Start();
        }

        protected override void OnStop()
        {
            _wsServer?.Stop();
            _guiThread?.Abort();
        }
    }
}