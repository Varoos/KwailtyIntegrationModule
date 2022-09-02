using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using ClosedXML.Excel;
using KwailtyIntegrationModule.Models;
using KwalityIntegrationLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public ActionResult BOM_Request(int CompanyId)
        {
            try
            {
                ViewBag.CompId = CompanyId;
                TempData["CompanyId"] = CompanyId;
                TempData.Keep();
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult BOM_Load(int CompanyId, string ReportDt,string DateString)
        {
            try
            {
                ViewBag.CompId = CompanyId;
                DBClass.SetLog("Entered BOM_Load" );
                DBClass.SetLog("BOM_Load CompanyId = " + CompanyId.ToString());
                DBClass.SetLog("BOM_Load ReportDt = " + ReportDt);
                DBClass.SetLog("BOM_Load DateString = " + DateString);
                TempData["CompanyId"] = CompanyId;
                TempData["ReportDate"] = ReportDt;
                TempData["DateString"] = DateString;
                TempData.Keep();
                string retrievequery = string.Format($@"exec pCore_CommonSp @Operation=RawMaterialRequest, @p1='{DateString}'");
                DBClass.SetLog("BOM_Load retrievequery = " + retrievequery);
                DataSet ds = DBClass.GetData(retrievequery, CompanyId, ref errors1);
                DBClass.SetLog("BOM_Load ds count = " + ds.Tables.Count.ToString());
                if (ds != null)
                {
                    DBClass.SetLog("BOM_Load ds <> null");
                    TempData["listdata"] = ds.Tables[0];
                    DBClass.SetLog("BOM_Load ds.Tables[0]");
                    TempData.Keep();
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Col");
                    DBClass.SetLog("BOM_Load dt Col");
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        DataRow dr = dt.NewRow();
                        if (column.ColumnName != "sVoucherNo" && column.ColumnName != "iFaTag" && column.ColumnName != "iDate")
                        {
                            dr["Col"] = column.ColumnName;
                            dt.Rows.Add(dr);
                        }
                    }
                    DBClass.SetLog("BOM_Load ds Add table");
                    ds.Tables.Add(dt);
                    string JSONString = JsonConvert.SerializeObject(ds);
                    DBClass.SetLog("BOM_Load JSONString = " + JSONString);
                    return Json(JSONString, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DBClass.SetLog("BOM_Load No Data");
                    return Json("No Data", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                DBClass.SetLog("BOM_Load Exception = "+ ex.Message);
                return Json("Error," + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult BOM_Post(int CompanyId, string ReportDt, string DateString)
        {
            
            try
            {
                ViewBag.CompId = CompanyId;
                DBClass.SetLog("BOM_Post entered");
                DBClass.SetLog("BOM_Post CompanyId = " + CompanyId);
                DBClass.SetLog("BOM_Post ReportDt = " + ReportDt);
                DBClass.SetLog("BOM_Post DateString = " + DateString);
                string baseUrl = ConfigurationManager.AppSettings["Server_API_IP"];
                DBClass.SetLog("BOM_Post baseUrl = " + baseUrl);
                string sessionID = GetSessionId(CompanyId);
                DBClass.SetLog("BOM_Post sessionID = " + sessionID);
                string retrievequery = string.Format($@"exec pCore_CommonSp @Operation=RawMaterialRequest, @p1='{DateString}'");
                DBClass.SetLog("BOM_Load retrievequery = " + retrievequery);
                DataSet ds = DBClass.GetData(retrievequery, CompanyId, ref errors1);
                DBClass.SetLog("BOM_Load ds count = " + ds.Tables.Count.ToString());
                DataTable dt = ds.Tables[0];
                if (dt != null)
                {
                    DBClass.SetLog("BOM_Post dt count = " + dt.Rows.Count.ToString());
                }
                else
                {
                    DBClass.SetLog("BOM_Post dt is null");
                }
                Hashtable headerJV = new Hashtable();
                
                List<Hashtable> listBodyJV = new List<Hashtable>();
                var list = dt.AsEnumerable().Select(r => r["sVoucherNo"].ToString()).Distinct();
                DBClass.SetLog("BOM_Post voucher number list = " + list);
                string FGPReqNo = string.Join(",", list);
                DBClass.SetLog("BOM_Post FGPReqNo = " + FGPReqNo);
                headerJV.Add("Date", Convert.ToInt32(dt.Rows[0]["iDate"]));
                headerJV.Add("Company Master__Id",Convert.ToInt32(dt.Rows[0]["iFaTag"]));
                headerJV.Add("FGPReqNo", FGPReqNo); 
                DBClass.SetLog("BOM_Post header data ready ");
                foreach (DataRow dr in dt.Rows)
                {
                    Hashtable objJVBody = new Hashtable();
                    objJVBody.Add("Item__Name", dr["RawMaterial"].ToString());
                    objJVBody.Add("Description", dr["RawMaterial"].ToString());
                    objJVBody.Add("Unit__Code", dr["Unit"].ToString());
                    objJVBody.Add("Quantity", Convert.ToDecimal(dr["RequiredQty"].ToString()));
                    listBodyJV.Add(objJVBody);
                }
                DBClass.SetLog("BOM_Post body data ready. count =  "+ listBodyJV.Count.ToString());
                var postingData1 = new PostingData();
                postingData1.data.Add(new Hashtable { { "Header", headerJV }, { "Body", listBodyJV } });
                string sContent1 = JsonConvert.SerializeObject(postingData1);
                DBClass.SetLog("BOM_Post API sContent =  " + sContent1);
                string err1 = "";
                string Url1 = baseUrl + "/Transactions/Vouchers/Material Requisition - Production";
                DBClass.SetLog("BOM_Post API url =  " + Url1);
                var response1 = Focus8API.Post(Url1, sContent1, sessionID, ref err1);
                DBClass.SetLog("BOM_Post API response =  " + response1);
                if (response1 != null)
                {
                    var responseData1 = JsonConvert.DeserializeObject<APIResponse.PostResponse>(response1);
                    DBClass.SetLog("BOM_Post API response data =  " + responseData1);
                    if (responseData1.result == -1)
                    {
                        DBClass.SetLog("BOM_Post API response. Posting Failed");
                        return Json("Posting Failed", JsonRequestBehavior.AllowGet);
                        //return View("Error", new { msg = "Posting Failed" });
                    }
                    else
                    {
                        string retrievequery2 = string.Format($@"exec pCore_CommonSp @Operation=setPlanningStatus, @p1='{DateString}'");
                        DBClass.SetLog("BOM_Post setPlanningStatus Query = "+ retrievequery2);
                        int a = DBClass.GetExecute(retrievequery2, CompanyId, ref errors1);
                        DBClass.SetLog("BOM_Post setPlanningStatus response = " + a.ToString());
                        DBClass.SetLog("BOM_Post setPlanningStatus errors1 = " + errors1.ToString());
                        if (a == 1)
                        {
                            DBClass.SetLog("BOM_Post setPlanningStatus response = updated Successfully" );
                            DBClass.SetLog("BOM_Post API response. Posted Successfully");
                            return Json("Success", JsonRequestBehavior.AllowGet);
                            //return View("Success");
                        }
                        else
                        {
                            DBClass.SetLog("BOM_Post setPlanningStatus response = Updation Failed");
                            //return View("Error", new { msg = "Planning Status Updation Failed" });
                            return Json("Planning Status Updation Failed", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    DBClass.SetLog("BOM_Post API response is null");
                    return Json("Posting Failed", JsonRequestBehavior.AllowGet);
                    //return View("Error", new { msg = "Posting Failed" });
                }
            }
            catch (Exception ex)
            {
                //return View("Error",new { msg = ex.Message.ToString()});
                return Json(ex.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ExcelGenerate()
        {
            try
            {
                #region TempData
                DataTable dt2 = (DataTable)TempData["listdata"];
                DBClass.SetLog("Entered ExcelGenerate data count = " + dt2.Rows.Count);
                int CompanyId = (int)TempData["CompanyId"];
                DBClass.SetLog("Entered ExcelGenerate companyid = " + CompanyId);
                if (dt2 == null || dt2.Rows.Count == 0)
                {
                    return RedirectToAction("Error", new { msg = "No Data To Export", compID = CompanyId });
                }
                else
                {
                    string ReportDate = TempData["ReportDate"].ToString();
                    TempData.Keep();
                    #endregion

                    #region DataColumns
                    DataTable dtcol = new DataTable();
                    dtcol.Columns.Add("cols", typeof(string));
                    System.Data.DataTable data = new System.Data.DataTable("Statement of Account");
                    foreach (DataColumn column in dt2.Columns)
                    {
                        if (column.ColumnName != "sVoucherNo" && column.ColumnName != "iFaTag" && column.ColumnName != "iDate" && column.ColumnName != "highlight")
                        {
                            if (column.ColumnName == "RawMaterial" || column.ColumnName == "Unit")
                                data.Columns.Add(column.ColumnName, typeof(string));
                            else
                                data.Columns.Add(column.ColumnName, typeof(decimal));

                            DataRow dr = dtcol.NewRow();
                            dr["cols"] = column.ColumnName;
                            dtcol.Rows.Add(dr);
                        }
                    }
                    #endregion
                    int colcount = dtcol.Rows.Count;

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add("Raw Material Request");


                        var wsReportNameHeaderRange = ws.Range(ws.Cell(1, 2), ws.Cell(1, colcount + 1));
                        wsReportNameHeaderRange.Style.Font.Bold = true;
                        wsReportNameHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        wsReportNameHeaderRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                        wsReportNameHeaderRange.Merge();
                        wsReportNameHeaderRange.Value = "Raw Material Request";
                        int r = 2;
                        ws.Range(ws.Cell(r, 2), ws.Cell(r, colcount + 1)).Merge();

                        r = 3;
                        int cell = 2;
                        ws.Cell(r, 2).Value = "Report Date : ";
                        ws.Range(ws.Cell(r, 3), ws.Cell(r, colcount + 1)).Merge().Value = ReportDate;

                        var TableRange = ws.Range(ws.Cell(r, 2), ws.Cell(r, colcount + 1));
                        TableRange.Style.Fill.BackgroundColor = XLColor.White;
                        TableRange.Style.Font.Bold = true;
                        TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        r = 4;
                        ws.Range(ws.Cell(r, 2), ws.Cell(r, colcount + 1)).Merge();
                        #region Headers
                        r = 5;
                        cell = 2;
                        for (int i = 0; i < data.Columns.Count; i++)
                        {
                            ws.Cell(r, cell).Value = dtcol.Rows[i][0].ToString();
                            cell = cell + 1;
                        }
                        #endregion
                        TableRange = ws.Range(ws.Cell(r, 2), ws.Cell(r, colcount + 1));
                        TableRange.Style.Font.FontColor = XLColor.White;
                        TableRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 115, 170);
                        TableRange.Style.Font.Bold = true;
                        TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        int c = 2;
                        int r2 = r;
                        #region TableLoop
                        foreach (DataRow obj in dt2.Rows)
                        {
                            c = 2;
                            r = r + 1;
                            if (obj["highlight"].ToString() == "1")
                            {
                                ws.Range(ws.Cell(r, 2), ws.Cell(r, colcount + 1)).Style.Font.FontColor = XLColor.Red;
                            }
                            foreach (DataRow col in dtcol.Rows)
                            {
                                ws.Cell(r, c).Value = obj["" + col["cols"] + ""].ToString() == "" ? "0.00" : obj["" + col["cols"] + ""].ToString();
                                c = c + 1;
                            }
                        }

                        ws.Range("B" + r2 + ":Z" + r + "").Style.Font.Bold = true;


                        #endregion

                        TableRange = ws.Range(ws.Cell(r2 - 1, 2), ws.Cell(r, colcount + 1));
                        TableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        TableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        TableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(ws.Cell(r2, 2), ws.Cell(r, 2)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;


                        ws.Range(ws.Cell(r2, 4), ws.Cell(r, colcount + 1)).Style.NumberFormat.Format = "0.00";
                        ws.Columns("A:BZ").AdjustToContents();

                        using (MemoryStream stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RawMaterialRequest" + "_" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { msg = ex.Message, compID = (int)TempData["CompanyId"] });
            }
        }
        public ActionResult Error(string msg,int compID)
        {
            ViewBag.msg = msg;
            ViewBag.CompanyId = compID;
            return View();
        }
        public ActionResult Success()
        {
            return View();
        }
        public class HashData
        {
            public string url { get; set; }
            public List<Hashtable> data { get; set; }
            public int result { get; set; }
            public string message { get; set; }
        }
        public partial class Datum
        {
            [JsonProperty("fSessionId")]
            public string FSessionId { get; set; }
        }
        public string getServiceLink(string tagname)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string strFileName = "";
            string PrgmFilesPath = AppDomain.CurrentDomain.BaseDirectory;

            strFileName = PrgmFilesPath + "\\bin\\XMLFiles\\Settings.xml";
            xmlDoc.Load(strFileName);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/ServSetting/ExternalModule/" + tagname + "");
            string strValue;
            XmlNode node = nodeList[0];
            if (node != null)
                strValue = node.InnerText;
            else
                strValue = "";
            return strValue;
        }
        public string GetSessionId(int CompId)
        {
            string sSessionId = "";
            try
            {
                string strServer = getServiceLink("ServerName");
                int ccode = CompId;
                string User_Name = getServiceLink("UserName");
                string Password = getServiceLink("Password");


                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://" + strServer + "/focus8api/Login");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{" + "\"data\": [{" + "\"Username\":\"" + User_Name + "\"," + "\"password\":\"" + Password + "\"," + "\"CompanyId\":\"" + ccode + "\"}]}";
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader Updatereader = new StreamReader(httpResponse.GetResponseStream());
                string Udtcontent = Updatereader.ReadToEnd();

                JObject odtbj = JObject.Parse(Udtcontent);
                Temperatures Updtresult = JsonConvert.DeserializeObject<Temperatures>(Udtcontent);
                if (Updtresult.Result == 1)
                {
                    sSessionId = Updtresult.Data[0].FSessionId;
                }


                return sSessionId;
            }
            catch (Exception ex)
            {
            }
            return sSessionId;
        }
        public partial class Temperatures
        {
            [JsonProperty("data")]
            public Datum[] Data { get; set; }

            [JsonProperty("url")]
            public Uri Url { get; set; }

            [JsonProperty("result")]
            public long Result { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

       
    }
}