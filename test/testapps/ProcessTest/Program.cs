using System;
using System.Diagnostics;
using System.IO;

namespace ProcessTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dotnetPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet";
            var startInfo = new ProcessStartInfo()
            {
                FileName = dotnetPath,
                Arguments = "--version",
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            if (process.WaitForExit(10000))
            {
                Console.WriteLine($"Process {process.Id} exited cleanly");
            }
            else
            {
                Console.WriteLine($"Process {process.Id} failed to exit. HasExited: {process.HasExited}");
            }
        }
    }
}
