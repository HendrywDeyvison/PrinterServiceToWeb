using System;
using System.IO;
using System.IO.Compression;
using System.Net;

public static class Updater
{
    static string ftpServer = "ftp://ftp.softquality.com.br/PrinterServiceToWeb/update/";
    static string ftpUser = "WebUpdate";
    static string ftpPass = "AG21k174$";

    static string localPath = @"C:\Quality\PrinterServiceToWeb";
    static string versionFile = "versao.txt";
    static string zipFile = "PrinterServiceToWeb.zip";

    static void Main()
    {
        try
        {
            Console.WriteLine("Verificando versão...");

            // 1. Baixa o versao.txt
            string remoteVersion = DownloadString(ftpServer + versionFile);
            string localVersion = File.Exists(Path.Combine(localPath, versionFile))
                ? File.ReadAllText(Path.Combine(localPath, versionFile)).Trim()
                : "0.0.0";

            Console.WriteLine($"Versão remota: {remoteVersion}, Local: {localVersion}");

            // 2. Compara versões
            if (remoteVersion != localVersion)
            {
                Console.WriteLine("Atualização encontrada! Baixando...");

                string localZip = Path.Combine(Path.GetTempPath(), zipFile);

                DownloadFile(ftpServer + zipFile, localZip);

                Console.WriteLine("Extraindo arquivos...");
                if (Directory.Exists(localPath))
                    Directory.Delete(localPath, true);

                ZipFile.ExtractToDirectory(localZip, localPath);

                // Atualiza o arquivo de versão
                File.WriteAllText(Path.Combine(localPath, versionFile), remoteVersion);

                Console.WriteLine("Atualização concluída!");
            }
            else
            {
                Console.WriteLine("Já está na versão mais recente.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex.Message);
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

    static void DownloadFile(string url, string localPath)
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
    }
}
