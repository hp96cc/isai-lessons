using ISAI.Lessons.EntityFramework.Models;
using Newtonsoft.Json;
using Syncfusion.EJ2.FileManager.Base;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ISAI.Lessons.Web.Portal.Controllers
{

    [Authorize]
    public class LessonController : BaseAdminController
    {

        private PhysicalFileProvider _operation = new PhysicalFileProvider();
        private readonly string _conetentRootFolder = ConfigurationManager.AppSettings["ISAI.Lessons.ContentRootFolder"];

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.ItemName = "Lesson";

        }

        public async Task<ActionResult> Index()
        {

            var breadCrumbList = new List<LessonGroup>();

            if (Request.QueryString["lessonGroupId"] != null)
            {
                var lessonGroupId = Convert.ToInt32(Request.QueryString["lessonGroupId"]);
                await GetParentLessonGroup(lessonGroupId, breadCrumbList);

            }

            breadCrumbList.Reverse();

            ViewBag.BreadCrumbList = breadCrumbList;

            return View();
        }


        public async Task<ActionResult> ManageFiles(int lessonId)
        {

            var record = await db.Lesson.FirstOrDefaultAsync(x => x.Id == lessonId);

            if (record == null || record.Deleted == true)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return View();
        }


        public ActionResult FileOperations(FileManagerDirectoryContent args)
        {

            SetRootFolder(Request.QueryString["lessonId"]);

            switch (args.Action)
            {
                case "read":
                    return Json(_operation.ToCamelCase(_operation.GetFiles(args.Path, args.ShowHiddenItems)));
                case "delete":
                    return Json(_operation.ToCamelCase(_operation.Delete(args.Path, args.Names)));
                case "details":
                    if (args.Names == null)
                    {
                        args.Names = new string[] { };
                    }
                    return Json(_operation.ToCamelCase(_operation.Details(args.Path, args.Names, args.Data)));
                case "create":
                    return Json(_operation.ToCamelCase(_operation.Create(args.Path, args.Name)));
                case "search":
                    return Json(_operation.ToCamelCase(_operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive)));
                case "copy":
                    return Json(_operation.ToCamelCase(_operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)));
                case "move":
                    return Json(_operation.ToCamelCase(_operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)));
                case "rename":
                    return Json(_operation.ToCamelCase(_operation.Rename(args.Path, args.Name, args.NewName)));
            }
            return null;

        }
        public ActionResult Upload(string path, IList<System.Web.HttpPostedFileBase> uploadFiles, string action)
        {
            SetRootFolder(Request.QueryString["lessonId"]);

            if (path == null)
            {
                return Content("");
            }
            FileManagerResponse uploadResponse;
            uploadResponse = _operation.Upload(path, uploadFiles, action, null);
            if (uploadResponse.Error != null)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.Status = uploadResponse.Error.Code + " " + uploadResponse.Error.Message;
                Response.StatusCode = Int32.Parse(uploadResponse.Error.Code);
                Response.StatusDescription = uploadResponse.Error.Message;
                Response.End();
            }

            return Content("");
        }

        public ActionResult Download(string downloadInput)
        {
            SetRootFolder(Request.QueryString["lessonId"]);
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            return _operation.Download(args.Path, args.Names);

        }

        public ActionResult GetImage(FileManagerDirectoryContent args)
        {
            SetRootFolder(Request.QueryString["lessonId"]);
            return _operation.GetImage(args.Path, args.Id, false, null, null);
        }

        private void SetRootFolder(string lessonId)
        {
            var rootFolder = Path.Combine(_conetentRootFolder, lessonId);
            _operation.RootFolder(rootFolder);

        }

    }
}