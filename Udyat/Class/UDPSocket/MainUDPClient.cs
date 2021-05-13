using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDPEvents;
using UDPEvents.AppCommands;
using UDPEvents.Remoting;
using UDPEvents.UDP;
//using Udyat.Class.UDPSocket.Interfaces;

namespace Udyat.Class.UDPSocket
{
    public class MainUDPClient
    {
        private string serverResponse;
        private int clientNumber;
        private bool mHaveBeenPinged;
        private DataConfig dataConfig;

        public MainUDPClient(DataConfig pDataConfig)
        {
            dataConfig = pDataConfig;
        }

        public int ClientNumber
        {
            get { return clientNumber; }
            set { clientNumber = value; }
        }

        private string ServerName()
        {
            return "localhost";
        }

        public void Load()
        {
            // Cria uma thread que ficará na escuta do servidor
            new UDPBroadcastListener(new CommandReceived(ReceivedCommand), dataConfig.ClientPort); // UDPPorts.ClientPort(ClientNumber));
        }

        public void Connect(string pSendText)
        {
            IRemoteMonitorServer srv = RemoteServer();
            if (srv != null)
            {
                serverResponse = srv.MonitorExtension(pSendText);
            }
        }

        public void Disconnect(string pSendText)
        {
            IRemoteMonitorServer srv = RemoteServer();
            if (srv != null)
            {
                serverResponse = srv.ReleaseExtension(pSendText);
            }
        }

        private IRemoteMonitorServer RemoteServer()
        {
            return (IRemoteMonitorServer)RemotingHelper.GetRemoteObject(typeof(IRemoteMonitorServer), "IRemoteTelephoneServer", ServerName());
        }

        private void ReceivedCommand(AppCommand cmd)
        {
            try
            {
                if (cmd.CommandType == SystemCommandEnum.CloseAllClients)
                {
                    Environment.Exit(0);
                }
                else
                if (cmd.CommandType == SystemCommandEnum.Direct_Client_CheckLicense)
                {
                    // Se a licença e o nome da máquina forem iguais a licença instalada na máquina, então retorna a mensagem
                    if ((cmd.Args.Length > 0) && (cmd.Args[0].ToString().ToUpper() == dataConfig.SerialKey.ToUpper()) && (cmd.Args[1].ToString().ToUpper() == Environment.MachineName.ToUpper()))
                    {
                        // Os eventos do servidor são reconhecidos através do UDP para que o servidor não nos convoca tempo após vez.
                        AppCommand answerCommand = new AppCommand(SystemCommandEnum.Direct_Client_CheckLicense_ACK, DateTime.Now);
                        answerCommand.BroadcastIndex = cmd.BroadcastIndex;
                        answerCommand.ToDoActionIndex = cmd.ToDoActionIndex;
                        UDPBroadcaster.BroadcastCommand(answerCommand, dataConfig.ServerPort, dataConfig.ClientPort, dataConfig.ServerHost);
                        // Cria o objeto remoto
                        RetrieveTelephoneData(cmd.ToDoActionIndex);
                    }
                }
                else if (cmd.CommandType == SystemCommandEnum.PingClients)
                {
                    Connect("2223");
                    if (!mHaveBeenPinged)
                      UDPBroadcaster.BroadcastCommand(new AppCommand(SystemCommandEnum.PongClients, "2223"), dataConfig.ServerPort, dataConfig.ClientPort, dataConfig.ServerHost);
                }
                else if (cmd.CommandType == SystemCommandEnum.PongClientAcknowledge)
                {
                      if (cmd.Args.Length > 0 && cmd.Args[0].ToString() == "")
                      {
                        mHaveBeenPinged = true;
                      }
                }
                else if (cmd.CommandType == SystemCommandEnum.PingReset)
                {
                    mHaveBeenPinged = false;
                }
                else if (cmd.CommandType == SystemCommandEnum.RequestMonitorExtension)
                {
                    if (cmd.Args.Length > 0 && cmd.Args[0].ToString() == "")
                    {
                        Connect("");
                    }
                }
                else if (cmd.CommandType == SystemCommandEnum.RequestReleaseExtension)
                {
                    if (cmd.Args.Length > 0 && cmd.Args[0].ToString() == "")
                    {
                        Disconnect("");
                    }
                }
            }             
            catch (SocketException ex)
            {
                Log.AddLog("P2P", "Erro ao receber pacote: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log.AddLog("P2P", "Erro ao receber pacote: " + ex.Message);
            }
        }

        private void RetrieveTelephoneData(int pServerActionIndex)
        {
            IRemoteMonitorServer srv = RemoteServer();
            if (srv != null)
            {
                srv.OldLicenseSK = dataConfig.OldSerialKey;
                srv.NewLicenseSK = dataConfig.SerialKey;
                srv.RemoteDomain = Environment.UserDomainName;
                srv.RemoteWorkGroup = Util.GetWorkGroup();
                srv.RemoteMacName = Environment.MachineName;
                srv.RemoteIP = Util.GetLocalIpAddress();
                srv.RemoteMacAddress = Util.GetMacAddres();
                if (srv.ClientMonitorActive(pServerActionIndex))
                {
                    Log.AddLog("RPC", "Instalado com sucesso.");
                }
                else
                {
                    Log.AddLog("RPC", "Instalado com sucesso, mas servidor não recebeu retorno.");
                }
                /*if (srv.PhoneCallActive(pText))
                    lblPhoneStatus.Text = "Phone Picked Up";
                else
                    lblPhoneStatus.Text = "Phone Put Down";*/
            }
        }

    }
}
