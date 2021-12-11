using System;
using System.Collections.Generic;
using System.Windows.Forms;
//using CsvExample;

namespace Experiment
{
    public enum SystemType { VoiceOnly, VoiceGaze, VoiceMouse };
    public enum CommandType { Replace, Fix, Delete, Add, Copy, Past, Change };
    public enum SubCommandType { No, Options, Number };
    public enum TaskType { Typing, Fixing }

    public delegate void EndTask(DateTime timeStamp, string missionText);
    public delegate void StartTask(DateTime timeStamp, string missionText, int missionIndex);
    public delegate void EndExperiment(DateTime timeStamp);
    public class MainClass
    {
        //permenent for each new MainClass()
        private TextAndMissions textAndMissions;
        private writeToCSV csvWriter;

        //foreach Experiment
        private string experimentID;
        private string userID;
        private string userName;
        private string systemName;
        private string currentTextPath;
        private int currentTextNumber;

        //foreach Mission
        //private int currentMissionIndex;
        private string missionID;

        //foreach BaseCommand
        private string baseCommandID;
        private string baseCommand;

        //when to change??
        private string missionStartState;
        private string missionEndState;

        public MainClass()
        {
            this.textAndMissions = new TextAndMissions();
            this.csvWriter = new writeToCSV(); // init csv files
            this.missionStartState = "";
            this.missionEndState = "";
        }

        /*
         * TextAndMissions:
            * Dictionary => systemName : Text path
            * Dictionary => TextPath   : Missions 
         * ------------------------------------
         * GUI:
            * the user not choose a file by himself and every system need to be able to open a file.
            * add UserId
            * add start Experiment buttons
         * ------------------------------------
         * GUI Missions:
            * gets an event to pop up the mission 
            * and pop up the mission when event accure
         */


        /// This function return the correct Text path according to systemname 
        /// insert to db - experiment ID, timeStamp, userId, system name
        /// return path to text file
        /// <param name="user_name">user name string , contains characters </param>
        /// <param name="user_id">user id string , contains 9 numbers </param>
        /// <param name="systemName"> string VoiceOnly, VoiceGaze, VoiceMouse, MouseAndKeyboard</param>
        /// <param name="number">0=pilot, 1=text number 1, 2= text number 2 </param>
        /// <returns>string - path of word documen</returns>
        public string GetPath(string user_id, string user_name, string systemName, int number)
        {
            this.csvWriter.initTables(user_id, user_name, DateTime.Now.ToString("dd-MM-yyyy"));
            this.csvWriter.writeUser(user_id, user_name, DateTime.Now.ToString("dd-MM-yyyy"));
            //get path to text file from TextAndMissions
            try
            {
                string textPath = textAndMissions.getTextPath(systemName, number);
                this.userName = user_name;
                this.userID = user_id;
                this.currentTextPath = textPath;
                this.currentTextNumber = number;
                this.systemName = systemName;
               
                //  this.currentMissionIndex = 0;
                return textPath;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exceprion eccure in GetPath function: " + e.StackTrace);
                return "";
            }
        }
        /// <summary>
        /// after you open the document, call to this fucntion
        /// This function activate the GUI of Experiment System and save to DB 
        /// </summary>
        /// <param name="timeStamp">DateTime current date</param>
        public void StartExperiment(DateTime timeStamp)
        {
            //get path to text file from TextAndMissions
            try
            {
                string timeStamp_ours = GetTimestamp(timeStamp);
                // create experimentID and save it globaly
                createExperimentId();
                // insert event to db - Exp_id, timeStamp, userId
                csvWriter.writeNewExperimentEvent(this.experimentID, this.systemName, this.currentTextNumber, this.userID, timeStamp_ours, timeStamp.ToString("dd-MM-yyyy"));
                startCommandsGui();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception eccure in StartExperiment function: " + e.StackTrace);
            }
        }

        /// <summary>
        /// This function is for both - pilot and experiment
        /// when you want to end the experiment you will call this function
        /// and this function insert to db - experiment ID, timeStamp, userId, system name
        /// </summary>
        /// <param name="timeStamp"></param>
        public void EndExperiment(DateTime timeStamp)
        {

            string timeStamp_ours = GetTimestamp(timeStamp);
            // save to db - exp finish Exp_id, timeStamp userId
            csvWriter.writeEndExperimentEvent(this.experimentID, this.systemName, this.currentTextNumber, this.userID, timeStamp_ours, timeStamp.ToString("dd-MM-yyyy"));
            this.experimentID = null;
            //this.currentMissionIndex = 0;
        }

        /// <summary>
        /// This function get the commands event- and save it to db 
        /// This fucntion handle each command differently 
        /// 
        /// example:
        /// "commandName":"replace", "argsOfCommand":"replace mother father",
        /// "beforeChange">"all the document string before the command", "afterChange">"all the document string after the command"
        /// "baseCommand">true, "timeStamp">current date
        /// 
        /// example:
        /// "commandName">"Cancle", "argsOfCommand">"Cancle"
        /// "baseCommand">false, "DocString">"all the document string", "timeStamp">current date
        /// 
        /// TODO: Option or more - check if we want to save the option in DB and if yes so pass the arg to function
        /// </summary>
        /// <param name="commandName">"replace"</param>
        /// <param name="argsOfCommand">"replace mother father"</param>
        /// <param name="beforeChange">"all the document string before the command"</param>
        /// <param name="afterChange">"all the document string after the command"</param>
        /// <param name="baseCommand">true</param>
        /// <param name="timeStamp">current date</param>
        public void EventCommand
                           (
                            string commandName,
                            string argsOfCommand,
                            string beforeChange,
                            string afterChange,
                            bool baseCommand,
                            DateTime timeStamp
                           )
        {
            // Replace, Fix, Delete, Add, Copy, Past,  No, Options, Number
            string CID = generateID();
            string timeStamp_ours = GetTimestamp(timeStamp);
            string[] arrayArgsOfCommand = argsOfCommand.Split(' ');
            this.missionEndState = afterChange;

            if (baseCommand)
            {
                // Unique num of base command -create every time baseCommand true
                this.baseCommandID = CID;
                this.baseCommand = commandName;
            }

            /*
                "Command" = Replace, Fix, Add, Copy from,  Past after, Past before ,Delete ,Delete from, No, Number Options WHAT ELSE??
                Args of command,
                word before change, // more ="",  delete-word , copy=word, make sure we are the same 
                word after change,// more ="", delete="", copy =word
                Timestamp,
                bool baseCommand,
                Unique num of base command ,
                Num of mission,
                fmainSYSTEM_Name,
                USER_ID
             */


            csvWriter.writeCommandEvent(CID, this.experimentID, this.systemName, this.userID,
           commandName, this.baseCommand, this.baseCommandID, this.missionID,
           argsOfCommand, beforeChange, afterChange, timeStamp_ours, timeStamp.ToString("dd-MM-yyyy"));


            //beforeChange, afterChange voiceit will be "", only on number command gets value

            /*
            when write add info: unique id including date(only day)+system name+userid, current mission?
            Enum name of command,
            Args of command,
            Timestamp,
            Enum of base command,
            Unique num of base command
            */
            //local Enum 
            //what about args ? gets as string and parse here?  or as array and parse here ?
            // this function maps to start state and finis state accurding to the command.
            // if command delete => ... 
            // if command fix => ...
        }


        /***********************************************************************************/
        /************************************private****************************************/
        /***********************************************************************************/
        private static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        //pilot_VoiceOnly_123456789 
        //todo:adi
        private string createExperimentId()
        {
            // Get the current date.
            DateTime thisDay = DateTime.Now.Date;
            var date = thisDay.ToString("dd-MM-yyyy");
            // get only day format 
            this.experimentID = this.currentTextNumber + "_" + this.systemName + "_" + this.userID + "_" + date;
            return experimentID;
        }
        private string createMissionId(int missionIndex)
        {
            // Get the current date.
            DateTime thisDay = DateTime.Today;
            // get only day format 
            this.missionID = this.systemName + "_#" + this.currentTextNumber + "_" + missionIndex;
            return this.missionID;
        }
        private static string generateID()
        {
            // its unique identifier - 16 bytes. but it is not useful for small databases, maybe we consider something else.
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// This function send to db the end mission event.
        /// if there isnt more missions so call to End Experiment 
        /// </summary>
        /// <param name="timeStamp"></param>
        private void EventEndMission(DateTime timeStamp, string missionText)
        {

            string timeStamp_ours = GetTimestamp(timeStamp);
            // save the end mission event

            csvWriter.writeEndMissionEvent(this.experimentID, this.missionID, this.currentTextNumber, this.systemName,
                                            this.userID, timeStamp_ours, this.missionEndState, missionText, timeStamp.ToString("dd-MM-yyyy"));

        }

        /// <summary>
        /// GUI call to this function
        /// after file open the system call to this function
        /// </summary>
        /// <param name="timeStamp"></param>
        private void EventStartMission(DateTime timeStamp, string missionText, int missionIndex)
        {
            //update missionStartState
            this.missionStartState = this.missionEndState;
            string timeStamp_ours = GetTimestamp(timeStamp);
            this.missionID = createMissionId(missionIndex);

            //save event 
            csvWriter.writeNewMissionEvent(this.experimentID, this.missionID, this.currentTextNumber, this.systemName,
                                            this.userID, timeStamp_ours, this.missionStartState, missionText, timeStamp.ToString("dd-MM-yyyy"));
            
        }
        private void startCommandsGui()
        {
            EndExperiment endExp = new EndExperiment(this.EndExperiment);
            EndTask end = new EndTask(this.EventEndMission);
            StartTask start = new StartTask(this.EventStartMission);
            Guide guide = new Guide(this.systemName, this.currentTextNumber, end, start, endExp);
            guide.Refresh();
            guide.Show();
            guide.TopMost = true;
            Application.DoEvents();
            Application.Run(guide);
        }
        // just for test 
        //static void Main(string[] args)
        //{
        //    //TextAndMissions tx = new TextAndMissions();
        //    MainClass mc = new MainClass();
        //    //*******************************startExp_0 * ****************************
        //    string path = mc.GetPath("3333", "VoiceOnly", 1);
        //    Console.WriteLine(path);
        //    mc.StartExperiment(DateTime.Now);


        //    path = mc.GetPath("3333", "VoiceOnly", 3);
        //    Console.WriteLine(path);
        //    mc.StartExperiment(DateTime.Now);
        //    //eventComm

        //}
        /*
        EndTask end = new EndTask(mc.EventEndMission);
        StartTask start = new StartTask(mc.EventStartMission);
        EndExperiment endExp = new EndExperiment(mc.EndExperiment);
        Guide guide = new Guide("VoiceOnly", 1, end, start, endExp);
        guide.Refresh();
        guide.Show();
        guide.TopMost = true;
        Application.DoEvents();
        Application.Run(guide);*/
    }
}
