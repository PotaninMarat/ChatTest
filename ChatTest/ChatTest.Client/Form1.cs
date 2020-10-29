using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatTest.Client
{
    public partial class Form1 : Form
    {
        readonly string host = "ws://localhost:5000/ws";
        readonly string hostApi = "http://localhost:5000/api";
        ChatClient client;
        bool authorized; // if connection authorized
        bool newRequest; // if get response on request
        bool IsInitial; // if winform show
        Encoding encoding;
        public Form1()
        {
            InitializeComponent();

            encoding = Encoding.ASCII;

            client = new ChatClient(host);
            if (!client.Connect())
                throw new Exception("Connection failed");
            client.OnMessage += Client_OnMessage;

            Login();

            IsInitial = true;
        }

        private void Client_OnMessage(WebSocketReceiveResult result, byte[] buffer)
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var text = encoding.GetString(buffer, 0, result.Count);
                if (text != "Error login")
                    authorized = true;

                if (IsInitial)
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text += text + "\n"));
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                client.CloseOutput();
            }

            newRequest = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            var text = richTextBox2.Text;
            if(string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Empy text");
                return;
            }

            client.Send(text);
        }

        private void Login()
        {
            bool isCancel = false; // if login cancel
            FormLogin formLogin;
            while (!authorized && !isCancel)
            {
                formLogin = new FormLogin();
                formLogin.ShowDialog();
                if (formLogin.IsRegister)
                {
                    var formRegister = new FormRegister();
                    formRegister.ShowDialog();
                    var client = new RestClient(hostApi + "/Account/Register");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", "{\r\n    \"Login\": \""+ formRegister.Login + "\",\r\n    \"Password\": \"" + formRegister.Password + "\",\r\n    \"Name\": \"" + formRegister.Name + "\"\r\n}", ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);

                    MessageBox.Show(response.Content);
                }
                else
                {
                    // create login model
                    var json = JsonConvert.SerializeObject(new
                    {
                        formLogin.Login,
                        formLogin.Password
                    });

                    newRequest = true;

                    client.Send(json);

                    //Wait
                    while (newRequest)
                    {
                        Thread.Sleep(400);
                    }

                }

                isCancel = formLogin.Cancel;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Dispose();
        }
    }
}
