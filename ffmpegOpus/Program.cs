using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ffmpegOpus
{
    class Program
    {
        private static readonly Stack<Process> s_processes = new Stack<Process>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            foreach (var arg in args)
            {
                HandleArg(arg);
            }

            var fullCount = s_processes.Count;
            while (s_processes.TryPop(out var p))
            {
                Console.WriteLine($"Active: {s_processes.Count}/{fullCount}");
                p.WaitForExit();
            }
            Console.WriteLine($"Active: {s_processes.Count}/{fullCount}");
            Console.WriteLine("Press return to exit");
            Console.ReadLine();
        }

        private static void HandleArg(string arg)
        {
            var fi = new FileInfo(arg);
            if (fi.Exists)
            {
                ConvertFile(fi);
            }
            else
            {
                var di = new DirectoryInfo(arg);
                if (di.Exists)
                {
                    foreach (var dfi in di.GetFiles("*.flac, *.mp3, *.vorbis"))
                    {
                        ConvertFile(dfi);
                    }
                }
            }
        }

        private static void ConvertFile(FileInfo fi)
        {
            var p = Process.Start("ffmpeg.exe", $@"-hide_banner -loglevel warning -n -nostats -i ""{fi.FullName}"" -b:a 128000 ""{Path.ChangeExtension(fi.FullName, "opus")}""");
            s_processes.Push(p);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Console.WriteLine("Press return to exit");
            Console.ReadLine();
        }
    }
}
