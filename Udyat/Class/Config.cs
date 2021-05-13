using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class.Core
{
    public class Config
    {
        // Nome do arquivo de configuração
        private const string INIFILE_NAME = "Udyat.ini";
        // Nome da pasta temporária onde as imagens serão armazenadas
        private string tempOutputPathName = "Udyat";
        // Nome da pasta destino padrão para onde as imagens serão enviadas
        private string targetOutputPathName = "output";
        DataConfig configRecord;
        // Pasta de armazenamento temporário e destino das imagens
        private string tempPath;
        private string targetPath;
        // Identificador única da máquina onde o sistema está sendo executado
        private string machineUniqueID;
        // Atributos puros do INI
        private string attempts;
        private string proxy;
        private string proxyHost;
        private string port;
        private string defaultAuth;
        private string domain;
        private string user;
        private string password;
        private string modeType;        
        private string locallog;
        private string clock;
        private string autodir;
        private string movefile;
        private string moveafter;
        private string target;
        private string procdir;
        private string interval;
        private string imgtype;
        private string jpgquality;
        private string legend;
        private string ntpserver;
        private string ntpport;
        private string perscreen;
        private string signature;
        private string oldSerialKey = "";
        private string serialKey;
        private string customer = "";
        private string customerMN = "";
        private string customerWord = "";
        private string customerDueDate = "";
        private string wstartup;
        private string licaddress = "";
        private string trialLicense;
        private string serverHost;
        private string serverPort;
        private string clientPort;

        public Config()
        {
            configRecord = new DataConfig();
        }

        public static string ConfigFullFileName
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + INIFILE_NAME; }
        }

        /// <summary>
        /// Carrega as configurações (do arquivo .ini se ele existir ou do registro se o .ini não existir -- se ambos não existirem: retorna falso)
        /// </summary>
        /// <returns></returns>
        public bool LoadConfig()
        {
            bool result = false;

            // Guarda o serial atualmente sendo usado (usado em novas instalações)
            oldSerialKey = GetLastSerialkey();

            bool iniExists = File.Exists(ConfigFullFileName);
            if (iniExists)
            {
                // Lê as configurações do arquivo .ini
                result = ReadIniFile();
            }
            else
            {
                if (IsIniInRegister())
                {
                    // Lê as configurações do registro do Windows
                    result = ReadIniInFromRegister();
                }
            }
            if (result)
            {
                ApplyConfig();
            }
            return result;
        }

        /// <summary>
        /// Pasta onde as imagens serão armazenadas temporariamente
        /// </summary>
        public string TempPath
        {
            get { return tempPath; }
            set { tempPath = value; }
        }

        /// <summary>
        /// Pasta para onde as imagens serão movidas
        /// </summary>
        public string TargetPath
        {
            get { return targetPath; }
            set { targetPath = value; }
        }

        public string MachineUniqueID
        {
            get { return machineUniqueID; }
        }

        /// <summary>
        /// Contém a classe que detém os dados de configuração que estão ou serão armazenados
        /// </summary>
        public DataConfig Data
        {
            get { return configRecord; }
            set { configRecord = value; }
        }

        /// <summary>
        /// Lê o arquivo .INI 
        /// </summary>
        public bool ReadIniFile()
        {
            // Abre o arquivo de configuração (se existir) 
            INIFile iniFile = new INIFile(AppDomain.CurrentDomain.BaseDirectory + INIFILE_NAME);
            try
            {
                // CONNECTION       
                attempts = iniFile.Read("CONNECTION", "attempts").ToUpper();
                proxy = iniFile.Read("CONNECTION", "proxy").ToUpper();
                proxyHost = iniFile.Read("CONNECTION", "host");
                port = iniFile.Read("CONNECTION", "port");
                defaultAuth = iniFile.Read("CONNECTION", "defaultAuth").ToUpper();
                domain = iniFile.Read("CONNECTION", "domain");
                user = iniFile.Read("CONNECTION", "user");
                password = iniFile.Read("CONNECTION", "password");
                // EXECUTION
                modeType = iniFile.Read("EXECUTION", "mode");
                locallog = iniFile.Read("EXECUTION", "locallog").ToUpper();
                clock = iniFile.Read("EXECUTION", "clock");
                autodir = iniFile.Read("EXECUTION", "autodir").ToUpper();
                movefile = iniFile.Read("EXECUTION", "movefile").ToUpper();
                moveafter = iniFile.Read("EXECUTION", "moveafter");
                target = iniFile.Read("EXECUTION", "target").ToUpper();
                procdir = Util.AddBackslashInPath(iniFile.Read("EXECUTION", "procdir"));
                wstartup = iniFile.Read("EXECUTION", "wstartup").ToUpper();
                // CAPTURE
                interval = iniFile.Read("CAPTURE", "interval");
                imgtype = iniFile.Read("CAPTURE", "imgtype");
                jpgquality = iniFile.Read("CAPTURE", "jpgquality");
                legend = iniFile.Read("CAPTURE", "legend").ToUpper();
                ntpserver = iniFile.Read("CAPTURE", "ntpserver");
                ntpport = iniFile.Read("CAPTURE", "ntpport");
                perscreen = iniFile.Read("CAPTURE", "perscreen").ToUpper();
                signature = iniFile.Read("CAPTURE", "signature").ToUpper();
                // COSTUMER
                serialKey = iniFile.Read("CUSTOMER", "serialkey").ToString();
                licaddress = iniFile.Read("CUSTOMER", "licaddress").ToString();
                // P2P
                serverHost = iniFile.Read("P2P", "server");
                serverPort = iniFile.Read("P2P", "serverport");
                clientPort = iniFile.Read("P2P", "clientport");
                // Variáveis que são gravadas somente ao ativar uma licença
                WinRegistry ureg = new WinRegistry();
                if (ureg.KeyExists("customer"))
                {
                    customer = ureg.Read("customer").ToString();
                }
                if (ureg.KeyExists("customerMN"))
                {
                    customerMN = ureg.Read("customerMN").ToString();
                }
                if (ureg.KeyExists("customerWord"))
                {
                    customerWord = ureg.Read("customerWord").ToString();
                }
                if (ureg.KeyExists("UdyatTrial"))
                {
                    trialLicense = ureg.Read("UdyatTrial").ToString();
                }
                return true;
            }
            catch
            {
                return false;
            }            
        }

        public void SaveIniInWinRegister()
        {
            WinRegistry ureg = new WinRegistry();
            ureg.Write("attempts", attempts);
            ureg.Write("proxy", proxy);
            ureg.Write("host", proxyHost);
            ureg.Write("port", port);
            ureg.Write("defaultAuth", defaultAuth);
            ureg.Write("domain", domain);
            ureg.Write("user", user);
            ureg.Write("password", password);
            ureg.Write("mode", modeType);
            ureg.Write("locallog", locallog);
            ureg.Write("clock", clock);
            ureg.Write("autodir", autodir);
            ureg.Write("movefile", movefile);
            ureg.Write("moveafter", moveafter);
            ureg.Write("target", target);
            ureg.Write("procdir", procdir);
            ureg.Write("interval", interval);
            ureg.Write("imgtype", imgtype);
            ureg.Write("jpgquality", imgtype);
            ureg.Write("legend", legend);
            ureg.Write("ntpserver", ntpserver);
            ureg.Write("ntpport", ntpport);
            ureg.Write("perscreen", perscreen);
            ureg.Write("signature", signature);
            ureg.Write("serialkey", serialKey);
            ureg.Write("customer", customer);
            ureg.Write("customerMN", customerMN);
            ureg.Write("customerDueDate", customerDueDate); 
            ureg.Write("wstartup", wstartup);
            ureg.Write("licaddress", licaddress);
            ureg.Write("server", licaddress);
            ureg.Write("serverport", licaddress);
            ureg.Write("clientport", licaddress);
        }

        private bool ReadIniInFromRegister()
        {
            WinRegistry ureg = new WinRegistry();
            try
            {
                attempts = ureg.Read("attempts");
                proxy = ureg.Read("proxy").ToUpper();
                proxyHost = ureg.Read("host");
                port = ureg.Read("port");
                defaultAuth = ureg.Read("defaultAuth").ToUpper();
                domain = ureg.Read("domain");
                user = ureg.Read("user");
                password = ureg.Read("password");
                modeType = ureg.Read("mode");
                locallog = ureg.Read("locallog").ToUpper();
                clock = ureg.Read("clock");
                autodir = ureg.Read("autodir").ToUpper();
                movefile = ureg.Read("movefile").ToUpper();
                moveafter = ureg.Read("moveafter");
                target = ureg.Read("target").ToUpper();
                procdir = Util.AddBackslashInPath(ureg.Read("procdir"));
                interval = ureg.Read("interval");
                imgtype = ureg.Read("imgtype");
                jpgquality = ureg.Read("jpgquality");
                legend = ureg.Read("legend").ToUpper();
                ntpserver = ureg.Read("ntpserver");
                ntpport = ureg.Read("ntpport");
                perscreen = ureg.Read("perscreen").ToUpper();
                signature = ureg.Read("signature").ToUpper();
                serialKey = ureg.Read("serialkey").ToUpper();
                customer = ureg.Read("customer").ToString();
                customerMN = ureg.Read("customerMN").ToString();
                customerWord = ureg.Read("customerWord").ToString();
                customerDueDate = ureg.Read("customerDueDate").ToString();
                wstartup = ureg.Read("wstartup").ToUpper();
                licaddress = ureg.Read("licaddress").ToUpper();
                trialLicense = "false";
                if (ureg.KeyExists("UdyatTrial"))
                {
                    trialLicense = ureg.Read("UdyatTrial");
                }
                serverHost = ureg.Read("server").ToString();
                serverPort = ureg.Read("serverport").ToString();
                clientPort = ureg.Read("clientport").ToString();
                return true;
            }
            catch
            {
                return false;
            }            
        }

        private string GetLastSerialkey()
        {
            WinRegistry ureg = new WinRegistry();
            try
            {
                if (ureg.KeyExists("serialkey"))
                {
                    return ureg.Read("serialkey");
                }                
            }
            catch
            {
                return "";
            }
            return "";
        }

        private bool IsIniInRegister()
        {
            WinRegistry ureg = new WinRegistry();
            return ureg.KeyExists("attempts") &&
                   ureg.KeyExists("proxy") &&
                   ureg.KeyExists("host") &&
                   ureg.KeyExists("port") &&
                   ureg.KeyExists("defaultAuth") &&
                   ureg.KeyExists("domain") &&
                   ureg.KeyExists("user") &&
                   ureg.KeyExists("password") &&
                   ureg.KeyExists("mode") &&
                   ureg.KeyExists("locallog") &&
                   ureg.KeyExists("clock") &&
                   ureg.KeyExists("autodir") &&
                   ureg.KeyExists("movefile") &&
                   ureg.KeyExists("moveafter") &&
                   ureg.KeyExists("target") &&
                   ureg.KeyExists("procdir") &&
                   ureg.KeyExists("interval") &&
                   ureg.KeyExists("wstartup") &&
                   ureg.KeyExists("interval") &&
                   ureg.KeyExists("imgtype") &&
                   ureg.KeyExists("jpgquality") &&
                   ureg.KeyExists("legend") &&
                   ureg.KeyExists("ntpserver") &&
                   ureg.KeyExists("ntpport") &&
                   ureg.KeyExists("perscreen") &&
                   ureg.KeyExists("signature") &&
                   ureg.KeyExists("serialkey") &&
                   //ureg.KeyExists("customer") &&
                   //ureg.KeyExists("customerMN") &&
                   //ureg.KeyExists("customerDueDate") &&
                   ureg.KeyExists("wstartup") &&
                   ureg.KeyExists("licaddress") &&
                   ureg.KeyExists("server") &&
                   ureg.KeyExists("serverport") &&
                   ureg.KeyExists("clientport");
        }

        public bool RemoveIniFromRegister()
        {
            WinRegistry ureg = new WinRegistry();
            bool result = ureg.DeleteKey("attempts") &&
                ureg.DeleteKey("proxy") &&
                ureg.DeleteKey("host") &&
                ureg.DeleteKey("port") &&
                ureg.DeleteKey("defaultAuth") &&
                ureg.DeleteKey("domain") &&
                ureg.DeleteKey("user") &&
                ureg.DeleteKey("password") &&
                ureg.DeleteKey("mode") &&
                ureg.DeleteKey("locallog") &&
                ureg.DeleteKey("clock") &&
                ureg.DeleteKey("autodir") &&
                ureg.DeleteKey("movefile") &&
                ureg.DeleteKey("moveafter") &&
                ureg.DeleteKey("target") &&
                ureg.DeleteKey("procdir") &&
                ureg.DeleteKey("interval") &&
                ureg.DeleteKey("wstartup") &&
                ureg.DeleteKey("interval") &&
                ureg.DeleteKey("imgtype") &&
                ureg.DeleteKey("jpgquality") &&
                ureg.DeleteKey("legend") &&
                ureg.DeleteKey("ntpserver") &&
                ureg.DeleteKey("ntpport") &&
                ureg.DeleteKey("perscreen") &&
                ureg.DeleteKey("signature") &&
                ureg.DeleteKey("serialkey") &&
                ureg.DeleteKey("customer") &&
                ureg.DeleteKey("customerMN") &&
                ureg.DeleteKey("customerDueDate") &&
                ureg.DeleteKey("wstartup") &&
                ureg.DeleteKey("licaddress") &&
                ureg.DeleteKey("server") &&
                ureg.DeleteKey("serverport") &&
                ureg.DeleteKey("clientport");
            if (ureg.KeyExists("customer"))
            {
                ureg.DeleteKey("UdyatTrial");
            }
            if (ureg.KeyExists("UdyatRegister"))
            {
                ureg.DeleteKey("UdyatRegister");
            }
            if (ureg.KeyExists("MacSalt"))
            {
                ureg.DeleteKey("MacSalt");
            }
            if (ureg.KeyExists("customerWord"))
            {
                ureg.DeleteKey("customerWord");
            }           
            return result;
        }

        private void ApplyConfig()
        {
            //*******************
            // CONNECTION
            //*******************
            int intattempts;
            configRecord.ConnectionAttempts = 0;
            if (int.TryParse(attempts, out intattempts))
            {
                configRecord.ConnectionAttempts = intattempts;
            }
            configRecord.Proxy.UseProxy = proxy == "ON";
            configRecord.Proxy.Domain = domain;
            configRecord.Proxy.Host = proxyHost;
            int auxPort;
            if (int.TryParse(port, out auxPort))
            {
                configRecord.Proxy.Port = auxPort;
            }
            else
            {
                configRecord.Proxy.Port = 0;
            }            
            configRecord.Proxy.DefaultAuthentication = defaultAuth == "ON";
            configRecord.Proxy.User = user;
            configRecord.Proxy.Password = password;

            //*******************
            // EXECUTION
            //*******************

            // Mode de execução (0=produção 1=teste)
            configRecord.TestMode = modeType == "1";
            // On = Ativa log local com informações cotidianas do sistema
            configRecord.UseLocalLog = locallog == "ON";
            // Tempo em milesegundos para o processamento do relógio interno do sistema
            int intClock;
            if (int.TryParse(clock, out intClock))
            {
                configRecord.ClockLoop = intClock;
            }
            else
            {
                configRecord.ClockLoop = 5000;
            }
            // Indica se a aplicação irá utilizar ou diretório temporário (on) ou um personalizado (off) para armazenar as imagens temporariamente
            configRecord.AutoTempPath = autodir == "ON";
            // on = Indica que é para mover as imagens capturadas para uma pasta 
            configRecord.MoveImagesToTarget = movefile == "ON";
            // Número de imagens que devem ser capturadas antes que o sistema envie as imagens para a pasta configurada na seção TARGET
            int intNoveAfterSeq;
            if (int.TryParse(moveafter, out intNoveAfterSeq))
            {
                configRecord.MoveFileAfterSeq = intNoveAfterSeq;
            }
            else
            {
                configRecord.MoveFileAfterSeq = 30;
            }
            // Indica se a pasta destino das imagens é a pasta padrão do sistema (output) ou outra
            configRecord.UseDefaultTargetPath = target == "DEFAULT";
            if ((!configRecord.UseDefaultTargetPath) && (!Directory.Exists(target)))
            {
                configRecord.UseDefaultTargetPath = true;
            }
            // Caminho temporário alternativo (quando não se usa caminho temporário automático)
            configRecord.AlternativeTempPath = procdir;
            // Pasta temporária (Caminho de armazenamento temporário das imagens capturadas)
            if (!configRecord.AutoTempPath)
            {
                TempPath = configRecord.AlternativeTempPath;
                configRecord.AutoTempPath = !Directory.Exists(targetPath);
            }
            if (configRecord.AutoTempPath)
            {
                // Pasta temporária onde o vídeo será salvo temporariamente
                TempPath = Path.GetTempPath() + tempOutputPathName + @"\";
                if (!Directory.Exists(TempPath))
                {
                    Util.CreateDir(TempPath, "Criando pasta temporária");
                }
            }
            // Pasta destino onde as imagens serão enviadas
            if (configRecord.UseDefaultTargetPath)
            {
                // Path onde as imagens serão salvas
                TargetPath = AppDomain.CurrentDomain.BaseDirectory + targetOutputPathName + @"\";
                if (!Directory.Exists(TargetPath))
                {
                    Util.CreateDir(TargetPath, "Criando pasta destino");
                }
            }
            else
            {
                TargetPath = Util.AddBackslashInPath(target);
            }
            // Guarda o identificar único da máquina 
            DriveInfo sdrive = new DriveInfo(TempPath);
            machineUniqueID = Util.MachineUniqueID(sdrive.Name);

            // Inicializa automaticamente o sistema (ou não) ao iniciar o Windows
            configRecord.StartupWindows = wstartup == "ON";

            //*******************
            // CAPTURE
            //*******************
            
            // Tempo para disparo da thread que realiza o print
            int intInterval;
            if (int.TryParse(interval, out intInterval))
            {
                configRecord.IntervalToPrint = intInterval;
            }
            // Formato da imagem
            if (imgtype == "1")
            {
                configRecord.ImgFormat = ImageFormat.Jpeg;
                configRecord.ImageCodec = Util.GetEncoderInfo("image/jpeg");
                int imgQuality;
                if (Int32.TryParse(jpgquality, out imgQuality))
                {
                    imgQuality = imgQuality > 100 ? 100 : imgQuality < 0 ? 0 : imgQuality;
                    configRecord.JPGQuality = Convert.ToInt32(imgQuality);                    
                }
                else
                {
                    imgQuality = 80;
                }
                configRecord.QualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, configRecord.JPGQuality);
                configRecord.EncoderParams = new EncoderParameters(1);
                configRecord.EncoderParams.Param[0] = configRecord.QualityParam;
            }
            else if (imgtype == "3")
            {
                configRecord.ImgFormat = ImageFormat.Gif;
            }
            else
            {
                configRecord.ImgFormat = ImageFormat.Png;
            }
            configRecord.ExtImgFormat = configRecord.ImgFormat == ImageFormat.Png ? ".png" :
                                        configRecord.ImgFormat == ImageFormat.Jpeg ? ".jpg" :
                                        configRecord.ImgFormat == ImageFormat.Gif ? ".gif" : 
                                        ".png";
            // Indica se é para adicionar legenda nas imagens capturadas            
            configRecord.UseLegend = legend == "ON";
            // Servidor e porta do servidor NTP para ajuste do horário
            if (ntpserver.Length > 0)
            {
                configRecord.NTPServer = ntpserver;
            }
            int auxntpPort;
            if (int.TryParse(ntpport, out auxntpPort))
            {
                configRecord.NTPPort = auxntpPort;
            }
            else
            {
                configRecord.NTPPort = 123;
            }            
            // Indica que é para salvar 1 imagem por monitor
            configRecord.OneImagePerScreen = perscreen == "ON";
            // Assinatura das imagens com informações de segurança
            configRecord.UseSignature = signature == "ON";

            //*******************
            // COSTUMER
            //*******************

            configRecord.CustomerWord = "";
            if (configRecord.UseSignature)
            {
                // Palavra chave do cliente
                configRecord.CustomerWord = customerWord;
            }
            // Chave de ativiação do sistema
            configRecord.OldSerialKey = oldSerialKey;
            configRecord.SerialKey = serialKey;
            // Código único de Identificação do cliente
            configRecord.CustomerID = customer;
            // Identificação da máquina do usuário
            configRecord.CustomerMacNumber = customerMN;
            // Endereço para o qual a licença foi comprada
            configRecord.LicenseAddress = licaddress;
            // True se a licença for TRIAL (temporária)
            bool auxTrial;
            if (bool.TryParse(trialLicense, out auxTrial))
            {
                configRecord.IsTrialLicense = auxTrial;
            }
            else
            {
                configRecord.IsTrialLicense = true;
            }

            //*******************
            // P2P
            //*******************

            configRecord.ServerHost = serverHost;
            if (int.TryParse(serverPort, out auxPort))
            {
                configRecord.ServerPort = auxPort;
            }
            else
            {
                configRecord.ServerPort = 0;
            }
            if (int.TryParse(clientPort, out auxPort))
            {
                configRecord.ClientPort = auxPort;
            }
            else
            {
                configRecord.ClientPort = 0;
            }
        }

    }
}
