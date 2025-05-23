using Microsoft.Win32;
using PrinterServiceToWeb;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Windows.Forms;

public class WebSocketServer
{
    private HttpListener _listener;
    private CancellationTokenSource _cts;
    private WriterAndReaderConfigs _readerConfig;
    public string LabelPrinter { get; set; }

    public WebSocketServer()
    {
        _cts = new CancellationTokenSource();
        LabelPrinter = GetLabelPrinterFromConfigFile();
    }

    private string GetLabelPrinterFromConfigFile()
    {
        _readerConfig = new WriterAndReaderConfigs();

        string configText = _readerConfig.ReaderConfig();

        if (configText != null)
        {

            string[] parts = configText.Split('=');
            if (parts.Length > 1)
            {
                string printerName = parts[1].Trim();

                return printerName;
            }
        }

        return null;

        /*return Registry.GetValue(
            @"HKEY_CURRENT_USER\SOFTWARE\ZPLPrinterService",
            "SelectedPrinter",
            null) as string;*/
    }

    public void Start(string uriPrefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(uriPrefix);
        _listener.Start();

        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        ProcessWebSocketRequest(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Server error: {ex.Message}");
                }
            }
        }, _cts.Token);
    }

    private async void ProcessWebSocketRequest(HttpListenerContext context)
    {
        WebSocket webSocket = null;
        try
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            webSocket = wsContext.WebSocket;
            var buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cts.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessPrintCommand(message, webSocket);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WebSocket error: {ex.Message}");
        }
        finally
        {
            webSocket?.Dispose();
        }
    }

    private async Task ProcessPrintCommand(string command, WebSocket webSocket)
    {
        try
        {
            LabelPrinter = GetLabelPrinterFromConfigFile();

            if (string.IsNullOrEmpty(LabelPrinter))
            {
                throw new Exception("Nenhuma impressora selecionada, entre no QUALITY, configure e tente novamente");
            }

            string zplCommand = command.Trim();

            if (!zplCommand.EndsWith("\r\n"))
                zplCommand += "\r\n";

            if (RawPrinterHelper.SendStringToPrinter(LabelPrinter, zplCommand))
            {
                await SendResponse(webSocket, "SUCCESS: Comando ZPL enviado");
            }
            else
            {
                throw new Exception("ERROR: Falha no envio ZPL");
            }
        }
        catch (Exception ex)
        {
            await SendResponse(webSocket, $"ERROR: {ex.Message}");
        }
    }

    private async Task SendResponse(WebSocket webSocket, string message)
    {
        try
        {
            if (webSocket.State == WebSocketState.Open)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    _cts.Token);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao enviar resposta: {ex.Message}");
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
    }
}