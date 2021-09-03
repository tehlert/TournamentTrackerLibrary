using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    /// <summary>
    /// represents one team in the matchup
    /// </summary>
    public class MatchupEntryModel
    {
        public int Id { get; set; }
        /// <summary>
        /// The unique identifier for the team
        /// </summary>
        public int TeamCompetingId { get; set; }

        /// <summary>
        /// Represents one Team in the matchup
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Represents the score for this particular team
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The unique identifier for the parrent matchup (team)
        /// </summary>
        public int ParentMatchupId { get; set; }

        /// <summary>
        /// Represents the matchup that this team came
        /// from as the winner
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }

    }
}
