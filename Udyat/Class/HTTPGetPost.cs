using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class HTTPGetPost
    {

        // Faz uma requisição GET e retorna o resultado em uma string
        public static bool Get(string webUrl, DataProxy pProxy, out string WSResult, out string pExceptionMessage)
        {
            pExceptionMessage = "";
            WSResult = "";
            bool result = false;
            // Cria uma requisição GET para o servidor de licença
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webUrl);
            // Configura o proxy
            if (pProxy.UseProxy)
            {
                WebProxy proxy = new WebProxy(pProxy.Host, pProxy.Port);
                if (pProxy.DefaultAuthentication)
                {
                    request.Credentials = CredentialCache.DefaultCredentials;
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
                request.Proxy = proxy;
            }
            // Cria o stream da resposta
            StreamReader responseReader;
            // Recebe a resposta do servidor
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                try
                {
                    if (response != null)
                    {
                        using (responseReader = new StreamReader(response.GetResponseStream()))
                        {
                            WSResult = responseReader.ReadToEnd();                            
                        }
                    }
                }
                finally
                {
                    response.Close();
                    result = true;
                }
            }
            catch (WebException ex)
            {
                // Tratamento para erros do servidor ou de proxy
                pExceptionMessage = ex.Message;
                if (ex.Response != null)
                {
                    using (responseReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string exMessage = responseReader.ReadToEnd();
                        Log.AddLog("CONNECTION", "Erro: " + exMessage);
                    }
                }
            }
            return result;
        }

        // Faz uma requisição POST e retorna o resultado em uma string
        public static bool Post(string webUrl, string pParameters, DataProxy pProxy, out string WSResult, out string pExceptionMessage)
        {
            pExceptionMessage = "";
            WSResult = "";
            bool result = false;
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                //wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)";
                // Configura o proxy
                if (pProxy.UseProxy)
                {
                    WebProxy proxy = new WebProxy(pProxy.Host, pProxy.Port);
                    if (pProxy.DefaultAuthentication)
                    {
                        wc.Credentials = CredentialCache.DefaultCredentials;
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
                    wc.Proxy = proxy;
                }
                
                try
                {
                    WSResult = wc.UploadString(webUrl, pParameters);
                    result = true;
                }
                catch (WebException ex)
                {
                    StreamReader responseReader; 
                    // Tratamento para erros do servidor ou de proxy
                    pExceptionMessage = ex.Message;
                    if (ex.Response != null)
                    {
                        using (responseReader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            string exMessage = responseReader.ReadToEnd();
                            Log.AddLog("CONNECTION", "Erro: " + exMessage);
                        }
                    }
                }
            }
            return result;
        }

    }
}
