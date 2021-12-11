
using Experiment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;



namespace Experiment
{

    public partial class Guide : Form
    {
        int current_mission=0;
        int current_part = 0;
        string current_system = "";

        TextAndMissions tam;
        EndTask endTask;
        StartTask startTask;
        EndExperiment endExp;
        //MainClass _mainClass;
        public Guide(string system, int part, EndTask endTask, StartTask startTask, EndExperiment endExp)
        {

            InitializeComponent();

            tam = new TextAndMissions();
            this.endTask = endTask;
            this.startTask = startTask;
            this.endExp = endExp;
            // change form location
            this.StartPosition = FormStartPosition.Manual;
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }
            this.Left = (rightmost.WorkingArea.Width / 2) - (this.Width / 2);
            this.Top = 20;

            
            this.current_part = part;
            this.current_system = system;
            this.current_mission = 0;

            missionLabel.Text = tam.getmission(current_system, current_part, current_mission);
            this.startTask(DateTime.Now, missionLabel.Text, current_mission);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if( current_mission % 2 != 0)
            {
                var result = MessageBox.Show("Are you sure you want to continue?", "",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;
            }
            
            current_mission++;
            this.endTask(DateTime.Now, missionLabel.Text);
            if(nextButton.Text.Equals("Finish"))
            {
                endExp(DateTime.Now);
            }
            if(current_mission < tam.getSizeOfMissionList(current_system, current_part) - 1)
            {
                string miss = tam.getmission(current_system, current_part, current_mission);
                string[] missArr = miss.Split('.');
                if(missArr.Length > 1)
                {
                    miss = missArr[0] + ".\n\n" + missArr[1];
                }
                missionLabel.Text = miss;
                this.startTask(DateTime.Now, missionLabel.Text, current_mission);
            }
            if (current_mission == tam.getSizeOfMissionList(current_system,current_part) - 1)
            {
                string miss = tam.getmission(current_system, current_part, current_mission);
                string[] missArr = miss.Split('.');
                if (missArr.Length > 1)
                {
                    miss = missArr[0] + ".\n\n" + missArr[1];
                }
                missionLabel.Text = miss;
                nextButton.Text = "Finish";
                this.startTask(DateTime.Now, missionLabel.Text, current_mission);

            }
            else if(current_mission >= tam.getSizeOfMissionList(current_system, current_part))
            {
                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                {
                    if (!String.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.Contains("Word"))
                    {
                        process.Kill();
                    }
                }

                this.Close();
            }
                
        }
    }
}
