using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;
using System.Net;
using System.Linq;
using FluentEmail.Core.Models;
using Attachment = FluentEmail.Core.Models.Attachment;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.ViewModels;
using System;
using System.Globalization;

namespace ISAI.Lessons.EntityFramework.Services
{
    public static class EmailService
    {

        static SmtpClient _client;

        static void SetSMTPClient()
        {

            if (_client == null)
            {
                _client = new SmtpClient();
                _client.Host = "127.0.0.1";
                _client.Port = 25;

                Email.DefaultSender = new SmtpSender(_client);
            }


        }

      
        public static async Task SetPasswordResetEmail(IdentityMessage message)
        {

            SetSMTPClient();

            var email = await Email
                    .From(ConfigurationManager.AppSettings["FromEmailAddress"], ConfigurationManager.AppSettings["FromEmailName"])
                        .To(message.Destination)
                        .BCC(ConfigurationManager.AppSettings["SysAdminEmail"])
                        .Subject(message.Subject)
                        .Body(message.Body, true)

                    .SendAsync();


            bool sent = email.Successful;

        }



        public static async Task SendEmail(string subject, string message, List<string> emailTo, List<FileUpload> files = null)
        {

            SetSMTPClient();

            var addressTo = emailTo.Select(x => new Address()
            {
                EmailAddress = x,
                Name = x
            }).ToList();



            if (files != null)
            {

                var attachments = new List<Attachment>();

                foreach (var file in files)
                {

                    var stream = new MemoryStream(file.bytes);

                    attachments.Add(new Attachment()
                    {
                        Data = stream,
                        Filename = file.name,
                        IsInline = false

                    });


                }

                var email = await Email
                    .From(ConfigurationManager.AppSettings["FromEmailAddress"])
                    .To(addressTo)
                    .Attach(attachments)
                    .BCC(ConfigurationManager.AppSettings["SysAdminEmail"])
                    .Subject(subject)
                    .Body(message, true)
                    .SendAsync();

                bool sent = email.Successful;

                foreach (var attachment in attachments)
                {
                    attachment.Data.Dispose();
                }
            }
            else
            {

                var email = await Email
                    .From(ConfigurationManager.AppSettings["FromEmailAddress"])
                     .To(addressTo)
                     .BCC(ConfigurationManager.AppSettings["SysAdminEmail"])
                     .Subject(subject)
                     .Body(message, true)
                     .SendAsync();

                bool sent = email.Successful;

            }

      



        }


        public static string GetTemplateHTML(string url)
        {

            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }


        }
    }

}