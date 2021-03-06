using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    /// <summary>
    /// this represents one matchup in the tournament
    /// </summary>
    public class MatchupModel
    {
        public int Id { get; set; }


        /// <summary>
        /// This represents the teams involved in this matchup
        /// </summary>
        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();
        /// <summary>
        /// The ID from the database that will be used to identify the winner
        /// </summary>
        public int WinnerId { get; set; }
        /// <summary>
        /// This represents the team that has won the matchup
        /// </summary>
        public TeamModel Winner { get; set; }
        /// <summary>
        /// This represents what round of the tournament
        /// this matchup is being played in
        /// </summary>
        public int MatchupRound { get; set; }
        /// <summary>
        /// Get the display name for the matchup
        /// If first entry get the name
        /// else (second team) concat the vs. and second team name
        /// </summary>
        public string DisplayName
        {
            get
            {
                string output = "";
                foreach(MatchupEntryModel me in Entries)
                {
                    if (me.TeamCompeting != null)
                    {
                        if (output.Length == 0)
                        {
                            output = me.TeamCompeting.TeamName;
                        }
                        else
                        {
                            output += $" vs. { me.TeamCompeting.TeamName }";
                        }
                    }
                    else
                    {
                        output = "Matchup Not Yet Determined";
                        break;
                    }
                }

                return output;
            }
        }
    }
}
