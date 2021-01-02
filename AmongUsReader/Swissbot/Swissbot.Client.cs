using AmongUsReader.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public partial class Swissbot
    {
        private ClientWebSocket clientSocket;
        public bool IsConnected;
        public bool IsAuthed;

        public static string SwissbotAuthToken;

        public string AuthedUser;

        private TaskCompletionSource<CredentialResponse> CredentialToken;

        public async Task CreateSwissbotClient()
        {
            clientSocket = new ClientWebSocket();

            while (!IsConnected)
            {
                try
                {

#if DEBUG
                    await clientSocket.ConnectAsync(new Uri("http://localhost:3000/apprentice/v1/socket"), CancellationToken.None);
#else
                    await clientSocket.ConnectAsync(new Uri("http://api.swissdev.team/apprentice/v1/socket"), CancellationToken.None);
#endif
                    IsConnected = true;

                    Task.Run(() => ClientRecieveLoop().ConfigureAwait(false));
                }
                catch (Exception x)
                {
                    Logger.Write("Connection to swissbot was unsuccessful, retrying in 10 seconds", Logger.Severity.Socket, Logger.Severity.Error);
                    await Task.Delay(10000);
                }
            }

        }

        private async Task SendToSwissbot(ISendable packet)
        {
            if (!IsConnected)
                return;

            string content = JsonConvert.SerializeObject(packet);

            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(content), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<CredentialResponse> ValidateCredentials(ValidateCredentials c)
        {
            await SendToSwissbot(c);
            
            CredentialToken = new TaskCompletionSource<CredentialResponse>(null);

            var result = await CredentialToken.Task;

            return result;
        }

        private async Task ClientRecieveLoop()
        {
            while (clientSocket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] _buff = new byte[1024];
                    WebSocketReceiveResult r;
                    try
                    {
                        r = await clientSocket.ReceiveAsync(_buff, CancellationToken.None);

                        Task.Run(() => HandleClientRecieve(_buff, r).ConfigureAwait(false));
                    }
                    catch (Exception x)
                    {
                        Logger.Write($"Exception on recieving data from swissbot: {x}", Logger.Severity.Socket, Logger.Severity.Critical);
                        return;
                    }


                }
                catch (Exception x)
                {
                    Logger.Write($"Exception on recieving data from swissbot: {x}", Logger.Severity.Socket, Logger.Severity.Critical);
                }
            }
        }

        private async Task HandleClientRecieve(byte[] buff, WebSocketReceiveResult r)
        {
            if(r.MessageType == WebSocketMessageType.Close)
            {
                Logger.Write($"Swissbot closed the socket. <Red>Kinda sus</Red>", Logger.Severity.Socket);
                await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                clientSocket.Dispose();
                return;
            }

            string content = Encoding.UTF8.GetString(buff);

            IRecieveable message;

            try
            {
                message = JsonConvert.DeserializeObject<RawRecieveable>(content);
            }
            catch (Exception x)
            {
                await clientSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.InvalidMessageType, "Invalid IRecieveable", CancellationToken.None);
                Logger.Write("Failed to read a packet from swissbot, please restart this program", Logger.Severity.Socket, Logger.Severity.Critical);
                clientSocket.Dispose();
                return;
            }

            switch (message.type)
            {
                case "handshake_result":
                    {
                        var result = JsonConvert.DeserializeObject<HandshakeResult>(content);

                        if (result.valid)
                        {
                            Logger.Write($"Authed with swissbot <Green>successfully!</Green> Welcome {result.user}!", Logger.Severity.Socket);
                            this.IsAuthed = true;
                            AuthedUser = result.user;
                            return;
                        }
                        else
                        {
                            Logger.Write($"Failed to authed with swissbot: \'{result.reason}\'", Logger.Severity.Socket, Logger.Severity.Error);
                            Logger.Write($"Please make sure you have the browser open and you're logged into the events page, then restart this program", Logger.Severity.Critical);
                            return;
                        }
                    }

                case "validate_credentials_result":
                    {
                        var result = JsonConvert.DeserializeObject<CredentialResponse>(content);

                        if (!CredentialToken.TrySetResult(result))
                        {
                            Logger.Write("Failed to authenticate, please restart this client, if the issue presists please report it to quin", Logger.Severity.Socket, Logger.Severity.Critical);
                            return;
                        }
                    }
                    break;



            }
        }
    }
}
