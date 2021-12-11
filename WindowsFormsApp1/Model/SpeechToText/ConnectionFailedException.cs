using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeGaze.SpeechToText
{
    class ConnectionFailedException : Exception
    {
        public ConnectionFailedException(string message)
        : base(message)
        {
        }
    }
}
