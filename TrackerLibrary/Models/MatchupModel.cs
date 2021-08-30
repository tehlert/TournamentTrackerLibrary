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
        /// This represents the team that has won the matchup
        /// </summary>
        public TeamModel Winner { get; set; }
        /// <summary>
        /// This represents what round of the tournament
        /// this matchup is being played in
        /// </summary>
        public int MatchupRound { get; set; }
    }
}
