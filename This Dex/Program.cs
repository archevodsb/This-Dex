using System;
using System.IO;
using System.Net;
using System.Security.Principal;

class Program
{
    static void Main()
    {
        string[] linesToAdd = new string[]
        {
            "0.0.0.0 functions3.ultraviewer.net",
            "0.0.0.0 global.iperius-rs.com",
            "0.0.0.0 eu-rs1.iperius.com",
            "0.0.0.0 dispatcher.nanosystems.it",
            "0.0.0.0 alpemix.com",
            "0.0.0.0 www.alpemix.com",
            "127.0.0.1 www.ultraviewer.net",
            "127.0.0.1 www.alpemix.com"
        };

        string hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";
        if (IsAdministrator())
        {
            try
            {
                // Read the existing contents of the hosts file
                string hostsFileContent = File.ReadAllText(hostsFilePath);

                // Append new lines to the hosts file content if they don't already exist
                foreach (string line in linesToAdd)
                {
                    if (!hostsFileContent.Contains(line))
                    {
                        hostsFileContent += Environment.NewLine + line;
                    }
                }

                // Write the updated content back to the hosts file
                File.WriteAllText(hostsFilePath, hostsFileContent);

                Console.WriteLine("Lines successfully added to the hosts file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("This program must be run as an administrator.");
        }
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataPath, "ult", "index.html");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://*:80/");
        listener.Start();
        Console.WriteLine("Server is listening on port 80...");
        while (true)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/")
                {
                    byte[] buffer = File.ReadAllBytes(filePath);
                    response.ContentType = "text/html";
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    static bool IsAdministrator()
    {
        // Check if the current user has administrative privileges
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}