using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    public class PrizeModel
    {
        /// <summary>
        /// The unique identifier for the prize
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// This represents the number of the final place 
        /// a team must place in the tournament
        /// to recieve this prize
        /// </summary>
        public int PlaceNumber { get; set; }
        /// <summary>
        /// This represents the name of the final place 
        /// a team must place in the tournament
        /// to recieve this prize
        /// </summary>
        public string PlaceName { get; set; }
        /// <summary>
        /// This represents the prize amount the team will 
        /// win for this place in the tournament
        /// </summary>
        public decimal PrizeAmount { get; set; }
        /// <summary>
        /// This represents the percentage of the prize pool
        /// this prize represents
        /// </summary>
        public double PrizePercentage { get; set; }

        /// <summary>
        /// empty constructor
        /// </summary>
        public PrizeModel()
        {

        }

        /// <summary>
        /// overload constructor for prize model
        /// </summary>
        /// <param name="placeName"></param>
        /// <param name="placeNumber"></param>
        /// <param name="prizeAmount"></param>
        /// <param name="prizePercentage"></param>
        public PrizeModel(string placeName, string placeNumber, string prizeAmount, string prizePercentage)
        {
            PlaceName = placeName;

            int placeNumberValue = 0;
            int.TryParse(placeNumber, out placeNumberValue);
            PlaceNumber = placeNumberValue;

            decimal prizeAmountValue = 0;
            decimal.TryParse(prizeAmount, out prizeAmountValue);
            PrizeAmount = prizeAmountValue;

            double prizePercentageValue = 0;
            double.TryParse(prizePercentage, out prizePercentageValue);
            PrizePercentage = prizePercentageValue;

        }
    }
}
