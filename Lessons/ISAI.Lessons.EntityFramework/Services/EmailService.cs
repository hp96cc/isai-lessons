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
using ISAI.Lessons.Core.Services;

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

          
                var graphApi = new MicrosoftGraphApiService();
      

                await graphApi.SendEmail("noreply@scottishonlinelessons.com", message.Subject, message.Body, new List<string>() { message.Destination }, null, new List<string>() { ConfigurationManager.AppSettings["SysAdminEmail"] }, true);
               
        }


    }

}