using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeGaze.Engine
{
    public delegate void TriggerHandlerMessage(object sender, MessageEvent message);
    public class MessageEvent : EventArgs
    {
        public string message { get; set; }
        public enum messageType { ConnectionFail , WrongAuthentication, closeFile, TriggerWord };
        public messageType type { get; set; }

    }
}
