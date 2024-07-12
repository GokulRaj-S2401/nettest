using DASAPI.Filters;
using DASAPI.Models;
using DASAPI.SignalR;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
using System.Diagnostics;
using ZOI.BAL.Models;
using ZOI.BAL.Services.Interface;
using ZOI.BAL.Utilities;
using static ZOI.BAL.Utilities.Constants;

namespace DASAPI.Controllers
{
    [Route("api/template")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private readonly HttpContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly ITemplateService _templateService;
        private readonly IHubContext<CommHub> _hub;


        public TemplateController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, ITemplateService templateService, IHttpContextAccessor httpContextAccessor, IHubContext<CommHub> hub)
        {
            _hostingEnvironment = hostingEnvironment;
            _context = httpContextAccessor.HttpContext;
            _templateService = templateService;
            _hub = hub;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("GetTemplateDetails")]
        public JsonResponse GetTemplateDetails(DocumentCollection model)
        {
            return _templateService.GetTemplateDetails(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("GetTemplateDetailsByID")]
        public Task<JsonResponse> GetTemplateDetailsByID(DocumentCollection model)
        {
            return _templateService.GetTemplateDetailsByID(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("InsertDocument")]
        public async Task<JsonResponse> InsertDocument(DocumentCollection model)
        {
            var data = await _templateService.InsertDocument(model, _context);

            if (model.CollectionName == Tables.Notes)
            {
                var userList = UserHandler.ConnectedIds;
                List<string> userID = new List<string>();

                foreach (var value in userList)
                {
                    userID.Add(value.ConnectionID);
                }
                model.CollectionName = Tables.Documents;

                var summary = await _templateService.GetTemplateDetailsByID(model);

                _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", summary, UserHandler.ConnectedIds)));

            }
            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("InsertManyDocument")]
        public JsonResponse InsertManyDocument(DocumentCollection model)
        {
            return _templateService.InsertManyDocument(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateDocumentByID")]
        public JsonResponse UpdateDocumentByID(DocumentCollection model)
        {
            return _templateService.UpdateDocumentByID(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateJsonKeyValue")]
        public async Task<JsonResponse> UpdateJsonKeyValue(DocumentCollection model)
        {
            var data = await _templateService.UpdateJsonKeyValue(model);

            if (model.CollectionName == Tables.Documents && model.UserID != null)
            {
                var userList = UserHandler.ConnectedIds;
                List<string> userID = new List<string>();

                foreach (var value in userList)
                {
                    userID.Add(value.ConnectionID);
                }

                var summary = await _templateService.GetCollectionListByUserID(model);
                _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetCollectionListByUserID", summary)));

            }
            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateBlockData")]
        public async Task<JsonResponse> UpdateBlockData(DocumentCollection model)
        {

            var data = await _templateService.UpdateBlockData(model);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var dashboardSummary = await _templateService.GetTemplateDetailsByID(model);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", dashboardSummary, UserHandler.ConnectedIds)));
            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("DeleteRowFromCollection")]
        public async Task<JsonResponse> DeleteRowFromCollection(DocumentCollection model)
        {
            var data = await _templateService.DeleteRowFromCollection(model);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var dashboardSummary = await _templateService.GetTemplateDetailsByID(model);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", dashboardSummary, UserHandler.ConnectedIds)));

            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("InsertMultiCollection")]
        public async Task<JsonResponse> InsertMultiCollectionWithRoles(MultiCollection model)
        {
            var data = await _templateService.InsertMultiCollectionWithRoles(model);
            var userList = UserHandler.ConnectedIds;

            List<string> userID = new List<string>();
            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }

            DocumentCollection documentCollection = new DocumentCollection();
            documentCollection.CollectionName = model.CollectionName;
            documentCollection.UserID = model.UserID;

            var dashboardSummary = await _templateService.GetCollectionListByUserID(documentCollection);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetCollectionListByUserID", dashboardSummary)));


            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("InsertNewRowIntoCollection")]
        public async Task<JsonResponse> InsertNewRowIntoCollection(DocumentCollection model)
        {
            var data = await _templateService.InsertNewRowIntoCollection(model);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }

            var hubValue = await _templateService.GetTemplateDetailsByID(model);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", hubValue, UserHandler.ConnectedIds)));

            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("GetNotesDetails")]
        public JsonResponse GetNotesDetails(DocumentCollection model)
        {
            return _templateService.GetNotesDetails(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("GetCollectionListByUserID")]
        public async Task<JsonResponse> GetCollectionListByUserID(DocumentCollection model)
        {

            return await _templateService.GetCollectionListByUserID(model);
        }

        [HttpPost, Route("HtmlToPdf")]
        [IgnoreAntiforgeryToken]
        public ActionResult HtmlToPdf(HtmlToPDF model)
        {
            string htmlToPdfExePath = CommonFunction.GetConnectionString("pdfToHtmlExePath");
            var path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf");

            var pdfPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Pdf");
            string outputFilename = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf", CommonFunction.AppendTimeStamp("kotakdocument.pdf"));
            var htmlUrl = Path.Combine(_hostingEnvironment.ContentRootPath);
            if (!Directory.Exists(pdfPath))
            {
                Directory.CreateDirectory(pdfPath);
            }
            htmlUrl = Path.Combine(htmlUrl, "Pdf", CommonFunction.AppendTimeStamp("kotakdocument.html"));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!System.IO.File.Exists(htmlUrl))
                using (FileStream fs = System.IO.File.Create(htmlUrl))
                {
                    fs.Close();
                }
            FileStream fileStream = new FileStream(htmlUrl, FileMode.Append, FileAccess.Write);
            StreamWriter s1 = new StreamWriter(fileStream);
            s1.Write(model.Data);
            s1.Close();
            fileStream.Close();
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var pi = new ProcessStartInfo(htmlToPdfExePath);
                pi.CreateNoWindow = true;
                pi.UseShellExecute = false;
                pi.WorkingDirectory = path;
                pi.Arguments = "--viewport-size 216x279 --orientation  portrait " + '"' + htmlUrl + '"' + " " + outputFilename;
                using (var process = Process.Start(pi))
                {
                    process.WaitForExit(99999);
                    Debug.WriteLine(process.ExitCode);
                }
                var bytes = System.IO.File.ReadAllBytes(outputFilename);
                return File(bytes, "application/x-msdownload", Path.GetFileName(outputFilename));
            }
            catch (Exception ex)
            {
                return Ok(new JsonResponse()
                {
                    Status = APIResponseStatus.Failed,
                    Message = APIResponseMessage.Failed,
                    Data = ex.Message
                });
            }
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("ConvertHtmlToPDF")]
        public ActionResult ConvertHtmlToPDF(HtmlToPDF model)
        {
            try
            {
                MongoClient client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var setUpdate = Builders<BsonDocument>.Update.Set("htmlDocument", model.Data).Unset("1");
                var arrayFilters = new List<ArrayFilterDefinition>();
                var updateFilter = "{documentID:" + model.DocumentID + "}";
                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                   .GetCollection<BsonDocument>(model.CollectionName)
                   .UpdateOne(updateFilter, setUpdate);

            }
            catch (Exception ex)
            {
            }
            return null;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("HitoryListing")]
        public JsonResponse HitoryListing(DocumentCollection model)
        {
            return _templateService.HitoryListing(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("GetCountOfCollection")]
        public JsonResponse GetCountOfCollection()
        {
            return _templateService.GetCountOfCollection();
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UserAuthentication")]
        public JsonResponse UserAuthentication(Users model)
        {
            return _templateService.UserAuthentication(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateRole")]
        public JsonResponse UpdateRole(Roles model)
        {
            return _templateService.UpdateRole(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateContentBlock")]
        public JsonResponse UpdateContentBlock(DocumentCollection model)
        {
            return _templateService.UpdateContentBlock(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("ContentLevelWritePermission")]
        public JsonResponse ContentLevelWritePermission(DocumentCollection model)
        {
            return _templateService.ContentLevelWritePermission(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateUserDetails")]
        public JsonResponse UpdateUserDetails(Users model)
        {
            return _templateService.UpdateUserDetails(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateRoleDetails")]
        public JsonResponse UpdateRoleDetails(Roles model)
        {
            return _templateService.UpdateRoleDetails(model);
        }


        [UserAuthenticationFilter]
        [HttpPost, Route("UserInvitation")]
        public JsonResponse UserInvitation(string token)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    return _templateService.GetUserDetailsByEmail(token);
                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.TokenEmptyMessage;
                }
            }
            catch (Exception ex)
            {
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateKeyPairValue")]
        public JsonResponse UpdateKeyPairValue(DocumentCollection model)
        {
            return _templateService.UpdateKeyPairValue(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateChildNotes")]
        public JsonResponse UpdateChildNotes(Notes model)
        {
            return _templateService.UpdateChildNotes(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("AddUpdateNotesLike")]
        public JsonResponse AddUpdateNotesLike(Notes model)
        {
            return _templateService.AddUpdateNotesLike(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("FileUplad")]
        public JsonResponse FileUplad(FileUpload model)
        {
            return _templateService.AddUpdateFileUplad(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("AddUpdateChildData")]
        public async Task<JsonResponse> AddUpdateIndex(DocumentCollection model)
        {

            var data = await _templateService.AddUpdateIndex(model);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var hubData = await _templateService.GetTemplateDetailsByID(model);
            _hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", hubData, UserHandler.ConnectedIds);
            return data;

        }

        [UserAuthenticationFilter]
        [HttpPost, Route("ReplaceDocContent")]
        public async Task<JsonResponse> ReplaceDocContent(Document model)
        {
            var data = await _templateService.ReplaceDocContent(model);

            DocumentCollection documentCollection = new DocumentCollection();
            documentCollection.ObjectID = model.ObjectID;
            documentCollection.CollectionName = model.CollectionName;

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }

            var hubData = await _templateService.GetTemplateDetailsByID(documentCollection);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", hubData, UserHandler.ConnectedIds)));
            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("UpdateIndexValue")]
        public async Task<JsonResponse> UpdateIndexValue(DocumentCollection model)
        {

            var data = await _templateService.Updateindexvalue(model);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var hubData = await _templateService.GetTemplateDetailsByID(model);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", hubData, UserHandler.ConnectedIds)));


            return data;
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("InsertReadingTimeCapture")]
        public JsonResponse InsertReadingTimeCapture(ReadingTimeCapture model)
        {
            return _templateService.InsertReadingTimeCapture(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("DragAndDrop")]
        public async Task<JsonResponse> DragAndDrop(DocumentCollection model)
        {
            var data = await _templateService.DragAndDrop(model);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var hubData = await _templateService.GetTemplateDetailsByID(model);

            _ = Task.Run(() => Task.FromResult(_hub.Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", hubData, UserHandler.ConnectedIds)));
            return data;
        }
        [HttpPost, Route("UpdateVariableDetails")]
        public JsonResponse UpdateVariableDetails(Variable model)
        {
            return _templateService.UpdateVariableDetails(model);
        }

        [UserAuthenticationFilter]
        [HttpPost, Route("VideoUpload")]
        public JsonResponse VideoUpload(VideoUpload model)
        {
            return _templateService.AddUpdateVideoUpload(model);
        }

        [HttpPost, Route("GetDocumentDetails")]
        public async Task<JsonResponse> GetDocumentDetails(DocumentCollection model)
        {
            return await _templateService.GetDocumentDetails(model);
        }
        [UserAuthenticationFilter]
        [HttpPost, Route("GetFolderList")]
        public JsonResponse GetFolderList(DocumentCollection model)
        {
            return _templateService.GetFoderList(model);
        }

        //[UserAuthenticationFilter]
        //[HttpPost, Route("GetFolderList")]
        //public JsonResponse GetFileList(IFormFile formFile)
        //{
        //    return   
        //}
        //public DataTable ReadExcel(IFormFile formFile)
        //{
        //    var excel = new Microsoft.Office.Interop.Excel.Application();
        //    var wkb = excel.Workbooks.Open(formFile, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
        //                                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
        //                                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

        //    var sheet = wkb.Sheets["Drives"] as Excel.Worksheet;

        //    var range = sheet.Range[sheet.Cells[3, 3], sheet.Cells[29, 4]];
        //    var data = range.Value2;

        //    var dt = new DataTable();
        //    dt.Columns.Add("Drive");
        //    dt.Columns.Add("Path");

        //    for (int i = 1; i <= range.Rows.Count; i++)
        //    {
        //        dt.Rows.Add(data[i, 1], data[i, 2]);
        //    }

        //    return dt;
        //}


        //[HttpPost, Route("FileImport")]
        //public JsonResponse FileImport(IFormFile file)
        //{
        //    JsonResponse resp = new JsonResponse();
        //    var extension = Path.GetExtension(file.FileName);
        //    string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf");
        //    //string ExpectedHeader = UploadFileHeader.EqutiyScrips;
        //    ///string[] FileHeader = ExpectedHeader.Split(",");
        //    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }
        //    using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
        //    {
        //        fileStream.Position = 0;
        //        file.CopyTo(fileStream);
        //        IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(fileStream);
        //        var result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
        //        {
        //            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
        //            {
        //                UseHeaderRow = true
        //            }
        //        });
        //        excelReader.Close();
        //        fileStream.Close();
        //       // DataTable table = result.Tables[0];
        //        var rowvalue = string.Empty;


        //        resp.Data = result.Tables[0].AsEnumerable().Select(a => new
        //        {
        //            UserName = a.Field<string>(result.Tables[0].Columns[1].ColumnName)!=null ? a.Field<string>(result.Tables[1].Columns[0].ColumnName):"",

        //            //EUINHolder = a.Field<string>("EUINHolder"),
        //            //Default = a.Field<bool>("Default"),
        //            //EUINNo = a.Field<string>("EUINNo"),
        //            //Password = a.Field<string>("Password"),s
        //            //FreshAdditionalDPC = a.Field<bool>("FreshAdditionalDPC"),
        //            //RedemptionDPC = a.Field<bool>("RedemptionDPC"),
        //            //SwitchDPC = a.Field<bool>("SwitchDPC"),


        //        }).ToList();

        //        //foreach (DataRow row in table.Rows)
        //        //{
        //        //    //for (int i = 0; i < table.Columns.Count; i++)
        //        //    //{
        //        //    var name = table.Columns[1].ColumnName;
        //        //    string sample = Convert.ToString(row[name] != DBNull.Value ? Convert.ToString(row[name]) : "");
        //        //    //dataSet.Tables[0].Rows[i]["DimensionType"] != DBNull.Value ? Convert.ToInt32(dataSet.Tables[0].Rows[i]["DimensionType"]) : 0
        //        //    //string contact = row.Field<string>("CONTACT");
        //        //    // }

        //        //}



        //        //resp.Data = table;
        //        // return table;
        //        //DataTable table = ExcelToDataTable(file, _hostingEnvironment, path);
        //        return resp;
        //    }

        //}

        [HttpPost, Route("FileImport")]
        public dynamic FileImport(IFormFile file)

        {

            var extension = Path.GetExtension(file.FileName);
            string path = Path.Combine(_hostingEnvironment.ContentRootPath, "HtmlToPdf");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.Create))
            {

                fileStream.Position = 0;
                file.CopyTo(fileStream);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(fileStream);
                var result = excelReader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }

                });
                excelReader.Close();
                fileStream.Close();
                JsonResponse jsonResponse = new JsonResponse();
                DataTable table = result.Tables[0];
                List<dynamic> main = new List<dynamic>();
                List<dynamic> dns2 = new List<dynamic>();
                var validFlag = true;
                var elseFlag = true;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (table.Rows[i].ItemArray[0].ToString() != "" && !table.Rows[i].ItemArray[0].ToString().Contains("Since inception date") && !table.Rows[i].ItemArray[0].ToString().Contains("Different plans have different") && !table.Rows[i].ItemArray[0].ToString().Contains("Past performance may") && !table.Rows[i].ItemArray[0].ToString().Contains("TRI - Total Return Index") && !table.Rows[i].ItemArray[0].ToString().Contains("Scheme Inception date"))
                    {
                        validFlag = true;
                        elseFlag = true;
                        if (table.Rows[i].ItemArray != null)
                        {
                            dns2.Add(table.Rows[i].ItemArray);
                        }
                        
                     }
                   
                    //if (table.Rows[i].ItemArray[0].ToString().Contains("Since inception date") || table.Rows[i].ItemArray[0].ToString().Contains("Different plans have different") || table.Rows[i].ItemArray[0].ToString().Contains("Past performance may") || table.Rows[i].ItemArray[0].ToString().Contains("TRI - Total Return Index") || table.Rows[i].ItemArray[0].ToString().Contains("Scheme Inception date") || table.Rows[i].ItemArray[0].ToString().Contains("Alpha is difference"))
                    //{                       
                    //    elseFlag = false;

                    //}
                    //if ((table.Rows[i].ItemArray[0].ToString() != "") && (table.Rows[i].ItemArray[0].ToString().Contains("Since inception date") || table.Rows[i].ItemArray[0].ToString().Contains("Different plans have different") || table.Rows[i].ItemArray[0].ToString().Contains("Past performance may") || table.Rows[i].ItemArray[0].ToString().Contains("TRI - Total Return Index") || table.Rows[i].ItemArray[0].ToString().Contains("Scheme Inception date") || table.Rows[i].ItemArray[0].ToString().Contains("Alpha is difference")) && !elseFlag)
                    // {
                    //    if (validFlag)
                    //    {
                    //        main.Add(dns2);
                    //        dns2 = new List<dynamic>();
                    //        validFlag = false;
                    //    }

                    //}
                }
                jsonResponse.Data = dns2;
                return jsonResponse;

            }

        }




    }
}
