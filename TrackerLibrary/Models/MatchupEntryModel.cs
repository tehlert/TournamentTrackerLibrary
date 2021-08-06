﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    /// <summary>
    /// represents one team in the matchup
    /// </summary>
    public class MatchupEntryModel
    {
        /// <summary>
        /// Represents one Team in the matchup
        /// </summary>
        public PrizeModel TeamCompeting { get; set; }

        /// <summary>
        /// Represents the score for this particular team
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Represents the matchup that this team came
        /// from as the winner
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }

    }
}