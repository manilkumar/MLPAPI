using JenkinsClient;
using MLPAPI.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MLPAPI.Controllers
{
    public class MLPController : ApiController
    {


        public async Task<HttpResponseMessage> GetAllLogs()
        {

            try
            {
                //return Request.CreateResponse(HttpStatusCode.OK, new MongoDBWrapper().GetLogs());

                var result = await new MongoDBWrapper().GetLogsDetails();

                return  Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> RunAlgorithm([FromBody]LogDetails logModel)
        {
            try
            {
                MongoDBWrapper mongoBase = new MongoDBWrapper();

                var result = await mongoBase.RunJenkinsJob(logModel);

                return Request.CreateResponse(HttpStatusCode.OK, result);


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Exception occured at the time fetching data on server.\n" + ex.ToString());
            }
        }
        /// <summary>
        /// Run Jenkins Job
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> RunCTAlgorithm(HttpRequestMessage request)
        {
            try
            {
              

                MongoDBWrapper mongoBase = new MongoDBWrapper();

                var jsonString = await request.Content.ReadAsStringAsync();

                BsonDocument collection = BsonDocument.Parse(jsonString);

                var result = await mongoBase.RunJenkinsJob(collection);

                return Request.CreateResponse(HttpStatusCode.OK, "");


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Execute(string algorithm)
        {

            var client = Client.Create(
                MongoDBWrapper.JenkinsHost, MongoDBWrapper.JenkinsUsername, MongoDBWrapper.JenkinsPassword);

            var job = client.GetJob("InvokeRemotely");

            var buildTask = await job.BuildAsync(new Dictionary<string, string>()
            {
                {"imageName",algorithm}
            });

            string _id = await MongoDBWrapper.CreateRequestLog(new LogDetails() { Algorithm = algorithm });

            await buildTask.WaitForBuildStart();

            await buildTask.WaitForBuildEnd();

            return Request.CreateResponse(_id);
        }
    }
}