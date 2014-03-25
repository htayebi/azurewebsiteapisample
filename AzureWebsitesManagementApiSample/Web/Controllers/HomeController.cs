using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public HttpResponseMessage Index(CreateSiteViewModel site)
        {
            site.CertPath = @ConfigurationManager.AppSettings["CertificatePath"];
            site.Subscription = ConfigurationManager.AppSettings["SubscriptionId"];
            site.WebSpaceGeo = Regions[site.WebSpaceName];
            return CreateWebsite(site);
        }

        private HttpResponseMessage CreateWebsite(CreateSiteViewModel site)
        {
            var cert = X509Certificate.CreateFromCertFile(Server.MapPath(site.CertPath));
            string uri = string.Format("https://management.core.windows.net/{0}/services/WebSpaces/{1}/sites/", site.Subscription, site.WebSpaceName);

            // A url which is looking for the right public key with 
            // the incomming https request

            var req = (HttpWebRequest)WebRequest.Create(uri);

            String dataToPost =string.Format(
                @"<Site xmlns=""http://schemas.microsoft.com/windowsazure"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                  <HostNames xmlns:a=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">
                    <a:string>{0}.azurewebsites.net</a:string>
                  </HostNames>
                  <Name>{0}</Name>
                  <WebSpaceToCreate>
                    <GeoRegion>{1}</GeoRegion>
                    <Name>{2}</Name>
                    <Plan>VirtualDedicatedPlan</Plan>
                  </WebSpaceToCreate>
                </Site>", site.SiteName, site.WebSpaceGeo, site.WebSpaceName);

            req.Method = "POST";        // Post method
            //You can also use ContentType = "text/xml";
            
            // with the request
            req.UserAgent = "Fiddler";
            req.Headers.Add("x-ms-version", "2013-08-01");
            req.ClientCertificates.Add(cert);
            // Attaching the Certificate To the request

            // when you browse manually you get a dialogue box asking 
            // that whether you want to browse over a secure connection.
            // this line will suppress that message
            //(pragramatically saying ok to that message). 

            string postData = dataToPost;
            var encoding = new ASCIIEncoding();
            byte[] byte1 = encoding.GetBytes(postData);

            // Set the content length of the string being posted.
            req.ContentLength = byte1.Length;

            Stream newStream = req.GetRequestStream();
            
            newStream.Write(byte1, 0, byte1.Length);
            
            // Close the Stream object.
            newStream.Close();
            
            var rsp = (HttpWebResponse)req.GetResponse();

            var reader = new StreamReader(rsp.GetResponseStream());
            String retData = reader.ReadToEnd();

            req.GetRequestStream().Close();
            rsp.GetResponseStream().Close();

            return new HttpResponseMessage
            {
                StatusCode = rsp.StatusCode,
                Content = new StringContent(retData)
            };
        }

        private Dictionary<string, string> Regions
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {"eastuswebspace", "East US"},
                    {"westuswebspace", "West US"},
                    {"northcentraluswebspace", "North Central US"},
                    {"northeuropewebspace", "North Europe"},
                    {"westeuropewebspace", "West Europe"},
                    {"eastasiawebspace", "East Asia"}
                };
            }
        }
    }
}