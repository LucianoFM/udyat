
using CodeReflection.ScreenCapturingDemo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Udyat.Class;
using Udyat.Class.Core;
using Udyat.Class.UDPSocket;

namespace Udyat
{

    public class mainClass
    {
        private const string CMD_INSTAL = "-INSTALL";
        private const string CMD_INSTAL_NOCONSOLE = "-NC";
        private const string CMD_UNINSTAL = "-UNINSTALL";
        private const string CMD_QUIT = "-QUIT";
        private const string CMD_CHKIMAGE = "-VI";
        private const string DATA_DELIMITER = "[_]";
        // Thread do Relógio interno do sistema (executa a cada 500 milisegundos)
        private Timer ClockTimer;
        // Data anterior ao giro de um clock do relógio interno
        private static DateTime DataBeforeClock;
        // Relógio interno do sistema
        public static DateTime internalClock;
        // Thread de processamento da captura
        private Timer Timer1;
        private TimerCallback callbackTemp;
        // Contador de Imagens geradas (reiniciado a cada envio das imagens para o destino)
        private int Seq = 0;
        // Endereço físico da máquina
        private string MACAddress;
        // Identificar da assinatura do Udyat
        private const string UDYATSIGN = "UDYATwatchingYOU";
        // Lista com o nome de cada imagen gerada (inicializada após cada envio para o destino) 
        // Foi utilizado ArrayList devido ao método SyncRoot que garante acesso sincronizado na lista
        private ArrayList filesToSend = new ArrayList();
        // Indica se está movendo os arquivos (necessário para evitar sobreprosição de threads)
        public static bool movingFiles = false;
        // Configurações do sistema
        private Config sysConfig;
        // Ação solicitada via linha de comanda
        private string command = "";
        private string subCommand1 = "";
        // Indica que mensagens de console serão (true) ou não (false) exibidas
        private bool showConsoleMessages = true;

        public mainClass()
        {            
            // Cria o objeto de legendas
            ScreenCapturing.LegendData = new DataImage();          

            // Indica se o arquivo INI existe
            bool iniExists = File.Exists(Config.ConfigFullFileName);

            // Parâmetros de linha de comando que não necessitam de parâmetros do arquivo de configuração .ini            
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                command = args[1].ToUpper();

                if (args.Length > 2)
                {
                    subCommand1 = command = args[2].ToUpper();
                    showConsoleMessages = subCommand1 == CMD_INSTAL_NOCONSOLE;
                }                

                if ((command == CMD_INSTAL) && (!iniExists))
                {
                    Util.ShowConsoleMessage("Arquivo de configuração não encontrado", showConsoleMessages);
                    Environment.Exit(0);
                }
                else if (command == CMD_QUIT)
                {
                    // Finaliza o processo que estiver em execução
                    EndRunningProcess();
                    Environment.Exit(0);
                }
            }

            // Carrega as configurações do sistema
            sysConfig = new Config();
            try
            {
                if (!sysConfig.LoadConfig())
                {
                    Util.ShowConsoleMessage("Arquivo de configuração não encontrado", showConsoleMessages);
                    Environment.Exit(0);
                }
            } catch (Exception ex)
            {
                if (command == CMD_INSTAL)
                {
                    Util.ShowConsoleMessage("Erro ao ler o .ini: " + ex.Message, showConsoleMessages);
                }
                Environment.Exit(0);
            }
            
            
            // Arquivo de log
            Log.logFullFileName = sysConfig.TargetPath + string.Format("{0:yyyyMMdd}", DateTime.Now) + "_" + Log.LOG_FILE;
                        

            // Testa a conexão com a internet
            // Se não tem conexão com a internet: então fecha o sistema.
            if (!HasConection())
            {
                Environment.Exit(0);
            }

            // Iniciliza o relógio do sistema buscando a data e hora brasileira
            internalClock = NTPDateTime.GetNetworkTime(sysConfig.Data.Proxy);
            ClockTimer = new Timer(new TimerCallback(ClockTimerTick), null, 0, sysConfig.Data.ClockLoop);

            // Adiciona log informando que está em modo de teste
            Log.AddStartLog(sysConfig.Data.TestMode, sysConfig.TempPath, sysConfig.TargetPath);

            // Inicia a Thread de comunicação P2P com o Servidor
            try
            {
                StartServerConnection();
            }
            catch (Exception ex)
            {
                Log.AddLog("P2P", "Erro: " + ex.Message);
            }
            

            // Mode teste (gera somente 10 imagens)
            // sysConfig.Data.TestMode = true;



            // Parâmetros de linha de comando que necessitam de parâmetros
            if (args.Length > 1)
            {
                if (command == CMD_CHKIMAGE) // Verificação de Imagem
                {
                    // Verifica se uma imagem foi gerada pelo Udyat
                    ImageIsUdyat(args);
                    Environment.Exit(0);
                }
                else if (command == CMD_INSTAL) // Instalação do sistema
                {                    
                    // Tenta ativar a licença
                    License actLicense = new License(sysConfig.Data);
                    try
                    {
                        if (!actLicense.ActivateLicense())
                        {
                            if (actLicense.ExceptionMessage.Length > 0)
                            {
                                Log.AddLog("License", "Erro: " + actLicense.ExceptionMessage);
                                Util.ShowConsoleMessage(actLicense.ExceptionMessage, showConsoleMessages);
                            }
                            else
                            {
                                Log.AddLog("License", "Erro: essa licença não autorizada");
                                Util.ShowConsoleMessage("Essa não é uma licença válida.", showConsoleMessages);
                            }                            
                            Environment.Exit(0);
                        }
                        // Adiciona os parâmetros do arquivo .ini no registro do windows
                        sysConfig.SaveIniInWinRegister();
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("License", "Erro de validação da licença: " + ex.Message);
                        Util.ShowConsoleMessage("Erro ao tentar validar a licença: " + ex.Message, showConsoleMessages);
                        Environment.Exit(0);
                    }
                }
                else if (command == CMD_UNINSTAL) // Desinstalação do sistema
                {
                    // Encerra o processamento do sistema que estiver sendo executado (se houver algum)
                    EndRunningProcess(false);
                    // Elimina os registros de configuração do sistema
                    sysConfig.RemoveIniFromRegister();
                    // Finaliza
                    Environment.Exit(0);
                }

            }            

            // Finaliza a aplicação se ela já estiver sendo executada
            if (System.Diagnostics.Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Log.AddLog("Processo", "Tentativa de execução com processo rodando");
                Environment.Exit(0);
            }
            
            // Verifica LICENÇA do sistema quando não for a instalação
            if (command != CMD_INSTAL)
            {
                if (!License.IsLicenseOk(sysConfig.Data))
                {
                    Environment.Exit(0);
                }
            }

            // MAC Address da máquina
            MACAddress = Util.GetMacAddres();

            // Dados do build
            AssemblyName versionIndo = typeof(mainClass).Assembly.GetName();

            // Configurações de legenda
            ScreenCapturing.Version = versionIndo.Name + " " + versionIndo.Version.ToString();
            ScreenCapturing.BitmapLogo = Udyat.Properties.Resources.logo;
            ScreenCapturing.LegendFont = new Font("Tahoma", 15);
            ScreenCapturing.LegendDateFont = new Font("Tahoma", 30);
            ScreenCapturing.LegendColor = Color.Gray;
            ScreenCapturing.FooterBackgroundColor = Color.Black;            
            ScreenCapturing.LegendData.IP = Util.GetLocalIpAddress();
            ScreenCapturing.LegendData.IsTrialVersion = sysConfig.Data.IsTrialLicense;

            // Se existir arquivos de imanges no temp (que ainda não foram enviados), então envia para o destino
            MoveExistingFiles();

            // Inicia o timer
            if (sysConfig.Data.TestMode)
            {
                // Mode teste
                callbackTemp = new TimerCallback(ModeTestSaveTempVideoTick);
                Timer1 = new Timer(callbackTemp, null, 0, sysConfig.Data.IntervalToPrint);
            }
            else
            {
                // Mode produção
                callbackTemp = new TimerCallback(SaveTempVideoTick);
                Timer1 = new Timer(callbackTemp, null, 0, sysConfig.Data.IntervalToPrint);
            }            
        }

        /// <summary>
        /// Retorna true se existe conexão com a internet
        /// Tenta conectar de acordo com os parâmetros de configuração
        /// </summary>
        /// <returns></returns>
        private bool HasConection()
        {
            string conType;
            bool goTest, hasConn;
            int countTest = 0;
            do
            {
                countTest += 1;
                hasConn = Util.HasInternetConnection(out conType);
                goTest = (!hasConn) && ((sysConfig.Data.ConnectionAttempts != 0) || (sysConfig.Data.ConnectionAttempts == 999) || (countTest <= sysConfig.Data.ConnectionAttempts));
            } while (goTest);
            // Adiciona no log o tipo de conexão com a internet
            Log.AddLog("Internet", "Conexão: " + conType);
            return hasConn;
        }

        /// <summary>
        /// Verifica se uma imagem foi gerada pelo sistema
        /// A verificação é salva no arquivo de Log
        /// </summary>
        /// <param name="args"></param>
        private void ImageIsUdyat(string[] args)
        {
            if (Path.GetExtension(args[2]).ToUpper() != ".PNG")
            {
                Log.AddLog("-VI" + args[2], "Imagem não verificada. Somente arquivos PNG.");
                Util.ShowConsoleMessage("Somente imagens PNG podem ser verificadas.", showConsoleMessages);
                Environment.Exit(0);
            }
            if ((args.Length >= 3) && (args[2].Length > 0) && (File.Exists(args[2])))
            {
                string auxFileName = args[2];
                Bitmap teste = new Bitmap(auxFileName);

                string msg = SteganographyHelper.extractText(teste);
                if (msg.Length > UDYATSIGN.Length)
                {
                    string UdyatSign = MsgValue(msg, out msg);
                    if (UdyatSign == UDYATSIGN)
                    {
                        string customerIdent = MsgValue(msg, out msg);
                        string customerMacNum = MsgValue(msg, out msg);
                        string customerHash = MsgValue(msg, out msg);
                        string machineUID = MsgValue(msg, out msg);
                        string embedfileName = msg.Substring(0, msg.Length);
                        string strUserName = MsgValue(msg, out msg);
                        string macAddress = MsgValue(msg, out msg);
                        string macIP = MsgValue(msg, out msg);
                        string imgSeqMonitor = msg.Substring(22, msg.Length - 22).Substring(0, msg.Substring(22, msg.Length - 22).IndexOf("."));
                        string prtDateTime = msg.Substring(0, 21);
                        DateTime outputDateTimeValue;
                        string strDateTime;
                        if (DateTime.TryParseExact(prtDateTime, "yyyyMMdd_HH_mm_ss_fff", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out outputDateTimeValue))
                        {
                            strDateTime = outputDateTimeValue.ToString("dd/MM/yyyy HH:mm:ss FFF");
                        }
                        string customerWord = sysConfig.Data.CustomerWord;
                        string wordClienth = FnvHash.GetHash(customerWord, 120).ToHexString();
                        bool testSecurity = ((wordClienth == customerHash) && (embedfileName == auxFileName));
                        Log.AddLog("-VI " + args[2], "Imagem válida");
                        return;
                    }
                }
                Log.AddLog("-VI" + args[2], "Imagem inválida");
                return;
            }
            Log.AddLog("-VI", "Parâmetros inválidos");
        }

        /// <summary>
        /// Finaliza o sistema, eliminando o processo 
        /// </summary>
        public void EndRunningProcess(bool pShowErrorMessage = true)
        {
            Process[] myProcess = System.Diagnostics.Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (myProcess.Length > 1)
            {
                Process myOldestProcess = myProcess.OrderBy(it => it.StartTime).FirstOrDefault();
                try
                {
                    if (myOldestProcess != null)
                    {
                        myOldestProcess.Close();
                        Util.ShowConsoleMessage("Processo encerrado adequadamente.", showConsoleMessages);
                        return;
                    }
                    Util.ShowConsoleMessage("Tentativa de encerrar o processo, porém nenhum processo foi localizado.", showConsoleMessages);
                }
                catch (Exception ex)
                {
                    Util.ShowConsoleMessage("Erro ao tentar encerra processo: " + ex.Message, showConsoleMessages);
                }
            }
            else if (pShowErrorMessage)
            {
                Util.ShowConsoleMessage("Tentativa de encerrar o processo, mas nenhum processo localizado.", showConsoleMessages);
            }            
        }

        /// <summary>
        /// Realiza a captura da tela (modo produção)
        /// Move os arquivos (a cada N capturas) para a pasta destino
        /// Não captura quando os arquivos estão sendo movidos para a pasta destino
        /// </summary>
        /// <param name="stateInfo"></param>
        private void SaveTempVideoTick(object stateInfo)
        {
            if (movingFiles)
            {
                return;
            }
            CreatePrintScreen();
            if (Seq == sysConfig.Data.MoveFileAfterSeq)
            {
                movingFiles = true;
                MoveFilesToTarget();
                Seq = !sysConfig.Data.TestMode ? 0 : Seq;
                movingFiles = false;
            }
        }

        /// <summary>
        /// Realiza a captura da tela (modo teste)
        /// Move os arquivos (a cada N capturas) para a pasta destino e finaliza o sistema
        /// </summary>
        /// <param name="stateInfo"></param>
        private void ModeTestSaveTempVideoTick(object stateInfo)
        {
            if (movingFiles)
            {
                return;
            }
            SaveTempVideoTick(stateInfo);
            if ((sysConfig.Data.TestMode) && (Seq >= sysConfig.Data.MaxImagesToTest))
            {
                // Adiciona log 
                Log.AddLog("Fim", "Modo Teste");
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Relógio interno da aplicação
        /// Dispara o evento DayChanged quando ocorre a troca do dia
        /// </summary>
        /// <param name="stateInfo"></param>
        private void ClockTimerTick(object stateInfo)
        {
            DataBeforeClock = internalClock;
            internalClock = internalClock.AddMilliseconds(sysConfig.Data.ClockLoop);
            // Dispara ação sempre que troca o dia
            if (internalClock.Day != DataBeforeClock.Day)
            {
                DayChanged(DataBeforeClock);
            }
        }

        /// <summary>
        /// Valida se a licença é válida. Se não for: finaliza o sistema
        /// Esse método é executado sempre que existe uma virada de dia
        /// </summary>
        /// <param name="pDateBeforeClock"></param>
        private void DayChanged(DateTime pDateBeforeClock)
        {
            // Testa a conexão com a internet
            // Se não tem conexão com a internet: então fecha o sistema.
            if (!HasConection())
            {
                Environment.Exit(0);
            }
            // Verifica a licença (se não for válida: fecha o sistema)
            if (!License.IsLicenseOk(sysConfig.Data))
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Método responsável pela captura das imagens
        /// O nome de imagem capturada é adiciona numa lista que, posteriormente, é percorrida para enviar as imagens para a pasta destino
        /// </summary>
        private void CreatePrintScreen()
        {
            Seq += 1;
            // Sequência 
            ScreenCapturing.LegendData.FrameID = Seq;
            // Captura a tela
            Image[] imgList;
            if (sysConfig.Data.OneImagePerScreen)
            {
                imgList = ScreenCapturing.GetDesktopWindowCaptureAsBitmaps();
            }
            else
            {
                // Captura uma única imagem para todos monitores
                imgList = new Image[1];
                imgList[0] = ScreenCapturing.GetDesktopWindowCaptureAsBitmap();
            }
            for (int i = 0; i < imgList.Length; i++)
            {
                Image tempImg = imgList[i];
                try
                {
                    // Nome do arquivo
                    string fileToSave = Environment.UserName + DATA_DELIMITER +
                                        MACAddress + DATA_DELIMITER +
                                        Util.GetLocalIpAddress() + DATA_DELIMITER +
                                        string.Format("{0:yyyyMMdd_HH_mm_ss_fff}", internalClock) + DATA_DELIMITER +
                                        i.ToString() +
                                        sysConfig.Data.ExtImgFormat;
                    // No do arquivo temporário (com path)
                    string tempFullFileToSave = sysConfig.TempPath + fileToSave;
                    // Adiciona assinatura com dados de segurança na imagem quando a imagem for PNG
                    if ((sysConfig.Data.ImgFormat == ImageFormat.Png) && (sysConfig.Data.UseSignature))
                    {
                        // Pabavra chave de segurança e identificação que é embutida na imagem
                        string wordSecurity = UDYATSIGN + DATA_DELIMITER +
                                              sysConfig.Data.CustomerID.ToString() + DATA_DELIMITER +
                                              sysConfig.Data.CustomerMacNumber.ToString() + DATA_DELIMITER + 
                                              FnvHash.GetHash(sysConfig.Data.CustomerWord, 120).ToHexString() + DATA_DELIMITER +
                                              sysConfig.MachineUniqueID + DATA_DELIMITER + 
                                              fileToSave;
                        // Esconde o texto na imagem (esteganografia)
                        SteganographyHelper.embedText(wordSecurity, (Bitmap)tempImg);
                    }
                    // Salva o arquivo no Temp
                    if (sysConfig.Data.ImgFormat == ImageFormat.Jpeg)
                    {
                        tempImg.Save(tempFullFileToSave, sysConfig.Data.ImageCodec, sysConfig.Data.EncoderParams);
                    }
                    else
                    {
                        tempImg.Save(tempFullFileToSave, sysConfig.Data.ImgFormat);
                    }                    
                    // Adiciona o arquivo na lista de arquivos que devem ser movidos
                    if (sysConfig.Data.MoveImagesToTarget)
                    {
                        lock (filesToSend.SyncRoot)
                        {
                            filesToSend.Add(fileToSave);
                        }                            
                    }
                }
                catch (Exception ex)
                {
                    Log.AddLog("Captura", ex.Message);
                }                
            }
        }

        /// <summary>
        /// Move as imagens capturadas para uma pasta destino
        /// Limpa a lista de imagens a mover
        /// </summary>
        private void MoveFilesToTarget()
        {
            if (!sysConfig.Data.MoveImagesToTarget)
            {
                return;
            }
            if (!Directory.Exists(sysConfig.TargetPath))
            {
                Log.AddLog("Movendo", ": pasta destino não encontrada");
                return;
            }
            lock (filesToSend.SyncRoot)
            {
                foreach (string fileItem in filesToSend)
                {
                    try
                    {
                        if (File.Exists(sysConfig.TempPath + fileItem))
                        {
                            Util.MoveFromTo(sysConfig.TempPath + fileItem, sysConfig.TargetPath + fileItem);
                        }
                        else
                        {
                            Log.AddLog("Movendo", fileItem + ": não foi possível mover o arquivo");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AddLog("Movendo", fileItem + ": " + ex.Message);
                    }
                }
                filesToSend.Clear();
            }            
        }

        /// <summary>
        /// Move as imagens existentes na pasta temporária para a pasta destino
        /// Esse método é evocado ao iniciar o sistema
        /// </summary>
        private void MoveExistingFiles()
        {
            if (!sysConfig.Data.MoveImagesToTarget)
            {
                return;
            }
            string destFullFileName;
            string[] fileEntries = Directory.GetFiles(sysConfig.TempPath,  Util.GetLocalIpAddress() + "*" + sysConfig.Data.ExtImgFormat);
            foreach (string fileName in fileEntries)
            {
                try
                {
                    destFullFileName = sysConfig.TargetPath + Path.GetFileName(fileName);
                    Util.MoveFromTo(fileName, destFullFileName);
                    Log.AddFileMovingLog(destFullFileName);
                }
                catch (Exception ex)
                {
                    Log.AddLog("Movendo ao iniciar", fileName + ": " + ex.Message);
                }
                
            }   
        }

        /// <summary>
        /// Fas o parsing do texto embutido em uma imagem
        /// </summary>
        /// <param name="actualMessage"></param>
        /// <param name="newMessage"></param>
        /// <returns></returns>
        private string MsgValue(string actualMessage, out string newMessage)
        {
            string val = actualMessage.Substring(0, actualMessage.IndexOf(DATA_DELIMITER, 0));
            newMessage = actualMessage.Substring(val.Length + DATA_DELIMITER.Length);
            return val;
        }

        public void StartServerConnection()
        {
            MainUDPClient udpClient = new MainUDPClient(sysConfig.Data);
            //udpClient.ClientNumber = 1;
            udpClient.Load();
        }

    }

}
