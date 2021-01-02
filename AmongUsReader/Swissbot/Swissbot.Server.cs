using AmongUsReader.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public partial class Swissbot
    {
        public static string UserAuth { get; set; }
        private HttpListener listener;
        private WebSocket browserSocket { get; set; }

        public Swissbot()
        {
            listener = new HttpListener();

            listener.Prefixes.Add($"http://localhost:8777/swissbot/amongus/socket/");

            listener.Start();

            Task.Run(() => HandleHttp().ConfigureAwait(false));
            Task.Run(() => Init().ConfigureAwait(false));
        }

        private async Task Init()
        {
            await this.CreateSwissbotClient();

        }

        private async Task HandleHttp()
        {
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync();

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    continue;
                }

                var socketContext = await context.AcceptWebSocketAsync(null);

                Task.Run(() => HandleBrowser(socketContext.WebSocket).ConfigureAwait(false));
            }
        }

        private async Task SendToBrowser(IBrowserSendable packet)
        {
            if (!IsConnected)
                return;

            string content = JsonConvert.SerializeObject(packet);

            await browserSocket.SendAsync(Encoding.UTF8.GetBytes(content), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private async Task SendToBrowser(IBrowserSendable packet, WebSocket socket)
        {
            if (!IsConnected)
                return;

            string content = JsonConvert.SerializeObject(packet);

            await socket.SendAsync(Encoding.UTF8.GetBytes(content), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task HandleBrowser(WebSocket socket)
        {
            byte[] buffer = new byte[1024];

            try
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if(result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    socket.Dispose();
                    return;
                }

                BrowserHandshake handshake = JsonConvert.DeserializeObject<BrowserHandshake>(Encoding.UTF8.GetString(buffer));

                var credentials = await ValidateCredentials(new Packets.ValidateCredentials(handshake.session));

                if (credentials.isValid)
                {
                    UserAuth = handshake.session;
                    SwissbotAuthToken = credentials.auth;

                    this.browserSocket = socket;

                    await SendToBrowser(new BrowserHandshakeResult().Accept());


                }
            }   
            catch(Exception x)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, null, CancellationToken.None);
                socket.Dispose();
                return;
            }
        }
    }
}
