using System;
using Microsoft.Scripting.Hosting.Shell;
using Omnia.Hosting;

namespace Omnia.Console
{
    class Program : ConsoleHost
    {
        static void Main(string[] args)
        {
            new Program().Run(new string[0]);
        }

        protected override CommandLine CreateCommandLine()
        {
            return new OmniaCommandLine();
        }

        protected override Type Provider
        {
            get { return typeof(OmniaContext); }
        }

        protected override void ParseHostOptions(string[] args)
        {
            foreach (string s in args)
            {
                Options.IgnoredArgs.Add(s);
            }
        }
    }

    class OmniaCommandLine : CommandLine
    {
        protected override string Prompt { get { return "$> "; } }
        
        protected override string Logo
        {
            get
            {
                var version = typeof(OmniaContext).Assembly.GetName().Version.ToString();
                return "Omnia v" + version + "\n\n";
            }
        }
    }
}
