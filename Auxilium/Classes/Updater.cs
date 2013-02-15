using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Auxilium.Classes
{
    class Updater
    {
        public event ProgressChangedEventHandler ProgressChanged;
        public delegate void ProgressChangedEventHandler(int percentage);

        private void OnProgressChanged(int percentage) {
            if (ProgressChanged != null)
                ProgressChanged(percentage);
        }

        private string DownloadUrl { get; set; }

        public Updater(string url)
        {
            DownloadUrl = url;
        }

        public void Initialize()
        {
            WebClient wc = new WebClient() {Proxy = null};
            wc.DownloadProgressChanged += WcOnDownloadProgressChanged;
            wc.DownloadDataCompleted += OnDownloadCompleted;
            wc.DownloadDataAsync(new Uri(DownloadUrl));
        }

        private void WcOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs downloadProgressChangedEventArgs)
        {
            double i = ((double) downloadProgressChangedEventArgs.BytesReceived / (double) downloadProgressChangedEventArgs.TotalBytesToReceive) * 100;
            OnProgressChanged((int)i);
        }

        private void OnDownloadCompleted(object sender, DownloadDataCompletedEventArgs eventArgs)
        {
            if (eventArgs.Error != null)
            {
                throw eventArgs.Error;
            }

            string file = Path.Combine(Application.StartupPath, DownloadUrl.Split('/')[DownloadUrl.Split('/').Length - 1]);

            File.WriteAllBytes(file, eventArgs.Result);

            string batch = string.Format(
                "@echo off{1}" +
                "TASKKILL /F /PID {0}{1}" +
                "PING 1.1.1.1 -n 1 -w 2000 >NUL{1}" +
                "DEL %2{1}" +
                "MOVE %1 %2{1}" +
                "%2{1}" + 
                "DEL %1{1}" +
                "DEL %3", Process.GetCurrentProcess().Id, Environment.NewLine);
            string batchFile = Path.Combine(Application.StartupPath, "update.bat");

            File.WriteAllText(batchFile, batch);

            ProcessStartInfo info = new ProcessStartInfo(batchFile, 
                string.Format("\"{0}\" \"{1}\" \"{2}\"", file, Application.ExecutablePath, batchFile))
                                        {CreateNoWindow = true, UseShellExecute = false};
            Process.Start(info);
        }
    }
}
