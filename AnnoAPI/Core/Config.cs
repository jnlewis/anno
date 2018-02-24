using System;
using System.Configuration;

namespace AnnoAPI.Core
{
    public static class Config
    {

        #region ConnectionStrings

        public static string ConnectionString_Anno { get { return GetConnectionString("Anno"); } }

        #endregion

        #region AppSettings

        //General Settings

        public static string ApplicationName { get { return GetAppSettings("ApplicationName"); } }
        public static bool LogRequests { get { return Convert.ToBoolean(GetAppSettings("LogRequests")); } }

        //Blockchain Settings

        public static bool CommitToBlockchain { get { return Convert.ToBoolean(GetAppSettings("CommitToBlockchain")); } }
        public static string ContractScriptHash { get { return GetAppSettings("ContractScriptHash"); } }
        public static string ContractPrivateKeyHEX { get { return GetAppSettings("ContractPrivateKeyHEX"); } }
        public static string OwnerScriptHash { get { return GetAppSettings("OwnerScriptHash"); } }

        #endregion

        #region Private Methods

        private static string GetConnectionString(string name)
        {
            if (ConfigurationManager.ConnectionStrings[name] == null)
                throw new Exception("ConnectionStrings not found: " + name);

            return ConfigurationManager.ConnectionStrings[name].ToString();
        }

        private static string GetAppSettings(string key)
        {
            if (ConfigurationManager.AppSettings[key] == null)
                throw new Exception("AppSettings not found: " + key);

            return ConfigurationManager.AppSettings[key].ToString();
        }

        #endregion

    }
}