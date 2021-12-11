using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeGaze.SpeechToText
{
    class WrongAuthenticationException : Exception
    {
        public WrongAuthenticationException(string message)
        : base(message)
        {
        }
    }
}
