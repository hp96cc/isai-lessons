using ISAI.Lessons.EntityFramework;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace ISAI.Lessons.Web.Portal.Helpers
{
    public static class SchedulerService
    {
        static string _systemUserId => ConfigurationManager.AppSettings["SystemUserId"];
        static string _systemGraphUserEmail => ConfigurationManager.AppSettings["MicrosoftGraph.SenderEmail"];
        static string _systemAdminEmail => ConfigurationManager.AppSettings["Email.SysAdmin"];
        static string _clientAuditEmail => ConfigurationManager.AppSettings["Email.ClientAudit"];
        
        static bool _isSendingTutorialEmails = false;

        static bool _isDeletingAbandonedCustomers = false;

        public static void DeleteAbandonedCustomers()
        {
            if (_isDeletingAbandonedCustomers) return;

            try
            {
                _isDeletingAbandonedCustomers = true;


                using (var db = new LessonsDbContext())
                {

                }
            } catch (Exception ex)
            {
                _isDeletingAbandonedCustomers = false;
                throw ex;
            }
            finally
            {
                _isDeletingAbandonedCustomers = false;
            }


        }

        public static void SendPendingTutorialEmails()
        {
            if (_isSendingTutorialEmails) return;

            try
            {
                _isSendingTutorialEmails = true;

                using (var db = new LessonsDbContext())
                {
                    var tutorialsPendingEmailSending = db.Tutorial
                        .Include(x => x.Customer)
                        .Where(x =>
                                x.Deleted == false &&
                                x.HasCompletedCheckout == true &&
                                (x.PendingEmailConfirmationTutor || x.PendingEmailConfirmationUser));

                    foreach (var tutorial in tutorialsPendingEmailSending)
                    {
                        if (tutorial.PendingEmailConfirmationUser && tutorial.HasCompletedCheckout)
                        {
                            var subject = string.Format("Scottish Online Lessons - Tutorial Booked - {0}", tutorial.DateTimeStart.ToString("dd/MM/yyyy @ HH:mm"));
                            var htmlBody = EmailService.GetTemplateHTML("https://portal.scottishonlinelessons.com/email-templates/tutorial-confirmation-user/");
                            htmlBody = AddDataToEmail(htmlBody, tutorial);

                            var graphApi = new MicrosoftGraphApiService();
                            graphApi.SendEmail(_systemGraphUserEmail, subject, htmlBody, new List<string>() { tutorial.Customer.Email }, null, new List<string>() { _systemAdminEmail, _clientAuditEmail }, new List<string>() { _systemGraphUserEmail }, true).Wait();

                            tutorial.PendingEmailConfirmationUser = false;
                            tutorial.DateModified = DateTime.UtcNow;
                            tutorial.ModifiedUserId = _systemUserId;
                            db.Entry(tutorial).State = EntityState.Modified;
                        }

                        if (tutorial.PendingEmailConfirmationTutor)
                        {
                            var tutorUser = db.Users.First(x => x.Id == tutorial.TutorUserId);
                            var subject = string.Format("Scottish Online Lessons - Tutorial Booked - {0}", tutorial.DateTimeStart.ToString("dd/MM/yyyy @ HH:mm"));
                            var htmlBody = EmailService.GetTemplateHTML("https://portal.scottishonlinelessons.com/email-templates/tutorial-confirmation-tutor/");
                            htmlBody = AddDataToEmail(htmlBody, tutorial);

                            var graphApi = new MicrosoftGraphApiService();
                            graphApi.SendEmail(_systemGraphUserEmail, subject, htmlBody, new List<string>() { tutorUser.Email }, null, new List<string>() { _systemAdminEmail, _clientAuditEmail }, new List<string>() { _systemGraphUserEmail }, true).Wait();

                            tutorial.PendingEmailConfirmationTutor = false;
                            tutorial.DateModified = DateTime.UtcNow;
                            tutorial.ModifiedUserId = _systemUserId;
                            db.Entry(tutorial).State = EntityState.Modified;
                        }
                    }

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                var t = true;
            }
            finally
            {
                _isSendingTutorialEmails = false;
            }


        }

        private static string AddDataToEmail(string htmlBody, Tutorial tutorial)
        {
            htmlBody = htmlBody.Replace("{{Name}}", tutorial.Customer.FirstName);
            htmlBody = htmlBody.Replace("{{Date}}", tutorial.DateTimeStart.DateTime.ToLongDateString());
            htmlBody = htmlBody.Replace("{{Time}}", tutorial.DateTimeStart.DateTime.ToString("HH:mm") + " to " + tutorial.DateTimeEnd.DateTime.ToString("HH:mm"));
            htmlBody = htmlBody.Replace("{{Duration}}", tutorial.DurationInMinutes + " minutes.");
            htmlBody = htmlBody.Replace("{{Cost}}", string.Format("{0:N2} £", tutorial.TutorialCost));
            htmlBody = htmlBody.Replace("/{{Link}}", tutorial.TeamsLink);
            return htmlBody;
        }
    }
}