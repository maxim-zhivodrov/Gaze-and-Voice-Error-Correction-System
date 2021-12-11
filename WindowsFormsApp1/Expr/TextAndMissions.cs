using System;
using System.IO;
using System.Collections.Generic;


namespace Experiment
{
    /// <summary>
    /// This class save 
    /// Dictionary: systemName_#textnum , textPath, 
    /// Dictionary: systemName_#textnum , missionsList
    /// Dictionary or csv :  missionID , mission Text , before state( = Document string) , after state( = Document string) , line of mistake or word to change
    /// </summary>
    public class TextAndMissions
    {
        //string FileName = string.Format("{0}Resources\\ShortStory.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")));

        public Dictionary<string, string> systemNameAndTextPath =
            new Dictionary<string, string>(){
                {"VoiceOnly_#0", string.Format("{0}Resources\\VoiceOnly_#0.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceOnly_#1", string.Format("{0}Resources\\VoiceOnly_#1.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceOnly_#2", string.Format("{0}Resources\\VoiceOnly_#2.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceOnly_#3", string.Format("{0}Resources\\VoiceOnly_#3.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceGaze_#0", string.Format("{0}Resources\\VoiceGaze_#0.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceGaze_#1", string.Format("{0}Resources\\VoiceGaze_#1.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceGaze_#2", string.Format("{0}Resources\\VoiceGaze_#2.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceGaze_#3", string.Format("{0}Resources\\VoiceGaze_#3.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceMouse_#0", string.Format("{0}Resources\\VoiceMouse_#0.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceMouse_#1", string.Format("{0}Resources\\VoiceMouse_#1.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceMouse_#2", string.Format("{0}Resources\\VoiceMouse_#2.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"VoiceMouse_#3", string.Format("{0}Resources\\VoiceMouse_#3.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"MouseAndKeyboard_#0", string.Format("{0}Resources\\MouseAndKeyboard_#0.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"MouseAndKeyboard_#1", string.Format("{0}Resources\\MouseAndKeyboard_#1.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"MouseAndKeyboard_#2", string.Format("{0}Resources\\MouseAndKeyboard_#2.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
                {"MouseAndKeyboard_#3", string.Format("{0}Resources\\MouseAndKeyboard_#3.docx", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")))},
            };

        public Dictionary<string, List<string>> textPathAndMissions;

        public TextAndMissions()
        {
            this.readMissionsFromCsv(); 
        }

        private void readMissionsFromCsv()
        {
            this.textPathAndMissions = new Dictionary<string, List<string>>()
            {
                {"VoiceOnly_#0", new List<string>()},
                {"VoiceGaze_#0", new List<string>()},
                {"VoiceMouse_#0", new List<string>()},
                {"MouseAndKeyboard_#0", new List<string>()},

                {"VoiceOnly_#1", new List<string>()},
                {"VoiceOnly_#2", new List<string>()},
                {"VoiceOnly_#3", new List<string>()},

                {"VoiceGaze_#1", new List<string>()},
                {"VoiceGaze_#2", new List<string>()},
                {"VoiceGaze_#3", new List<string>()},

                {"VoiceMouse_#1", new List<string>()},
                {"VoiceMouse_#2", new List<string>()},
                {"VoiceMouse_#3", new List<string>()},

                {"MouseAndKeyboard_#1", new List<string>()},
                {"MouseAndKeyboard_#2", new List<string>()},
                {"MouseAndKeyboard_#3", new List<string>()},

            };
            string path = string.Format("{0}Resources\\missions.csv", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\")));
            try
            {
                using (var reader = new StreamReader(path))
                {
                    //List<string> missionsVoiceMouse__0 = new List<string>();
                    //List<string> listB = new List<string>();

                    var columnsNames = reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(new string[] { ",\"" }, StringSplitOptions.None);
                        string newMission = values[1].Remove(values[1].Length - 3);
                        newMission = newMission.Replace("\\\"", "");
                        //Console.WriteLine(newMission);
                        this.textPathAndMissions[values[0].Split(',')[0]].Add(newMission);
                    }
                }
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Error: The file missions.csv in use with another program or perhaps not exist in path: " + path + ".");
            }

        }
        public string getTextPath(string systemName, int number)
        {
            string textPath;
            string key = systemName + "_#" + number;
            if (!systemNameAndTextPath.TryGetValue(key, out textPath))
            {
                // the key isn't in the dictionary.
                throw new Exception("textPathAndMissions not conains the given key: " + key);
            }
            return textPath;
        }
       
        public string getmission(string systemName, int number, int missionIndex)
        {
            string mission;
            List<string> listOfMissions;
            string key = systemName + "_#" + number;
            if (!textPathAndMissions.TryGetValue(key, out listOfMissions))
            {
                // the key isn't in the dictionary.
                throw new Exception("textPathAndMissions not conains the given text path as a key: " + key);
            }
            if (listOfMissions.Count <= missionIndex)
            {
                throw new Exception("mission index out of range");
            }
            mission = listOfMissions[missionIndex];
            return mission;
        }

        public int getSizeOfMissionList(string systemName, int number)
        {
            List<string> listOfMissions;
            string key = systemName + "_#" + number;
            if (!textPathAndMissions.TryGetValue(key, out listOfMissions))
            {
                // the key isn't in the dictionary.
                throw new Exception("textPathAndMissions not conains the given text path as a key: " + key);
            }
            return listOfMissions.Count;
        }

    }
}
