using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Scada.Update
{
    public enum WriteFileResult
    {
        SameFile,
        DiffFile
    }

    public class Updater
    {
        // ~
        private string destPath;

        private bool force = false;

        public Updater()
        {            
        }

        public bool ForceReplaceConfigFiles
        {
            private get
            {
                return this.force;
            }
            set
            {
                this.force = value;
            }
        }

        public bool NeedUpdateConfigFiles
        {
            get;
            set;
        }

        public bool UnzipProgramFiles(string programZipFile, string destPath)
        {
            if (!File.Exists(programZipFile))
            {
                // Error: bin.zip NOT found.
                return false;
            }
            this.destPath = destPath;
            Zip zip = new Zip();
            string errorMessage;
            bool ret = zip.UnZipFile(programZipFile, destPath, UnzipFileHandler, out errorMessage);

            string logFileName = string.Format("logs\\{0}.ulog", DateTime.Now.ToString("yyyy_MM_dd_HH_mm"));
            string logFile = Path.Combine(Program.GetCurrentPath(), logFileName);
            UpdateLog.Instance().Dump(logFile);
            return ret;
        }

        private UnzipCode UnzipFileHandler(string fileName, Stream fileStream)
        {
            fileName = fileName.ToLower();
            if (fileName.EndsWith(".cfg") ||
                fileName.EndsWith("local.ip") ||
                fileName.EndsWith("password") ||
                fileName.EndsWith(".bat") ||
                fileName.EndsWith(".settings") ||
                fileName.EndsWith(".sql"))
            {
                if (this.NeedUpdateConfigFiles)
                {
                    if (fileStream != null)
                    {
                        WriteFile(fileStream, this.destPath + "\\" + fileName);
                    }
                    return UnzipCode.Compare;
                }
                return UnzipCode.Ignore;
            }

            if (fileName.EndsWith("scada.update.exe") || 
                fileName.EndsWith("scada.watch.exe") ||
                fileName.EndsWith("icsharpcode.sharpziplib.dll"))
            {
                Console.WriteLine("File <" + fileName + "> In use:!");
                UpdateLog.Instance().AddName(fileName + " <iu>");
                return UnzipCode.Ignore;
            }

            return UnzipCode.None;
        }

        private WriteFileResult WriteFile(Stream stream, string fileName)
        {
            string nzFileName = fileName;
            if (File.Exists(fileName) && !this.force)
            {
                nzFileName += ".n!";
            }
            
            using (FileStream streamWriter = File.Create(nzFileName))
            {
                stream.Seek(0, SeekOrigin.Begin);
                byte[] buffer = new byte[stream.Length];
                while (true)
                {
                    int r = stream.Read(buffer, 0, buffer.Length);
                    if (r > 0)
                    {
                        streamWriter.Write(buffer, 0, r);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (fileName == nzFileName)
            {
                return WriteFileResult.SameFile;
            }

            HashAlgorithm hash = HashAlgorithm.Create();
            if (GetFileHashString(fileName, hash) == GetFileHashString(nzFileName, hash))
            {
                File.Delete(nzFileName);
                return WriteFileResult.SameFile;
            }
            return WriteFileResult.DiffFile;
        }

        private string GetFileHashString(string fileName, HashAlgorithm hash)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                byte[] hashbyte = hash.ComputeHash(stream);
                return BitConverter.ToString(hashbyte);
            }
        }

        private string GetUpdateBinZipPath()
        {
            string p = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(p);
        }

    }
}
