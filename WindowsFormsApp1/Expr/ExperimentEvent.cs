using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment
{
    public class ExperimentEvent
    {
        public SystemType _system { get; set; }
        public CommandType _command { get; set; }
        public SubCommandType _subCommand { get; set; }
        public DateTime _timeStamp { get; set; }
        public string _userKey { get; set; }
        public TaskType _task { get; set; }
        
    }
}
