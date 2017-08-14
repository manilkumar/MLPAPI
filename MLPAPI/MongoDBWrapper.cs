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
        public string DBUserName { get; set; }
        public string DBPassword { get; set; }
        public string DBSecurity { get; set; }
        public string JenkinsUsername { get; set; }
        public string JenkinsPassword { get; set; }
        public string JenkinsHost { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public MongoDBWrapper()
        {
            Server = MongoDBConfig.GetServer().Trim();
            Port = MongoDBConfig.GetPort().Trim();
            DBName = MongoDBConfig.GetDB().Trim();
            mongoDB = MongoHelper.InitializeMongo();
        }
        public static IMongoDatabase mongoDB;


        public class Parameter
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        //public static async Task<string> CreateRequestLog(LogDetails model)
        //{
        //    model.Id = ObjectId.GenerateNewId().ToString();

        //    model.RequestedDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");

        //    try
        //    {

        //        var logs = mongoDB.GetCollection<LogDetails>("Logs");

        //        await logs.InsertOneAsync(model);

        //        return model.Id;
        //    }

        //    catch (Exception ex)
        //    {
        //        return string.Empty;

        //    }

        //}

        /// <summary>
        ///  Insert a json document to collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public  async Task<string> CreateRequestLog(BsonDocument collection)
        {
            var _bsonId = ObjectId.GenerateNewId().ToString();

            collection.Add("_id", _bsonId);

            collection.Add("RequestedDate", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));

            try
            {

                var logs = GetCollection("AlgorithmExecution");

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

        public async Task<IEnumerable<LogDetails>> GetLogsDetails()
        {

            var logs = mongoDB.GetCollection<LogDetails>("Logs");

            var list = await logs.AsQueryable().ToListAsync<LogDetails>();

            return list;
        }

        public async Task<string> GetLogs()
        {

            var logs = mongoDB.GetCollection<BsonDocument>("Logs");

            var list = await logs.AsQueryable().ToListAsync<BsonDocument>();

            return list.ToJson();
        }


        #region Commented Code

        //public static HttpWebRequest CreateCTPhantonPipelineRequest(string algorithm, string job_id, string inputfilename, string resultFolder)
        //{
        //    try
        //    {

        //        Uri result = default(Uri);

        //        Uri.TryCreate("http://10.200.0.199:8080/job/RunDockerImage/buildWithParameters?", UriKind.Absolute, out result);


        //        var request = (HttpWebRequest)WebRequest.Create(result.AbsoluteUri);
        //        request.Method = "POST";

        //        byte[] credentials = new UTF8Encoding().GetBytes(JenkinsUsername + ":" + "b85f3ee4794cec10cc7e7ac097095cef");

        //        request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentials);
        //        request.PreAuthenticate = true;

        //        var parameterList = new List<Parameter>();

        //        parameterList.Add(new Parameter() { name = "inputfolder", value = inputfilename });
        //        parameterList.Add(new Parameter() { name = "job_id", value = job_id });
        //        parameterList.Add(new Parameter() { name = "imageName", value = algorithm });
        //        parameterList.Add(new Parameter() { name = "outputfolder", value = resultFolder });

        //        var json = JsonConvert.SerializeObject(new { parameter = parameterList.ToArray() });
        //        json = System.Web.HttpUtility.UrlEncode(json);

        //        byte[] byteArray = Encoding.UTF8.GetBytes(json);

        //        request.ContentType = "application/x-www-form-urlencoded";

        //        request.ContentLength = byteArray.Length;

        //        Stream dataStream = request.GetRequestStream();

        //        dataStream.Write(byteArray, 0, byteArray.Length);
        //        dataStream.Close();

        //        return request;



        //    string parameters = "imageName=" + algorithm + (job_id != "" ? "&job_id=" + job_id : "") + (inputfilename != "" ? "&inputfolder=" + inputfilename : "") + (resultFolder != "" ? "&outputfolder=" + resultFolder : "");

        //    Uri result = default(Uri);

        //    Uri.TryCreate("http://10.200.0.199:8080/job/RunDockerImage/buildWithParameters?" + parameters + "", UriKind.Absolute, out result);


        //    var request = (HttpWebRequest)WebRequest.Create(result.AbsoluteUri);
        //    request.Method = "POST";

        //    byte[] credentials = new UTF8Encoding().GetBytes(JenkinsUsername + ":" + "b85f3ee4794cec10cc7e7ac097095cef");

        //    request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentials);
        //    request.PreAuthenticate = true;

        //    return request;


        //    }
        //    catch (Exception ex)
        //    {

        //        return null;
        //    }
        //}

        #endregion

        public async Task<string> RunJenkinsJob(LogDetails model)
        {
            try
            {
                var logs = mongoDB.GetCollection<LogDetails>("Logs");

                model.IP = Dns.GetHostName();

                model.URL = HttpContext.Current.Request.Url.AbsoluteUri;

                var _id = await CreateRequestLog(model);

                var filter = Builders<LogDetails>.Filter.Eq("_id", _id);

                var jenkins_client = Client.Create(

                MongoDBWrapper.JenkinsHost, MongoDBWrapper.JenkinsUsername, MongoDBWrapper.JenkinsPassword);

                var job = jenkins_client.GetJob("RunDockerImage");

                var pararmDict = new Dictionary<string, string>();

                pararmDict.Add("imageName", model.Algorithm);

                if (model.JobID != "")
                {
                    pararmDict.Add("job_id", model.JobID);
                }
                if (model.InputFolder != "")
                {
                    pararmDict.Add("inputfolder", model.InputFolder);
                }
                if (model.ResultsFolder != "")
                {
                    pararmDict.Add("outputfolder", model.ResultsFolder);
                }

                var buildTask = await job.BuildAsync(pararmDict);

                await buildTask.WaitForBuildStart();

                var update = Builders<LogDetails>.Update
                  .Set("ExecutionStartTime", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"));

                await logs.UpdateOneAsync(filter, update);

                await buildTask.WaitForBuildEnd();

                update = Builders<LogDetails>.Update
                   .Set("ExecutionEndTime", DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt"))
                   .Set("Status", buildTask.result);

                await logs.UpdateOneAsync(filter, update);

                //return buildTask.result;

                string parameters = "?imageName=" + model.Algorithm + (model.JobID != "" ? "&job_id=" + model.JobID : "") + (model.InputFolder != "" ? "&inputfolder=" + model.InputFolder : "") + (model.ResultsFolder != "" ? "&outputfolder=" + model.ResultsFolder : "");

                return "Requested URL:" + model.URL + "" + parameters + "";
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }

        public async Task<string> RunJenkinsJob(BsonDocument collection)
        {
            try
            {
                var logs = mongoDB.GetCollection<BsonDocument>("AlgorithmExecution");

                collection.Add("IP", Dns.GetHostName());

                collection.Add("URL", HttpContext.Current.Request.Url.AbsoluteUri);

                var _id = await CreateRequestLog(collection);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", _id);

                var jenkins_client = Client.Create(

                MongoDBWrapper.JenkinsHost, MongoDBWrapper.JenkinsUsername, MongoDBWrapper.JenkinsPassword);

                var job = jenkins_client.GetJob("RunDockerImage");

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