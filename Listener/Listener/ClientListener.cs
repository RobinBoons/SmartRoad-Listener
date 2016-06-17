using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;

namespace TCPClient
{
    class ClientListener
    {
        private List<MessageHandler> messageHandlers;
        private volatile bool acceptingClients = false;
        private BackgroundWorker bwClientAccepter;
        private TcpListener tcpListener;

        ClientListener(int port)
        {
            messageHandlers = new List<MessageHandler>();
            bwClientAccepter.DoWork += BwClientAccepter_DoWork;
            bwClientAccepter.ProgressChanged += BwClientAccepter_ProgressChanged;
            tcpListener = new TcpListener(IPAddress.Any, port);
        }

        public void StartAccepting()
        {
            acceptingClients = true;
        }

        public void StopAccepting()
        {
            acceptingClients = false;
        }
        private void BwClientAccepter_ProgressChanged(object sender, ProgressChangedEventArgs e) { 
            TcpClient client = e.UserState as TcpClient;
            if (client == null)
            {
                return;
            }
            messageHandlers.Add(new MessageHandler(client));
            messageHandlers.Last().MessageReceived += ClientListener_MessageReceived;
            messageHandlers.Last().ListenForCommands();
        }

        private void ClientListener_MessageReceived(object sender, MessageReceivedEventArgs e)
        {

        }

        private void BwClientAccepter_DoWork(object sender, DoWorkEventArgs e)
        {
            while (acceptingClients)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                if (sender is BackgroundWorker)
                {
                    BackgroundWorker thisBw = sender as BackgroundWorker;
                    thisBw.ReportProgress(0, tcpClient);
                }
            }
        }
    }
}
