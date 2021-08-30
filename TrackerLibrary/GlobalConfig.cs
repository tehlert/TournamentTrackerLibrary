using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.DataAccess;
using System.Configuration;

namespace TrackerLibrary
{
    /// <summary>
    /// Our global
    /// static makes it more like a house rather than a blueprint of a house
    /// </summary>
    public static class GlobalConfig  
    {
        // files to save to 
        public const string PrizesFile       = "PrizeModels.csv";
        public const string PeopleFile       = "PersonModels.csv";
        public const string TeamFile         = "TeamModels.csv";
        public const string TournamentFile   = "TournamentModels.csv";
        public const string MatchupFile      = "MatchupModels.csv";
        public const string MatchupEntryFile = "MatchupEntryModels.csv";

        public static IDataConnection Connection { get; private set; }

        public static void InitializeConnection(DatabaseType db)
        {
            //TODO - Add MySQL connection?
            switch (db)
            {
                case DatabaseType.Sql:
                    SqlConnector sql = new SqlConnector();
                    Connection = sql;
                    break;
                case DatabaseType.TextFile:
                    TextConnector text = new TextConnector();
                    Connection = text;
                    break;
                default:
                    break;
            }
        }


        /* Multiple DatabaseTypes -- Requires refactoring to implement
        /// <summary>
        /// A List of our data connections they must follow the IDataConnection contract(interface)
        /// static       -- makes it more like a house rather than a blueprint of a house
        /// get;         -- anyone can see it
        /// private set; -- only methods in this class can add or change 
        /// </summary>
        public static List<IDataConnection> Connections { get; private set; } = new List<IDataConnection>();

        
        /// <summary>
        /// A method to initialize our connection
        /// </summary>
        /// <param name="database"></param>
        /// <param name="textFiles"></param>
        public static void InitializeConnections(bool database, bool textFiles)
        {
            if (database) //no need to compare with true - its already a bool variable
            {
                SqlConnector sql = new SqlConnector();
                Connections.Add(sql);
            }

            if (textFiles)
            {
                TextConnector text = new TextConnector();
                Connections.Add(text);
            }
        }
        */


        public static string CnnString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
