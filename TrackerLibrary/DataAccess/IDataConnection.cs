using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TrackerLibrary.DataAccess
{
    //Every interface(contract) will start with an I
    public interface IDataConnection
    {
        // looks like a method - ITS NOT - no {} rather ; 
        void CreatePrize(PrizeModel model);
        void CreatePerson(PersonModel model);
        void CreateTeam(TeamModel model);
        void CreateTournament(TournamentModel model);

        void UpdateMatchup(MatchupModel model);

        List<TeamModel> GetTeam_All();
        List<PersonModel> GetPerson_All();
        List<TournamentModel> GetTournament_All();
        
    }
}
