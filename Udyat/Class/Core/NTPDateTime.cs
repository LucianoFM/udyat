using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class NTPDateTime
    {
        public static string ntpServer = "a.ntp.br";
        public static int ntpPort = 123;
        private const string timeServer = "http://www.geolicenses.com/getdatetime.php";

        public static DateTime GetNetworkTime(DataProxy pProxy)
        {
            try
            {
                // Tamanho da mensagem NTP - 16 bytes (RFC 2030)
                var ntpData = new byte[48];

                //Indicador de Leap (ver RFC), Versão e Modo
                ntpData[0] = 0x1B; //LI = 0 (sem warnings), VN = 3 (IPv4 apenas), Mode = 3 (modo cliente)

                var addresses = Dns.GetHostEntry(ntpServer).AddressList;

                //123 é a porta padrão do NTP
                var ipEndPoint = new IPEndPoint(addresses[0], ntpPort);
                
                //NTP usa UDP
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.Connect(ipEndPoint);

                //Caso NTP esteja bloqueado: nao trava o sistema
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();

                //Offset para chegar no campo "Transmit Timestamp" (que é
                //o do momento da saída do servidor, em formato 64-bit timestamp
                const byte serverReplyTime = 40;

                //Pegando os segundos
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                //e a fração de segundos
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                //Passando de big-endian pra little-endian
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

                //Tempo em **UTC**
                var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

                Log.AddStartingClockLog("Relógio Global", networkDateTime);

                return networkDateTime.ToLocalTime();
            }
            catch (Exception ex)
            {                
                string requestResult;
                try
                {
                    // Tenta buscar do site
                    string exceptionMessage;
                    if (HTTPGetPost.Get(timeServer, pProxy, out requestResult, out exceptionMessage))
                    {
                        DateTime resultDT;
                        if (DateTime.TryParse(requestResult, out resultDT))
                        {
                            Log.AddStartingClockLog("Relógio Global - Geolicenses", DateTime.Now);
                            return resultDT;
                        }                        
                    }
                    Log.AddStartingClockLog("Relógio Local - Error: " + ex.Message, DateTime.Now);
                    return DateTime.Now;
                }
                catch
                {
                    Log.AddStartingClockLog("Relógio Local - Error: " + ex.Message, DateTime.Now);
                    return DateTime.Now;
                }                
            }            
        }

        // Resposta aqui: stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

    }
}
