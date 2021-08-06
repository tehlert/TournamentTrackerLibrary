using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    public class TournamentModel
    {
        /// <summary>
        /// The unique identifier for the tournament
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// This represents the name of the tournament
        /// </summary>
        public string TournamentName { get; set; }
        /// <summary>
        /// This represents the entry fee each team must pay to enter the tournament
        /// </summary>
        public decimal EntryFee { get; set; }
        /// <summary>
        /// This represents a list of all the teams in the tournament
        /// </summary>
        public List<TeamModel> EnteredTeams { get; set; } = new List<TeamModel>();
        /// <summary>
        /// This represents a list of the prizes available from the tournament
        /// </summary>
        public List<PrizeModel> Prizes { get; set; } = new List<PrizeModel>();
        /// <summary>
        /// This represents a list of each round in the tournament
        /// Each round in the tournament is represented by a list
        /// of matchups 
        /// </summary>
        public List<List<MatchupModel>> Rounds { get; set; } = new List<List<MatchupModel>>();

    }
}
