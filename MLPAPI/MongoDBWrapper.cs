using JenkinsClient;
using MLPAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MLPAPI
{
    /// <summary>
    /// This class handles all MongoDB related operations.
    /// </summary>
    public class MongoDBWrapper
    {

        #region Class members
        public string Server { get; set; }
        public string Port { get; set; }
        public string DBName { get; set; }
        public string CollectionName { get; set; }
        public string DBUserName { get; set; }
        public string DBPassword { get; set; }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public MongoDBWrapper()
        {
            Server = MongoDBConfig.GetServer().Trim();
            Port = MongoDBConfig.GetPort().Trim();
            DBName = MongoDBConfig.GetDB().Trim();
            CollectionName = MongoDBConfig.GetCollectionName().Trim();
            mongoDB = MongoHelper.InitializeMongo();
        }

        public static IMongoDatabase mongoDB;

        /// <summary>
        ///  Insert a json document to collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public async Task<string> CreateRequestLog(BsonDocument collection)
        {
            var _bsonId = ObjectId.GenerateNewId().ToString();

            collection.Add("_id", _bsonId);

            collection.Add("RequestedDate", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));

            try
            {

                var logs = GetCollection(CollectionName);

                await logs.InsertOneAsync(collection);

                return _bsonId;
            }

            catch (Exception ex)
            {
                return string.Empty;

            }

        }


        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <param name="collection"> Collection name. </param>
        /// <returns> Collection instance. </returns>
        public IMongoCollection<BsonDocument> GetCollection(string collection)
        {
            if (String.IsNullOrEmpty(collection))
                throw new Exception("The collection is null or empty");

            var dataCol = mongoDB.GetCollection<BsonDocument>(collection);

            return dataCol;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<LogDetails>> GetLogsDetails()
        {

            var logs = mongoDB.GetCollection<LogDetails>(DBName);

            var list = await logs.AsQueryable().ToListAsync<LogDetails>();

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetLogs()
        {

            var logs = GetCollection(CollectionName);

            var list = await logs.AsQueryable().ToListAsync<BsonDocument>();

            return list.ToJson();
        }

        /// <summary>
        /// Run Jenkins job
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public async Task<string> RunJenkinsJob(BsonDocument collection)
        {
            try
            {
                var logs = GetCollection(CollectionName);

                collection.Add("IP", Dns.GetHostName());

                collection.Add("Client", HttpContext.Current.Request.Url.AbsoluteUri);

                var _id = await CreateRequestLog(collection);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", _id);

                var jenkins_client = Client.Create(MLPConstants.JenkinsHost, MLPConstants.JenkinsUsername, MLPConstants.JenkinsPassword);

                var job = jenkins_client.GetJob(MLPConstants.JenkinsJob);

                var pararmDict = new Dictionary<string, string>();

                pararmDict.Add("imageName", (string)collection["Algorithm"]);

                if ((string)collection["JobID"] != "")
                {
                    pararmDict.Add("job_id", (string)collection["JobID"]);
                }

                if ((string)collection["NotificationURL"] != "")
                {
                    pararmDict.Add("NotificationURL", (string)collection["NotificationURL"]);
                }

                var isJsonString = isValidJSON(collection["Input"].ToString());

                if (isJsonString)
                {

                    foreach (var element in collection["Input"].AsBsonDocument.Elements)
                    {

                        pararmDict.Add(element.Name.ToString(), element.Value.ToString());

                    }
                }
                else
                {
                    if ((string)collection["Input"] != "")
                    {
                        var input = (string)collection["Input"];

                        pararmDict.Add("input", input);
                    }
                }

                var buildTask = await job.BuildAsync(pararmDict);

                var buildItem = await buildTask.WaitForBuildStart();
                

                var update = Builders<BsonDocument>.Update
                  .Set("ExecutionStartTime", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));

                await logs.UpdateOneAsync(filter, update);

                var buildDetails = await jenkins_client.GetBuildDetails(job.name, buildItem);


                //await buildTask.WaitForBuildEnd();

                //update = Builders<BsonDocument>.Update
                //   .Set("ExecutionEndTime", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"))
                //   .Set("Status", buildTask.result);

                //await logs.UpdateOneAsync(filter, update);

                return "Build Started";

                //string parameters = "?imageName=" + collection.Algorithm + (collection.JobID != "" ? "&job_id=" + collection.JobID : "") + (collection.InputFolder != "" ? "&inputfolder=" + collection.InputFolder : "") + (collection.ResultsFolder != "" ? "&outputfolder=" + collection.ResultsFolder : "");

                //return "Requested URL:" + collection.URL + "" + parameters + "";
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }


        /// <summary>
        /// validating json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool isValidJSON(String json)
        {
            try
            {
                JToken token = JObject.Parse(json);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}