﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using Cliver.CisteraScreenCapture;

namespace Cliver.CisteraScreenCaptureTestServer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, int.Parse(localPort.Text));
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
        }
        Socket socket;

        private void start_Click(object sender, EventArgs e)
        {
            socket.Connect(remoteHost.Text, int.Parse(remotePort.Text));

            TcpMessage m = new TcpMessage(TcpMessage.FfmpegStart, "test 123");
            TcpMessage m2 = m.SendAndReceiveReply(socket);
            
            socket.Disconnect(true);
        }

        private void stop_Click(object sender, EventArgs e)
        {
            socket.Connect(remoteHost.Text, int.Parse(remotePort.Text));

            TcpMessage m = new TcpMessage(TcpMessage.FfmpegStop, null);
            TcpMessage m2 = m.SendAndReceiveReply(socket);

            socket.Disconnect(true);
        }
    }
}