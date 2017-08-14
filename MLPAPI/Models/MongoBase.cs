using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLPAPI.Models
{
    public static class MongoHelper
    {
        public static IMongoDatabase InitializeMongo()
        {

            MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");

            return mongoClient.GetDatabase("MLP");

        }
    }
}