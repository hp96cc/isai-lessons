using ISAI.Lessons.EntityFramework;
using ISAI.Lessons.EntityFramework.Models;
using ISAI.Lessons.EntityFramework.Services;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;

namespace ISAI.Lessons.Web.Portal.Helpers
{
    public static class SchedulerService
    {
        static string _systemBaseUrl => ConfigurationManager.AppSettings["ISAI.Lessons.Web.Portal.Url"];
        static string _systemUserId => ConfigurationManager.AppSettings["SystemUserId"];
        static string _systemGraphUserEmail => ConfigurationManager.AppSettings["MicrosoftGraph.SenderEmail"];
        static string _systemAdminEmail => ConfigurationManager.AppSettings["Email.SysAdmin"];
        static string _clientAuditEmail => ConfigurationManager.AppSettings["Email.ClientAudit"];
        static string _returnUrl => ConfigurationManager.AppSettings["ISAI.Lessons.Web.ReturnUrl"];

        static string _systemTimeZone = ConfigurationManager.AppSettings["SystemTimeZone"];

        static bool _isSendingTutorialEmails = false;

        static bool _isDeletingAbandonedTutorials = false;

        static bool _isCreateTeamsMeetingForGroupLessons = false;

        static bool _isSendingAbandonedSubscriptionEmails = false;

        static bool _isCheckTutorialTimeChanges = false;


        public static void SystemHeartBeat()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(_systemBaseUrl + "?guid=" + Guid.NewGuid().ToString()).GetAwaiter().GetResult(); ;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    bool b = true;
                }

            }
        }

        public static void PromptForAbandonedSubscription()
        {
                if (_isSendingAbandonedSubscriptionEmails) return;

                try
                {

                using (var db = new LessonsDbContext())
                {
                    var dateTime1HourAgo = DateTime.Now.AddHours(1);
                    var dateTimeFeatureStarted = new DateTime(2025, 05, 19);

                    var subscriptions = db.Subscription
                        .Include(x => x.Customer)
                        .Where(
                        x => x.Deleted == false &&
                        x.Active == false &&
                        x.HasAbondonedSubscriptionEmailBeenSent == false &&
                        x.DateCreated == dateTime1HourAgo &&
                        x.DateCreated == dateTimeFeatureStarted)
                        .ToList();

                    foreach (var subscription in subscriptions)
                    {

                        var subject = "Scottish Online Lessons";
                        var htmlBody = EmailService.GetTemplateHTML(_returnUrl + "/email-templates/abandoned-subscription/");

                        var graphApi = new MicrosoftGraphApiService();
                        graphApi.SendEmail(_systemGraphUserEmail, subject, htmlBody, new List<string>() { subscription.Customer.Email }, new List<string> { "info@scottishonlinelessons.com" }, new List<string>() { _systemAdminEmail, _clientAuditEmail }, new List<string>() { _systemGraphUserEmail }, true).Wait();

                        subscription.HasAbondonedSubscriptionEmailBeenSent = true;
                        subscription.DateModified = DateTime.UtcNow;
                        subscription.ModifiedUserId = _systemUserId;
                        db.Entry(subscription).State = EntityState.Modified;

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
                _isSendingAbandonedSubscriptionEmails = false;
            }
        }


        public static async void CheckTutorialTimeChanges()
        {
            if (_isCheckTutorialTimeChanges) return;

            try
            {

                _isCheckTutorialTimeChanges = true;

                using (var db = new LessonsDbContext())
                {
                    var now = DateTime.Now;

                    var tutorials = await db.Tutorial.Where(x => x.DateTimeStart > now && x.Deleted == false);

                    var dateTime1HourAgo = DateTime.Now.AddHours(1);
                    var dateTimeFeatureStarted = new DateTime(2025, 05, 19);

                    var subscriptions = db.Subscription
                        .Include(x => x.Customer)
                        .Where(
                        x => x.Deleted == false &&
                        x.Active == false &&
                        x.HasAbondonedSubscriptionEmailBeenSent == false &&
                        x.DateCreated == dateTime1HourAgo &&
                        x.DateCreated == dateTimeFeatureStarted)
                        .ToList();

                    foreach (var subscription in subscriptions)
                    {

                        var subject = "Scottish Online Lessons";
                        var htmlBody = EmailService.GetTemplateHTML(_returnUrl + "/email-templates/abandoned-subscription/");

                        var graphApi = new MicrosoftGraphApiService();
                        graphApi.SendEmail(_systemGraphUserEmail, subject, htmlBody, new List<string>() { subscription.Customer.Email }, new List<string> { "info@scottishonlinelessons.com" }, new List<string>() { _systemAdminEmail, _clientAuditEmail }, new List<string>() { _systemGraphUserEmail }, true).Wait();

                        subscription.HasAbondonedSubscriptionEmailBeenSent = true;
                        subscription.DateModified = DateTime.UtcNow;
                        subscription.ModifiedUserId = _systemUserId;
                        db.Entry(subscription).State = EntityState.Modified;

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
                _isCheckTutorialTimeChanges = false;
            }
        }

        public static void CreateTeamsMeetingForGroupLessons()
        {
            if (_isCreateTeamsMeetingForGroupLessons) return;

            try
            {
                _isCreateTeamsMeetingForGroupLessons = true;

                using (var db = new LessonsDbContext())
                {


                    var groupTutorials = db.GroupTutorial
                        .Include(x => x.TutorUser)
                        .Where(x => x.Deleted == false && string.IsNullOrEmpty(x.TeamsId)).ToList();

                    foreach (var groupTutorial in groupTutorials)
                    {
                        try
                        {

                            var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);
                            var startDate = TimeZoneInfo.ConvertTimeFromUtc(groupTutorial.DateTimeStart.DateTime, gmtStandardTimeZone);
                            var endDate = TimeZoneInfo.ConvertTimeFromUtc(groupTutorial.DateTimeEnd.DateTime, gmtStandardTimeZone);


                            var graphApi = new MicrosoftGraphApiService();
                            var teamsEventResponse = graphApi.CreateOrUpdateTeamsEvent(
                                                           null,
                                                           groupTutorial.TutorUser.Email,
                                                           "Group Tutorial " + groupTutorial.Name,
                                                           groupTutorial.Description,
                                                           _systemTimeZone,
                                                           startDate.ToString("s"),
                                                           endDate.ToString("s"),
                                                           null
                                                           ).Result;



                            groupTutorial.TeamsId = teamsEventResponse.Id;
                            groupTutorial.TeamsLink = teamsEventResponse.WebLink;
                            groupTutorial.DateModified = DateTime.Now;
                            groupTutorial.ModifiedUserId = _systemUserId;
                            db.Entry(groupTutorial).State = EntityState.Modified;

                            db.SaveChanges();


                        }
                        catch (Exception ex)
                        {
                            //TODO: need to log somehow, but also need to ski ones already removed
                        }

                   

                    }

                }
            }
            catch (Exception ex)
            {
                var t = true;
            }
            finally
            {
                _isCreateTeamsMeetingForGroupLessons = false;
            }


        }
        public static void DeleteAbandonedTutorials()
        {
            if (_isDeletingAbandonedTutorials) return;

            try
            {
                _isDeletingAbandonedTutorials = true;

                using (var db = new LessonsDbContext())
                {
                    var last20Minutes = DateTime.Now.AddMinutes(-20);

                    var tutorials = db.Tutorial
                        .Include(x => x.TutorUser)
                        .Where(x => 
                                x.Deleted == false && 
                                x.DateCreated < last20Minutes && 
                                x.HasCompletedCheckout == false)
                        .ToList();

                    foreach (var tutorial in tutorials)
                    {
                        try
                        {

                            if (!tutorial.GroupTutorialId.HasValue)
                            {
                                var graphApi = new MicrosoftGraphApiService();
                                graphApi.DeleteTeamsEvent(tutorial.TutorUser.Email, tutorial.TeamsId).RunSynchronously();
                            }

                        }
                        catch (Exception ex)
                        {
                            //TODO: need to log somehow, but also need to ski ones already removed
                        }

                        tutorial.Deleted = true;
                        tutorial.DateModified = DateTime.Now;
                        tutorial.ModifiedUserId = _systemUserId;
                        db.Entry(tutorial).State = EntityState.Modified;

                        db.SaveChanges();

                    }

                }
            } 
            catch (Exception ex)
            {
                var t = true;
            }
            finally
            {
                _isDeletingAbandonedTutorials = false;
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
                        .Include(x => x.Lesson)
                        .Include(x => x.TutorUser)
                        .Include(x => x.TutorialSubject)
                        .Where(x =>
                                x.Deleted == false &&
                                x.HasCompletedCheckout == true &&
                                (x.PendingEmailConfirmationTutor || x.PendingEmailConfirmationUser)).ToList();

                    foreach (var tutorial in tutorialsPendingEmailSending)
                    {

                        var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);
                        var startDate = TimeZoneInfo.ConvertTimeFromUtc(tutorial.DateTimeStart.DateTime, gmtStandardTimeZone);


                        if (tutorial.PendingEmailConfirmationUser && tutorial.HasCompletedCheckout)
                        {
                        
                            var subject = string.Format("Scottish Online Lessons - Tutorial Booked - {0}", startDate.ToString("dd/MM/yyyy @ HH:mm"));
                            var htmlBody = EmailService.GetTemplateHTML(_returnUrl + "/email-templates/tutorial-confirmation-user/");
                            htmlBody = AddDataToEmail(htmlBody, tutorial);

                            var addressCC = new List<string>();

                            if(tutorial.GroupTutorialId.HasValue)
                            {
                                addressCC.Add(tutorial.TutorUser.Email);

                                if(!string.IsNullOrWhiteSpace(tutorial.TutorUser.TutorEmail))
                                    addressCC.Add(tutorial.TutorUser.TutorEmail);
                            }

                            var graphApi = new MicrosoftGraphApiService();
                            graphApi.SendEmail(_systemGraphUserEmail, subject, htmlBody, new List<string>() { tutorial.Customer.Email }, addressCC, new List<string>() { _systemAdminEmail, _clientAuditEmail }, new List<string>() { _systemGraphUserEmail }, true).Wait();

                            tutorial.PendingEmailConfirmationUser = false;
                            tutorial.DateModified = DateTime.UtcNow;
                            tutorial.ModifiedUserId = _systemUserId;
                            db.Entry(tutorial).State = EntityState.Modified;
                        }

                        if (tutorial.PendingEmailConfirmationTutor)
                        {
                            var tutorUser = db.Users.First(x => x.Id == tutorial.TutorUserId);
                            var subject = string.Format("Scottish Online Lessons - Tutorial Booked - {0}", startDate.ToString("dd/MM/yyyy @ HH:mm"));
                            var htmlBody = EmailService.GetTemplateHTML(_returnUrl + "/email-templates/tutorial-confirmation-tutor/");
                            htmlBody = AddDataToEmail(htmlBody, tutorial);

                            var graphApi = new MicrosoftGraphApiService();
                            graphApi.SendEmail(_systemGraphUserEmail, subject, htmlBody, new List<string>() { tutorUser.Email, tutorUser.TutorEmail }, null, new List<string>() { _systemAdminEmail, _clientAuditEmail }, new List<string>() { _systemGraphUserEmail }, true).Wait();

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

            var gmtStandardTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_systemTimeZone);
            var startDate = TimeZoneInfo.ConvertTimeFromUtc(tutorial.DateTimeStart.DateTime, gmtStandardTimeZone);
            var endDate = TimeZoneInfo.ConvertTimeFromUtc(tutorial.DateTimeEnd.DateTime, gmtStandardTimeZone);


            htmlBody = htmlBody.Replace("{{Name}}", tutorial.Customer.FirstName);
            htmlBody = htmlBody.Replace("{{Tutor}}", tutorial.TutorUser.Firstname + " " + tutorial.TutorUser.Surname);
            htmlBody = htmlBody.Replace("{{Date}}", startDate.ToLongDateString());
            htmlBody = htmlBody.Replace("{{Time}}", startDate.ToString("HH:mm") + " to " + endDate.ToString("HH:mm"));
            htmlBody = htmlBody.Replace("{{Duration}}", tutorial.DurationInMinutes + " minutes.");
            htmlBody = htmlBody.Replace("{{Cost}}", string.Format("£{0:N2}", tutorial.TutorialCost));
            htmlBody = htmlBody.Replace("/{{Link}}", tutorial.TeamsLink);

            var lesson = "";

            if(tutorial.Lesson != null)
            {
                lesson = string.Format("<strong>Lesson:</strong> " + tutorial.Lesson.Name);
            } 
            else if (tutorial.TutorialSubject != null)
            {
                lesson = string.Format("<strong>Subject:</strong> " + tutorial.TutorialSubject.Name);
            } else
            {

            }

            htmlBody = htmlBody.Replace("{{Subject}}", lesson);

            return htmlBody;
        }
    }
}