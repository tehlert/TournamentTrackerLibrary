using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";

        public PersonModel CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                // uses Dapper to create dynamic parameters and execute
                var p = new DynamicParameters();
                p.Add("@FirstName",       model.FirstName);
                p.Add("@LastName",        model.LastName);
                p.Add("@EmailAddress",    model.EmailAddress);
                p.Add("@CellphoneNumber", model.CellphoneNumber);
                p.Add("@id",              0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id"); // store the created and returned id for the new model

                return model;
            }
        }

        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize information</param>
        /// <returns>The prize informaiton, including the unique identifier</returns>
        public PrizeModel CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                // uses Dapper to create dynamic parameters and execute
                var p = new DynamicParameters();
                p.Add("@PlaceNumber",     model.PlaceNumber);
                p.Add("@PlaceName",       model.PlaceName);
                p.Add("@PrizeAmount",     model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id",              0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);
               
                model.Id = p.Get<int>("@id"); // store the created and returned id for the new model

                return model;
            }
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                // uses Dapper to create dynamic parameters and execute
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);          
                p.Add("@id",       0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id"); // store the created and returned id for the new model

                foreach (PersonModel tm in model.TeamMembers)
                {
                    p = new DynamicParameters();
                    p.Add("@TeamId",   model.Id);  
                    p.Add("@PersonId", tm.Id);  

                    connection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
                }
                return model;
            }
        }

        /// <summary>
        /// Think of this like a "quarterback" method that calls other private methods that do one thing each
        /// As it is already getting large and we have a lot more to do, we decide to switch our method
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                // Create the record of the tournament in the database
                SaveTournament(connection, model);
                // Save the prizes to the tournament
                SaveTournamentPrizes(connection, model);
                // Save the teams to the tournament
                SaveTournamentEntries(connection, model); 
            }
        }

        // Create the record of the tournament in the database
        private void SaveTournament(IDbConnection connection, TournamentModel model)
        {
            // uses Dapper to create dynamic parameters and execute
            var p = new DynamicParameters();
            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee",       model.EntryFee);
            p.Add("@id",             0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTournaments_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id"); // store the created and returned id for the new model
        }

        private void SaveTournamentPrizes(IDbConnection connection, TournamentModel model)
        {
            // loop through the prizes and insert each one into the TournamentPrizes table
            foreach (PrizeModel pz in model.Prizes)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@PrizeId",      pz.Id);
                p.Add("@id",           0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournamentEntries(IDbConnection connection, TournamentModel model)
        {
            // loop through the teams and insert each one into the TournamentEntries table
            foreach (TeamModel tm in model.EnteredTeams)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@TeamId",       tm.Id);
                p.Add("@id",           0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        // Get a list of people
        public List<PersonModel> GetPerson_All() 
        {
            List<PersonModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        // Get a list of teams
        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TeamModel>("spTeam_GetAll").ToList();

                // for each team in our list of teams we are going to query 
                // and put the PersonModels in the TeamMembers list
                foreach (TeamModel team in output)
                {
                    // Pass in the parameters we need
                    var p = new DynamicParameters();
                    p.Add("@TeamId", team.Id);
                 
                    // we have a list of PersonModel in TeamModel so we can do this
                    // see the stored procedure to see the join

                    team.TeamMembers = connection.Query<PersonModel>("spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            return output;
        }
    }
}
