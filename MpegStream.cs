﻿//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using System.Net.Http;
using Zeroconf;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cliver.CisteraScreenCapture
{
    public class MpegStream
    {
        static MpegStream()
        {
        }

        public static bool Running
        {
            get
            {
                return mpeg_stream_process != null; 
            }
        }

        public static void Start(string arguments)
        {
            if (antiZombieJob == null)
                antiZombieJob = new ProcessRoutines.AntiZombieJob();
            if (mpeg_stream_process != null)
                try
                {
                    ProcessRoutines.KillProcessTree(mpeg_stream_process.Id);
                }
                catch (Exception e)
                {
                    Log.Warning(e);
                }

            int x = 0, y = 0, w = 0, h = 0;
            Win32.MonitorEnumDelegate callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32.RECT lprcMonitor, IntPtr dwData) =>
            {
                Win32.MONITORINFOEX mi = new Win32.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi.GetType());
                if (Win32.GetMonitorInfo(hMonitor, ref mi) && mi.DeviceName != Settings.General.CapturedMonitorDeviceName)
                {
                    x = lprcMonitor.Left;
                    y = lprcMonitor.Top;
                    w = lprcMonitor.Right - lprcMonitor.Left;
                    h = lprcMonitor.Bottom - lprcMonitor.Top;
                    return false;
                }
                return true;
            };
            Win32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            string source = " -offset_x " + x + " -offset_y " + y + " -video_size " + w + "x" + h + " -show_region 1 -i desktop ";
            arguments = Regex.Replace(arguments, @"-framerate\s+\d+", "$0" + source);

            Log.Inform("Launching:\r\n" + "ffmpeg " + arguments);

            mpeg_stream_process = new Process();
            mpeg_stream_process.StartInfo = new ProcessStartInfo("ffmpeg.exe", arguments)
            {
                ErrorDialog = false,
                UseShellExecute = false,
                CreateNoWindow = !Settings.General.ShowMpegWindow
                //WindowStyle = ProcessWindowStyle.Hidden;
            };
            if (Settings.General.WriteMpegOutput2Log)
            {
                mpeg_stream_process.StartInfo.RedirectStandardOutput = true;
                mpeg_stream_process.StartInfo.RedirectStandardError = true;

                string file0 = Log.WorkDir + "ffmpeg_" + DateTime.Now.ToString("yyMMddHHmmss");
                string file = file0;
                for (int count = 1; File.Exists(file); count++)
                    file = file0 + "_" + count.ToString();
                TextWriter tw = new StreamWriter(file, false);
                mpeg_stream_process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    tw.Write(e.Data);
                };
                mpeg_stream_process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    tw.Write(e.Data);
                };
            }
            mpeg_stream_process.Start();
            antiZombieJob.MakeProcessLiveNoLongerThanJob(mpeg_stream_process);
        }
        static Process mpeg_stream_process = null;
        static ProcessRoutines.AntiZombieJob antiZombieJob = null;

        public static void Stop()
        {
            if (mpeg_stream_process != null)
            {
                ProcessRoutines.KillProcessTree(mpeg_stream_process.Id);
                mpeg_stream_process = null;
            }
            if (antiZombieJob != null)
            {
                antiZombieJob.Dispose();
                antiZombieJob = null;
            }
        }
    }
}