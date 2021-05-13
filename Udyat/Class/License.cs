using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Udyat.Class.Core;

namespace Udyat.Class
{
    public class License
    {
        private const string LicToken = "aBrAkAdAbRa";
        private DataLicense dataLicense;
        private bool isLicenseDistanceValid = false;
        private string exceptionMessage;

        public License(DataConfig pConfig)
        {
            dataLicense = new DataLicense(pConfig);
        }

        // Retorna true os dados da localização do computador estão de acordo com a licença (ou seja: existe cadastro preparado para liberar essa licença e a localização está de acordo)
        public bool ActivateLicense()
        {
            // Gera nova chave para a criptografia 
            string SaltKey = Guid.NewGuid().ToString();
            string[] result;
            // Parâmetros para ativação da licença
            string urlParameters = string.Format("?TOKEN={0}&CSERIALK={1}&MHOST={2}&MACADDRESS={3}&AUSER={4}&LICADDRESS={5}&MODE=9", LicToken, dataLicense.SerialKey, dataLicense.ClientIP, dataLicense.CustomerMACAddress, dataLicense.UserName, dataLicense.LicenseAddress);
            // Evoca o serviço de ativação
            // Retorna true se a licença foi validada e ativada
            isLicenseDistanceValid = LicenseWS.get_CustomerReadyForActivation(urlParameters, dataLicense.Proxy, out result, out exceptionMessage);
            // Caso a licença não tenha sido ativada: é possível que a licença tenha sido desinstalada, então verifica-se se a licença foi instalada pelo
            // computador atual e, se foi: permite reativar a licença na máquina.
            if (!isLicenseDistanceValid)
            {                
                try
                {
                    // Verifica se a licença ainda é válida (via site) caso a licença tenha sido ativada e depois o sistema tenha sido desinstalado
                    // Logo: se o sistema foi desinstalado e agora está sendo instalado novamente: deve apenas verificar se a licença é válida via site
                    isLicenseDistanceValid = IsLicenseActivatedOnline(out result);
                }
                catch
                {
                    // Caso existe algum erro de conexão: mantém a licença válida, pois a data do registro está ok
                    isLicenseDistanceValid = false;
                }
            }            
            if (isLicenseDistanceValid)
            {
                // Seta os dados retornados pela licença (automaticamente são setados no Config também)
                DateTime dateValue;
                dataLicense.CustomerWord = result[1];
                if (DateTime.TryParse(result[2], out dateValue))
                {
                    dataLicense.DueDate = dateValue;
                }
                else
                {
                    return false;
                }
                dataLicense.CustomerID = result[3];
                dataLicense.CustomerMachineID = result[4];
                dataLicense.IsTrialLicense = result[5].Substring(0,1) == "T";
                // Salva as inormações no registro
                string regInfo = result[2] + "|" + dataLicense.CustomerWord + "|" + dataLicense.CustomerID + "|" + dataLicense.CustomerMachineID + "|" + dataLicense.IsTrialLicense.ToString() + "|" + dataLicense.SerialKey;
                regInfo = CriptoRijndael.Encrypt(regInfo, SaltKey, 0);
                WinRegistry ureg = new WinRegistry();
                ureg.Write("UdyatRegister", regInfo);
                ureg.Write("MacSalt", SaltKey);
                ureg.Write("UdyatTrial", dataLicense.IsTrialLicense.ToString());
                ureg.Write("customerWord", dataLicense.CustomerWord);
                ureg.Write("customerMN", dataLicense.CustomerMachineID);
                ureg.Write("customer", dataLicense.CustomerID);
                return true;
            }                
            return false;
        }

        /// <summary>
        /// Verifica se a licença está registrada e se ainda é válida
        /// </summary>
        /// <param name="TestMode"></param>
        /// <returns></returns>
        public bool IsLicenseActivated(bool TestMode)
        {
            if (TestMode)
            {
                return true;
            }
            bool isActivated = false;
            WinRegistry ureg = new WinRegistry();
            if (ureg.KeyExists("MacSalt"))
            {
                string SaltKey = ureg.Read("MacSalt");
                if (ureg.KeyExists("UdyatRegister"))
                {
                    string regInfo = ureg.Read("UdyatRegister");
                    regInfo = CriptoRijndael.Decrypt(regInfo, SaltKey, 0);
                    if (regInfo.Length > 0)
                    {
                        string[] licData = regInfo.Split('|');
                        if (licData.Count() == 6)
                        {
                            DateTime dueDate;
                            if (DateTime.TryParse(licData[0], out dueDate))
                            {
                                if (mainClass.internalClock <= dueDate)
                                {
                                    bool IsTrial = false;
                                    bool auxIsTrial;
                                    if (bool.TryParse(licData[4], out auxIsTrial))
                                    {
                                        IsTrial = auxIsTrial;
                                    }
                                    ureg.Write("UdyatTrial", IsTrial);
                                    isActivated = licData[5] == dataLicense.SerialKey;
                                }
                            }
                        }
                    }
                }
            }
            return isActivated;
        }

        public bool IsLicenseActivatedOnline(out string[] result)
        {
            result = null;
            // Verifica a licença
            string urlParameters = string.Format("?TOKEN={0}&CSERIALK={1}&MHOST={2}&MACADDRESS={3}&AUSER={4}&LICADDRESS={5}&MODE=0", LicToken, dataLicense.SerialKey, dataLicense.ClientIP, dataLicense.CustomerMACAddress, dataLicense.UserName, dataLicense.LicenseAddress);
            return LicenseWS.get_CustomerReadyForActivation(urlParameters, dataLicense.Proxy, out result, out exceptionMessage);
        }

        public static bool IsLicenseOk(DataConfig pConfig)
        {
            string[] result;
            License chkLicense = new License(pConfig);
            // Verifica se existe o registro de ativação e se a data de validade está dentro do período de validade da licença
            bool licenseIsOk = chkLicense.IsLicenseActivated(pConfig.TestMode);
            if (!pConfig.TestMode)
            {
                if (licenseIsOk)
                {
                    try
                    {
                        // Se houver conexão com a internet: verifica se a licença ainda é válida (via site)
                        string conType;
                        if (Util.HasInternetConnection(out conType))
                        {
                            licenseIsOk = chkLicense.IsLicenseActivatedOnline(out result);
                        }                        
                    }
                    catch
                    {
                        // Caso existe algum erro de conexão: mantém a licença válida, pois a data do registro está ok
                        licenseIsOk = true;
                    }
                }
                else
                {
                    // Loga a mensagem de erro da licença e finaliza o sistema
                    Log.AddLog("Licença", "Erro: a licença não é válida ou está vencida.");
                }
            }                              
            return licenseIsOk;
        }
        
        public string ExceptionMessage
        {
            get { return exceptionMessage; }
        }

    }
}
