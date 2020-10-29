using ChatTest.DataBusiness.Models;
using ChatTest.DataBusiness.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebsocketLibrary;

namespace ChatTest.DataBusiness.Handlers
{
    public class ChatMessageHandler : WebSocketHandler
    {
        private readonly ILogger _logger;
        private readonly AccountService _accountService;
        private readonly Encoding _encoding;

        public ChatMessageHandler(IServiceScopeFactory serviceScopeFactory, ILogger<ChatMessageHandler> logger, ConnectionManager webSocketConnectionManager, Encoding encoding = null) : base(webSocketConnectionManager)
        {
            _accountService = serviceScopeFactory.CreateScope().ServiceProvider.GetService<AccountService>();
            _logger = logger;
            _encoding = encoding ?? Encoding.UTF8;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);
            _logger.LogInformation("New connection");
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketId = ConnectionManager.GetId(socket);
            var message = _encoding.GetString(buffer, 0, result.Count);

            //_accountService = serviceScopeFactory.CreateScope().ServiceProvider.GetService<AccountService>();
            if (ConnectionManager.Authorized(socketId))
            {
                var name = ConnectionManager.GetName(socketId);
                var messageView = $"{name}: {message}";

                await SendMessageToAllAsync(messageView);

                _logger.LogInformation("New message");
            }
            else
            {
                //try login
                LoginModel model = TryParseJson<LoginModel>(message);

                if(model != null)
                {
                    var login = _accountService.CheckPassword(model);

                    if (login.Succeeded)
                    {
                        ConnectionManager.AddMapping(socketId, login.Name);

                        await SendMessageAsync(socketId, "Success");
                    }
                    else
                    {
                        await SendMessageAsync(socketId, "Error login");
                    }
                }
            }
        }

        private T TryParseJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch { }

            return default;
        }
    }
}
