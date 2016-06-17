using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;


namespace TCPClient
{
    class MessageHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private State state;
        private readonly char beginDelimiter;
        private readonly char endDelimiter;
        private string received;
        private volatile bool receiving;
        private string identifier;

        public bool Connected { get; private set; }
        private string lastCommand;
        public event MessageReceivedDelegate MessageReceived;
        public delegate void MessageReceivedDelegate(object sender, MessageReceivedEventArgs e);

        private const int MaxCommandLength = 1024;

        private BackgroundWorker bwMessageListener;

        private enum State
        {
            waiting,
            receiving
        }

        public MessageHandler(TcpClient tcpclient, char beginDelimiter = '%', char endDelimiter = '$')
        {
            this.client = tcpclient;
            this.state = State.waiting;
            this.beginDelimiter = beginDelimiter;
            this.endDelimiter = endDelimiter;
            this.received = string.Empty;
            this.lastCommand = string.Empty;
            this.receiving = false;
            this.bwMessageListener = new BackgroundWorker();
            this.bwMessageListener.DoWork += ListenForCommandsBw;
            this.bwMessageListener.ProgressChanged += bwMessageListener_ReportProgress;
        }

        private void bwMessageListener_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            string rawCommand = e.UserState as string;
            if (rawCommand == null)
            {
                return;
            }
            Command command = new Command(rawCommand);
            if (identifier == null)
            {
                if (command.command == Command.id)
                {
                    identifier = command.parameter;
                }
                else
                {
                    SendMessage("INEEDIDENTIFIERPLS");
                    // probleem, er is niet als eerste een identifier gestuurd (of hij wordt verkeerd geparsed)
                }
                return;
            }
            OnMessageReceived(new MessageReceivedEventArgs(command.command, identifier));
        }

        public void ListenForCommands()
        {
            bwMessageListener.RunWorkerAsync();
        }

        private void ListenForCommandsBw(object sender, DoWorkEventArgs e)
        {
            while (receiving)
            {
                while (stream?.DataAvailable ?? false)
                {
                    byte[] bytes = new byte[1];
                    stream.Read(bytes, 0, bytes.Length);
                    char[] incoming = Encoding.ASCII.GetChars(bytes);
                    switch (state)
                    {
                        case State.waiting:
                            if (incoming[0] == beginDelimiter)
                            {
                                received = string.Empty;
                                state = State.receiving;
                            }
                            break;
                        case State.receiving:
                            if (incoming[0] != endDelimiter)
                            {
                                received += incoming[0];
                            }
                            else
                            {
                                lastCommand = received;
                                received = string.Empty;
                                state = State.waiting;
                                BackgroundWorker worker = (BackgroundWorker)sender;
                                worker.WorkerReportsProgress = true;
                                worker.ReportProgress(0, lastCommand);
                            }
                            if (received.Length > MaxCommandLength)
                            {
                                throw new LengthException();
                            }
                            break;
                        default:
                            throw new ArgumentException(nameof(state));
                    }
                }
                Thread.Sleep(1);
            }
        }

        public void SendMessage(string message)
        {
            if (stream == null || Connected == false)
            {
                throw new InvalidOperationException("can not send message when not connected");
            }
            string messageWithDelimiters = $"{beginDelimiter}{message}{endDelimiter}";
            if (messageWithDelimiters.Length > MaxCommandLength)
            {
                throw new LengthException();
            }
            byte[] messageBytes = Encoding.ASCII.GetBytes(messageWithDelimiters);
            stream.Write(messageBytes, 0, messageBytes.Length);
        }

        public void Disconnect()
        {
            stream.Close();
            client.Close();
            Connected = false;
            receiving = false;
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }
    }

    public class LengthException : Exception
    {

        public LengthException()
        {
        }

        public LengthException(string message) : base(message)
        {
        }

        public LengthException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Identifier { get; set; }

        public MessageReceivedEventArgs(string message, string identifier)
        {
            Message = message;
            Identifier = identifier;
        }
    }
}
