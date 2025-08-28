using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RawPrint;
using RawPrint.NetStd;

namespace PrinterServiceToWeb
{
    public static class RawPrinterHelper
    {
        private static readonly IPrinter _rawPrinter = new Printer();

        private const int MaxSingleJobSize = 10 * 1024 * 1024; // 10 MB
        private const int ChunkSize = 64 * 1024; // 64 KB

        public static bool SendStringToPrinter(string printerName, string zplCommand)
        {
            bool success = false;

            try
            {
                var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                byte[] data = utf8NoBom.GetBytes(zplCommand);

                if (data.Length <= MaxSingleJobSize)
                {
                    using (var stream = new MemoryStream(data))
                    {
                        _rawPrinter.PrintRawStream(printerName, stream, "Etiquetas_Produtos");
                    }
                }
                else
                {
                    int offset = 0;
                    int jobCount = 0;

                    while (offset < data.Length)
                    {
                        int size = Math.Min(ChunkSize, data.Length - offset);
                        using (var stream = new MemoryStream(data, offset, size))
                        {
                            _rawPrinter.PrintRawStream(printerName, stream, $"Etiquetas_Produtos_Pagina{++jobCount}");
                        }
                        offset += size;
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro crítico: {ex.Message}\nStackTrace: {ex.StackTrace}", "Erro");
            }

            return success;
        }
    }
}
