using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    public class TeamModel
    {
        public int Id { get; set; }
        /// <summary>
        /// This represents the name of the team
        /// </summary>
        public string TeamName { get; set; }
        /// <summary>
        /// This represents a list of the team members for one team
        /// </summary>
        public List<PersonModel> TeamMembers { get; set; } = new List<PersonModel>();
       
    }
}
