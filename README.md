# 🖨️ PrinterServiceToWeb

![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue)

Projeto em **C# (.NET Framework 4.7.2)** que implementa um serviço Windows para gerenciamento de impressoras, podendo também rodar como aplicação Windows Forms em modo interativo.  
Suporta envio direto de impressão via **RawPrint.NetStd** e comunicação via WebSocket.

---

## ✨ Funcionalidades

- Executa como **serviço do Windows** ou **aplicação interativa**.  
- Inicializa o **PrinterService** para gerenciar impressoras conectadas.  
- Suporte a envio direto para impressoras via `RawPrint.NetStd`.  
- Comunicação WebSocket configurável (porta padrão: 9090).  
- Autenticação e permissões configuradas via **Windows Authentication**.

---

## ⚙️ Configuração (`App.config`)

### appSettings
```xml
<add key="WebSocketPort" value="9090" />
<add key="AllowedOrigins" value="http://localhost,http://127.0.0.1" />
<add key="ClientSettingsProvider.ServiceUri" value="" />
<add key="ClientSettingsProvider.ConnectionStringName" value="DefaultConnection" />
