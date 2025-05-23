using PrinterServiceToWeb;
using System.ServiceProcess;
using System;
using System.Windows.Forms;

static class Program
{
    static void Main()
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