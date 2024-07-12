using DASAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ZOI.BAL.Models
{
    public class Mailer
    {
        private readonly ICompositeViewEngine _viewEngine;
        private const string ControllerStr = "controller";
        private readonly ITempDataProvider _tempDataProvider;
        ITempDataProvider tempDataProvider;
        public Mailer(ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _tempDataProvider = tempDataProvider;
            _viewEngine = viewEngine;
        }
       
        public static MailerResult SendMailUsingSMTP(string fromMailId, string toMailID, string mailBodyHtml, string mailSubject, string smtpServer, int smtpPort, string smtpUserName, string smtpPassword)
        {
            try
            {
                MailerResult returnData = new MailerResult();
                using (SmtpClient SmtpServer = new SmtpClient(smtpServer))
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(fromMailId);
                        mail.To.Add(toMailID);
                        mail.Subject = mailSubject;
                        mail.Body = mailBodyHtml;
                        mail.IsBodyHtml = true;
                       
                        SmtpServer.Port = smtpPort;
                        SmtpServer.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);
                        SmtpServer.EnableSsl = true;
                        try
                        {
                            SmtpServer.Send(mail);
                            returnData = new MailerResult(1, "Email Send Successfully");
                        }
                        catch (SmtpException ex)
                        {
                            returnData = new MailerResult(-1, ex.Message);
                        }
                    }
                }
                return returnData;
            }
            catch (Exception ex)
            {
                MailerResult returnData = new MailerResult(-1, ex.Message);
                return returnData;
            }
        }

        public class MailerResult
        {
            public MailerResult()
            {
            }
            public MailerResult(int StatusCode, string StatusMessage)
            {
                this.statusCode = StatusCode;
                this.Message = StatusMessage;
            }
            public int statusCode { get; set; }
            public string Message { get; set; }
        }
    }
}
