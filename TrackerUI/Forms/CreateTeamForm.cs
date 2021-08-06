using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.DataAccess;

namespace TrackerUI
{
    public partial class CreateTeamForm : Form
    {
        private List<PersonModel> availableTeamMembers = GlobalConfig.Connection.GetPerson_All();
        private List<PersonModel> selectedTeamMembers  = new List<PersonModel>();
        private ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            InitializeComponent();

            callingForm = caller;

            //CreateSampleData();

            WireUpLists();
        }
        

        private void CreateSampleData()
        {
            availableTeamMembers.Add(new PersonModel { FirstName = "Tom",   LastName = "Mayhaveavaginas" });
            availableTeamMembers.Add(new PersonModel { FirstName = "Ethan", LastName = "CollinssickTowork" });

            selectedTeamMembers.Add(new PersonModel { FirstName = "Adam", LastName = "Meachamgoti" });
            selectedTeamMembers.Add(new PersonModel { FirstName = "Mooch", LastName = "Cassis" });
        }

        private void WireUpLists()
        {
            // Setting to null first triggers a refresh everytime we call this
            // TODO - better way of data refreshing?
            selectTeamMemberDropDown.DataSource = null;

            selectTeamMemberDropDown.DataSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMember = "FullName"; // method in PersonModel

            teamMembersListBox.DataSource = null;

            teamMembersListBox.DataSource = selectedTeamMembers;
            teamMembersListBox.DisplayMember = "FullName";
        }


        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PersonModel p = new PersonModel();

                p.FirstName = firstNameValue.Text;
                p.LastName = lastNameValue.Text;
                p.EmailAddress = emailValue.Text;
                p.CellphoneNumber = cellphoneValue.Text;

                /*
                foreach (IDataConnection db in GlobalConfig.Connections)
                {
                    db.CreatePerson(p);
                */
                GlobalConfig.Connection.CreatePerson(p);

                selectedTeamMembers.Add(p);

                WireUpLists();

                firstNameValue.Text = "";
                lastNameValue.Text = "";
                emailValue.Text = "";
                cellphoneValue.Text = "";
            }
            else
            {
                MessageBox.Show("This form has empty fields. Please try again.");
            }
        }

        private bool ValidateForm()
        {
            // TODO - Add better Validation to the form
            if(firstNameValue.Text.Length == 0)
            {
                return false;
            }

            if (lastNameValue.Text.Length == 0)
            {
                return false;
            }

            if (emailValue.Text.Length == 0)
            {
                return false;
            }

            if (cellphoneValue.Text.Length == 0)
            {
                return false;
            }

            return true;
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)selectTeamMemberDropDown.SelectedItem; // cast to PersonModel

            // we don't want to let null into our lists -- breaks stuff
            if (p != null)
            {
                // find out who the person is that is selected and add them to the selectedTeamMembers List
                availableTeamMembers.Remove(p);
                selectedTeamMembers.Add(p);

                // call again to refresh lists with new values
                WireUpLists();
            }
        }

        private void removeSelectedTeamMemberButton_Click(object sender, EventArgs e)
        {
            PersonModel p = (PersonModel)teamMembersListBox.SelectedItem;

            if (p != null)
            {
                selectedTeamMembers.Remove(p);
                availableTeamMembers.Add(p);

                WireUpLists();
            }
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            PrizeModel t = new PrizeModel();

            t.TeamName = teamNameValue.Text;
            t.TeamMembers = selectedTeamMembers;

            // Create the Team
            GlobalConfig.Connection.CreateTeam(t);

            callingForm.TeamComplete(t);

            this.Close();
        }
    }
}
