using CodeReflection.ScreenCapturingDemo;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class DataConfig
    {
        // Palavra chave do cliente em modo teste
        public static string wordClientToTest = "Teste Udyat";
        private const int maxImagesToTest = 10;
        private bool testMode;
        private bool startupWindows = true;
        private int clockLoop = 200;
        private int intervalToPrint = 2000;
        private ImageFormat imgFormat;
        private int jpgQuality = 80;
        private ImageCodecInfo imageCodec;
        private EncoderParameter qualityParam;
        private EncoderParameters encoderParams;
        private string extImgFormat;
        private bool oneImagePerScreen = false;
        private int moveFileAfterSeq = 10;
        private bool autoTempPath = true;
        private bool moveImagensToTarget = false;
        private bool useDefaultTargetPath = true;
        private string alternativeTempPath;
        private bool useSignature = false;
        private string oldSerialKey;
        private string serialKey;
        private string customerID;
        private string customerMacNumber;
        private string customerWord;
        private bool useLocalLog = false;
        private bool useLegend = false;
        private string ntpServer;
        private int ntpPort;
        private string licAddress;
        private int attempts = 0;
        private DataProxy proxy = new DataProxy();
        private bool isTrialLicense = false;
        private string serverHost;
        private int serverPort;
        private int clientPort;

        /// <summary>
        /// Retorna true se o modo for teste
        /// </summary>
        public bool TestMode
        {
            get { return testMode; }
            set { testMode = value; }
        }

        /// <summary>
        /// Total de imagens que serão gerada ao usar o sistema no modo teste
        /// </summary>
        public int MaxImagesToTest
        {
            get { return maxImagesToTest; }
        }

        /// <summary>
        /// Retorna true se o sistema deve iniciar junto com o windows (padrão true)
        /// </summary>
        public bool StartupWindows
        {
            get { return startupWindows; }
            set
            {
                startupWindows = value;
                if (startupWindows)
                {
                    Util.AddToStartUpWindows(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    Util.RemoveFromStartUpWindows();
                }
            }
        }

        /// <summary>
        /// Ciclo em milesegundos do relógio interno
        /// </summary>
        public int ClockLoop
        {
            get { return clockLoop; }
            set { clockLoop = value; }
        }

        /// <summary>
        /// Tempo em milesegundos para captura das imagens 
        /// </summary>
        public int IntervalToPrint
        {
            get { return intervalToPrint; }
            set { intervalToPrint = value; }
        }

        /// <summary>
        /// Formato em que a imagem será salva
        /// </summary>
        public ImageFormat ImgFormat
        {
            get { return imgFormat; }
            set { imgFormat = value; }
        }

        /// <summary>
        /// Número entre 0 e 100 que representa a qualidade da imagem (próximo de 0 = menor qualidade, próximo de 100 = maior qualidade)
        /// Válido somente para imagens JPG
        /// </summary>
        public int JPGQuality
        {
            get { return jpgQuality; }
            set { jpgQuality = value; }
        }

        /// <summary>
        /// Parâmetros de qualidade de um CODEC
        /// </summary>
        public EncoderParameter QualityParam
        {
            get { return qualityParam; }
            set { qualityParam = value; }
        }

        /// <summary>
        /// Parâmetros para CODEC da imagem
        /// </summary>
        public EncoderParameters EncoderParams
        {
            get { return encoderParams; }
            set { encoderParams = value; }
        }

        /// <summary>
        /// CODEC para o tipo de imagem
        /// </summary>
        public ImageCodecInfo ImageCodec
        {
            get { return imageCodec; }
            set { imageCodec = value; }
        }

        /// <summary>
        /// Extensão do formato da imagem
        /// </summary>
        public string ExtImgFormat
        {
            get { return extImgFormat; }
            set { extImgFormat = value; }
        }

        /// <summary>
        /// Indica que é para salvar uma imagem por monitor (se houver 2 monitores: salva duas imagens)
        /// </summary>
        public bool OneImagePerScreen
        {
            get { return oneImagePerScreen; }
            set { oneImagePerScreen = value; }
        }

        /// <summary>
        /// Total de imagens que devem ser feitas antes de enviar para o destino
        /// </summary>
        public int MoveFileAfterSeq
        {
            get { return moveFileAfterSeq; }
            set { moveFileAfterSeq = value; }
        }

        /// <summary>
        /// Indica se é para utilizar o path temporário do windows (auto) ou não (outro path)
        /// </summary>
        public bool AutoTempPath
        {
            get { return autoTempPath; }
            set { autoTempPath = value; }
        }

        /// <summary>
        /// Indica se é para mover as imagens capturadas para uma pasta da rede
        /// </summary>
        public bool MoveImagesToTarget
        {
            get { return moveImagensToTarget; }
            set { moveImagensToTarget = value; }
        }

        /// <summary>
        /// Indica se é para enviar as imagens capturadas para a pasta destino padrão (...exe path\output)
        /// </summary>
        public bool UseDefaultTargetPath
        {
            get { return useDefaultTargetPath; }
            set { useDefaultTargetPath = value; }
        }

        /// <summary>
        /// Caminho temporário alternativo (quando não se usa caminho temporário automático)
        /// </summary>
        public string AlternativeTempPath
        {
            get { return alternativeTempPath; }
            set { alternativeTempPath = value; }
        }

        /// <summary>
        /// Indica se utiliza assinatura em cada imagem
        /// </summary>
        public bool UseSignature
        {
            get { return useSignature; }
            set { useSignature = value; }
        }

        /// <summary>
        /// Retorna a última serial key salva no registro de instalação
        /// </summary>
        public string OldSerialKey
        {
            get { return oldSerialKey; }
            set { oldSerialKey = value; }
        }

        /// <summary>
        /// Chave de ativação do sistema
        /// </summary>
        public string SerialKey
        {
            get { return serialKey; }
            set { serialKey = value; }
        }

        /// <summary>
        /// ID único do cliente na base de dados 
        /// </summary>
        public string CustomerID
        {
            get { return customerID; }
            set { customerID = value; }
        }

        /// <summary>
        /// Número da máquina do cliente (se o cliente tem licença para 3 máquinas: então customerMacNumber pode conter 1 ou 2 ou 3)
        /// </summary>
        public string CustomerMacNumber
        {
            get { return customerMacNumber; }
            set { customerMacNumber = value; }
        }

        /// <summary>
        /// Palavra chave do cliente que será embutida na imagem com hash FNV (Fowler-Noll-Vo)
        /// </summary>
        public string CustomerWord
        {
            get
            {
                if (testMode)
                {
                    return wordClientToTest;
                }
                return customerWord;
            }
            set { customerWord = value; }
        }

        /// <summary>
        /// Endereço para o qual a licença foi comprada
        /// Esse endereço deve ser informado em casos onde o IP da máquina local não corresponde ao endereço da licença
        /// </summary>
        public string LicenseAddress
        {
            get { return licAddress; }
            set { licAddress = value; }
        }

        /// <summary>
        /// Número que indica a quantidade de tentativas de conexão quando o sistema não consegue se conectar na internet
        /// O sistema é finalizado depois que o número de tentativas é ultrapassado
        /// Se o número for 0 (zero), então o sistema não faz qualquer tentativa e finaliza.
        /// Se o número for 999 então o sistema continua tentanto (o sistema não é finalizado)
        /// </summary>
        public int ConnectionAttempts
        {
            get { return attempts; }
            set { attempts = value; }
        }

        /// <summary>
        /// Indica se o sistema deverá ou não salvar um log local com informações de execução cotidiana do sistema
        /// </summary>
        public bool UseLocalLog
        {
            get { return useLocalLog; }
            set { useLocalLog = value; }
        }

        /// <summary>
        /// Indica se o sistema deve ou não adicionar legenda nas imagens
        /// </summary>
        public bool UseLegend
        {
            get { return useLegend; }
            set
            {
                useLegend = value;
                ScreenCapturing.LegendData.UseLegend = value;
            }
        }

        /// <summary>
        /// Servidor NTP para ajuste do horário
        /// </summary>
        public string NTPServer
        {
            get { return ntpServer; }
            set
            {
                ntpServer = value;
                NTPDateTime.ntpServer = value;
            }
        }

        /// <summary>
        /// Porta do servidor NTP para ajuste do horário
        /// </summary>
        public int NTPPort
        {
            get { return ntpPort; }
            set
            {
                ntpPort = value;
                NTPDateTime.ntpPort = value;
            }
        }

        /// <summary>
        /// Informações de conexão com o proxy
        /// </summary>
        public DataProxy Proxy
        {
            get { return proxy; }
        }

        /// <summary>
        /// True quando a licença é TRIAL (temporária)
        /// </summary>
        public bool IsTrialLicense
        {
            get { return isTrialLicense; }
            set { isTrialLicense = value; }
        }

        /// <summary>
        /// Nome ou IP do servidor P2P
        /// </summary>
        public string ServerHost
        {
            get { return serverHost; }
            set { serverHost = value; }
        }

        /// <summary>
        /// Porta do servidor P2P
        /// </summary>
        public int ServerPort
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        /// <summary>
        /// Porta de escuta do cliente P2P
        /// </summary>
        public int ClientPort
        {
            get { return clientPort; }
            set { clientPort = value; }
        }

    }

    public class DataProxy
    {
        private bool useProxy;
        private string host;
        private int port;
        private bool defaultAuth;
        private string domain;
        private string user;
        private string password;

        /// <summary>
        /// Indica se é ou não para usar proxy nas conexões
        /// </summary>
        public bool UseProxy
        {
            get { return useProxy; }
            set { useProxy = value; }
        }

        /// <summary>
        /// Host ou IP do servidor de proxy que é utilizado para acesso com a internet
        /// </summary>
        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        /// <summary>
        /// Número da porta do proxy 
        /// </summary>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// on = utiliza as credenciais padrão (utiliza usuário, senha e domínio do usuário logado no Windows) 
        /// off = não utiliza as credenciais padrão. Nesse caso o sistema procura pelas credenciais informadas nas chaves DOMAIN, USER e PASSWORD
        /// </summary>
        public bool DefaultAuthentication
        {
            get { return defaultAuth; }
            set { defaultAuth = value; }
        }

        /// <summary>
        /// Nome do domínio de rede que o usuário do proxy irá utilizar
        /// </summary>
        public string Domain
        {
            get { return domain; }
            set { domain = value; }
        }

        /// <summary>
        /// Nome do usuário de rede que será logado no proxy
        /// </summary>
        public string User
        {
            get { return user; }
            set { user = value; }
        }

        /// <summary>
        /// Senha do usuário de rede que será logado no proxy 
        /// </summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

    }
}
