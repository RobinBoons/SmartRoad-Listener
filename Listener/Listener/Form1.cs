using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Net.Sockets;

namespace TCPClient
{
    public partial class Form1 : Form
    {
        private MessageHandler messageHandler;
        private Task connectionTask;
        private BackgroundWorker bwClientAcception;

        public Form1()
        {
            InitializeComponent();
            //messageHandler = new MessageHandler();
            connectionTask = null;
            //messageHandler.MessageReceived += messageHandler_MessageReceived;
        }

        private void RfidManagerOnCollectionChanged(object sender, EventArgs e)
        {
            //als uitbreiding een lijst bijhouden met welke RFID's je binnen hebt gekregen.
            ;
        }
    }
}
