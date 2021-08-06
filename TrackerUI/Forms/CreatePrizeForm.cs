using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.DataAccess;

namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        IPrizeRequester callingForm;
        public CreatePrizeForm(IPrizeRequester caller)
        {
            InitializeComponent();

            callingForm = caller;
        }

        /// <summary>
        /// upon clicking the button the form is first validated,
        /// then for each connection in our connection list we create a new prize in both SQl database and text file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // check if form is valid -- true/false
            if (ValidateForm())
            {
                PrizeModel model = new PrizeModel(
                    placeNameValue.Text, 
                    placeNumberValue.Text, 
                    prizeAmountValue.Text, 
                    prizePercentageValue.Text);

                GlobalConfig.Connection.CreatePrize(model);
                //foreach (IDataConnection db in GlobalConfig.Connections)
                //{ db.CreatePrize(model); }

                callingForm.PrizeComplete(model);

                this.Close();

                // Clear out the form when we are finished 
                // placeNameValue.Text       = "";
                // placeNumberValue.Text     = "";
                // prizeAmountValue.Text     = "0";
                // prizePercentageValue.Text = "0";
            }
            else
            {
                MessageBox.Show("This form has invalid information. Please try again.");
            }
        }

        /// <summary>
        /// validates all the fields on the prize form
        /// </summary>
        /// <returns></returns>
        private bool ValidateForm()
        {
            // create variables for validating the place
            bool output = true;
            int placeNumber = 0;
            bool placeNumberValidNumber = int.TryParse(placeNumberValue.Text, out placeNumber);

            // verify the input place number as an integer
            if (!placeNumberValidNumber)
            {
                output = false;
            }

            // verify the input place number is > 1
            if (placeNumber < 1)
            {
                output = false;
            }

            // verify that something has been entered in the place name form
            if (placeNameValue.Text.Length == 0)
            {
                output = false;
            }

            // create variables to validate the prize
            decimal prizeAmount     = 0;
            double  prizePercentage = 0;

            bool prizeAmountValid     = decimal.TryParse(prizeAmountValue.Text, out prizeAmount);
            bool prizePercentageValid = double.TryParse(prizePercentageValue.Text, out prizePercentage);

            // verify the input type of prize amount and percentage
            if(!prizeAmountValid || !prizePercentageValid)
            {
                output = false;
            }

            // prize amount and percantage must be > 0
            if(prizeAmount <= 0 && prizePercentage <= 0)
            {
                output = false;
            }

            // restrict the percentage between 0 and 100
            if(prizePercentage < 0 || prizePercentage > 100)
            {
                output = false;
            }

            return output;
        }
    }
}
