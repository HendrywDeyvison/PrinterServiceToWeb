using PrinterServiceToWeb;
using System.ServiceProcess;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

static class Program
{
    static async Task Main()
    {
        
        if (Environment.UserInteractive)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var service = new PrinterService();
            service.StartService();

            Application.Run();
        }
        else
        {
            ServiceBase[] ServicesToRun = new ServiceBase[] { new PrinterService() };
            ServiceBase.Run(ServicesToRun);
        }


    }
}