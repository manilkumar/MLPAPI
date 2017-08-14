using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLPAPI.Models
{
    public class LogDetails
    {
        [BsonId]
        public string Id { get; set; }

        public string URL { get; set; }

        public string IP { get; set; }

        public string InputFolder { get; set; }

        public string ResultsFolder { get; set; }

        public string Algorithm { get; set; }

        public string JobID { get; set; }

        public string RequestedDate { get; set; }

        public string ExecutionStartTime { get; set; }

        public string ExecutionEndTime { get; set; }

        public string Status { get; set; }

    }

    
}