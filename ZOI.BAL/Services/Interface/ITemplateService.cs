using DASAPI.Models;
using Microsoft.AspNetCore.Http;
using ZOI.BAL.Models;

namespace ZOI.BAL.Services.Interface
{
    public interface ITemplateService
    {
        public JsonResponse GetTemplateDetails(DocumentCollection model);
        public Task<JsonResponse> GetTemplateDetailsByID(DocumentCollection model);
        public Task<JsonResponse> InsertDocument(DocumentCollection model, HttpContext context);
        public JsonResponse InsertManyDocument(DocumentCollection model);
        public JsonResponse UpdateDocumentByID(DocumentCollection model);
        public Task<JsonResponse> UpdateJsonKeyValue(DocumentCollection model);
        public Task<JsonResponse> UpdateBlockData(DocumentCollection model);
        public Task<JsonResponse> DeleteRowFromCollection(DocumentCollection model);
        public Task<JsonResponse> InsertMultiCollectionWithRoles(MultiCollection model);
        public Task<JsonResponse> InsertNewRowIntoCollection(DocumentCollection model);
        public JsonResponse GetNotesDetails(DocumentCollection model);
        public Task<JsonResponse> GetCollectionListByUserID(DocumentCollection model);
        public JsonResponse HitoryListing(DocumentCollection model);
        public JsonResponse GetCountOfCollection();
        public JsonResponse UserAuthentication(Users model);
        public JsonResponse UpdateRole(Roles model);
        public JsonResponse UpdateContentBlock(DocumentCollection model);
        public JsonResponse ContentLevelWritePermission(DocumentCollection model);
        public JsonResponse UpdateUserDetails(Users model);
        public JsonResponse UpdateRoleDetails(Roles model);
        public JsonResponse GetUserDetailsByEmail(string token);
        public JsonResponse UpdateKeyPairValue(DocumentCollection model);
        public JsonResponse UpdateChildNotes(Notes model);
        public JsonResponse AddUpdateNotesLike(Notes model);
        public JsonResponse AddUpdateFileUplad(FileUpload model);
        public Task<JsonResponse> AddUpdateIndex(DocumentCollection model);
        public Task<JsonResponse> ReplaceDocContent(Document model);
        public Task<JsonResponse> Updateindexvalue(DocumentCollection model);
        public JsonResponse InsertReadingTimeCapture(ReadingTimeCapture model);
        public Task<JsonResponse> DragAndDrop(DocumentCollection model);
        public JsonResponse UpdateVariableDetails(Variable model);
        public JsonResponse AddUpdateVideoUpload(VideoUpload model);

        public Task<JsonResponse> GetDocumentDetails(DocumentCollection model);
        public JsonResponse GetFoderList(DocumentCollection model);
    }
}
