using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scada.Update
{
    class Program
    {
        static string GetCurrentPath()
        {
            string p = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(p);
        }

        static void Main(string[] args)
        {
            Updater u = new Updater();
            u.ForceReplaceConfigFiles = false;
            u.NeedUpdateConfigFiles = false;

            string binZipPath = GetCurrentPath() + "\\update\\bin.zip";
            KillProcesses();
            bool r = u.UnzipProgramFiles(binZipPath, GetCurrentPath());
            if (!r)
            {
                Console.WriteLine("Failed to update!");
            }
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
