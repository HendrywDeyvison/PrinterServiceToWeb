using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PrinterServiceToWeb
{
    public static class RawPrinterHelper
    {
        private const string RegistryKeyPath = @"SOFTWARE\ZPLPrinterService";
        private const string ValueName = "SelectedPrinter";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", CharSet = CharSet.Ansi)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", CharSet = CharSet.Ansi)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", CharSet = CharSet.Ansi)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOCINFOA di);

        [DllImport("winspool.Drv", CharSet = CharSet.Ansi)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", CharSet = CharSet.Ansi)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", CharSet = CharSet.Ansi)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", CharSet = CharSet.Ansi)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendStringToPrinter(string printerName, string zplCommand)
        {

            IntPtr pBytes = IntPtr.Zero;
            IntPtr hPrinter = IntPtr.Zero;
            bool success = false;

            try
            {

                zplCommand = zplCommand.Replace("\r\n", "\n").Replace("\n", "\r\n");
                // Adiciona quebra de linha no final se não existir
                if (!zplCommand.EndsWith("\r\n"))
                {
                    zplCommand += "\r\n";
                }

                // Conversão para ASCII (ZPL requer ASCII)
                byte[] asciiBytes = Encoding.GetEncoding("UTF-8").GetBytes(zplCommand);//Encoding.ASCII.GetBytes(zplCommand);
                pBytes = Marshal.AllocCoTaskMem(asciiBytes.Length);
                Marshal.Copy(asciiBytes, 0, pBytes, asciiBytes.Length);

                // Abre a impressora
                if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                {
                    Debug.WriteLine($"Falha ao abrir impressora {printerName}. Erro: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                // Configura o documento
                DOCINFOA di = new DOCINFOA
                {
                    pDocName = "ZPL_Print_Job",
                    pDataType = "RAW" // Tipo RAW é essencial para ZPL
                };

                // Inicia o documento
                if (!StartDocPrinter(hPrinter, 1, ref di))
                {
                    Debug.WriteLine($"Falha ao iniciar documento. Erro: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                // Inicia a página
                if (!StartPagePrinter(hPrinter))
                {
                    Debug.WriteLine($"Falha ao iniciar página. Erro: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                // Escreve os dados
                if (!WritePrinter(hPrinter, pBytes, asciiBytes.Length, out int bytesWritten))
                {
                    Debug.WriteLine($"Falha ao escrever na impressora. Bytes escritos: {bytesWritten}, Erro: {Marshal.GetLastWin32Error()}");
                    return false;
                }

                Debug.WriteLine($"Sucesso! Bytes escritos: {bytesWritten}");
                success = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Erro crítico: {ex.Message}\nStackTrace: {ex.StackTrace}", "Error");
            }
            finally
            {
                // Limpeza adequada
                if (hPrinter != IntPtr.Zero)
                {
                    try
                    {
                        EndPagePrinter(hPrinter);
                        EndDocPrinter(hPrinter);
                        ClosePrinter(hPrinter);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Erro durante limpeza: {ex.Message}");
                    }
                }

                if (pBytes != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pBytes);
                }
            }

            return success;
        }
    }
}