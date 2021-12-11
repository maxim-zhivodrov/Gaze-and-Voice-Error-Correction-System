using EyeGaze.Logger;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeGaze.SpeechToText
{
    class MicrosoftCloudSpeechToText : InterfaceSpeechToText
    {
        private SpeechRecognizer recognizer;
        private string keyinfo;
        public void connect(string key, string keyInfo)
        {
            keyinfo = keyInfo;
            //keyinfo = "eastus";
            var config = SpeechConfig.FromSubscription(key, keyInfo);
            recognizer = new SpeechRecognizer(config);
            SystemLogger.getEventLog().Info("Connect to Microsoft cloud");

        }

        public string listen()
        {
            try
            {
                Task<string> text = Task.Run(async () => await RecognizeSpeechAsync());
                text.Wait();
               // Task<string> text = RecognizeSpeechAsync();
                string result = text.Result;
                if (result != "")
                    result = result.Substring(0, result.Length - 1);    //remove the dot at the end of a sentence
                SystemLogger.getEventLog().Info("Microsoft cloud found : " + result);
                return result;
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        public void disconnect()
        {
            recognizer.Dispose();
            SystemLogger.getEventLog().Info("Disconnect from Microsoft cloud");
        }

        public async Task<string> RecognizeSpeechAsync()
        {
            var result = await recognizer.RecognizeOnceAsync();
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                return result.Text;
            }
             else if (result.Reason == ResultReason.Canceled)
             {
                 var cancellation = CancellationDetails.FromResult(result);

                 if (cancellation.Reason == CancellationReason.Error)
                 {
                    if (cancellation.ErrorCode == CancellationErrorCode.AuthenticationFailure)
                    {
                        SystemLogger.getErrorLog().Error("Microsoft cloud Authentication is Wrong");
                        throw new WrongAuthenticationException("Microsoft cloud Authentication is Wrong");
                    }
                    if (cancellation.ErrorCode == CancellationErrorCode.ConnectionFailure)
                    {
                        SystemLogger.getErrorLog().Error("Microsoft Speech to Text - connection to Internet failed");
                        throw new ConnectionFailedException("Microsoft - connection to Internet failed");
                    }
                 }
             }
            return "";


        }
    }
}
