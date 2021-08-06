using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        // Order our list randomly of teams
        // Check if it is big enough - if not, add in bye matchups - 2*2*2*2 - 2^N
        // Create our first round of matchups
        // Create every round after that -- Example 16 teams - 8 matchups - 4 matchups - 2 matchups - 1 matchup

        public static void CreateRounds(TournamentModel model)
        {
            // create a randomized list of teams based off the entered teams
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            // get the number of rounds
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            // find the number of byes 
            int byes = NumberOfByes(rounds, randomizedTeams.Count);
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {

        }

        /// <summary>
        /// Find the number of "byes" we will have by finding the totalTeams a tournament with this many rounds could support
        /// </summary>
        /// <param name="rounds">The number of rounds for the tournament</param>
        /// <param name="numberOfTeams">The total number of teams playing in the tournament</param>
        /// <returns></returns>
        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output     = 0;
            int totalTeams = 1;
     
            for (int i = 1; i <+ rounds; i++)
            {
                totalTeams *= 2;
            }

            output = totalTeams - numberOfTeams;

            return output;
        }

        /// <summary>
        /// Find the number of rounds for the tournament based on the team count
        /// </summary>
        /// <param name="teamCount"></param>
        /// <returns></returns>
        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val    = 2;

            while (val < teamCount)
            {
                output += 1;
                val *= 2;
            }

            return output;
        }

        /// <summary>
        /// This is a very simple psuedo-random for our use
        /// </summary>
        /// <param name="teams"></param>
        /// <returns> List of TeamModel </returns>
        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            // Simple Example - cards.OrderBy(a => Guid.NewGuid()).ToList();
            return teams.OrderBy(x => Guid.NewGuid()).ToList();
        }
    }
}
