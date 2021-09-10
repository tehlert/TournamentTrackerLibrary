using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    public partial class TournamentViewerForm : Form
    {
        private TournamentModel tournament;
        BindingList<int>          rounds           = new BindingList<int>();
        BindingList<MatchupModel> selectedMatchups = new BindingList<MatchupModel>();

        

        // Pass in tournamentModel as a parameter of class TournamentModel
        public TournamentViewerForm(TournamentModel tournamentModel)
        {
            InitializeComponent();
            // store tournament object in private variable at form level
            tournament = tournamentModel;

            WireUpLists();

            LoadFormData();

            LoadRounds();
        }

        private void LoadFormData()
        {   // Take value from instance and put into textbox on form
            tournamentName.Text = tournament.TournamentName;
        }

        private void WireUpLists()
        {
            roundDropDown.DataSource = rounds;
            matchupListBox.DataSource = selectedMatchups;
            matchupListBox.DisplayMember = "DisplayName";
        }

        private void LoadRounds()
        {
            rounds.Clear();

            rounds.Add(1);
            int currRound = 1;

            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {
                if(matchups.First().MatchupRound > currRound)
                {
                    currRound = matchups.First().MatchupRound;
                    rounds.Add(currRound);                   
                }
            }

            LoadMatchups(1);
        }

        private void roundDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchups((int)roundDropDown.SelectedItem);
        }

        // Loads list of matchups
        private void LoadMatchups(int round)
        {
            // loop through rounds
            foreach (List<MatchupModel> matchups in tournament.Rounds)
            {   // if we are in the selected round
                if (matchups.First().MatchupRound == round)
                {
                    selectedMatchups.Clear();
                    foreach (MatchupModel m in matchups)
                    {
                        selectedMatchups.Add(m);
                    }
                }
            }
            if (selectedMatchups.Count > 0)
            {
                LoadMatchup(selectedMatchups.First());
            }

        }

        // loads one matchup 
        private void LoadMatchup(MatchupModel m)
        {
            if (m != null) {
                for (int i = 0; i < m.Entries.Count; i++)
                {
                    if (i == 0)
                    {
                        if (m.Entries[0].TeamCompeting != null)
                        {
                            teamOneName.Text = m.Entries[0].TeamCompeting.TeamName;
                            teamOneScoreValue.Text = m.Entries[0].Score.ToString();

                            teamTwoName.Text = "<bye>";
                            teamTwoScoreValue.Text = "0";
                        }
                        else
                        {
                            teamOneName.Text = "Not Yet Set";
                            teamOneScoreValue.Text = "";
                        }
                    }

                    if (i == 1)
                    {
                        if (m.Entries[1].TeamCompeting != null)
                        {
                            teamTwoName.Text = m.Entries[1].TeamCompeting.TeamName;
                            teamTwoScoreValue.Text = m.Entries[1].Score.ToString();
                        }
                        else
                        {
                            teamTwoName.Text = "Not Yet Set";
                            teamTwoScoreValue.Text = "";
                        }
                    }
                }

            }// end if

        }

        private void matchupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMatchup((MatchupModel)matchupListBox.SelectedItem);
        }


    }
}
