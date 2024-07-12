using DASAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ZOI.BAL.Models;
using ZOI.BAL.Services.Interface;
using ZOI.BAL.Utilities;
using static ZOI.BAL.Utilities.Constants;

namespace ZOI.BAL.Services
{
    public class TemplatesService : ITemplateService
    {
        private readonly ICompositeViewEngine _viewEngine;
        private const string ControllerStr = "controller";
        ITempDataProvider _tempDataProvider;

        public TemplatesService(ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _tempDataProvider = tempDataProvider;
            _viewEngine = viewEngine;
        }

        public JsonResponse GetTemplateDetails(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                MongoClient mongoClient = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var documentData = mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName);
                var documents = (model.CollectionName == Tables.Users ? documentData.Find(Builders<BsonDocument>.Filter.Eq("isActive", true)).ToList() : documentData.Find(_ => true).ToList());
                if (model.CollectionName == Tables.Template || model.CollectionName == Tables.Documents)
                {
                    var roleData = CommonFunction.BsonSerializer(mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Roles).Find(new BsonDocument().Add("isCollaborator", new BsonBoolean(true))).ToList());
                    GroupOfModel groupOfModel = new GroupOfModel();
                    groupOfModel.Collection = CommonFunction.BsonSerializer(documents);
                    groupOfModel.Roles = roleData;
                    if (model.CollectionName == Tables.Documents)
                    {
                        groupOfModel.DocumentStatus = CommonFunction.BsonSerializer(mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.DocumentStatus).Find(new BsonDocument().Add("IsActive", new BsonBoolean(true))).ToList());
                    }
                    response.Data = groupOfModel;
                }
                else
                {
                    response.Data = CommonFunction.BsonSerializer(documents);
                }
                response.Message = APIResponseMessage.Data + APIResponseMessage.Retrieved + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> GetTemplateDetailsByID(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var options = new AggregateOptions()
                {
                    AllowDiskUse = false
                };

                PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
             {
                new BsonDocument("$addFields", new BsonDocument()
                        .Add("docID", new BsonDocument()
                                .Add("$toString", "$_id")
                        )),
                new BsonDocument("$lookup", new BsonDocument()
                        .Add("from", Tables.Notes)
                        .Add("localField", "docID")
                        .Add("foreignField", "objectID")
                        .Add("as", "Comments")),
                new BsonDocument("$match",new BsonDocument()
                        .Add("docID", model.ObjectID)
                        ),

                new BsonDocument("$project", new BsonDocument()
                        .Add("tempName", 1.0)
                        .Add("tempID", 1.0)
                        .Add("createdBy", 1.0)
                        .Add("Comments.rowID", 1.0)
                        .Add("Comments.blockID", 1.0)
                        .Add("Comments.notesID", 1.0)
                        .Add("Comments.note", 1.0)
                        .Add("statusID", 1.0)
                        .Add("ShowIndex", 1.0)
                        .Add("status", 1.0)
                        .Add("index",1.0)
                         .Add("content", 1.0)
                        .Add("groupId", 1.0)
                        .Add("createdOn", 1.0)
                        .Add("createdByID", 1.0))
             };

                using (var cursor = await client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                    .GetCollection<BsonDocument>(model.CollectionName).AggregateAsync(pipeline, options))
                {
                    BsonArray bsonDocument = new();
                    while (await cursor.MoveNextAsync())
                    {
                        foreach (BsonDocument document in cursor.Current)
                        {
                            bsonDocument.Add(document);
                        }
                    }
                    GroupOfModel groupOfModel = new GroupOfModel();
                    if (model.CollectionName == Tables.Documents)
                    {
                        groupOfModel.DocumentRoles = CommonFunction.BsonSerializer(client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.DocumentRoles).Find("{'documentID':'" + model.ObjectID + "'}").ToList());
                    }
                    groupOfModel.DocumentStatus = CommonFunction.BsonSerializer(client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.DocumentStatus).Find(new BsonDocument().Add("isActive", new BsonBoolean(true))).ToList());
                    groupOfModel.Collection = CommonFunction.BsonSerializer(bsonDocument);
                    response.Data = groupOfModel;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;

        }

        public async Task<JsonResponse> InsertDocument(DocumentCollection model, HttpContext context)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                if (Tables.Users == model.CollectionName)
                {
                    var userData = JsonConvert.DeserializeObject<Users>(model.Data.ToString());
                    string message = CheckEmailAndMobile(userData.Mobile, userData.Email);
                    if (!string.IsNullOrEmpty(message))
                    {
                        response.Status = APIResponseStatus.Failed;
                        response.Message = message;
                        return response;
                    }
                }
                if (Tables.Variables == model.CollectionName)
                {
                    var userData = JsonConvert.DeserializeObject<Variable>(model.Data.ToString());
                    string message = CheckDublicateName(userData.variableName);
                    if (!string.IsNullOrEmpty(message))
                    {
                        response.Status = APIResponseStatus.Failed;
                        response.Message = message;
                        return response;
                    }
                }
                 if (Tables.Roles == model.CollectionName)
                {
                    //var userData = JsonConvert.DeserializeObject<Variable>(model.Data.ToString());
                    string message = CheckDuplicateRole(model.Value);
                    if (!string.IsNullOrEmpty(message))
                    {
                        response.Status = APIResponseStatus.Failed;
                        response.Message = message;
                        return response;
                    }
                }
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var db = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"));
                var mongoCollection = db.GetCollection<BsonDocument>(model.CollectionName);
                var newTemplateDocument = BsonSerializer.Deserialize<BsonDocument>(Convert.ToString(model.Data));
                mongoCollection.InsertOne(newTemplateDocument);
                if (newTemplateDocument["_id"] != null)
                {
                    response.Data = Convert.ToString(newTemplateDocument["_id"]);
                    if (model.CollectionName == Tables.Users)
                    {
                        var userData = JsonConvert.DeserializeObject<Users>(model.Data.ToString());
                        TriggerInvitationMail(context, userData);
                    }
                }
                response.Message = model.CollectionName + APIResponseMessage.Inserted + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse InsertManyDocument(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var db = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"));
                var mongoCollection = db.GetCollection<BsonDocument>(model.CollectionName);
                var newTemplateDocument = BsonSerializer.Deserialize<List<BsonDocument>>(Convert.ToString(model.Data));
                mongoCollection.InsertMany(newTemplateDocument);
                response.Message = model.CollectionName + APIResponseMessage.Inserted + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse UpdateDocumentByID(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{ _id: ObjectId('" + model.ObjectID + "') }";
                var updateDocument = "{ $set: " + model.Data + "}";
                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName)
                    .UpdateOne(updateFilter, updateDocument);
                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);
                response.Message = model.CollectionName + APIResponseMessage.Updated + APIResponseMessage.Success;

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> UpdateJsonKeyValue(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var updateFilter = "{ _id: ObjectId('" + model.ObjectID + "') }";
                BsonDocument projection = new BsonDocument()
                {
                    {model.Key, 1.0},
                    {"_id", 0},
                };
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection")).GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName);
                var documents = client.Find(updateFilter).Project(projection).FirstOrDefault();
                if (documents != null)
                {
                    using (History history = new History())
                    {
                        history.data = JObject.Parse(CommonFunction.BsonSerializer(documents)).Value<string>(model.Key);
                        history.objectID = model.ObjectID;
                        history.createdByID = model.CreatedByID;
                        history.createdBy = model.CreatedBy;
                        history.createdOn = CommonFunction.GetCurrentDateTime();
                        InsertHistory(history);
                    }
                }

                var update = Builders<BsonDocument>.Update.Set(model.Key, model.Value);
                var updateResult = client.UpdateOne(updateFilter, update);
                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);



                response.Message = APIResponseMessage.Updated + APIResponseMessage.Success;

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> UpdateBlockData(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";
                //var setUpdate = "{$set{'content.$[a].rowValue.$[b].data':'" + model.Value + "'}}";
                var setUpdate = Builders<BsonDocument>.Update.Set("content.$[a].rowValue.$[b].data", model.Value);
                var arrayFilters = new List<ArrayFilterDefinition>();
                ArrayFilterDefinition<BsonDocument> rowFilter = new BsonDocument("a.rowID", new BsonDocument("$eq", model.RowID));
                ArrayFilterDefinition<BsonDocument> blockFilter = new BsonDocument("b.blockID", new BsonDocument("$eq", model.BlockID));
                arrayFilters.Add(rowFilter);
                arrayFilters.Add(blockFilter);
                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
                try
                {
                    PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
                    {
                             new BsonDocument("$project", new BsonDocument()
                                     .Add("content.rowValue.data", 1.0)
                                     .Add("content.rowValue.blockID", 1.0)
                                     .Add("content.rowID", 1.0)),
                             new BsonDocument("$unwind", new BsonDocument()
                                     .Add("path", "$content")
                                     .Add("preserveNullAndEmptyArrays", new BsonBoolean(true))),
                             new BsonDocument("$unwind", new BsonDocument()
                                     .Add("path", "$content.rowValue")
                                     .Add("preserveNullAndEmptyArrays", new BsonBoolean(true))),
                             new BsonDocument("$match", new BsonDocument()
                                     .Add("_id", new BsonObjectId(new ObjectId(model.ObjectID)))
                                     .Add("content.rowID", model.RowID)
                                     .Add("content.rowValue.blockID", model.BlockID))
                    };
                    using (var cursor = await client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName).AggregateAsync(pipeline))
                    {
                        while (await cursor.MoveNextAsync())
                        {
                            foreach (BsonDocument document in cursor.Current)
                            {
                                var data = document.GetValue("content")["rowValue"]["data"];
                                if (data != null)
                                {
                                    using (History history = new History())
                                    {
                                        history.data = Convert.ToString(data);
                                        history.objectID = model.ObjectID;
                                        history.createdByID = model.CreatedByID;
                                        history.createdBy = model.CreatedBy;
                                        history.rowID = model.RowID;
                                        history.blockID = model.BlockID;
                                        history.createdOn = CommonFunction.GetCurrentDateTime();
                                        InsertHistory(history);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                }
                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                .GetCollection<BsonDocument>(model.CollectionName)
                .UpdateOne(updateFilter, setUpdate, updateOptions);
                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);
                response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> DeleteRowFromCollection(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var retriveFilter = "{'_id': ObjectId('" + model.ObjectID + "'), 'content' : {$elemMatch :{'rowID': '" + model.RowID + "'}}}";
                BsonDocument filter = new BsonDocument();
                filter.Add("_id", new BsonObjectId(new ObjectId(model.ObjectID)));
                filter.Add("content.rowID", model.RowID);
                filter.Add("content.$.rowID", model.RowID);

                /* To select the single object from array */
                BsonDocument projection = new BsonDocument();
                projection.Add("content.$", 1.0);
                projection.Add("_id", 0);
                var options = new FindOptions<BsonDocument>()
                {
                    Projection = projection
                };
                var cursor = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName).Find(filter).Project(projection).FirstOrDefault();
                try
                {
                    using (History history = new History())
                    {
                        history.objectID = model.ObjectID;
                        history.rowID = model.RowID;
                        history.createdByID = model.CreatedByID;
                        history.createdBy = model.CreatedBy;
                        history.createdOn = CommonFunction.GetCurrentDateTime();
                        InsertHistory(history);
                    }
                }
                catch (Exception e)
                {
                    StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                }

                var updates = Builders<BsonDocument>.Update.PullFilter("content",
                    Builders<BsonDocument>.Filter.Eq("rowID", model.RowID));
                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName)
                    .UpdateOne("{'_id': ObjectId('" + model.ObjectID + "')}", updates);
                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);


                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";
                UpdateDefinition<BsonDocument> addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("index",
                  Builders<BsonDocument>.Filter.Eq("rowID", model.RowID));
                client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                                         .GetCollection<BsonDocument>(model.CollectionName)
                                                         .UpdateOne(updateFilter, addUpdateFilter);


                response.Message = APIResponseMessage.Deleted.TrimStart() + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> InsertMultiCollectionWithRoles(MultiCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var docModel = new DocumentCollection()
                {
                    CollectionName = model.CollectionName,
                    Data = model.Data
                };
                var collectionResult = await InsertDocument(docModel, null);

                if (model.Roles != null)
                {
                    var rolesList = JsonConvert.DeserializeObject<List<Roles>>(Convert.ToString(model.Roles));
                    for (int incRoles = 0; incRoles < rolesList.Count(); incRoles++)
                    {
                        if (string.IsNullOrEmpty(rolesList[incRoles].documentID))
                        {
                            rolesList[incRoles].documentID = Convert.ToString(collectionResult.Data);
                        }
                    }
                    if (rolesList != null && collectionResult != null && collectionResult.Data != null)
                    {
                        var roleModel = new DocumentCollection()
                        {
                            CollectionName = Tables.DocumentRoles,
                            Data = JsonConvert.SerializeObject(rolesList)
                        };
                        var rolesResult = InsertManyDocument(roleModel);
                    }
                }
                if (collectionResult.Data != null)
                {
                    response.Data = collectionResult.Data;
                }
                response.Message = APIResponseMessage.Inserted.TrimStart() + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> InsertNewRowIntoCollection(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                #region Need to work on find index - sankar

                //PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
                //{
                //new BsonDocument("$project", new BsonDocument()
                //        .Add("matchedIndex", new BsonDocument()
                //        .Add("$indexOfArray", new BsonArray()
                //        .Add("$content.rowID")
                //        .Add("2022072817541223490"))))};
                //var options = new AggregateOptions()
                //{
                //    AllowDiskUse = false
                //};

                //int indexPosition = 0;
                //using (var cursor = await client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                //    .GetCollection<BsonDocument>("Documents").AggregateAsync(pipeline, options))
                //{
                //    while (await cursor.MoveNextAsync())
                //    {
                //        var batch = cursor.Current;
                //        foreach (BsonDocument document in batch)
                //        {
                //            if (document["_id"].ToString() == "62f31020651b2542640382ff")
                //            {
                //                indexPosition = Convert.ToInt32(document["matchedIndex"]) + 1;
                //                break;
                //            }
                //        }
                //    }
                //}
                #endregion
                var insertObject = "{$push:{content:{'$each':[" + model.Data + "], '$position': " + (model.IndexOfRow == -1 ? 0 : model.IndexOfRow + 1) + "}}}";
                var updateFilter = "{ _id: ObjectId('" + model.ObjectID + "') }";

                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                   .GetCollection<BsonDocument>(model.CollectionName)
                   .UpdateOne(updateFilter, insertObject);
                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);
                response.Message = APIResponseMessage.Inserted.TrimStart() + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse GetNotesDetails(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new BsonDocument();
                if (!string.IsNullOrEmpty(model.ObjectID))
                    filter.Add("objectID", model.ObjectID);
                if (!string.IsNullOrEmpty(model.RowID))
                    filter.Add("rowID", model.RowID);
                if (!string.IsNullOrEmpty(model.BlockID))
                    filter.Add("blockID", model.BlockID);

                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                    .GetCollection<BsonDocument>(model.CollectionName)
                                    .Find(filter).ToList();

                if (updateResult != null)
                    response.Data = CommonFunction.BsonSerializer(updateResult);
                response.Message = APIResponseMessage.Data.TrimStart() + APIResponseMessage.Retrieved + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> GetCollectionListByUserID(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var options = new AggregateOptions()
                {
                    AllowDiskUse = false
                };

                PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
             {
                new BsonDocument("$addFields", new BsonDocument()
                        .Add("docID", new BsonDocument()
                                .Add("$toString", "$_id")
                        )),

                new BsonDocument("$lookup", new BsonDocument()
                        .Add("from", Tables.DocumentRoles)
                        .Add("localField", "docID")
                        .Add("foreignField", "documentID")
                        .Add("as", "roles")),

                 new BsonDocument("$addFields", new BsonDocument()
                        .Add("docgroupID", new BsonDocument()
                                .Add("$toObjectId", "$groupId")
                        )),
                new BsonDocument("$lookup", new BsonDocument()

                        .Add("from", Tables.DocumentGroup)
                        .Add("localField", "docgroupID")
                        .Add("foreignField", "_id")
                        .Add("as", "group")),
                 new BsonDocument("$match", new BsonDocument()
                        .Add("$or", new BsonArray()
                                .Add(new BsonDocument()
                                        .Add("roles", new BsonDocument()
                                                .Add("$elemMatch", new BsonDocument()
                                                        .Add("members.memberID", model.UserID)
                                                )
                                        )
                                )
                                .Add(new BsonDocument()
                                        .Add("createdByID", model.UserID)
                                )
                        )),
                new BsonDocument("$project", new BsonDocument()
                        .Add("tempName", 1.0)
                        .Add("createdBy", 1.0)
                        .Add("roles.roleID", 1.0)
                        //.Add("content", 1.0)
                        .Add("roles.roleName", 1.0)
                        .Add("roles.members", 1.0)
                        .Add("statusID", 1.0)
                        .Add("createdOn", 1.0)
                        .Add("createdByID", 1.0)
                        //.Add("group._id",1.0)
                        .Add("group.name", 1.0))


             };

                using (var cursor = await client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                    .GetCollection<BsonDocument>(model.CollectionName).AggregateAsync(pipeline, options))
                {
                    BsonArray bsonDocument = new();
                    while (await cursor.MoveNextAsync())
                    {
                        foreach (BsonDocument document in cursor.Current)
                        {
                            bsonDocument.Add(document);
                        }
                    }
                    GroupOfModel groupOfModel = new GroupOfModel();
                    groupOfModel.DocumentStatus = CommonFunction.BsonSerializer(client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.DocumentStatus).Find(new BsonDocument().Add("isActive", new BsonBoolean(true))).ToList());
                    groupOfModel.Collection = CommonFunction.BsonSerializer(bsonDocument);
                    response.Data = groupOfModel;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        public JsonResponse HitoryListing(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(model.ObjectID));
                MongoClient mongoClient = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var data = mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName);
                var stringFilter = "{ 'documentID': '" + model.ObjectID + "'}";
                BsonDocument projection = new BsonDocument()
                {
                    {"_id", 0},
                };
                var documents = data.Find(stringFilter).Project(projection).ToList();
                if (documents != null)
                    response.Data = CommonFunction.BsonSerializer(documents);
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public void InsertHistory(History model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.data))
                    InsertDocument(new DocumentCollection()
                    {
                        CollectionName = Tables.History
                    ,
                        Data = JsonConvert.SerializeObject(model)
                    }, null);
            }
            catch { }
        }

        public JsonResponse GetCountOfCollection()
        {
            JsonResponse response = new JsonResponse();
            try
            {
                List<Dictionary<string, object>> collectionKeyValuePairs = new List<Dictionary<string, object>>();
                Dictionary<string, object> keyValuePair = new Dictionary<string, object>();
                keyValuePair.Add("documentCount", GetCollectionCount(Tables.Documents));
                keyValuePair.Add("templateCount", GetCollectionCount(Tables.Template));
                keyValuePair.Add("usersCount", GetCollectionCount(Tables.Users));
                keyValuePair.Add("activeUsers", 0/*GetCollectionCount(Tables.Users)*/);
                keyValuePair.Add("inActiveUsers", 0/* GetCollectionCount(Tables.Users)*/);
                collectionKeyValuePairs.Add(keyValuePair);
                response.Data = JsonConvert.SerializeObject(collectionKeyValuePairs);
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public long GetCollectionCount(string collectionName)
        {
            try
            {
                MongoClient mongoClient = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new BsonDocument();
                if (collectionName == Tables.Users)
                {
                    filter.Add("isActive", true);
                }
                return mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(collectionName).Find(filter).Count();
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return 0;
            }
        }

        public JsonResponse UserAuthentication(Users model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new BsonDocument();
                filter.Add("email", model.Email);
                filter.Add("password", model.Password);
                filter.Add("isActive", true);
                var documents = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                                 .GetCollection<BsonDocument>(Tables.Users)
                                                 .Find(filter).FirstOrDefault();
                if (documents != null)
                {
                    response.Data = CommonFunction.BsonSerializer(documents);
                    response.Message = APIResponseMessage.UserAuthentication.TrimStart() + APIResponseMessage.Success;
                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.InvalidEmailOrPassword;
                }
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return response;
            }
        }

        public JsonResponse UpdateRole(Roles model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new BsonDocument();
                filter.Add("roleID", model.roleID);
                filter.Add("documentID", model.documentID);
                UpdateResult updateResult;
                if (model.isAdd != null && model.isAdd == false)
                {
                    var addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("members",
                        Builders<BsonDocument>.Filter.Eq("memberID", model.memberID));
                    updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                                     .GetCollection<BsonDocument>(Tables.DocumentRoles)
                                                     .UpdateOne(filter, addUpdateFilter);

                }
                else
                {
                    var addUpdateFilter = "{$push:{members:{'$each':[" + model.members + "]}}}";
                    updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                                 .GetCollection<BsonDocument>(Tables.DocumentRoles)
                                                 .UpdateOne(filter, addUpdateFilter);

                }
                if (updateResult != null && updateResult.MatchedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse UpdateContentBlock(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";
                var setUpdate = Builders<BsonDocument>.Update.Set("content.$[a].rowValue.$[b]." + model.Key, model.KeyValue);
                var arrayFilters = new List<ArrayFilterDefinition>();
                ArrayFilterDefinition<BsonDocument> rowFilter = new BsonDocument("a.rowID", new BsonDocument("$eq", model.RowID));
                ArrayFilterDefinition<BsonDocument> blockFilter = new BsonDocument("b.blockID", new BsonDocument("$eq", model.BlockID));
                arrayFilters.Add(rowFilter);
                arrayFilters.Add(blockFilter);
                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                .GetCollection<BsonDocument>(model.CollectionName)
                .UpdateOne(updateFilter, setUpdate, updateOptions);
                if (updateResult != null && updateResult.ModifiedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }
                return response;

            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse ContentLevelWritePermission(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";
                UpdateDefinition<BsonDocument> addUpdateFilter = null;
                if (model.IsAdd != null && model.IsAdd == false)
                {
                    addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("content.$[a].rowValue.$[b].writePermission",
                  Builders<BsonDocument>.Filter.Eq("memberID", model.UserID));
                }
                else
                {
                    addUpdateFilter = "{$push:{'content.$[a].rowValue.$[b].writePermission':{'$each':[" + model.Data + "]}}}";
                }
                var arrayFilters = new List<ArrayFilterDefinition>();
                ArrayFilterDefinition<BsonDocument> rowFilter = new BsonDocument("a.rowID", new BsonDocument("$eq", model.RowID));
                ArrayFilterDefinition<BsonDocument> blockFilter = new BsonDocument("b.blockID", new BsonDocument("$eq", model.BlockID));
                arrayFilters.Add(rowFilter);
                arrayFilters.Add(blockFilter);
                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                             .GetCollection<BsonDocument>(Tables.Documents)
                                             .UpdateOne(updateFilter, addUpdateFilter, updateOptions);
                if (updateResult != null && updateResult.ModifiedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = Tables.Documents + APIResponseMessage.Updated + APIResponseMessage.Success;
                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        public JsonResponse UpdateUserDetails(Users model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                string message = CheckEmailAndMobileForUpdates(model.Mobile, model.Email, model.ObjectID);
                if (!string.IsNullOrEmpty(message))
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = message;
                    return response;
                }
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));

                var setUpdate = "{$set:{'name':'" + model.Name + "','email':'" + model.Email + "','mobile':'" + model.Mobile +
                    "','department':" + model.Department + ",'reportingTo':" + model.ReportingTo + ",'role':" + model.Role + "}}";
                //var update = Builders<BsonDocument>.Update.Set("name", model.Name).Set("email", model.Email)
                //    .Set("mobile", model.Mobile).Set("department", model.Department)
                //    .Set("reportingTo", model.ReportingTo).Set("role", model.Role);

                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                             .GetCollection<BsonDocument>(Tables.Users)
                                             .UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(model.ObjectID)), setUpdate);
                //if (updateResult != null && updateResult.ModifiedCount == 1)
                //{
                response.Data = updateResult;
                response.Message = Tables.Users + APIResponseMessage.Updated + APIResponseMessage.Success;
                //}
                //else
                //{
                //    response.Status = APIResponseStatus.Failed;
                //    response.Message = APIResponseMessage.Failed;
                //    response.Data = updateResult;
                //}
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        public JsonResponse UpdateRoleDetails(Roles model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                bool collab = model.isCollaborator;
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                //var setUpdate = "{$set:{'name':'" + model.roleName + "','isCollaborator':" + model.isCollaborator
                //    + ",'rights':" + model.rights + "}}";
                var updateDefinition = Builders<BsonDocument>.Update.Set("rights", BsonSerializer.Deserialize<BsonArray>(Convert.ToString(model.rights))).Set("isCollaborator", (bool)model.isCollaborator).Set("name", (string)model.roleName);
                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                             .GetCollection<BsonDocument>(Tables.Roles)
                                             .UpdateOne("{'_id': ObjectId('" + model.objectID + "')}"
                                             , updateDefinition);
                if (updateResult != null)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;
                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        public void TriggerInvitationMail(HttpContext httpContext, Users model)
        {
            try
            {
                if (model.IsInvite != null && model.IsInvite == true)
                {
                    UserInvitation userInvitation = new UserInvitation();
                    userInvitation.Token = Guid.NewGuid().ToString();
                    userInvitation.Email = model.Email;
                    InsertDocument(new DocumentCollection()
                    {
                        Data = JsonConvert.SerializeObject(userInvitation),
                        CollectionName = Tables.UserInvite
                    }, null);
                    model.RequestToken = Encryption.EncryptData(userInvitation.Token + "|" + model.Email);
                    model.Url = CommonFunction.GetConnectionString("baseUrl") + model.RequestToken;
                }
                string viewName = "Templates/TeamRequestTemplate.cshtml";
                string absoluteViewName = "~/Templates/TeamRequestTemplate.cshtml";
                string htmlText = RenderToStringAsync(httpContext, viewName, absoluteViewName, model, null);
                string senderEmail = CommonFunction.GetConnectionString("senderEmail");
                string sMTPHost = CommonFunction.GetConnectionString("sMTPHost");
                string sMTPPort = CommonFunction.GetConnectionString("sMTPServer");
                string sMTPUserName = CommonFunction.GetConnectionString("sMTPUserName");
                string sMTPPassword = CommonFunction.GetConnectionString("sMTPPassword");
                Mailer.SendMailUsingSMTP(senderEmail, model.Email, htmlText, "Document authorization system - Invitation"
                    , sMTPHost, Convert.ToInt32(sMTPPort), sMTPUserName, sMTPPassword);
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
            }
        }

        public string RenderToStringAsync(HttpContext _context, string viewName, string absoluteViewName, object model, ViewDataDictionary viewDataDictionary)
        {
            try
            {
                var controller = string.Empty;
                viewName = viewName?.TrimStart(new char[] { '/' });
                Regex rex = new Regex(@"^(\w+)\/(.*)$");
                Match match = rex.Match(viewName);
                if (match.Success)
                {
                    controller = match.Groups[1].Value;
                    viewName = match.Groups[2].Value;
                }
                var routeData = new RouteData();
                routeData.Values.Add(ControllerStr, controller);
                var actionContext = new ActionContext(_context, routeData, new ActionDescriptor());
                var viewResult = _viewEngine.GetView(absoluteViewName, viewName, false);
                if (viewResult.View == null)
                {
                    Console.WriteLine($"Searched the following locations: {string.Join(", ", viewResult.SearchedLocations)} for folder \"{controller}\" and view \"{viewName}\"");
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }
                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
                if (viewDataDictionary != null)
                {
                    foreach (var obj in viewDataDictionary)
                    {
                        viewDictionary.Add(obj);
                    }
                }
                using (var sw = new StringWriter())
                {
                    var viewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext(
                        actionContext,
                        viewResult.View,
                        viewDictionary,
                        new TempDataDictionary(_context, _tempDataProvider),
                        sw,
                        new HtmlHelperOptions()
                    );
                    viewContext.RouteData = _context.GetRouteData();
                    viewResult.View.RenderAsync(viewContext);
                    return sw.ToString();
                }
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        public JsonResponse GetUserDetailsByEmail(string token)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                token = Decryption.DecryptData(token.Replace(' ', '+'));
                var tokenData = token.Split("|");
                token = tokenData[1];
                if (!CheckTokenValidity(tokenData[0]))
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.TokenHasExpired;
                    return response;
                }
                if (!CheckPasswordHasSet(token))
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("email", token);
                    MongoClient mongoClient = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                    var data = mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Users).Find(filter).FirstOrDefault();
                    if (data != null)
                        response.Data = CommonFunction.BsonSerializer(data);
                    return response;
                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.PasswordAlreadySet;
                    return response;
                }
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public bool CheckTokenValidity(string token)
        {
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var data = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.UserInvite);
                string filter = "{Token:'" + token + "'}";
                var documents = data.Find(filter).Project("{'_id':0.0}").FirstOrDefault();
                if (documents != null)
                {
                    var userInvitation = JsonConvert.DeserializeObject<UserInvitation>(CommonFunction.BsonSerializer(documents));
                    if (userInvitation != null && userInvitation.ExpiredOn != null && DateTime.Parse(userInvitation.ExpiredTill) >= DateTime.Now)
                    {
                        return true;
                    }
                    else
                    {
                        data.UpdateOne(filter, Builders<BsonDocument>.Update.Set("IsExpired", true).Set("ExpiredOn", DateTime.Now.ToString(DateTimeFormat.DateTimeWith24HrsFormat)));
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        public bool CheckPasswordHasSet(string email)
        {
            try
            {
                BsonDocument filter = new BsonDocument();

                filter.Add("email", email);
                filter.Add("password", new BsonDocument()
                        .Add("$exists", new BsonBoolean(true))
                );
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));

                var data = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Users);
                var documents = data.Find(filter).FirstOrDefault();
                if (documents == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name); return true;
            }

        }

        public string CheckDublicateName(string name)
        {
            try
            {

                string duplicateMessage = string.Empty;
                bool nameExists = false;
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new();
                filter.Add("variableName", name);
                var data = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Variables);
                var documents = data.Find(filter).FirstOrDefault();
                if (documents != null)
                {
                    nameExists = true;
                    duplicateMessage = APIResponseMessage.nameAlreadyExists;
                }

                return duplicateMessage;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }
         public string CheckDuplicateRole(string name)
        {
            try
            {

                string duplicateMessage = string.Empty;
                bool nameExists = false;
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new();
                filter.Add("name", name);
                var data = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Roles);
                var documents = data.Find(filter).FirstOrDefault();
                if (documents != null)
                {
                    nameExists = true;
                    duplicateMessage = APIResponseMessage.nameAlreadyExists;
                }

                return duplicateMessage;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }


        public string CheckEmailAndMobile(string mobile, string email)
        {
            try
            {

                string duplicateMessage = string.Empty;
                bool mobileExists = false;
                bool emailExists = false;
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                BsonDocument filter = new();
                filter.Add("mobile", mobile);
                filter.Add("isActive", true);
                var data = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Users);
                var documents = data.Find(filter).FirstOrDefault();
                if (documents != null)
                {
                    mobileExists = true;
                    duplicateMessage = APIResponseMessage.MobileAlreadyExists;
                }
                filter = new();
                filter.Add("email", email);
                filter.Add("isActive", true);
                var emailDocuments = data.Find(filter).FirstOrDefault();
                if (emailDocuments != null)
                {
                    emailExists = true;
                    duplicateMessage = APIResponseMessage.EmailAlreadyExists;
                }
                if (emailExists && mobileExists)
                {
                    duplicateMessage = APIResponseMessage.MobileAndEmailAlreadyExists;
                }
                return duplicateMessage;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }


        public string CheckEmailAndMobileForUpdates(string mobile, string email, string objectID)
        {
            try
            {
                string duplicateMessage = string.Empty;
                bool mobileExists = false;
                bool emailExists = false;
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var filter = "{ _id: {$ne: ObjectId('" + objectID + "')}, mobile:'" + mobile + "' }";
                var data = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(Tables.Users);
                var documents = data.Find(filter).FirstOrDefault();
                if (documents != null)
                {
                    mobileExists = true;
                    duplicateMessage = APIResponseMessage.MobileAlreadyExists;
                }
                filter = "{ _id: {$ne: ObjectId('" + objectID + "')}, email:'" + email + "' }";
                var emailDocuments = data.Find(filter).FirstOrDefault();
                if (emailDocuments != null)
                {
                    emailExists = true;
                    duplicateMessage = APIResponseMessage.EmailAlreadyExists;
                }
                if (emailExists && mobileExists)
                {
                    duplicateMessage = APIResponseMessage.MobileAndEmailAlreadyExists;
                }
                return duplicateMessage;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }


        public JsonResponse UpdateKeyPairValue(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var updateFilter = "{ _id: ObjectId('" + model.ObjectID + "') }";
                BsonDocument projection = new BsonDocument()
                {
                    {model.Key, 1.0},
                    {"_id", 0},
                };
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection")).GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName);
                var documents = client.Find(updateFilter).Project(projection).FirstOrDefault();
                if (documents != null)
                {
                    using (History history = new History())
                    {
                        history.data = JObject.Parse(CommonFunction.BsonSerializer(documents)).Value<string>(model.Key);
                        history.objectID = model.ObjectID;
                        history.createdByID = model.CreatedByID;
                        history.createdBy = model.CreatedBy;
                        history.createdOn = CommonFunction.GetCurrentDateTime();
                        InsertHistory(history);
                    }
                }

                var update = Builders<BsonDocument>.Update.Set(model.Key, model.KeyValue);
                var updateResult = client.UpdateOne(updateFilter, update);
                if (updateResult != null)
                {
                    response.Data = JsonConvert.SerializeObject(updateResult);
                    if ((model.CollectionName == Tables.Users || model.CollectionName == Tables.Roles || model.CollectionName == Tables.Gallery || model.CollectionName == Tables.Variables)
                        && model.Key == "isActive" && model.KeyValue == false)
                    {
                        response.Message = APIResponseMessage.Deleted.TrimStart() + APIResponseMessage.Success;
                    }
                    else
                    {
                        response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }



        /*Store the errorlog */

        public void StoreErrorLog(string message, string stkTrace, string methodName)
        {
            try
            {
                var errObj = new ErrorLog()
                {
                    Message = message,
                    StkStrace = stkTrace,
                    MethodName = methodName
                };

                InsertDocument(new DocumentCollection()
                {
                    CollectionName = Tables.ErrorLog
                    ,
                    Data = JsonConvert.SerializeObject(errObj)
                }, null);
            }
            catch
            {

            }
        }

        public JsonResponse UpdateChildNotes(Notes model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.objectID + "')}";

                UpdateDefinition<BsonDocument> addUpdateFilter = null;

                addUpdateFilter = "{$push:{'replay':{'$each':[" + model.Data + "]}}}";

                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                           .GetCollection<BsonDocument>(Tables.Notes)
                                           .UpdateOne(updateFilter, addUpdateFilter);

                if (updateResult != null && updateResult.MatchedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse AddUpdateNotesLike(Notes model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.objectID + "')}";

                UpdateDefinition<BsonDocument> addUpdateFilter = null;


                if (model.IsAdd != null && model.IsAdd == false)
                {
                    addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("likes",
                  Builders<BsonDocument>.Filter.Eq("createdByID", model.rowID));
                }
                else
                {
                    addUpdateFilter = "{$push:{'likes':{'$each':[" + model.Data + "]}}}";
                }

                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                           .GetCollection<BsonDocument>(Tables.Notes)
                                           .UpdateOne(updateFilter, addUpdateFilter);

                if (updateResult != null && updateResult.MatchedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse AddUpdateFileUplad(FileUpload model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.objectID + "')}";

                UpdateDefinition<BsonDocument> addUpdateFilter = null;
                if (model.isAdd != null && model.isAdd == false)
                {
                    addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("imageList",
                  Builders<BsonDocument>.Filter.Eq("imageID", model.imageID));
                }
                else
                {
                    addUpdateFilter = "{$push:{'imageList':{'$each':[" + model.imageList + "]}}}";
                }

                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                          .GetCollection<BsonDocument>(Tables.Gallery)
                                          .UpdateOne(updateFilter, addUpdateFilter);

                if (updateResult != null && updateResult.MatchedCount == 1 && model.isAdd == false)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Deleted.TrimStart() + APIResponseMessage.Success;

                }
                else if (updateResult != null && updateResult.MatchedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Inserted.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }

            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }

        }

        public async Task<JsonResponse> AddUpdateIndex(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";

                UpdateDefinition<BsonDocument> addUpdateFilter = null;


                if (model.IsAdd != null && model.IsAdd == false)
                {
                    addUpdateFilter = Builders<BsonDocument>.Update.PullFilter(model.Key,
                  Builders<BsonDocument>.Filter.Eq(model.Value, model.RowID));
                }
                else
                {
                    addUpdateFilter = "{$push:{" + model.Key + ":{'$each':[" + model.Data + "]}}}";
                }

                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                           .GetCollection<BsonDocument>(model.CollectionName)
                                           .UpdateOne(updateFilter, addUpdateFilter);

                if (updateResult != null && updateResult.MatchedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }

        }


        public async Task<JsonResponse> ReplaceDocContent(Document model)
        {
            JsonResponse response = new JsonResponse();
            try
            {

                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";

                foreach (var item in model.Content)
                {
                    var setUpdate = Builders<BsonDocument>.Update.Set("content.$[a].rowValue.$[b].data", item.Value);
                    var arrayFilters = new List<ArrayFilterDefinition>();
                    ArrayFilterDefinition<BsonDocument> rowFilter = new BsonDocument("a.rowID", new BsonDocument("$eq", item.RowID));
                    ArrayFilterDefinition<BsonDocument> blockFilter = new BsonDocument("b.blockID", new BsonDocument("$eq", item.BlockID));
                    arrayFilters.Add(rowFilter);
                    arrayFilters.Add(blockFilter);
                    var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

                    PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
                    {
                             new BsonDocument("$project", new BsonDocument()
                                     .Add("content.rowValue.data", 1.0)
                                     .Add("content.rowValue.blockID", 1.0)
                                     .Add("content.rowID", 1.0)),
                             new BsonDocument("$unwind", new BsonDocument()
                                     .Add("path", "$content")
                                     .Add("preserveNullAndEmptyArrays", new BsonBoolean(true))),
                             new BsonDocument("$unwind", new BsonDocument()
                                     .Add("path", "$content.rowValue")
                                     .Add("preserveNullAndEmptyArrays", new BsonBoolean(true))),
                             new BsonDocument("$match", new BsonDocument()
                                     .Add("_id", new BsonObjectId(new ObjectId(model.ObjectID)))
                                     .Add("content.rowID", item.RowID)
                                     .Add("content.rowValue.blockID", item.BlockID))
                    };
                    using (var cursor = await client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName).AggregateAsync(pipeline))
                    {
                        while (await cursor.MoveNextAsync())
                        {
                            foreach (BsonDocument document in cursor.Current)
                            {
                                var data = document.GetValue("content")["rowValue"]["data"];
                                if (data != null)
                                {
                                    using (History history = new History())
                                    {
                                        history.data = Convert.ToString(data);
                                        history.objectID = model.ObjectID;
                                        history.createdByID = model.CreatedByID;
                                        history.createdBy = model.CreatedBy;
                                        history.rowID = item.RowID;
                                        history.blockID = item.BlockID;
                                        history.createdOn = CommonFunction.GetCurrentDateTime();
                                        InsertHistory(history);
                                    }
                                }
                            }
                        }
                    }



                    var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName)
                    .UpdateOne(updateFilter, setUpdate, updateOptions);
                    if (updateResult != null)
                        response.Data = JsonConvert.SerializeObject(updateResult);
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;
                }
                return response;


            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        public async Task<JsonResponse> Updateindexvalue(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var updateFilter = "{ _id: ObjectId('" + model.ObjectID + "') }";

                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                //.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName);


                var setUpdate = Builders<BsonDocument>.Update.Set("index.$[a].data", model.Value);
                var arrayFilters = new List<ArrayFilterDefinition>();
                ArrayFilterDefinition<BsonDocument> rowFilter = new BsonDocument("a.indexID", new BsonDocument("$eq", model.RowID));
                arrayFilters.Add(rowFilter);
                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                .GetCollection<BsonDocument>(model.CollectionName)
                .UpdateOne(updateFilter, setUpdate, updateOptions);


                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);



                response.Message = APIResponseMessage.Updated + APIResponseMessage.Success;

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse InsertReadingTimeCapture(ReadingTimeCapture model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection")).GetDatabase(CommonFunction.GetConnectionString("DatabaseName"));
                var TimeSpent = client.GetCollection<BsonDocument>(Tables.TimeSpentOnDocumentUsers);

                var newTemplateDocument = BsonSerializer.Deserialize<BsonDocument>(Convert.ToString(model.TimeSpentOnDocumentUsers));

                TimeSpent.InsertOne(newTemplateDocument);
                if (newTemplateDocument["_id"] != null)
                {

                    var TimeSpentDocTable = client.GetCollection<BsonDocument>(Tables.TimeSpentOnDocuments);

                    var timeSpentOnDoc = new BsonDocument
                    {
                        {"uniqueIDOfTimeSpentOnDocumentUsers",Convert.ToString(newTemplateDocument["_id"])},
                        {"type", model.TimeSpentOnDocuments.Type},
                        {"fromTime", model.TimeSpentOnDocuments.FromTime},
                        {"toTime",model.TimeSpentOnDocuments.ToTime }
                    };

                    TimeSpentDocTable.InsertOne(timeSpentOnDoc);

                }
                response.Message = APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public async Task<JsonResponse> DragAndDrop(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var retriveFilter = "{'_id': ObjectId('" + model.ObjectID + "'), 'content' : {$elemMatch :{'rowID': '" + model.RowID + "'}}}";
                BsonDocument filter = new BsonDocument();
                filter.Add("_id", new BsonObjectId(new ObjectId(model.ObjectID)));
                filter.Add("content.rowID", model.RowID);
                filter.Add("content.$.rowID", model.RowID);
                BsonDocument projection = new BsonDocument();
                projection.Add("content.$", 1.0);
                projection.Add("_id", 0);
                var options = new FindOptions<BsonDocument>()
                {
                    Projection = projection
                };
                var cursor = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName).Find(filter).Project(projection).FirstOrDefault();

                var updates = Builders<BsonDocument>.Update.PullFilter("content",
                    Builders<BsonDocument>.Filter.Eq("rowID", model.RowID));
                var updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                    .GetCollection<BsonDocument>(model.CollectionName)
                    .UpdateOne("{'_id': ObjectId('" + model.ObjectID + "')}", updates);
                if (updateResult != null)
                    response.Data = JsonConvert.SerializeObject(updateResult);


                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";
                UpdateDefinition<BsonDocument> addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("index",
                  Builders<BsonDocument>.Filter.Eq("rowID", model.RowID));
                client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                                         .GetCollection<BsonDocument>(model.CollectionName)
                                                         .UpdateOne(updateFilter, addUpdateFilter);
                var clientdrag = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var insertObject = "{$push:{content:{'$each':[" + model.Data + "], '$position': " + (model.IndexOfRow == -1 ? 0 : model.IndexOfRow + 1) + "}}}";
                var updateFilterdes = "{ _id: ObjectId('" + model.ObjectID + "') }";

                var updateResultdrag = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                   .GetCollection<BsonDocument>(model.CollectionName)
                   .UpdateOne(updateFilterdes, insertObject);
                if (updateResultdrag != null)
                    response.Data = JsonConvert.SerializeObject(updateResultdrag);
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }
        public JsonResponse UpdateVariableDetails(Variable model)
        {
            JsonResponse response = new JsonResponse();
            try
            {

                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));

                var updateDefinition = Builders<BsonDocument>.Update.Set("variableName", model.variableName).Set("variableValue", model.variableValue).Set("isApplyAll", model.isApplyAll).Set("group", BsonSerializer.Deserialize<BsonArray>(Convert.ToString(model.group)));
                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                             .GetCollection<BsonDocument>(Tables.Variables)
                                             .UpdateOne("{'_id': ObjectId('" + model.objectID + "')}"
                                             , updateDefinition);
                if (updateResult != null)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Updated.TrimStart() + APIResponseMessage.Success;
                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }

            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }

        public JsonResponse AddUpdateVideoUpload(VideoUpload model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var updateFilter = "{'_id': ObjectId('" + model.ObjectID + "')}";

                UpdateDefinition<BsonDocument> addUpdateFilter = null;
                if (model.isAdd != null && model.isAdd == false)
                {
                    addUpdateFilter = Builders<BsonDocument>.Update.PullFilter("videoList",
                  Builders<BsonDocument>.Filter.Eq("videoId", model.videoId));
                }
                else
                {
                    addUpdateFilter = "{$push:{'videoList':{'$each':[" + model.videoList + "]}}}";
                }

                UpdateResult updateResult = client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                          .GetCollection<BsonDocument>(Tables.Videos)
                                          .UpdateOne(updateFilter, addUpdateFilter);

                if (updateResult != null && updateResult.MatchedCount == 1 && model.isAdd == false)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Deleted.TrimStart() + APIResponseMessage.Success;

                }
                else if (updateResult != null && updateResult.MatchedCount == 1)
                {
                    response.Data = updateResult;
                    response.Message = APIResponseMessage.Inserted.TrimStart() + APIResponseMessage.Success;

                }
                else
                {
                    response.Status = APIResponseStatus.Failed;
                    response.Message = APIResponseMessage.Failed;
                    response.Data = updateResult;
                }

                return response;
            }

            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }

        }

        public async Task<JsonResponse> GetDocumentDetails(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {
                var client = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                var options = new AggregateOptions()
                {
                    AllowDiskUse = false
                };

                PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
             {
                new BsonDocument("$addFields", new BsonDocument()
                        .Add("docID", new BsonDocument()
                                .Add("$toString", "$_id")
                        )),

                new BsonDocument("$lookup", new BsonDocument()
                        .Add("from", Tables.DocumentRoles)
                        .Add("localField", "docID")
                        .Add("foreignField", "documentID")
                        .Add("as", "roles")),

                 new BsonDocument("$addFields", new BsonDocument()
                        .Add("docgroupID", new BsonDocument()
                                .Add("$toObjectId", "$groupId")
                        )),
                new BsonDocument("$lookup", new BsonDocument()

                        .Add("from", Tables.DocumentGroup)
                        .Add("localField", "docgroupID")
                        .Add("foreignField", "_id")
                        .Add("as", "group")),

                new BsonDocument("$project", new BsonDocument()
                        .Add("tempName", 1.0)
                        .Add("createdBy", 1.0)
                        .Add("roles.roleID", 1.0)
                        .Add("content", 1.0)
                        .Add("roles.roleName", 1.0)
                        .Add("roles.members", 1.0)
                        .Add("statusID", 1.0)
                        .Add("createdOn", 1.0)
                        .Add("createdByID", 1.0)
                        //.Add("group._id",1.0)
                        .Add("group.name", 1.0))


             };

                using (var cursor = await client.GetDatabase(CommonFunction.GetConnectionString("DatabaseName"))
                                    .GetCollection<BsonDocument>(model.CollectionName).AggregateAsync(pipeline, options))
                {
                    BsonArray bsonDocument = new();
                    while (await cursor.MoveNextAsync())
                    {
                        foreach (BsonDocument document in cursor.Current)
                        {
                            bsonDocument.Add(document);
                        }
                    }
                    GroupOfModel groupOfModel = new GroupOfModel();

                    groupOfModel.Collection = CommonFunction.BsonSerializer(bsonDocument);
                    response.Data = groupOfModel;
                }

                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
            }
            return response;
        }

        public JsonResponse GetFoderList(DocumentCollection model)
        {
            JsonResponse response = new JsonResponse();
            try
            {

                //var mongoClient = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                //var database = mongoClient.GetDatabase("DatabaseName");
                //IMongoCollection<VideoUpload> _collection = database.GetCollection<VideoUpload>("Videos");
                //var condition = Builders<VideoUpload>.Filter.Eq(p => p.folder, "33");
                //var fields = Builders<VideoUpload>.Projection.Include(p => p.folder).Include(p => p.objectID);
                //var results = _collection.Find(condition).Project<VideoUpload>(fields).ToList().AsQueryable();

                MongoClient mongoClient = new MongoClient(CommonFunction.GetConnectionString("DBConnection"));
                //    var server = MongoServer.Create(mongoClient);
                 var documentData = mongoClient.GetDatabase(CommonFunction.GetConnectionString("DatabaseName")).GetCollection<BsonDocument>(model.CollectionName);
                var db = mongoClient.GetDatabase("DatabaseName");
                BsonDocument projection = new BsonDocument()
                {
                    {"_id", 1.0},
                    {"folder",1.0 }
                };
                var documents =  documentData.Find(Builders<BsonDocument>.Filter.Eq("isActive", true))
                      .Project(projection)
                    .ToList();
                // model.CollectionName.find({},{ "folder":1,_id:0});
                //var documents = (model.CollectionName == Tables.Users ? documentData.Find(Builders<BsonDocument>.Filter.Eq("isActive", true)).ToList() : documentData.Find(_ => true).ToList(), new BsonDocument("$project", new BsonDocument()
                //        .Add("folder", 1.0)
                //        .Add("_id", 1.0))
                //var collection = db.GetCollection<VideoUpload>("Videos");
                //var results = collection.Find(Builders<BsonDocument>.Filter.Eq("isActive", true)))
                //        //.Project(u => new { u.folder, u.objectID })
                //        .ToList();
                 
                response.Data = CommonFunction.BsonSerializer(documents);

                response.Message = APIResponseMessage.Data + APIResponseMessage.Retrieved + APIResponseMessage.Success;
                return response;
            }
            catch (Exception e)
            {
                StoreErrorLog(e.Message, e.StackTrace, MethodBase.GetCurrentMethod().Name);
                response.Status = APIResponseStatus.Failed;
                response.Message = APIResponseMessage.Failed;
                return response;
            }
        }


    }
}