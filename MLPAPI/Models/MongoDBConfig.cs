using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLPAPI.Models
{
    /// <summary>
    /// Holds all mongo db configurations
    /// </summary>
    public class MongoDBConfig
    {

        /// <summary>
        /// Gets the server IP.
        /// </summary>
        /// <returns>Server IP</returns>
        public static string GetServer()
        {
            return MLPConstants.DefaultServerIP; 
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <returns>Port</returns>
        public static string GetPort()
        {
            return  MLPConstants.DefaultPort;
        }

        /// <summary>
        /// Gets db name from config
        /// </summary>
        /// <returns>DB name</returns>
        public static string GetDB()
        {
            return    MLPConstants.DefaultDBName;
        }

    }
}