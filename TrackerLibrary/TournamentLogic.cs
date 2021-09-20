using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TrackerLibrary
{
    // Since we don't need to store data long term we make it static  -- aka -- our data comes in and goes out in the same method
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
            // Create the first round with byes
            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));
            // Create the rest of the rounds and add them to our model
            CreateOtherRounds(model, rounds);
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            List<MatchupModel> toScore = new List<MatchupModel>();

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Winner == null && (rm.Entries.Any(x => x.Score != 0) || rm.Entries.Count == 1))
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            int endingRound = model.CheckCurrentRound();

            if (endingRound > startingRound)
            {
                model.AlertUsersToNewRound();
            }
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (MatchupModel matchup in currentRound)            
            {
                foreach (MatchupEntryModel me in matchup.Entries)
                {
                    foreach (PersonModel p in me.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(p, me.TeamCompeting.TeamName, matchup.Entries.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
        {
            if(p.EmailAddress.Length == 0)
            {
                return;
            }

            string to          = "";
            string subject     = "";
            StringBuilder body = new StringBuilder();

            if (competitor != null)
            {
                subject = $"You have a new matchup with { competitor.TeamCompeting.TeamName }";

                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Comeptitor: </strong>");
                body.AppendLine(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~Tournament Tracker App");
            }
            else
            {
                subject = "You have a bye this round.";

                body.AppendLine("Enjoy your round off!");
                body.AppendLine("~Tournament Tracker App");
            }

            to = p.EmailAddress;            

            EmailLogic.SendEmail( to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;
            foreach (List<MatchupModel> round in model.Rounds)
            {
                if (round.All(x => x.Winner != null))
                {
                    output += 1;
                }
                else
                {
                    return output;
                }
            }

            // Tournament is complete
            CompleteTournament(model);

            return output - 1;
        }

        private static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);
            TeamModel winners = model.Rounds.Last().First().Winner;
            TeamModel runnerUp = model.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;

                PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize != null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize != null)
                {
                    runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            // Send Email to all 
            string subject = "";
            StringBuilder body = new StringBuilder();

            subject = $"In { model.TournamentName },  { winners.TeamName } has won!";

            body.AppendLine("<h1>We have a Winner!</h1>");
            body.Append("<p>Congratulations to our winner on a great tournament.</p>");
            body.AppendLine("<br />");

            if (winnerPrize > 0 )
            {
                body.AppendLine($"<p>{ winners.TeamName } will receive ${ winnerPrize }</p>");
            }

            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{ runnerUp.TeamName } will receive ${ runnerUpPrize }</p>");
            }

            body.AppendLine("<p>Thanks for a great Tournament everyone!</p>");
            body.AppendLine("~Tournament Tracker App");

            List<string> bcc = new List<string>();

            foreach  (TeamModel t in model.EnteredTeams)
            {
                foreach (PersonModel p in t.TeamMembers)
                {
                    if (p.EmailAddress.Length > 0)
                    {
                        bcc.Add(p.EmailAddress);
                    }
                }
            }

            EmailLogic.SendEmail(new List<string>(), bcc, subject, body.ToString());

            // complete tournament
            model.CompleteTournament();
        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;

            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }

            return output;
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (MatchupModel m in models)
            {
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel rm in round)
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            // greater or lesser from App.config
            string greaterWins = ConfigurationManager.AppSettings["winnerDetermination"];

            foreach (MatchupModel m in models)
            {
                // Checks for bye 
                if (m.Entries.Count == 1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue; // go to next iteration of foreach loop (similar to break where it says foreach is done)
                }

                // 0 indicates false (low score wins)
                if (greaterWins == "0")
                {
                    if (m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score < m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow Ties in this application.");
                    }
                }
                // 1 indicates true (high score wins)
                else
                {
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score > m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow Ties in this application.");
                    }
                } 
            }

        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];          // set the previous round to round[0] (first round) 
            List<MatchupModel> currRound     = new List<MatchupModel>(); // new List for the current round (round 2)
            MatchupModel       currMatchup   = new MatchupModel();       // new matchup model for each matchup in each round
            // create rounds
            while(round <= rounds)
            {
                foreach (MatchupModel match in previousRound)
                {   // TODO - Take the winner of the previous round?
                    currMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match});
                    // if we have more than one entry (2) add a new matchup to the round
                    if(currMatchup.Entries.Count > 1)
                    {
                        currMatchup.MatchupRound = round;  // Sets the round we are in
                        currRound.Add(currMatchup);        // Add the Matchup to the round
                        currMatchup = new MatchupModel();  // Reset the matchup model in currMatchup 
                    }
                }

                model.Rounds.Add(currRound);           // Add current round to our model
                previousRound = currRound;             // Set the current round to the previous round so we can get the next round

                currRound = new List<MatchupModel>();  // Reset currRound to be blank for the next round
                round++;                               // increment to the next round
            }
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel       curr   = new MatchupModel();

            foreach(TeamModel team in teams)
            {
                curr.Entries.Add(new MatchupEntryModel { TeamCompeting = team });
                // if byes then create a matchup with 1 entry OR if 2 entries create a matchup
                if(byes > 0 || curr.Entries.Count > 1)
                {
                    curr.MatchupRound = 1;      // first round hardcoded
                    output.Add(curr);           // Add the matchup to the list of first round matchups 
                    curr = new MatchupModel();  // Reset the matchup model

                    if(byes > 0)
                    {   // if a bye got us here, get rid of one
                        byes --;
                    }
                }
            }
            return output;
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
     
            for (int i = 1; i <= rounds; i++)
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
                output ++;
                val = val * 2;
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
