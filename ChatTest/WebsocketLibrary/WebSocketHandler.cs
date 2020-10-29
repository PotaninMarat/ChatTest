using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketLibrary
{
    public abstract class WebSocketHandler
    {
        protected ConnectionManager ConnectionManager { get; set; }
        private readonly Encoding _encoding;

        public WebSocketHandler(ConnectionManager webSocketConnectionManager, Encoding encoding = null)
        {
            ConnectionManager = webSocketConnectionManager;
            _encoding = encoding ?? Encoding.ASCII;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            ConnectionManager.Add(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await ConnectionManager.RemoveSocket(ConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var id = ConnectionManager.GetId(socket);
            await ConnectionManager.SendAsync(id, message, _encoding);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(ConnectionManager.GetById(socketId), message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in ConnectionManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open && ConnectionManager.Authorized(pair.Key))
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
}
