using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Update
{
    public delegate UnzipCode UnzipHandler(string fileName, Stream stream);

    public enum UnzipCode
    {
        None,
        Ignore,
        Compare
    }

    class Zip
    {
        private byte[] buffer = new byte[1024 * 5];

        public bool UnZipFile(string zipFilePath, string unZipDir, UnzipHandler unzipHandler, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (zipFilePath == string.Empty || !File.Exists(zipFilePath))
            {
                errorMessage = "Zip File Not Found";
                return false;
            }

            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));

            if (!unZipDir.EndsWith("\\"))
                unZipDir += "\\";

            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            try
            {
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        string relFileName = directoryName + "\\" + fileName;
                        UnzipCode code = unzipHandler(relFileName, null);
                        if (directoryName.Length > 0)
                        {
                            string pathName = unZipDir + directoryName;
                            if (!Directory.Exists(pathName))
                            {
                                Directory.CreateDirectory(pathName);
                            }
                        }

                        if (!directoryName.EndsWith("\\"))
                            directoryName += "\\";
                        if (fileName != String.Empty)
                        {
                            MemoryStream ms = new MemoryStream();
                            while (true)
                            {
                                int r = s.Read(this.buffer, 0, this.buffer.Length);
                                if (r > 0)
                                {
                                    ms.Write(this.buffer, 0, r);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (code == UnzipCode.Compare)
                            {
                                ms.Seek(0, SeekOrigin.Begin);
                                unzipHandler(relFileName, ms);
                            }
                            else if (code == UnzipCode.Ignore)
                            {
                                continue;
                            }

                            string destFileName = unZipDir + theEntry.Name;
                            using (FileStream streamWriter = File.Create(destFileName))
                            {
                                ms.Seek(0, SeekOrigin.Begin);
                                while (true)
                                {
                                    int r = ms.Read(this.buffer, 0, this.buffer.Length);
                                    if (r > 0)
                                    {
                                        streamWriter.Write(this.buffer, 0, r);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                UpdateLog.Instance().AddName(theEntry.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            return true;
        }

    }
}
