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

            string binZipPath = args[0];



            Updater u = new Updater();
            u.ForceReplaceConfigFiles = false;
            u.NeedUpdateConfigFiles = false;

            if (args.Length > 1)
            {
                string opt = args[1];
                if (opt.StartsWith("--"))
                {
                    if (opt.IndexOf('w') > 0)
                    {
                        u.UpdateByWatch = true;
                    }
                }
                
            }
            
            
            // TODO:
            KillProcesses();
            


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
