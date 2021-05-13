using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Management;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Udyat.Class
{
    public class Util
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        // Permite a exibição de mensagens no Console
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        // Redimenciona uma imagem
        public static Image ResizeImage(int newWidth, int newHeight, string stPhotoPath)
        {

            using (Image src = Image.FromFile(@"C:\Users\myuser\Documents\Udyat\Udyat\img\original.png"))
            using (Bitmap dst = new Bitmap(100, 129))
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, newWidth, newHeight);
                dst.Save(@"C:\Users\myuser\Documents\Udyat\Udyat\img\logo.png", ImageFormat.Png);
            }
            return null;
        }

        // Retorna true se existe conexão com a internet e o tipo de conexão
        public static bool HasInternetConnection(out string ConType)
        {
            int flags;
            ConType = "";
            try
            {
                bool isConnected = InternetGetConnectedState(out flags, 0);
                ConType = ((ConnectionStates)flags).ToString();
                return isConnected;
            }
            catch
            {
                return false;
            }
            
        }

        // Retorna o IP Local
        public static string GetLocalIpAddress()
        {
            UnicastIPAddressInformation mostSuitableIp = null;
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;
                var properties = network.GetIPProperties();
                if (properties.GatewayAddresses.Count == 0)
                    continue;
                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (IPAddress.IsLoopback(address.Address))
                        continue;
                    if (!address.IsDnsEligible)
                    {
                        if (mostSuitableIp == null)
                            mostSuitableIp = address;
                        continue;
                    }
                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                            mostSuitableIp = address;
                        continue;
                    }
                    return address.Address.ToString();
                }
            }
            return mostSuitableIp != null
                ? mostSuitableIp.Address.ToString()
                : "";
        }

        // Retorna o IP externo
        public static string GetExternalIp(DataProxy pProxy)
        {
            try
            {
                string externalIP;                
                WebClient wClient = new WebClient();                
                // Configura o proxy
                if (pProxy.UseProxy)
                {
                    WebProxy proxy = new WebProxy(pProxy.Host, pProxy.Port);
                    if (pProxy.DefaultAuthentication)
                    {
                        wClient.Credentials = CredentialCache.DefaultCredentials;
                        proxy.Credentials = CredentialCache.DefaultCredentials;
                    }
                    else
                    {
                        string dm = "";
                        if (pProxy.Domain.Length > 0)
                        {
                            dm = pProxy.Domain + @"\";
                        }
                        ICredentials credentials = new NetworkCredential(dm + pProxy.User, pProxy.Password);
                        proxy.Credentials = credentials;
                    }
                    wClient.Proxy = proxy;
                }

                externalIP = wClient.DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
                return externalIP;
            }
            catch { return null; }
        }

        public static bool MoveFromTo(string pFromFullFileName, string pToFullFileName)
        {
            if (File.Exists(pFromFullFileName))
            {
                try
                {
                    File.Move(pFromFullFileName, pToFullFileName);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public static bool CreateDir(string pPath, string pProcess = "None")
        {
            try
            {
                Directory.CreateDirectory(pPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.AddLog(pProcess, ex.Message);
                return false;
            }            
        }

        /// <summary>
        /// Retorna o grupo de trabalho do computador
        /// </summary>
        /// <returns></returns>
        public static string GetWorkGroup()
        {
            ManagementObject computer_system = new ManagementObject(
                        string.Format(
                        "Win32_ComputerSystem.Name='{0}'",
                        Environment.MachineName));

            object result = computer_system["Workgroup"];
            if (result == null)
            {
                return "";
            }
            else
            {
                return result.ToString();
            }            
        }

        public static string GetMacAddres()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true");
            IEnumerable<ManagementObject> objects = searcher.Get().Cast<ManagementObject>();
            string mac = (from o in objects orderby o["IPConnectionMetric"] select o["MACAddress"].ToString()).FirstOrDefault();
            if (mac.Length == 0)
            {
                return "nomac";
            }
            return mac.Replace(":", "-");
        }

        public static string AddBackslashInPath(string aPath)
        {
            if (aPath.Trim().EndsWith("\\") != true)
            { return aPath + "\\"; }
            else return aPath;
        }

        // Retorna o modelo do computador
        public static string GetComputerModel()
        {
            Console.WriteLine("GetComputerModel");
            var s1 = new ManagementObjectSearcher("select * from Win32_ComputerSystem");
            foreach (ManagementObject oReturn in s1.Get())
            {
                return oReturn["Model"].ToString().Trim();
            }
            return string.Empty;
        }

        // Retorna o volume de um drive
        public static string GetVolumeSerial(string drive)
        {
            ManagementObject disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }

        // Retorna o ID do primeiro processador 
        public static string GetCPUID()
        {
            string cpuInfo = "";
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                if (cpuInfo == "")
                {
                    cpuInfo = managObj.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        // Retorna um ID único para a máquina, contendo 
        public static string MachineUniqueID(string drive)
        {
            if (drive == string.Empty)
            {
                //Primeiro drive
                foreach (DriveInfo compDrive in DriveInfo.GetDrives())
                {
                    if (compDrive.IsReady)
                    {
                        drive = compDrive.RootDirectory.ToString();
                        break;
                    }
                }
            }

            if (drive.EndsWith(":\\"))
            {
                //C:\ -> C
                drive = drive.Substring(0, drive.Length - 2);
            }

            string volumeSerial = GetVolumeSerial(drive);
            string cpuID = GetCPUID();

            //cpuID.Substring(13) = remove zeros à esquerda que não são utilizados 
            return /*cpuID.Substring(13) +*/ cpuID.Substring(1, 4) + volumeSerial + cpuID.Substring(4, 4);
        }

        public static void AddToStartUpWindows(string pFullFileName)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("Udyat", pFullFileName);
        }

        public static void RemoveFromStartUpWindows()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.DeleteValue("Udyat", false);
        }

        public static void ShowConsoleMessage(string pMessage, bool pShowConsole)
        {
            if (pShowConsole)
            {
                AllocConsole();
                Console.WriteLine("");
                Console.WriteLine(pMessage);
                Console.ReadKey(false);
            }            
        }

        public static void SaveJpeg(string path, Image img, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

            // Encoder parameter for image quality 
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
            // JPEG image codec 
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            img.Save(path, jpegCodec, encoderParams);
        }

        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }

    }


    [Flags]
    public enum ConnectionStates
    {
        Modem = 0x1,
        LAN = 0x2,
        Proxy = 0x4,
        RasInstalled = 0x10,
        Offline = 0x20,
        Configured = 0x40,
    }


}
