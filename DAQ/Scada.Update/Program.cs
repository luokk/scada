using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scada.Update
{
    class Program
    {
        public static string GetCurrentPath()
        {
            string p = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(p);
        }

        static void Main(string[] args)
        {
            // Debug.Assert(false);
            if (args.Length == 0)
                return;

            Updater u = new Updater();
            u.ForceReplaceConfigFiles = false;
            u.NeedUpdateConfigFiles = false;
            
            // TODO:
            KillProcesses();
            string binZipPath = args[0];
            bool r = u.UnzipProgramFiles(binZipPath, Path.GetDirectoryName(GetCurrentPath()));
            if (!r)
            {
                Console.WriteLine("Failed to update!");
            }
            // TODO:
            RestoreProcesses();
        }


        static void KillProcesses()
        {

        }

        static void RestoreProcesses()
        {

        }

    }
}
