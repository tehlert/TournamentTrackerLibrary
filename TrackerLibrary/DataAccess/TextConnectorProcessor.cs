using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace TrackerLibrary.DataAccess.TextHelpers // because we don't want everyone to get this stuff unless the have the using statement
{
            /* Plan
               * Load the text file
               * Convert the text to List<PrizeModel>
               Find the max ID -- in Text.Connector.cs
               Add the new record with the new ID (max + 1)
               * Convert the prizes to list<string>
               * Save the list<string> to the text file
           */
    public static class TextConnectorProcessor
    {
        /// <summary>
        /// takes in a file name and returns the full path to that file
        /// </summary>
        /// <param name="fileName"> example fileName - PrizeModels.csv </param>
        /// <returns> example - D:\CODING 2021\Data\TournamentTracker\PrizeModels.csv </returns>
        public static string FullFilePath(this string fileName)
        {
            return $"{ ConfigurationManager.AppSettings["filePath"] }\\{ fileName }"; 
            // $ allows us to concat  \\ allows us to escape the escape key (we want to use a \)
        }

        /// <summary>
        /// Load the text file
        /// Create a new List if the file doesn't exist
        /// </summary>
        /// <param name="file"> pass in full path</param>
        /// <returns></returns>
        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p      = new PrizeModel();
                p.Id              = int.Parse(cols[0]);
                p.PlaceNumber     = int.Parse(cols[1]);
                p.PlaceName       = cols[2];
                p.PrizeAmount     = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);
                output.Add(p);
            }

            return output;
        }
        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();

            foreach (string line in lines)
            {
                // Below could cause problems if your data has commas -- to fix use a different character to split on
                string[] cols = line.Split(',');

                PersonModel p     = new PersonModel();
                p.Id              = int.Parse(cols[0]);
                p.FirstName       = cols[1];
                p.LastName        = cols[2];
                p.EmailAddress    = cols[3];
                p.CellphoneNumber = cols[4];
                output.Add(p);
            }

            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines, string peopleFileName)
        {
            //id, team name, list of ids seperated by the pipe
            //3,Tim's Team, 1|3|5
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = peopleFileName.FullFilePath().LoadFile().ConvertToPersonModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel t = new TeamModel();
                t.Id        = int.Parse(cols[0]);
                t.TeamName  = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach (string id in personIds)
                {
                    // take the list of people in our text file and
                    // filter where the id of the person in the list equals the id from foreach loop
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
                }

                output.Add(t);
            }

            return output;
        }
        public static List<TournamentModel> ConvertToTournamentModels(
            this List<string> lines, 
            string teamFileName, 
            string peopleFileName,
            string prizeFileName)
        {
            // id             = 0
            // TournamentName = 1
            // EntryFee       = 2
            // EnteredTeams   = 3
            // Prizes         = 4
            // Rounds         = 5
            // id,TournamentName,EntryFee,(id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams        = teamFileName.FullFilePath().LoadFile().ConvertToTeamModels(peopleFileName);
            List<PrizeModel> prizes      = prizeFileName.FullFilePath().LoadFile().ConvertToPrizeModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tm = new TournamentModel();
                tm.Id              = int.Parse(cols[0]);
                tm.TournamentName  = cols[1];
                tm.EntryFee        = decimal.Parse(cols[2]);

                string[] teamIds   = cols[3].Split('|');

                foreach (string id in teamIds)
                {
                    // look for the team that has the same id as ours - if found put it into list of teams
                    tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }

                string[] prizeIds = cols[4].Split('|');

                foreach (string id in prizeIds)
                {
                    tm.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
                }

                // TODO - capture rounds information

                output.Add(tm);
            }
            return output;
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName }, { p.PrizeAmount },{ p.PrizePercentage }");
            }
            // This will overwrite the original file -- creates a new file everytime
            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            // write a new line for each person in the list
            foreach (PersonModel p in models)
            {
                lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.CellphoneNumber }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveToTeamFile(this List<TeamModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach(TeamModel t in models)
            {               
                lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPeopleListToString(t.TeamMembers) }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static void SaveToTournamentFile(this List<TournamentModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (TournamentModel tm in models)
            {
                lines.Add($@"{ tm.Id },
                             { tm.TournamentName },
                             { tm.EntryFee },
                             { ConvertTeamListToString(tm.EnteredTeams) },
                             { ConvertPrizeListToString(tm.Prizes) },
                             { ConvertRoundListToString(tm.Rounds) }");
            }

            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            // create empty string to return
            string output = "";

            // Return empty if no rounds
            if (rounds.Count == 0) { return ""; }

            // address the list with pipes |            
            foreach (List<MatchupModel> r in rounds)
            {
                output += $"{ ConvertMatchupListToString(r) }|";
            }
            // Remove the trailing | 
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchupListToString(List<MatchupModel> matchups)
        {
            // create empty string to return
            string output = "";

            // Return empty string if no matchups
            if (matchups.Count == 0) { return ""; }

            // address the list with carrots            
            foreach (MatchupModel m in matchups)
            {
                output += $"{ m.Id }^";
            }
            // Remove the trailing ^ 
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPrizeListToString(List<PrizeModel> prizes)
        {
            // create empty string to return
            string output = "";

            // Return empty if no prizes
            if (prizes.Count == 0)
            {
                return "";
            }

            // address the list with pipes |            
            foreach (PrizeModel p in prizes)
            {
                output += $"{ p.Id }|";
            }
            // Remove the trailing | 
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            // create empty string to return
            string output = "";

            // Return empty string if no teams
            if (teams.Count == 0) { return ""; }

            // address the list with pipes |            
            foreach (TeamModel t in teams)
            {
                output += $"{ t.Id }|";
            }
            // Remove the trailing | 
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            // create empty string to return
            string output = "";

            // Return if no people on the team
            if (people.Count == 0)
            {
                return "";
            }

            // address the list with pipes |
            // for each person in the list take their id, concat a |
            // makes things look like this -- 2|5|
            foreach (PersonModel p in people)
            {
                output += $"{ p.Id }|";
            }

            // Remove the trailing | 
            // This will crash if there are no people in the list 0 - 1
            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
