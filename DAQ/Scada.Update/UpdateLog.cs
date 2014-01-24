using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Update
{
    class UpdateLog
    {
        private static UpdateLog logger = new UpdateLog();

        public static UpdateLog Instance()
        {
            return logger;
        }

        public List<string> names = new List<string>();

        public void AddName(string name)
        {
            names.Add(name);
        }

        public void Dump(string fileName)
        {
            using (FileStream fw = new FileStream(fileName, FileMode.Create))
            {
                foreach (var name in this.names)
                {
                    string line = name + "\n";
                    var bs = Encoding.UTF8.GetBytes(line);
                    fw.Write(bs, 0, bs.Length);
                }
            }
        }
    }
}
