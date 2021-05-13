using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class DataLicense
    {
        private DataConfig dataConfig;
        private DataProxy proxy;
        private string serialKey;
        private string clientIP;
        private string externalIP;
        private string customerMACAddress;
        private string userName;        
        private DateTime dueDate;
        private string customerWord;
        private string customerID;
        private string customerMachineID;
        private string licAddress;
        private bool isTrialLicense = false;

        public DataLicense(DataConfig pConfig)
        {
            dataConfig = pConfig;
            proxy = pConfig.Proxy;
            serialKey = pConfig.SerialKey;
            clientIP = Util.GetLocalIpAddress();
            externalIP = Util.GetExternalIp(proxy);
            customerMACAddress = Util.GetMacAddres();
            userName = Environment.UserName;
            customerID = pConfig.CustomerID;
            customerMachineID = pConfig.CustomerMacNumber;
            licAddress = pConfig.LicenseAddress;
        }

        public DataProxy Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }

        public string SerialKey
        {
            get { return serialKey; }
            set { serialKey = value; }
        }

        public string ClientIP
        {
            get { return clientIP; }
        }

        public string ExternalIP
        {
            get { return externalIP; }
        }

        public string CustomerMACAddress
        {
            get { return customerMACAddress; }
            set { customerMACAddress = value; }
        }

        public string UserName
        {
            get { return userName; }
        }

        public DateTime DueDate
        {
            get { return dueDate; }
            set { dueDate = value; }
        }

        public string CustomerWord
        {
            get { return customerWord; }
            set { customerWord = value; dataConfig.CustomerWord = value; }
        }

        public string CustomerID
        {
            get { return customerID; }
            set { customerID = value; dataConfig.CustomerID = value; }
        }
       

        public string CustomerMachineID
        {
            get { return customerMachineID; }
            set { customerMachineID = value; dataConfig.CustomerMacNumber = value; }
        }

        public string LicenseAddress
        {
            get { return licAddress; }
            set { licAddress = value; }
        }

        public bool IsTrialLicense
        {
            get { return isTrialLicense; }
            set { isTrialLicense = value; dataConfig.IsTrialLicense = value; }
        }

    }
}
