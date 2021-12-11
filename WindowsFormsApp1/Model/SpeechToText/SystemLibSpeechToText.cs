using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Google.Cloud.Dialogflow.V2;
using System.Speech.Recognition;
using System.Globalization;
using System.Threading;
using EyeGaze.SpeechToText;
using EyeGaze.Logger;

namespace EyeGaze.SpeechToText
{
    class SystemLibSpeechToText : InterfaceSpeechToText
    {
        private ManualResetEvent completedEvent;
        private string result;
        private SpeechRecognitionEngine recognizer;

        public SystemLibSpeechToText()
        {
            completedEvent = null;
            result = "";
        }

        public void connect(string key, string keyInfo)
        {

            SystemLogger.getEventLog().Info("connect to system lib");
            RecognizerInfo checkEnglishInfo = null;
            foreach (RecognizerInfo ri in SpeechRecognitionEngine.InstalledRecognizers()) //search installed language in the pc
            {
                if (ri.Culture.Name.Equals("en-US"))
                {
                    checkEnglishInfo = ri;
                    break;
                }
            }
            if (checkEnglishInfo == null)
                SystemLogger.getErrorLog().Error("system lib speech to text - there is no english on the computer. for more information - https://stackoverflow.com/questions/33281858/speechrecognitionengine-recognizers");

            completedEvent = new ManualResetEvent(false);
            CultureInfo info = new CultureInfo("en-US");
            recognizer = new SpeechRecognitionEngine(info);
            recognizer.SetInputToDefaultAudioDevice();

            Choices choice1 = new Choices();
            choice1.Add(new string[] { "fix", "add", "replace" }); //trigger words with optional words after them
            GrammarBuilder triggerWordWithContinue = new GrammarBuilder(choice1);
            triggerWordWithContinue.AppendDictation(); //add all the possible words
            triggerWordWithContinue.Culture = info;

            Choices choice2 = new Choices();
            choice2.Add(new string[] { "change", "fix", "move", "done"}); 
            GrammarBuilder triggerWordWithoutContinue = new GrammarBuilder(choice2);
            triggerWordWithoutContinue.Culture = info;

            Choices final = new Choices(new GrammarBuilder[] { triggerWordWithoutContinue, triggerWordWithContinue });
            GrammarBuilder finalGrammarBulider = new GrammarBuilder(final);
            finalGrammarBulider.Culture = info;
            Grammar finalGrammar = new Grammar(finalGrammarBulider);
            recognizer.LoadGrammar(finalGrammar);
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
        }

        private void rec(object sender, SpeechRecognizedEventArgs e)
        {
            result = e.Result.Text;
            completedEvent.Set();
        }

        public string listen()
        {
            completedEvent.WaitOne(); // wait until speech recognition is completed
            completedEvent.Reset();
            SystemLogger.getEventLog().Info("system lib found : " + result);
            return result;
            
        }


        public void disconnect()
        {
            SystemLogger.getEventLog().Info("disconnect from system lib");
            recognizer.Dispose(); // dispose the speech recognition 
        }
    }
}
