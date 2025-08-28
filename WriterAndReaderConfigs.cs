using System;
using System.IO;
using System.Windows.Forms;

namespace PrinterServiceToWeb
{
    public class WriterAndReaderConfigs
    {
        private const string docPath = @"c:\Quality\config-label-printer.ini";

        public void WriterConfig(string printerConfig)
        {
            string text = "PrinterName = " + printerConfig + Environment.NewLine;

            File.WriteAllText(docPath, text);
        }

        public string ReaderConfig()
        {
            if (!File.Exists(docPath))
            {
                MessageBox.Show("Arquivo de configuração não encontrado, entre no QUALITY, configure e tente novamente!", "Aviso!");
                return null;
            }

            var configEtiquetas = File.ReadAllText(docPath);

            if(configEtiquetas.Length <= 0)
            {
                MessageBox.Show("Arquivo de configuração vazio, entre no QUALITY, configure e tente novamente!", "Aviso!");
            }

            return configEtiquetas;
        }
    }
}
