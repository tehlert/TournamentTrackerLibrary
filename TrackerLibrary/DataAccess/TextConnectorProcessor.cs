﻿using System;
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

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            //id, team name, list of ids seperated by the pipe
            //3,Tim's Team, 1|3|5
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

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
        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            // id             = 0
            // TournamentName = 1
            // EntryFee       = 2
            // EnteredTeams   = 3
            // Prizes         = 4
            // Rounds         = 5
            // id,TournamentName,EntryFee,(id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams        = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes      = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups  = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel tm = new TournamentModel();
                tm.Id              = int.Parse(cols[0]);
                tm.TournamentName  = cols[1];
                tm.EntryFee        = int.Parse(cols[2]);

                string[] teamIds   = cols[3].Split('|');

                foreach (string id in teamIds)
                {
                    // look for the team that has the same id as ours - if found put it into list of teams
                    tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }
                if (cols[4].Length > 0)
                {
                    string[] prizeIds = cols[4].Split('|');

                    foreach (string id in prizeIds)
                    {
                        tm.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
                    }
                }
                
                // Capture rounds information
                // split the rounds 
                string[] rounds = cols[5].Split('|');                
                // loop through each round
                foreach (string round in rounds)
                {   // split the matchup ids
                    string[] msText = round.Split('^');
                    List<MatchupModel> ms = new List<MatchupModel>();
                    // loop through each matchup
                    foreach (string matchupModelTextId in msText)
                    {   // Find the matching ID
                        ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                    }
                    // Add the matchups to the Rounds in the model
                    tm.Rounds.Add(ms);
                }                      
                output.Add(tm);
            }
            return output;
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName }, { p.PrizeAmount },{ p.PrizePercentage }");
            }
            // This will overwrite the original file -- creates a new file everytime
            File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models)
        {
            List<string> lines = new List<string>();

            // write a new line for each person in the list
            foreach (PersonModel p in models)
            {
                lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.CellphoneNumber }");
            }

            File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
        }

        public static void SaveToTeamFile(this List<TeamModel> models)
        {
            List<string> lines = new List<string>();

            foreach(TeamModel t in models)
            {               
                lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPeopleListToString(t.TeamMembers) }");
            }

            File.WriteAllLines(GlobalConfig.TeamFile.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            // Loop through each Round
            // Loop through each Matchup
            // Get the id for the new Matchup and save the record
            // Loop through each Entry, get the id, and save it

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach(MatchupModel matchup in round)
                {
                    // Load all the matchups from file
                    // Get the top id and add one
                    // Store the id
                    // Save the matchup record
                    matchup.SaveMatchupToFile();              
                }
            }
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            foreach (string line in lines)
            {
                // Below could cause problems if your data has commas -- to fix use a different character to split on
                string[] cols = line.Split(',');

                MatchupEntryModel me = new MatchupEntryModel();
                me.Id            = int.Parse(cols[0]);
                if(cols[1].Length == 0)
                {
                    me.TeamCompeting = null;
                }
                else
                {
                    me.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
                }              
                me.Score         = double.Parse(cols[2]);

                int parentId = 0;
                if (int.TryParse(cols[3], out parentId))
                {   // TODO - once we have a parent ID we hit an infinite loop
                    me.ParentMatchup = LookupMatchupById(parentId);
                }
                else
                {
                    me.ParentMatchup = null;
                }
                
                output.Add(me);
            }

            return output;
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('|');
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();
            List<string> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile();    // col[0] of each string if we seperate by , will be id
            List<string> matchingEntries = new List<string>();

            // loop through each id 
            foreach(string id in ids)
            {
                foreach (string entry in entries)
                {
                    string[] cols = entry.Split(',');

                    if(cols[0] == id)
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }

            output = matchingEntries.ConvertToMatchupEntryModels();

            return output;
        }

        private static TeamModel LookupTeamById(int id)
        {   // Get a list of all teams
            List<string> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile();
           
            foreach(string team in teams)
            {
                string[] cols = team.Split(',');

                if(cols[0] == id.ToString())
                {
                    List<string> matchingTeams = new List<string>();
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModels().First();
                }
            }
            // If we dont find a matching id, return no team found -- throw an exception?
            return null;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            List<string> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile();

            foreach(string matchup in matchups)
            {
                string[] cols = matchup.Split(',');

                if(cols[0] == id.ToString())
                {
                    List<string> matchingMatchups = new List<string>();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModels().First();
                }
            }

            return null;
        }

        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            //id=0, entries=1(pipe delimited by id), winner=2, matchupRound=3
            List<MatchupModel> output = new List<MatchupModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel p = new MatchupModel();
                p.Id           = int.Parse(cols[0]);                          // the id of the matchup
                p.Entries      = ConvertStringToMatchupEntryModels(cols[1]);  // the list of entries
                // Upon creation of the tourny Winners are unknown (null)
                if (cols[2].Length == 0)
                {
                    p.Winner = null;
                }
                else
                {
                    p.Winner       = LookupTeamById(int.Parse(cols[2]));          // the winner of the matchup (TeamModel)
                }
                

                p.MatchupRound = int.Parse(cols[3]);                          // round this matchup is in
                output.Add(p);
            }

            return output;
        }

        // This method will save a matchup to file and call the method to save the entries for that matchup
        public static void SaveMatchupToFile(this MatchupModel matchup)
        {            
            // Load all the matchups
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            // find the largest id
            int currentId = 1;

            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }

            matchup.Id = currentId;

            matchups.Add(matchup);     

            // Loop through each entry and save it to file
            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.SaveEntryToFile();
            }

            // save to file
            List<string> lines = new List<string>();

            // id=0, entries=1(pipe delimited by id), winner=2, matchupRound=3
            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id }, { ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            // Load all the matchups
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            MatchupModel oldMatchup = new MatchupModel();

            foreach(MatchupModel m in matchups)
            {
                if(m.Id == matchup.Id)
                {
                    oldMatchup = m;
                }
            }

            matchups.Remove(oldMatchup);

            matchups.Add(matchup);

            // Loop through each entry and save it to file
            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.UpdateEntryToFile();
            }

            // save to file
            List<string> lines = new List<string>();

            // id=0, entries=1(pipe delimited by id), winner=2, matchupRound=3
            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner != null)
                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id }, { ConvertMatchupEntryListToString(m.Entries) },{ winner },{ m.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }


        // This file will save an Entry when called from SaveMatchupToFile
        public static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            int currentId = 1;

            if(entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }

            entry.Id = currentId;
            entries.Add(entry);

            // save to file
            List<string> lines = new List<string>();

            // MatchupEntry pattern --  id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if(e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);            
        }

        public static void UpdateEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();
            MatchupEntryModel oldEntry = new MatchupEntryModel();

            foreach(MatchupEntryModel e in entries)
            {
                if(e.Id == entry.Id)
                {
                    oldEntry = e;
                }
            }

            entries.Remove(oldEntry);
            
            entries.Add(entry);

            // save to file
            List<string> lines = new List<string>();

            // MatchupEntry pattern --  id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompeting = "";
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompeting },{ e.Score },{ parent }");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        public static void SaveToTournamentFile(this List<TournamentModel> models)
        {
            List<string> lines = new List<string>();

            foreach (TournamentModel tm in models)
            {
                lines.Add($"{ tm.Id },{ tm.TournamentName },{ tm.EntryFee },{ ConvertTeamListToString(tm.EnteredTeams) },{ ConvertPrizeListToString(tm.Prizes) },{ ConvertRoundListToString(tm.Rounds) }");
            }

            File.WriteAllLines(GlobalConfig.TournamentFile.FullFilePath(), lines);
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {   // (Rounds - id^id^id|id^id^id|id^id^id)
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

        private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> entries)
        {
            // create empty string to return
            string output = "";

            // Return empty if no prizes
            if (entries.Count == 0)
            {
                return "";
            }

            // address the list with pipes |            
            foreach (MatchupEntryModel e in entries)
            {
                output += $"{ e.Id }|";
            }
            // Remove the trailing | 
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
