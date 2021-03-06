﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Cliver.CisteraScreenCapture
{
    public partial class SettingsWindow : Window
    {
        SettingsWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);

            Icon = AssemblyRoutines.GetAppIconImageSource();

            ContentRendered += delegate
            {
                this.MinHeight = this.ActualHeight;
                this.MaxHeight = this.ActualHeight;
                this.MinWidth = this.ActualWidth;
            };

            IsVisibleChanged += (object sender, DependencyPropertyChangedEventArgs e) =>
            {
                if (Visibility == Visibility.Visible)
                {
                    DoubleAnimation da = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                    this.BeginAnimation(UIElement.OpacityProperty, da);
                }
            };

            Closing += (object sender, System.ComponentModel.CancelEventArgs e) =>
            {
                if (Opacity > 0)
                {
                    e.Cancel = true;
                    DoubleAnimation da = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    da.Completed += delegate { Close(); };
                    this.BeginAnimation(UIElement.OpacityProperty, da);
                }
            };

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //DefaultServerIp.ValueDataType = typeof(IPAddress);

            set();
        }

        void set()
        { 
            //ServerDefaultPort.Text = Settings.General.TcpClientDefaultPort.ToString();
            ServerDefaultIp.Text = Settings.General.TcpClientDefaultIp.ToString();
            ClientPort.Text = Settings.General.TcpServerPort.ToString();
            ServiceDomain.Text = Settings.General.ServiceDomain;
            ServiceType.Text = Settings.General.ServiceType;

            //using (ManagementObjectSearcher monitors = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
            //{
            //    foreach (ManagementObject monitor in monitors.Get())
            //    {
            //        MonitorName.Items.Add(monitor["Name"].ToString() + "|" + monitor["DeviceId"].ToString());// + "(" + monitor["ScreenHeight"].ToString() +"x"+ monitor["ScreenWidth"].ToString() + ")");
            //    }
            //}
            //foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            //{
            //    // For each screen, add the screen properties to a list box.
            //    MonitorName.Items.Add("Device Name: " + screen.DeviceName);
            //    MonitorName.Items.Add("Bounds: " + screen.Bounds.ToString());
            //    //MonitorName.Items.Add("Type: " + screen.GetType().ToString());
            //    //MonitorName.Items.Add("Working Area: " + screen.WorkingArea.ToString());
            //    MonitorName.Items.Add("Primary Screen: " + screen.Primary.ToString());
            //}
            Monitors.DisplayMemberPath = "Text";
            Monitors.SelectedValuePath = "Value";
            foreach (MonitorRoutines.MonitorInfo mi in MonitorRoutines.GetMonitorInfos())
            {
                Monitors.Items.Add(new
                {
                    Text = mi.DeviceString + " (" + (mi.Area.Bottom - mi.Area.Top) + "x" + (mi.Area.Right - mi.Area.Left) + ")",
                    Value = mi.DeviceName
                });
            }
            if (Monitors.Items.Count > 0)
                if (!string.IsNullOrWhiteSpace(Settings.General.CapturedMonitorDeviceName))
                    Monitors.SelectedValue = Settings.General.CapturedMonitorDeviceName;
                else
                    Monitors.SelectedIndex = 0;
            
            ShowMpegWindow.IsChecked = Settings.General.ShowMpegWindow;
            WriteMpegOutput2Log.IsChecked = Settings.General.WriteMpegOutput2Log;
        }

        static public void Open()
        {
            if (w == null)
            {
                w = new SettingsWindow();
                w.Closed += delegate 
                {
                    w = null;
                };
            }
            w.Show();
            w.Activate();
        }
        static SettingsWindow w = null;

        void close(object sender, EventArgs e)
        {
            Close();
        }

        void save(object sender, EventArgs e)
        {
            try
            {
                ushort v;

                //if (!ushort.TryParse(ServerDefaultPort.Text, out v))
                //    throw new Exception("Server port must be an integer between 0 and " + ushort.MaxValue);
                //Settings.General.TcpClientDefaultPort = v;

                if (string.IsNullOrWhiteSpace(ServerDefaultIp.Text))
                    throw new Exception("Default server ip is not specified.");
                IPAddress ia;
                if (!IPAddress.TryParse(ServerDefaultIp.Text, out ia))
                    throw new Exception("Default server ip is not a valid value.");
                Settings.General.TcpClientDefaultIp = ia.ToString();

                if (!ushort.TryParse(ClientPort.Text, out v))
                    throw new Exception("Client port must be an between 0 and " + ushort.MaxValue);
                Settings.General.TcpServerPort = v;

                if (string.IsNullOrWhiteSpace(ServiceDomain.Text))
                    throw new Exception("Service domian is not specified.");
                Settings.General.ServiceDomain = ServiceDomain.Text.Trim();

                if (string.IsNullOrWhiteSpace(ServiceType.Text))
                    throw new Exception("Service type is not specified.");
                Settings.General.ServiceType = ServiceType.Text.Trim();

                if (Monitors.SelectedIndex < 0)
                    throw new Exception("Captured Video Source is not specified.");
                Settings.General.CapturedMonitorDeviceName = (string)Monitors.SelectedValue;

                Settings.General.ShowMpegWindow = ShowMpegWindow.IsChecked ?? false;

                Settings.General.WriteMpegOutput2Log = WriteMpegOutput2Log.IsChecked ?? false;

                Settings.General.Save();
                Config.Reload();

                if (Message.YesNo("The last changes have been saved. However, to engage them, the service must be restarted. All the present connections if any will be broken. Proceed with restarting?", null, Message.Icons.Exclamation))
                {
                    bool running = Service.Running;
                    Service.Running = false;
                    Service.Running = running;
                }

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }

        //class ValidationException:Exception
        //{

        //}
               
        void reset_settings(object sender, RoutedEventArgs e)
        {
            if (!Message.YesNo("Do you want to reset settings to their initial state?"))
                return;
            Settings.General.Reset();
            set();
        }
    }
}
