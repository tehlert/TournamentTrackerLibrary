using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the database connections
            TrackerLibrary.GlobalConfig.InitializeConnection(DatabaseType.Sql);
            // TrackerLibrary.GlobalConfig.InitializeConnections(true, true);

            // This is where we change which form loads on launch
            Application.Run(new TournamentDashboardForm());
            //Application.Run(new CreateTournamentForm()); 
        }
    }
}
