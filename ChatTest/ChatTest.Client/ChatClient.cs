using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatTest.Client
{
    public class ChatClient : IDisposable
    {
        private readonly ClientWebSocket _client;
        private readonly Uri _host;
        private readonly int bufferSize;
        private readonly Encoding _encoding;
        private Task ReceiveTask { get; set; }

        public event Action<WebSocketReceiveResult, byte[]> OnMessage;

        public ChatClient(string host)
        {
            _client = new ClientWebSocket();
            _host = new Uri(host);
            bufferSize = 4 * 1024;
            _encoding = Encoding.ASCII;
        }

        public bool Connect()
        {
            var task = Task.Run(async () => await _client.ConnectAsync(_host, CancellationToken.None));
            task.Wait();

            ReceiveTask = Task.Run(async () => await ReceivingAsync());

            return task.Status == TaskStatus.RanToCompletion;
        }

        private async Task ReceivingAsync()
        {
            while (true)
            {
                try
                {
                    var buffer = new byte[bufferSize];
                    var arraySegment = new ArraySegment<byte>(buffer);
                    var result = await _client.ReceiveAsync(arraySegment, CancellationToken.None);

                    OnMessage(result, buffer);
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        public void CloseOutput()
        {
            Task.Run(async()=>await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None))
                .Wait();
        }

        public void Send(string message)
        {
            var bytes = _encoding.GetBytes(message);
            Task.Run(async () => await _client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None))
                .Wait();
        }

        public void Dispose()
        {
            ReceiveTask.Dispose();
            _client.Dispose();

            ReceiveTask = null;
        }
    }
}
