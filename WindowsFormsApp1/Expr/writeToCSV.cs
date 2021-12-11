using System;
using System.IO;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using Spire.Doc;

namespace Experiment
{

    public class writeToCSV : Experiment.DBWriterInterface
    {
        private string timestampStartMission;
        public writeToCSV()
        {
            
        }
        public void initTables(string userID, string userName, string date)
        {
            // create tables directory
            Directory.CreateDirectory("./Tables");
            Directory.CreateDirectory("./Tables/"+date+"_"+userID);
            /***********************************************************************************/
            /**************************************Users****************************************/
            /***********************************************************************************/
            if (!File.Exists("./Tables/users.csv"))
            {
                var csv = new StringBuilder();
                var newLine = string.Format(
                                        "{0},{1},{2}",
                                        "UserID","UserName", "Timestamp"
                                        );
                csv.AppendLine(newLine);

                File.WriteAllText("./Tables/users.csv", csv.ToString());
            }

            /***********************************************************************************/
            /********************************ExperimentsEvents**********************************/
            /***********************************************************************************/
            if (!File.Exists("./Tables/" + date + "_" + userID + "/experimentsEvents.csv"))
            {
                //""Event" = StartExperiment, EndExperiment
                var csv = new StringBuilder();
                var newLine = string.Format(
                                        "{0},{1},{2},{3},{4},{5}",
                                        "ExperimentID", "SystemName", "TextNumber",
                                        "UserID", "Event", "Timestamp"
                                        );
                csv.AppendLine(newLine);
                File.WriteAllText("./Tables/" + date + "_" + userID + "/experimentsEvents.csv", csv.ToString());
            }

            /***********************************************************************************/
            /**********************************MissionsEvent************************************/
            /***********************************************************************************/

            if (!File.Exists("./Tables/" + date + "_" + userID + "/missionsEvent.csv"))
            {
                /*"Event" = StartMission, EndMissiont*/
                var csv = new StringBuilder();
                var newLine = string.Format(
                                        "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                                        "ExperimentID", "MissionID", "TextNumber",
                                        "SystemName", "UserID", "Event", "Timestamp",
                                         "ActualResult", "MissionText","TotalTime","Verdict"
                                        );
                csv.AppendLine(newLine);
                File.WriteAllText("./Tables/" + date + "_" + userID + "/missionsEvent.csv", csv.ToString());
            }


            /***********************************************************************************/
            /*********************************CommandsEvents************************************/
            /***********************************************************************************/
            if (!File.Exists("./Tables/" + date + "_" + userID + "/commandsEvents.csv"))
            {
                var csv = new StringBuilder();
                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    "CID", "ExperimentID", "SystemName", "UserID",
                    "Command", "BaseCommand", "BaseCID", "MissionID",
                    "args", "before", "after", "Timestamp");
                csv.AppendLine(newLine);
                File.WriteAllText("./Tables/" + date + "_" + userID + "/commandsEvents.csv", csv.ToString());
            }
        }

        // textAfterChange and textBeforeChange are the document before the command and after the command , if there is no change they will be the same 
        // textAfterChange and textBeforeChange are the document before the command and after the command , if there is no change they will be the same 
        public void writeCommandEvent(string CID, string experimentID, string systemName, string userID,
                        string commandName, string baseCommandName, string baseCommandID, string missionID,
                        string argsOfCommand, string textBeforeChange, string textAfterChange, string timeStamp, string date)
            
        {
            var csv = new StringBuilder();
            textBeforeChange = Regex.Unescape(textBeforeChange);
            textBeforeChange = removePunctuation(textBeforeChange);
            textAfterChange = Regex.Unescape(textAfterChange);
            textAfterChange = removePunctuation(textAfterChange);
            var newLine = string.Format(
                                        "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                        CID, experimentID, systemName, userID,
                                        commandName, baseCommandName, baseCommandID, missionID,
                                        argsOfCommand, textBeforeChange, textAfterChange, timeStamp
                                        );
            csv.AppendLine(newLine);
            File.AppendAllText("./Tables/" + date + "_" + userID + "/commandsEvents.csv", csv.ToString());
        }
        public void writeUser(string userID, string userName, string timestamp)
        {
            var csv = new StringBuilder();
            var newLine = string.Format(
                                        "{0},{1},{2}",
                                        userID, userName, timestamp
                                        );
            csv.AppendLine(newLine);
            File.AppendAllText("./Tables/users.csv", csv.ToString());

        }
        public void writeNewExperimentEvent(string experimentID, string systemName, int textNumber, string user_id, string timestamp, string date)
        {
            var csv = new StringBuilder();
            var newLine = string.Format(
                                        "{0},{1},{2},{3},{4},{5}",
                                        experimentID, systemName, textNumber,
                                        user_id, "NewExperiment", timestamp
                                        );
            csv.AppendLine(newLine);
            File.AppendAllText("./Tables/" + date + "_" + user_id + "/experimentsEvents.csv", csv.ToString());

        }
        public void writeEndExperimentEvent(string experimentID, string systemName, int textNumber, string user_id, string timestamp, string date)
        {
            var csv = new StringBuilder();
            var newLine = string.Format(
                                        "{0},{1},{2},{3},{4},{5}",
                                        experimentID, systemName, textNumber,
                                        user_id, "EndExperiment", timestamp
                                        );
            csv.AppendLine(newLine);
            File.AppendAllText("./Tables/" + date + "_" + user_id + "/experimentsEvents.csv", csv.ToString());


        }
        public void writeNewMissionEvent(string experimentID, string missionID, int textNumber, string systemName,
                        string user_id, string timestamp, string currState, string missionText, string date)
        {
            var csv = new StringBuilder();
            missionText = removePunctuation(missionText);
            currState = removePunctuation(currState);
            var newLine = string.Format(
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                    experimentID, missionID, textNumber,
                                    systemName, user_id, "StartMission", timestamp,
                                   currState, missionText);
            csv.AppendLine(newLine);
            this.timestampStartMission = timestamp;
            File.AppendAllText("./Tables/" + date + "_" + user_id + "/missionsEvent.csv", csv.ToString());

        }

        public void writeEndMissionEvent(string experimentID, string missionID, int textNumber, string systemName,
                        string user_id, string timestamp, string currState, string missionText, string date)
        {
            var csv = new StringBuilder();
            missionText = removePunctuation(missionText);
            currState = Regex.Replace(currState, @"[\d-]", string.Empty);
            currState = removePunctuation(currState);
            var totalTime = calcTotalTime(experimentID,missionID,timestamp, user_id, date);
            //var verdict = compare(missionID, currState);
            var verdict = "";
            if (missionText.Contains("Navigate") && missionText.Contains("line"))
                verdict = "Pass";
            else
                verdict = compare(missionID, currState);
            var newLine = string.Format(
                                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                                    experimentID, missionID, textNumber,
                                    systemName, user_id, "EndMission", timestamp,
                                     currState, missionText,totalTime,verdict
                                    );
            csv.AppendLine(newLine);
            File.AppendAllText("./Tables/" + date + "_" + user_id + "/missionsEvent.csv", csv.ToString());
        }

        private double calcTotalTime(string experimentID, string missionID, string timestamp, string userID, string date)
        {            
            DateTime dateTime1 = DateTime.ParseExact(this.timestampStartMission, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime dateTime2 = DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            var diffInSeconds = (dateTime2 - dateTime1).TotalSeconds;
            return diffInSeconds;
        }
        private string compare(string missionID, string currState)
        {
            string expectedState = readWord(missionID);
            Regex rgx = new Regex("[^a-zA-Z]");
            currState = rgx.Replace(currState, "");
            Console.WriteLine(currState);
            expectedState = rgx.Replace(expectedState, "");
            if (currState.ToLower().Equals(expectedState.ToLower()))
                return "Pass";
            else
                return "Fail";
        }
        private string removePunctuation(string str)
        {
            StringBuilder newStr = new StringBuilder();
            foreach (char c in str.ToCharArray())
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ' || c == '"' || c == '\"' || c == '-')
                {
                    newStr.Append(c);
                }
            }
            return newStr.ToString();
        }
        public static void deleteFromCSV(string userID, string date)
        {
            var csvTable = new DataTable();
            string pa = string.Format("{0}Resources\\pilot.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")));
            pa = "./Tables/" + date + "_" + userID + "/experimentsEvents.csv";
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(pa)), true))
            {
                csvTable.Load(csvReader);
            }
        }
        public string readWord(string missionID)
        {            
            try
            {
                if (int.Parse(missionID.Split('_')[2]) % 2 != 0)
                {
                    string path = string.Format("{0}Resources\\" + missionID + ".docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")));
                    Console.WriteLine(path);
                    Document doc = new Document();
                    doc.LoadFromFile(path);
                    string text = doc.GetText();
                    text = Regex.Replace(text, @"[\d-]", string.Empty);
                    if (text.Contains(".NET.\r\n"))
                        text = text.Substring(text.IndexOf(".NET.\r\n"));
                    var lines = Regex.Split(text, "\r\n|\r|\n").Skip(1);
                    //Console.WriteLine(text);
                    text = string.Join(Environment.NewLine, lines.ToArray());
                    text = removePunctuation(text);
                    Console.WriteLine(text);
                    return text;
                }
            }
            catch (Exception e)
            {
                return "";
            }
            return "";
        }
        public static void deleteFromCSV(string userID, string experimentID, string date)
        {
            var csvTable = new DataTable();
            string pa = string.Format("{0}Resources\\pilot.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")));
            pa = "./Tables/" + date + "_" + userID + "/experimentsEvents.csv";
            using (var csvReader = new LumenWorks.Framework.IO.Csv.CsvReader(new StreamReader(System.IO.File.OpenRead(pa)), true))
            {
                csvTable.Load(csvReader);
            }
        }
        
        /*public static void Main(string[] args)
        { 
             writeToCSV x  = new writeToCSV();
        }*/
    }
}


