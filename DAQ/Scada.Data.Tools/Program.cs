using Scada.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Scada.Data.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "--init-database" };
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Args required");
            }

            int interval = 10;
            if (args.Length > 1)
            {
                string opt = args[1].Trim();
                if (opt.StartsWith("-i="))
                {
                    string v = opt.Substring(3);
                    int.TryParse(v.Trim(), out interval);
                }

            }

            string fa = args[0].ToLower();
            if (fa == "--init-database")
            {
                InitDataBase(args);
            }
            else if (fa == "--init-database-s")
            {
                // Debug.Assert(false);
                InitDataBaseSilent(args);
            }
            else if (fa == "--import-hpic")
            {
                ImportData(args);
            }
            else if (fa == "--m-hpic")
            {
                MockInsertData("hpic", interval);
            }
            else if (fa == "--m-weather")
            {
                MockInsertData("weather", interval);
            }
            else if (fa == "--m-shelter")
            {
                MockInsertData("shelter", interval);
            }
            else if (fa == "--m-nai")
            {
                MockInsertData("nai", interval);
            }
            else if (fa == "--m-dwd")
            {
                MockInsertData("dwd", interval);
            }
            else if (fa == "--m-mds")
            {
                MockInsertData("mds", interval);
            }
            else if (fa == "--m-ais")
            {
                MockInsertData("ais", interval);
            }

        }

        private static void MockCreateNaIFiles()
        {
            // throw new NotImplementedException();
        }

        private static string GetScadaSqlFile()
        {
            Type type = typeof(Program);
            string fn = type.Assembly.Location;
            string sqlFileName = ConfigPath.GetConfigFilePath("scada.sql");
            return sqlFileName;
        }

        static void InitDataBase(string[] args)
        {
            Console.WriteLine("Initialize the DataBase:");
            Console.WriteLine("Notice: This Command would clear all you record in tables!");
            Console.WriteLine("Tap 'Yes' to continue.");
            string input = Console.ReadLine();
            if (input == "Yes")
            {
                DataBaseCreator creator = new DataBaseCreator(GetScadaSqlFile());
                creator.Execute();
            }
        }

        static void InitDataBaseSilent(string[] args)
        {
            DataBaseCreator creator = new DataBaseCreator(GetScadaSqlFile());
            creator.Execute();
        }

        static void ImportData(string[] args)
        {
            string csvFile = args[1];
            DataBaseImporter creator = new DataBaseImporter(csvFile);
            creator.Import();
        }

        static void MockInsertData(string device, int interval)
        {
            DataBaseInsertion ins = new DataBaseInsertion(device);
            ins.RecordInterval = interval;
            ins.Execute();
        }
    }
}
