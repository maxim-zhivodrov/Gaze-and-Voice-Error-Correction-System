using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment
{
    public interface DBWriterInterface
    {
        //void initTables();
        void writeCommandEvent(string CID, string experimentID, string systemName, string userID,
                      string commandName, string baseCommandName, string baseCommandID, string missionID,
                      string argsOfCommand, string textBeforeChange, string textAfterChange, string timeStamp, string date);

        void writeNewExperimentEvent(string experimentID, string systemName, int number, string user_id, string timestamp, string date);
        void writeEndExperimentEvent(string experimentID, string systemName, int number, string user_id, string timestamp, string date);
        void writeNewMissionEvent(string experimentID, string missionID, int textNumber, string systemName, string user_id, 
                        string timestamp, string currState, string missionText, string date);
        void writeEndMissionEvent(string experimentID, string missionID, int textNumber, string systemName, string user_id, 
                        string timestamp, string currState, string missionText, string date);
    }
}
