using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsocketLibrary
{
    /// <summary>
    /// manage all websocket connections
    /// </summary>
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets;
        private readonly Dictionary<string, string> _mapping; // <id, token>
        public ConnectionManager()
        {
            _sockets = new ConcurrentDictionary<string, WebSocket>();
            _mapping = new Dictionary<string, string>();
        }

        public WebSocket GetById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public void AddMapping(string id, string name)
        {
            _mapping.Add(id, name);
        }

        public async Task<bool> SendAsync(string id, string message, Encoding encoding)
        {
            WebSocket socket;
            _sockets.TryGetValue(id, out socket);

            var buffer = new ArraySegment<byte>(array: encoding.GetBytes(message),
                                                                   offset: 0,
                                                                   count: message.Length);

            await socket.SendAsync(buffer,
                                    messageType: WebSocketMessageType.Text,
                                    endOfMessage: true,
                                    cancellationToken: CancellationToken.None);

            return true;
        }

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public void Add(WebSocket socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        public async Task RemoveSocket(string id)
        {
            WebSocket socket;
            _sockets.TryRemove(id, out socket);
            _mapping.Remove(id);
            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by the ConnectionManager",
                                    cancellationToken: CancellationToken.None);
        }

        public string GetName(string id)
        {
            string name;
            _mapping.TryGetValue(id, out name);

            return name;
        }

        public bool Authorized(string id)
        {
            return _mapping.ContainsKey(id);
        }

        /// <summary>
        /// Create new guid
        /// </summary>
        /// <returns></returns>
        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
