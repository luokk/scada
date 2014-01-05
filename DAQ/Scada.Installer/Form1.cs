using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using Scada.Update;

// using IWshRuntimeLibrary;

namespace Scada.Installer
{
    public partial class InstallerForm : Form
    {
        public InstallerForm()
        {
            InitializeComponent();
        }

        private bool finished = false;

        private string binPath = "Release";

        private void SelectPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = @"C:\";
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                this.installPath.Text = fbd.SelectedPath;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.installPath.Text = @"C:\Scada";
        }

        private void InstallOrUpdateClick(object sender, EventArgs e)
        {
            if (!this.finished)
            {
                this.buttonInstall.Enabled = false;
                Thread thread = new Thread(new ThreadStart(() => 
                {
                    this.StartInstallProcess();
                }));
                thread.Start();
            }
            else
            {
                this.Close();
            }
        }

        // Start the Install process
        private bool StartInstallProcess()
        {
            if (!CreateFolders())
            {
                return false;
            }

            Updater u = new Updater();
            u.NeedUpdateConfigFiles = updateConfigCheckBox.Checked;
            // If Put the bin.zip @ Install Path, the Installer would unzip it into InstallPath.
            // If Put the bin.zip @ Update Path, The Update Program would update using this zip file.
            string binZipFilePath = this.GetInstallerPath() + "\\bin.zip";
            if (!u.UnzipProgramFiles(binZipFilePath, this.installPath.Text))
            {
                return false;
            }


            if (Directory.Exists(this.installPath.Text + "\\Debug"))
            {
                this.binPath = "Debug";
            }
            else if (Directory.Exists(this.installPath.Text + "\\Release"))
            {
                this.binPath = "Release";
            }

            if (!CreateTables())
            {
                return false;
            }

            if (!CreateStartupMenu())
            {
                return false;
            }

            if (CreateDesktopIcons("Scada.Main.exe", "系统设备管理器") &&
                CreateDesktopIcons("Scada.MainVision.exe", "Nuclover - SCADA"))
            {
                if (this.installMode)
                {
                    this.AddLog("安装成功!");
                    LaunchMainSettings();
                }
                else
                {
                    this.AddLog("更新成功!");
                    LaunchMainSettings();
                }

                this.Invoke(new MyInvoke((object sender, string p) => 
                {
                    this.buttonInstall.Enabled = true;
                    this.buttonInstall.Text = "关闭";
                }), null, "");
                
                this.finished = true;
                return true;
            }
            else
            {
                this.AddLog("安装未完成!");
                return false;
            }
        }

        private bool CheckVersions()
        {
            // TODO: Main

            // TODO: MainVision
            return true;
        }

        private void LaunchMainSettings()
        {
            string fileName = "Scada.MainSettings.exe";
            string filePath = string.Format("{0}\\{1}\\{2}", this.installPath.Text, this.binPath, fileName);
            using (Process process = new Process())
            {
                process.StartInfo.CreateNoWindow = false;           //设定不显示窗口
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = filePath;              //设定程序名  
                process.StartInfo.RedirectStandardInput = true;     //重定向标准输入
                process.StartInfo.RedirectStandardOutput = true;    //重定向标准输出
                process.StartInfo.RedirectStandardError = true;     //重定向错误输出

                process.StartInfo.Arguments = "--first-time";
                process.Start();
            }
        }

        private bool CreateStartupMenu()
        {
            try
            {
                string p = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string s = this.CreateStartupBatFile();
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

                IWshRuntimeLibrary.WshShortcut shortcut = (IWshRuntimeLibrary.WshShortcut)shell.CreateShortcut(p + "\\startup.lnk");
                shortcut.TargetPath = s;
                shortcut.Arguments = "";
                shortcut.Description = "启动";
                shortcut.WorkingDirectory = this.installPath.Text;
                shortcut.IconLocation = string.Format("{0},0", s);
                shortcut.Save();

                this.AddLog("启动组快捷方式创建成功");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CreateDesktopIcons(string fileName, string linkName)
        {
            try
            {
                string p = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string s = this.GetBinFile(fileName);
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

                string linkFilePath = p + "\\" + linkName + ".lnk";
                if (File.Exists(linkFilePath))
                {
                    File.Delete(linkFilePath);
                }
                IWshRuntimeLibrary.WshShortcut shortcut = (IWshRuntimeLibrary.WshShortcut)shell.CreateShortcut(linkFilePath);
                shortcut.TargetPath = s;
                shortcut.Arguments = "";
                shortcut.Description = fileName;
                shortcut.WorkingDirectory = Path.Combine(this.installPath.Text, this.binPath);
                shortcut.IconLocation = string.Format("{0},0", s);
                shortcut.Save();

                this.AddLog("桌面快捷方式创建成功");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetBinFile(string fileName)
        {
            return string.Format("{0}\\{1}", Path.Combine(this.installPath.Text, this.binPath), fileName);
        }

        private bool CreateTables()
        {
            if (!this.resetCheckBox.Checked)
            {
                // Or use this.installMode to check.
                return true;
            }
            string fileName = "Scada.Data.Tools.exe";
            string filePath = string.Format("{0}\\{1}", Path.Combine(this.installPath.Text, this.binPath), fileName);
            using (Process process = new Process())
            {
                process.StartInfo.CreateNoWindow = true;    //设定不显示窗口
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = filePath; //设定程序名  
                process.StartInfo.RedirectStandardInput = true;   //重定向标准输入
                process.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
                process.StartInfo.RedirectStandardError = true;//重定向错误输出

                process.StartInfo.Arguments = "--init-database-s";
                bool ret = process.Start();
                if (ret)
                {
                    this.AddLog("数据库初始化成功");
                }
                else
                {
                    this.AddLog("数据库初始化失败");
                }
                return ret;
            }
        }

        private void WriteFile(Stream stream, string fileName)
        {
            string nzFileName = fileName + ".n!";
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

            System.Security.Cryptography.HashAlgorithm hash = System.Security.Cryptography.HashAlgorithm.Create();
            
            FileStream stream1 = new FileStream(fileName, FileMode.Open);
            FileStream stream2 = new FileStream(nzFileName, FileMode.Open);
            byte[] hashbyte1 = hash.ComputeHash(stream1);
            byte[] hashbyte2 = hash.ComputeHash(stream2);
            stream1.Close();
            stream2.Close();

            if (BitConverter.ToString(hashbyte1) == BitConverter.ToString(hashbyte2))
            {
                File.Delete(nzFileName);
            }
            else
            {
                MessageBox.Show(string.Format("文件 '{0}' 与 {1} 存在差异，请在安装(更新)后手工合并。", fileName, nzFileName), "配置文件差异");
            }
  
        }

        private bool PrepareMySQLConfigFile()
        {
            return true;
        }

        // TODO:
        private bool CreateFolders()
        {
            try
            {
                string programPath = this.installPath.Text;
                Directory.CreateDirectory(programPath);

                this.AddLog("目录创建成功");
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        private string CreateStartupBatFile()
        {
            string p = string.Format("{0}\\startup.bat", Path.Combine(this.installPath.Text, this.binPath));
            if (File.Exists(p))
            {
                File.Delete(p);
            }
            FileStream fs = File.Create(p);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                string fullBinPath = Path.Combine(this.installPath.Text, this.binPath);
                // Run MDS.exe
                string startMDSScript = string.Format("start {0}\\mds.exe", fullBinPath);
                sw.WriteLine(startMDSScript);
                sw.WriteLine("ping -n 5 127.0.0.1");

                // Run AIS.exe
                string startAISScript = string.Format("start {0}\\ais.exe", fullBinPath);
                sw.WriteLine(startAISScript);
                sw.WriteLine("ping -n 5 127.0.0.1");

                // Run Scada.Main.exe
                string startMainScript = string.Format("start {0}\\Scada.Main.exe /ALL", fullBinPath);
                sw.WriteLine(startMainScript);
                sw.WriteLine("ping -n 30 127.0.0.1");
                sw.WriteLine();

                // Run Scada.DataCenterAgent.exe
                string startAgentScript = string.Format("start {0}\\Scada.DataCenterAgent.exe --start", fullBinPath);
                sw.WriteLine(startAgentScript);
                sw.WriteLine();
            }
            fs.Close();

            return p;
        }

        /// <summary>
        /// /////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        public delegate void MyInvoke(object sender, string msg);

        private void AddLog(string msg)
        {
            this.Invoke(new MyInvoke(this.AddString), this, msg);
        }

        private void AddString(object sender, string line)
        {
            this.progressBox.Items.Add(line);
        }

        private void resetCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private bool installMode = true;


        private string GetInstallerPath()
        {
            string p = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(p);
        }

        private void SelectPathButtonClick(object sender, EventArgs e)
        {
            this.SelectPath();
        }
        
    }
}
