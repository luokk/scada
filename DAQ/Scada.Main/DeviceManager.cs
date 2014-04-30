using Scada.Config;
using Scada.Declare;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Scada.Main
{

    class DevicesInfo
    {
        public List<string> Versions
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }
    }

    class DeviceRunContext
    {
        public DeviceRunContext(string deviceName, string version)
        {
            this.DeviceName = deviceName;
            this.Version = version;
        }

        public string DeviceName
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public SynchronizationContext SynchronizationContext
        {
            get;
            set;
        }

        public SendOrPostCallback Callback
        {
            get;
            set;
        }

        public Device Device
        {
            get;
            set;
        }
    }

	class DeviceManager
	{
        private const string DevicePath = @"devices";

        private const string DeviceConfigFile = @"device.cfg";

        private Dictionary<string, DevicesInfo> dict = new Dictionary<string, DevicesInfo>();

        /// <summary>
        /// 
        /// At most has 10 SerailPort device.
        /// </summary>
        private Dictionary<string, string> d2d = new Dictionary<string, string>(10);


        private Dictionary<string, DeviceRunContext> selectedDevices = new Dictionary<string, DeviceRunContext>();


        private Dictionary<string, long> lastUpdateDict = new Dictionary<string, long>();


        /// <summary>
        /// Running devices;
        /// </summary>
        // private List<Device> devices = new List<Device>();

        private SendOrPostCallback dataReceived;

		private Dictionary<string, FileRecord> records = null;

        public string[] Args
        {
            get;
            set;
        }

        public DeviceManager Instance()
        {
            return null;
        }

        public DeviceManager()
		{

		}

        public SendOrPostCallback DataReceived
        {
            set { this.dataReceived = value; }
        }

        public string[] DeviceNames
        {
            get 
            {
                List<string> deviceNames = new List<string>();
                foreach (var deviceInfo in dict.Values)
                {
                    deviceNames.Add(deviceInfo.Name);
                }
                return deviceNames.ToArray();
            }
        }

        // TODO:
        public string GetDeviceDisplayName(string deviceName)
        {
            return dict[deviceName.ToLower()].DisplayName;
        }

        public List<string> GetVersions(string deviceName)
        {
            return dict[deviceName].Versions;
        }

        static string DirectoryName(string dir)
        {
            int lastBackSlash = dir.LastIndexOf('\\');
            string name = dir.Substring(lastBackSlash + 1);
            return name;
        }
		
        private void LoadDevicesInfo(string installPath)
        {
            this.dict.Clear();
            string[] deviceConfigPaths = Directory.GetDirectories(ConfigPath.GetConfigFilePath(DevicePath));
            foreach (string devicePath in deviceConfigPaths)
            {
                string deviceName = DirectoryName(devicePath);
				if (deviceName.StartsWith("!") || deviceName.StartsWith("."))
				{
					continue;
				}
                string deviceKey = deviceName.ToLower();

                DevicesInfo di = null;
                if (!dict.ContainsKey(deviceKey))
                {
                    di = new DevicesInfo() { Name = deviceName };
                    di.Versions = new List<string>();
                    dict.Add(deviceKey, di);
                }
                else
                {
                    di = dict[deviceKey];
                }
                
                string displayConfig = devicePath + "\\display.cfg";
                if (File.Exists(displayConfig))
                {
                    using (ScadaReader sr = new ScadaReader(displayConfig))
                    {
                        // TODO: Xml Reader parse the whole file.
                        // And retrieve the resulr, no need to loop reading line
                        // 
                        SectionType secType = SectionType.None;
                        string line = null;
                        string key = null;
                        IValue value = null;
                        ReadLineResult result = sr.ReadLine(out secType, out line, out key, out value);

                        while (result == ReadLineResult.OK)
                        {
                            if (key.ToLower() == "name")
                            {
                                di.DisplayName = value.ToString();
                            }
                            result = sr.ReadLine(out secType, out line, out key, out value);
                        }
                    }
                }
                
                string[] versionPaths = Directory.GetDirectories(devicePath);
                foreach (string versionPath in versionPaths)
                {
                    string version = DirectoryName(versionPath);
                    di.Versions.Add(version);
                }
            }
        }

        public static string GetDeviceConfigPath(string deviceName, string version)
        {
            string deviceConfigPath = ConfigPath.GetConfigFilePath(DevicePath);
            return string.Format("{0}\\{1}\\{2}", deviceConfigPath, deviceName, version);
        }

        public static void SetDeviceConfigPath(string deviceName, bool hide)
        {
            string deviceConfigPath = ConfigPath.GetConfigFilePath(DevicePath);
            string devicePath1 = string.Format("{0}\\{1}", deviceConfigPath, deviceName);
            string devicePath2 = string.Format("{0}\\!{1}", deviceConfigPath, deviceName);

            if (hide && Directory.Exists(devicePath1))
            {
                Directory.Move(devicePath1, devicePath2);
                // Rename(devicePath1, devicePath2);
            }
            else if (!hide && Directory.Exists(devicePath2))
            {
                Directory.Move(devicePath2, devicePath1);
            }
        }

		public bool RegisterRecordModule(string module, FileRecord fileRecord)
		{
			if (!this.records.ContainsKey(module))
			{
				this.records.Add(module, new FileRecord("TODO:"));
				return true;
			}
			return false;
		}

        /// <summary>
        /// Add a device to run.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="version"></param>
        public void SelectDevice(string deviceName, string version, bool selected)
        {
            string deviceKey = deviceName.ToLower();
            if (selected)
            {
                this.selectedDevices.Add(deviceKey, new DeviceRunContext(deviceName, version));
            }
            else
            {
                this.selectedDevices.Remove(deviceKey);
            }
        }

        private Device Load(DeviceEntry entry)
        {
			if (entry == null)
				return null;

            StringValue className = (StringValue)entry[DeviceEntry.ClassName];
            if (typeof(StandardDevice).ToString() == className)
            {
                return new StandardDevice(entry);
            }
			else if (typeof(WebFileDevice).ToString() == className)
			{
				return new WebFileDevice(entry);
			}
            else if (typeof(FormProxyDevice).ToString() == className)
            {
                return new FormProxyDevice(entry);
            }
            else if (typeof(CinderlDataDevice).ToString() == className)
            {
                return new CinderlDataDevice(entry);
            }
            else if (typeof(CinderlStatusDevice).ToString() == className)
            {
                return new CinderlStatusDevice(entry);
            }
			// Other Device defined in some Assemblies.
            if (entry[DeviceEntry.Assembly] != null)
            {
                Assembly assembly = Assembly.Load((StringValue)entry[DeviceEntry.Assembly]);
                Type deviceClass = assembly.GetType((StringValue)entry[DeviceEntry.ClassName]);
                if (deviceClass != null)
                {
                    object device = Activator.CreateInstance(deviceClass, new object[] { entry });
                    return device as Device;
                }
            }
            MessageBox.Show("Create Device Failed");
            return (Device)null;
        }

        public bool Run(SynchronizationContext syncCtx, SendOrPostCallback callback)
        {
            foreach (string deviceName in selectedDevices.Keys)
            {
                DeviceRunContext context = this.selectedDevices[deviceName];
                context.SynchronizationContext = syncCtx;
                context.Callback = callback;

                this.RunDevice(context);
            }
            RecordManager.DoSystemEventRecord(Device.Main, "Devices are running now.", RecordType.Event);
            return true;
        }

        private bool RunDevice(DeviceRunContext context)
        {
            string path = GetDeviceConfigPath(context.DeviceName, context.Version);
            if (Directory.Exists(path))
            {
                string deviceCfgFile = string.Format("{0}\\{1}", path, DeviceConfigFile);
                // TODO: Config file reading
                if (deviceCfgFile != null)
                {
                    DeviceEntry entry = DeviceEntry.GetDeviceEntry(context.DeviceName, deviceCfgFile);
                    this.CheckVirtualDevice(entry, deviceCfgFile);
                    Device device = Load(entry);
                    if (device != null)
                    {
                        context.Device = device;

                        // Set thread-sync-context
                        device.SynchronizationContext = context.SynchronizationContext;
                        // Set data-received callback
                        device.DataReceived += context.Callback;

                        string address = this.GetCOMPort(entry);
                        string deviceLoadedStr = string.Format("Device: '{0}' Loaded @ '{1}'", entry[DeviceEntry.Identity], address);
                        RecordManager.DoSystemEventRecord(device, deviceLoadedStr);

                        device.Start(address);
                        return true;
                    }
                }
            }

            return false;
        }

        private void CheckVirtualDevice(DeviceEntry entry, string configFile)
        {
            DirectoryInfo di = Directory.GetParent(configFile);
            string virtualDeviceFlagFile = string.Format("{0}\\virtual-device", di.FullName);
            if (!File.Exists(virtualDeviceFlagFile))
            {
                return;
            }

            string deviceDisplayName = (StringValue)entry[DeviceEntry.Name];
            string caption = "连接虚拟设备提示";
            string message = string.Format("是否要连接 '{0}' 的虚拟设备，连接虚拟设备点击‘是’，\n连接真实设备点击‘否’", deviceDisplayName);
            DialogResult dr = MessageBox.Show(message, caption, MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                entry[DeviceEntry.Virtual] = new StringValue("true");
            }
            else
            {
                entry[DeviceEntry.Virtual] = new StringValue("false");

                string deleteVirtualFileMsg = string.Format("是否要删除 '{0}' 的虚拟设备标志文件？", deviceDisplayName);
                DialogResult del = MessageBox.Show(deleteVirtualFileMsg, caption, MessageBoxButtons.YesNo);
                if (del == DialogResult.Yes)
                {
                    File.Delete(virtualDeviceFlagFile);
                }
            }
        }

		public void Initialize()
		{
            // TODO: Remove the param;
            this.LoadDevicesInfo(MainApplication.InstallPath);
		}

		public void CloseAllDevices()
		{
            // Running Devices...
            foreach (string deviceName in selectedDevices.Keys)
            {
                DeviceRunContext context = this.selectedDevices[deviceName];
                if (context != null)
                {
                    Device device = context.Device;
                    device.Stop();
                }
            }

            this.selectedDevices.Clear();
		}

        private string GetCOMPort(DeviceEntry entry)
        {
            if (entry.Contains(DeviceEntry.SerialPort))
            {
                return (StringValue)entry[DeviceEntry.SerialPort];
            }
            return string.Empty;
        }

        /// <summary>
        /// Update the device last modify time.
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <param name="p"></param>
        internal void UpdateLastModifyTime(string deviceKey, long p)
        {
            if (this.lastUpdateDict.ContainsKey(deviceKey))
            {
                this.lastUpdateDict[deviceKey] = p;
            }
            else
            {
                this.lastUpdateDict.Add(deviceKey, p);
            }
        }

        // [Notice]; The the device no data arrived at the very beginning.
        // TODO: Should alert at all!
        internal void CheckLastModifyTime()
        {
            // At the very beginning, a device has NO data received, that case Not in rescue case.
            // So, the this.lastUpdateDict would NOT contain the device's last modify info.
            long now = DateTime.Now.Ticks;
            foreach (string deviceKey in this.lastUpdateDict.Keys)
            {
                if (deviceKey.Equals("Scada.HVSampler", StringComparison.OrdinalIgnoreCase) ||
                    deviceKey.Equals("Scada.ISampler", StringComparison.OrdinalIgnoreCase) ||
                    deviceKey.Equals("Scada.Cinderella.Status", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                long lastModifyTime = this.lastUpdateDict[deviceKey];
                long diffInSec = (now - lastModifyTime) / 10000000;
                // 5分钟无数据...
                // TODO: ...NaI 5分钟数据, 这里得改
                if (diffInSec > 60 * 5)
                {
                    this.RescueDevice(deviceKey);
                }
            }
        }

        private void RescueDevice(string deviceKey)
        {
            DeviceRunContext context = this.selectedDevices[deviceKey];

            if (context != null)
            {
                Device badDevice = context.Device;
                const string DeviceWillRestart = "The device will restart now.";
                RecordManager.DoSystemEventRecord(badDevice, DeviceWillRestart);
                if (badDevice != null)
                {
                    badDevice.Stop();
                }
                this.RunDevice(context);
            }
        }

        public void OpenMainProgram()
        {
            const string ScadaMainExe = "Scada.Main.exe";
            using (Process process = new Process())
            {
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = ScadaMainExe;      //设定程序名  
                process.StartInfo.RedirectStandardInput = true;   //重定向标准输入
                process.StartInfo.RedirectStandardOutput = true;  //重定向标准输出
                process.StartInfo.RedirectStandardError = true;//重定向错误输出
                process.Start();
            }
        }
    }
}
