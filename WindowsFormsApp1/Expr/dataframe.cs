using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment
{
    public static class OperationsUtlity
    {
        public static DataTable createDataTable()
        {
            DataTable missionsEvent = new DataTable();
            //columns  
            missionsEvent.Columns.Add("ExperimentID", typeof(string));
            missionsEvent.Columns.Add("MissionID", typeof(string));
            missionsEvent.Columns.Add("TextNumber", typeof(int));
            missionsEvent.Columns.Add("SystemName", typeof(string));
            missionsEvent.Columns.Add("UserID", typeof(string));
            missionsEvent.Columns.Add("Event", typeof(string));
            missionsEvent.Columns.Add("Timestamp", typeof(string));
            missionsEvent.Columns.Add("ActualResult", typeof(string));
            missionsEvent.Columns.Add("MissionText", typeof(string));

            //File.WriteAllText("./Tables/missionsEvent.csv", csv.ToString());
            //data  
            //missionsEvent.Rows.Add(111, "Devesh", "Ghaziabad");


            return missionsEvent;
        }


    }
    class dataframe
    {
    }
}
