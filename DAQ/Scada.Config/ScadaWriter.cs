using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Config
{
    public class ScadaWriter : IDisposable
    {
        private string fileName;

        private Dictionary<string, IValue> changes = new Dictionary<string, IValue>();

        public ScadaWriter(string fileName)
        {
            this.fileName = fileName;
        }

        public void WriteLine(string key, string value)
        {
            this.WriteKeyValueLine(key, new StringValue(value));    
        }

        public void WriteKeyValueLine(string key, IValue value)
        {
            changes.Add(key, value);
        }

        public void Commit()
        {
            List<string> lines = new List<string>();
            if (File.Exists(this.fileName))
            {
                using (StreamReader streamReader = new StreamReader(this.fileName))
                {
                    while (true)
                    {
                        string line = streamReader.ReadLine();
                        if (line != null)
                        {
                            lines.Add(line);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                try
                {
                    File.Copy(this.fileName, this.fileName + ".bank", true);
                    string tempFileName = this.fileName + ".temp";
                    using (StreamWriter sw = new StreamWriter(tempFileName))
                    {
                        foreach (var line in lines)
                        {
                            int assignPos = line.IndexOf("=");
                            if (assignPos > 0)
                            {
                                string key = line.Substring(0, assignPos).Trim();

                                if (changes.Keys.Contains(key))
                                {
                                    IValue value = changes[key];
                                    string newLine = string.Format("{0} = {1}", key, value.ToString());
                                    sw.WriteLine(newLine);
                                }
                                else
                                {
                                    sw.WriteLine(line);
                                }

                            }
                            else
                            {
                                sw.WriteLine(line);
                            }

                        }
                    }

                    File.Delete(this.fileName);
                    File.Move(tempFileName, this.fileName);
                }
                catch (Exception)
                {

                }

            }
        }

        public void Dispose()
        {
        }
    }
}
