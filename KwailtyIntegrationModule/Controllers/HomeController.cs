using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KwalityIntegrationLibrary;

namespace KwailtyIntegrationModule.Controllers
{
    public class HomeController : Controller
    {
        string errors1 = "";
        public ActionResult Index(int CompanyId)
        {
            try
            {
                ViewBag.CompId = CompanyId;
                return View();
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public ActionResult Posting(int CompanyId,string screenNames)
        {
            TempData["CompanyId"] = CompanyId;
            TempData.Keep();
            try
            {
               Trigger _trigger = new Trigger();
               bool status = _trigger.Integration_Trigger(screenNames, CompanyId);
               return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Error," + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}