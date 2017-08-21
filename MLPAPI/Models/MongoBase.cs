using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLPAPI.Models
{
    public static class MongoHelper
    {
        /// <summary>
        /// Initializing mongo db connection
        /// </summary>
        /// <returns></returns>
        public static IMongoDatabase InitializeMongo()
        {

            MongoClient mongoClient = new MongoClient(MLPConstants.ConnectionStringPrefix + MongoDBConfig.GetServer() + ":" + MongoDBConfig.GetPort());

            return mongoClient.GetDatabase(MongoDBConfig.GetDB());

        }
    }
}