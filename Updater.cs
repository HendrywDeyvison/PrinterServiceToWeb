using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO.Compression;

namespace PrinterServiceToWeb
{
    public class Updater
    {
        static readonly string ftpServer = "ftp://ftp.softquality.com.br/PrinterServiceToWeb/update/";
        static readonly string ftpUser = "WebUpdate";
        static readonly string ftpPass = "AG21k174$";

        static readonly string urlService = ftpServer + "service/";
        static readonly string localPathService = @"C:\Quality\PrinterServiceToWeb\service";

        static readonly string urlUpdater = ftpServer + "updater/";
        static readonly string localPathUpdater = @"C:\Quality\PrinterServiceToWeb\updater";
        static readonly string zipFile = "UpdaterPrinterServiceToWeb.zip";

        static readonly string updaterPath = @"C:\Quality\PrinterServiceToWeb\updater\UpdaterPrinterServiceToWeb.exe";

        static readonly string versionFile = "versao.txt";

        private void OpenUpdaterService()
        {
            if (File.Exists(updaterPath))
            {
                Process updater = new Process();
                updater.StartInfo.FileName = updaterPath;

                updater.Start();

                updater.WaitForExit();
            }
        }

        public void QuestionCheckUpdateService()
        {
            var resp = MessageBox.Show($"Deseja verificar se há atualizações para o Serviço de Impressão do Quality?", "Serviço de Atualização", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resp == DialogResult.Yes)
            {
                string remoteVersion = DownloadString(urlService + versionFile);
                string localVersion = File.Exists(Path.Combine(localPathService, versionFile))
                    ? File.ReadAllText(Path.Combine(localPathService, versionFile)).Trim()
                    : "0.0.0";

                if (remoteVersion != localVersion)
                {

                    var respQuestion = MessageBox.Show($"Deseja atualizar o Serviço de Impressão do Quality para versão: {remoteVersion}?", "Atualização encontrada!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (respQuestion == DialogResult.Yes)
                    {
                        OpenUpdaterService();
                    }

                }
                else
                {
                    MessageBox.Show("Sua versão já está atualizada!", "Serviço de Atualização", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    StartUpdateUpdater();
                }
            }

        }

        public void CheckAndUpdateService()
        {
            try
            {
                string remoteVersion = DownloadString(urlService + versionFile);
                string localVersion = File.Exists(Path.Combine(localPathService, versionFile))
                    ? File.ReadAllText(Path.Combine(localPathService, versionFile)).Trim()
                    : "0.0.0";

                if (remoteVersion != localVersion)
                {

                    var resp = MessageBox.Show($"Deseja atualizar o Serviço de Impressão do Quality para versão: {remoteVersion}?", "Atualização encontrada!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (resp == DialogResult.Yes)
                    {
                        OpenUpdaterService();
                    }

                } else
                {
                    StartUpdateUpdater();
                }

            }
            catch (Exception ex)
            {   
                MessageBox.Show("Erro: " + ex.Message);
            }

        }

        private async Task StartUpdateUpdater()
        {
            try
            {
                string remoteVersion = DownloadString(urlUpdater + versionFile);
                string localVersionPath = Path.Combine(localPathUpdater, versionFile);
                string localVersion = File.Exists(localVersionPath)
                    ? File.ReadAllText(localVersionPath).Trim()
                    : "0.0.0";

                if (remoteVersion != localVersion)
                {

                    Process[] running = Process.GetProcessesByName("UpdaterPrinterServiceToWeb");
                    foreach (var p in running)
                    {
                        p.Kill();
                        p.WaitForExit();
                    }


                    DeleteAllFiles(localPathUpdater);

                    string tempZip = Path.Combine(Path.GetTempPath(), zipFile);

                    await DownloadFileAsync(urlUpdater + zipFile, tempZip);

                    ZipFile.ExtractToDirectory(tempZip, localPathUpdater);

                    File.WriteAllText(Path.Combine(localPathUpdater, versionFile), remoteVersion);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar o Updater: " + ex.Message);
            }

        }

        private void DeleteAllFiles(string folderPath)
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                Directory.Delete(dir, true);
            }
        }


        static string DownloadString(string url)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(ftpUser, ftpPass);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd().Trim();
            }
        }

        private async Task DownloadFileAsync(string url, string localPath)
        {
            await Task.Run(() =>
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fs = new FileStream(localPath, FileMode.Create))
                {
                    responseStream.CopyTo(fs);
                }
            });
        }

    }
}
