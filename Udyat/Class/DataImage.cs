using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class DataImage
    {
        private int frameID;
        private string computerIP;
        private bool useLegend = true;
        private bool isTrialVersion = false;
        private int footerHeight = 70;
        private const string ComputerNameCaption = "Computador";
        private const string UserNameCaption = "Usuário";
        private const string DateTimeCaption = "Data";
        private const string LocalIPCaption = "IP Local";

        public bool UseLegend
        {
            get { return useLegend; }
            set { useLegend = value; if (!value) { FooterHeight = 0; } else { FooterHeight = 70; } }
        }

        public bool IsTrialVersion
        {
            get { return isTrialVersion; }
            set { isTrialVersion = value; }
        }

        public int FooterHeight
        {
            get { return footerHeight; }
            set { footerHeight = value; }
        }

        public string ComputerName
        {
            get { return ComputerNameCaption +": " + WindowsIdentity.GetCurrent().Name.ToString(); }
        }

        public string UserName
        {
            get { return UserNameCaption + ": " + WindowsIdentity.GetCurrent().User.ToString(); }
        }

        public string DateTimeCapture
        {
            get { return DateTime.Now.ToString(); }
        }

        public int FrameID
        {
            get { return frameID; }
            set { frameID = value; }
        }

        public string IP
        {
            get { return LocalIPCaption + ": " + computerIP; }
            set { computerIP = value; }
        }

    }
}
