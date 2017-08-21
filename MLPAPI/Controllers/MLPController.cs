using JenkinsClient;
using MLPAPI.Models;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MLPAPI.Controllers
{
    public class MLPController : ApiController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAllLogs()
        {

            try
            {
                //return Request.CreateResponse(HttpStatusCode.OK, new MongoDBWrapper().GetLogs());

                var result = await new MongoDBWrapper().GetLogsDetails();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        /// <summary>
        /// Call Jenkins Job
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> RunCTAlgorithm(HttpRequestMessage request)
        {
            try
            {

                //Logging Info
                MLPExecutionLogger.Info("CTPhantom", "Calling Jenkins Job, IP: " + HttpContext.Current.Request.UserHostAddress + ", Client: " + HttpContext.Current.Request.Url.AbsoluteUri);

                MongoDBWrapper mongoBase = new MongoDBWrapper();

                var jsonString = await request.Content.ReadAsStringAsync();

                BsonDocument collection = BsonDocument.Parse(jsonString);

                var result = await mongoBase.RunJenkinsJob(collection);

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                MLPExecutionLogger.Error("CTPhantom", ex.Message);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// getting build details of a job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="buildNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetBuildDetails(string jobName, int buildNo)
        {
            try
            {
                MLPExecutionLogger.Info("CTPhantom", "Getting build details, Params:" + jobName + "," + buildNo + ", IP: " + HttpContext.Current.Request.UserHostAddress + ", Client: " + HttpContext.Current.Request.Url.AbsoluteUri);

                var jenkins_client = Client.Create(MLPConstants.JenkinsHost, MLPConstants.JenkinsUsername, MLPConstants.JenkinsPassword);

                var response = await jenkins_client.GetBuildDetails(jobName, buildNo);

                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                MLPExecutionLogger.Error("CTPhantom", ex.Message);

                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);

            }

        }

        /// <summary>
        /// getting build progress of a job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="buildNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetBuildProgress(string jobName, int buildNo)
        {
            try
            {
                MLPExecutionLogger.Info("CTPhantom", "Getting build progress, Params:" + jobName + "," + buildNo + ", IP: " + HttpContext.Current.Request.UserHostAddress + ", Client: " + HttpContext.Current.Request.Url.AbsoluteUri);

                var jenkins_client = Client.Create(MLPConstants.JenkinsHost, MLPConstants.JenkinsUsername, MLPConstants.JenkinsPassword);

                var response = await jenkins_client.GetBuildProgress(jobName, buildNo);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                MLPExecutionLogger.Error("CTPhantom", ex.Message);

                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);

            }
        }

    }
}
