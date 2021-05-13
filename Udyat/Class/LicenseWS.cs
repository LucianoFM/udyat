using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;


namespace Udyat.Class
{
    public class LicenseWS
    {
        //private const string LicServer = "http://localhost/chkact.php";
        private const string LicServer = "http://www.geolicenses.com/chkact.php";
        private const string urlGetip = "http://www.geolicenses.com/getip.php";

        //private const string GeoServer = "http://www.freegeoip.net/xml";
        private const string LicToken = "aBrAkAdAbRa";

        public static bool get_CustomerReadyForActivation(string pParameters, DataProxy pProxy, out string[] WSResult, out string pExceptionMessage)
        {
            WSResult = null;
            string webUrl = LicServer + pParameters;
            string requestResult;
            pExceptionMessage = "";
            if (HTTPGetPost.Get(webUrl, pProxy, out requestResult, out pExceptionMessage))
            {
                requestResult = requestResult.Replace("\r", "").Replace("\n", "");
                WSResult = requestResult.Split('|');
                return ((requestResult.Length > 0) && (requestResult[0].ToString() == "0"));
            }
            return false;           
        }


        // Retorna os dados de um cliente se o mesmo possuir cadastro preparado para ativação
        public static bool post_CustomerReadyForActivation(string pParameters, DataProxy pProxy, out string[] WSResult, out string pExceptionMessage)
        {
            WSResult = null;
            string requestResult;
            if (HTTPGetPost.Post(LicServer, pParameters, pProxy, out requestResult, out pExceptionMessage))
            {
                requestResult = requestResult.Replace("\r", "").Replace("\n", "");
                WSResult = requestResult.Split('|');
                return ((requestResult.Length > 0) && (requestResult[0].ToString() == "0"));
            }
            return false;
        }

        
        /* DESCONTINUADO
        // Retorna os dados de geolocalização da máquina
        public static string[] GetMachineGeoLocation(DataProxy pProxy)
        {
            string IP = "";
            string localIP = "";
            //string strHostName = "";
            try
            {
                // Pega o Host do computador
                //strHostName = System.Net.Dns.GetHostName();
                // Resolve o nome do host ou endereço IP para pegar as informações
                //IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                // Lista de endereços
                //IPAddress[] addr = ipEntry.AddressList;
                string WSResult;
                if (HTTPGetPost.Post(urlGetip, "", pProxy, out WSResult))
                {
                    if (WSResult.Length > 0)
                    {
                        // Guarda IP local e de Internet
                        localIP = Util.GetLocalIpAddress();
                        IP = WSResult;
                        // Busca o XML das informações de geolocalização do IP
                        XmlDocument doc = new XmlDocument();
                        doc.Load(GeoServer);
                        XmlNodeList nCountryCode = doc.GetElementsByTagName("CountryCode");
                        XmlNodeList nRegionCode = doc.GetElementsByTagName("RegionCode");
                        XmlNodeList nCity = doc.GetElementsByTagName("City");
                        XmlNodeList nLat = doc.GetElementsByTagName("Latitude");
                        XmlNodeList nLong = doc.GetElementsByTagName("Longitude");
                        if ((nCountryCode != null) && (nRegionCode != null) && (nCity != null) && (nLat != null) && (nLong != null))
                        {
                            string[] result = new string[6];
                            result[0] = nCountryCode[0].InnerText;
                            result[1] = nRegionCode[0].InnerText;
                            result[2] = nCity[0].InnerText;
                            result[3] = nLat[0].InnerText.Replace(".", ",");
                            result[4] = nLong[0].InnerText.Replace(".", ",");
                            result[5] = localIP;
                            result[6] = IP;
                            return result;
                        }
                    }
                }                
                return null;
            }
            catch 
            {
                return null;
            }
        }
        */
        



    }
}
