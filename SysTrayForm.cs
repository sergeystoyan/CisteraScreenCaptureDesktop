﻿//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Zeroconf;

namespace Cliver.CisteraScreenCapture
{
    public partial class SysTray : Form
    {
        SysTray()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIcon();
            Service.StateChanged += delegate
            {
                if (!IsHandleCreated)
                    CreateHandle();
                this.Invoke(() => { StartStop.Checked = Service.Running; });
                if (Service.Running)
                    notifyIcon.Icon = Icon;
                else
                    //notifyIcon.Icon = Icon.FromHandle(ImageRoutines.GetGreyScale(Icon.ToBitmap()).GetHicon());
                    notifyIcon.Icon = Icon.FromHandle(ImageRoutines.GetInverted(Icon.ToBitmap()).GetHicon());
            };
        }

        public static readonly SysTray This = new SysTray();

        bool isAllowed()
        {
            if (WindowsUserRoutines.CurrentUserIsAdministrator())
                return true;
            Message.Exclaim("This action is permitted for Administrators only.");
            return false;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            settingsToolStripMenuItem_Click(null, null);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isAllowed())
                return;
            SettingsWindow.Open();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm.Open();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isAllowed())
                return;
            //Program.Exit();
            Environment.Exit(0);
        }

        private void SysTray_VisibleChanged(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void notifyIcon1_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void StartStop_CheckedChanged(object sender, EventArgs e)
        {
            //Service.Running = StartStop.Checked;
        }

        private void workDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Log.WorkDir);
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            //settingsToolStripMenuItem_Click(null, null);
        }

        private void StartStop_Click(object sender, EventArgs e)
        {
            if (!isAllowed())
            {
                StartStop.Checked = Service.Running;
                return;
            }
            Service.Running = StartStop.Checked;
        }

        async private void stateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> ls = new List<string>();
            ls.Add("Monitor: " + (Service.Running ? "started" : "stopped"));
            ls.Add("Logged in user: " + WindowsUserRoutines.GetUserName());

            //var domains = await ZeroconfResolver.BrowseDomainsAsync();
            //var responses = await ZeroconfResolver.ResolveAsync(domains.Select(g => g.Key));
            //IReadOnlyList<IZeroconfHost> zhs = await ZeroconfResolver.ResolveAsync("_printer._tcp.local.");//worked for server: "_printer._tcp"
            string service = Settings.General.ServiceType + Settings.General.ServiceDomain;
            IReadOnlyList<IZeroconfHost> zhs = await ZeroconfResolver.ResolveAsync(service);
            if (zhs.Count < 1)
                ls.Add("Service '" + service + "' could not be resolved.");
            else
                ls.Add("Service '" + service + "' has been resolved to: " + zhs[0].IPAddress);

            if (!TcpServer.Running)
                ls.Add("Tcp listening: -");
            else
                ls.Add("Tcp listening on: " + TcpServer.LocalIp + ":" + TcpServer.LocalPort);

            if (TcpServer.Connection == null)
                ls.Add("Tcp connection: -");
            else
                ls.Add("Tcp connection to: " + TcpServer.Connection.RemoteIp + ":" + TcpServer.Connection.RemotePort);

            if (!MpegStream.Running)
                ls.Add("Mpeg stream: -");
            else
                ls.Add("Mpeg stream: " + MpegStream.CommandLine);

            Message.Inform(string.Join("\r\n\r\n", ls));
        }
    }
}