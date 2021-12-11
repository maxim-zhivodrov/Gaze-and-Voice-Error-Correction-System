using EyeGaze.Engine;
using EyeGaze.Logger;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace EyeGaze.SpeechToText
{
    class SpeechToText
    {
        private InterfaceSpeechToText speechToText;
        public event TriggerWordHandler triggerHandler;
        public event TriggerHandlerMessage sendMessageToEngine;
        private bool terminate;
        private string className;
        private bool stopToSwitchCloud = false;
        string[] actions;
        private bool isReplaceAll = false;
        private string lastTriggerWord;

        public SpeechToText(string className)
        {
            this.className = className;
            Type speechToTextType = Type.GetType(className);
            speechToText = (InterfaceSpeechToText)Activator.CreateInstance(speechToTextType);
            actions = new string[] { "fix", "change", "add", "move", "replace", "done", "options", 
                "delete", "delete from", "copy" ,"copy from" ,"paste","paste before","paste after",
                "to","too","two","do","two,", "cancel",
                "1", "2", "3", "4", "5" };


            this.terminate = false;
        }
        public void FindActionFromSpeech(string key, string keyInfo)
        {
            try
            {
                speechToText.connect(key, keyInfo);
                while (!this.terminate && !stopToSwitchCloud)
                {
                    string result = speechToText.listen();
                    Debug.WriteLine("Enter while loop " + DateTime.Now.ToString("h:mm:ss tt") + "word " + result);
                    if (result != "")
                    {
                        result = result.Trim().ToLower();
                        Console.WriteLine(result);
                        string[] text = result.Split(' ');
                        TriggerWordEvent message = parseResult(text);
                        if (triggerHandler != null && message != null)
                        {
                            triggerHandler(this, message);
                        }
                    }
                }
            }
            catch (ConnectionFailedException e)
            {
                MessageEvent message = new MessageEvent();
                message.message = e.Message + " switch to system lib";
                message.type = MessageEvent.messageType.ConnectionFail;
                sendMessageToEngine(this, message);
                switchToSystemLib();
                Thread thread = new Thread(() => {
                    changeBackToCloud(key, keyInfo);
                });
                thread.Start();
                FindActionFromSpeech(key, keyInfo);
                if (stopToSwitchCloud && !this.terminate)
                {
                    this.stopToSwitchCloud = false;
                    this.speechToText.disconnect();
                    Type speechToTextType = Type.GetType(this.className);
                    this.speechToText = (InterfaceSpeechToText)Activator.CreateInstance(speechToTextType);
                    MessageEvent message2 = new MessageEvent();
                    message2.message = "Internet connection is back, switch to cloud speech to text";
                    message2.type = MessageEvent.messageType.ConnectionFail;
                    sendMessageToEngine(this, message2);
                    FindActionFromSpeech(key, keyInfo);
                }

                return;
            }
            catch (WrongAuthenticationException e)
            {
                MessageEvent message = new MessageEvent();
                message.message = e.Message;
                message.type = MessageEvent.messageType.WrongAuthentication;
                sendMessageToEngine(this, message);
                return;
            }
        }

        public TriggerWordEvent parseResult(string[] text)
        {
            try
            {
                text = GetSenteceWithoutPunctuation(text);
                if (text.Length > 0 && (actions.Contains(text[0]) || checkIfTriggerWordLevDisFromActions(text[0])))
                {
                    string triggerWord = text[0];
                    if ((triggerWord == "add" || triggerWord == "ed") && text.Length < 3)            // Add with less then two words after
                        return null;
                    if (triggerWord == "replace" && text.Length < 3)            // Replace with less then two words after
                        return null;
                    if (triggerWord == "replace" && text[1] == "all" && text.Length != 4)            // Replace with less then two words after
                        return null;
                    bool prevReplaceAll = isReplaceAll;
                    if (triggerWord == "done")
                    {
                        isReplaceAll = false;
                    }
                    if (triggerWord == "fix" && text.Length > 1)
                        triggerWord = "fix word";
                    if ((triggerWord == "delete" || checkLevinshteinDistance("delete", triggerWord)) && (text[1] == "from" || LevenshteinDistance(text[1], "from") <= 2))
                        triggerWord = "delete from";
                    else if ((triggerWord == "copy" || checkLevinshteinDistance("copy", triggerWord)) && (text[1] == "from" || LevenshteinDistance(text[1], "from") <= 2))
                        triggerWord = "copy from";
                    else if ((triggerWord == "paste" || triggerWord == "best" || checkLevinshteinDistance("paste", triggerWord))
                        && text[1] == "before")
                        triggerWord = "paste before";
                    else if ((triggerWord == "paste" || triggerWord == "best" || checkLevinshteinDistance("paste", triggerWord))
                        && text[1] == "after")
                        triggerWord = "paste after";
                    else if (checkLevinshteinDistance("add", triggerWord))
                        triggerWord = "add";

                    //in case of "delete from word1 to word2"
                    string[] toOptionsArray = { "to", "2", "two", "do", "too", "two," };
                    if (toOptionsArray.Contains(triggerWord) && lastTriggerWord == "delete from")
                        triggerWord = "to (delete)";
                    else if (toOptionsArray.Contains(triggerWord) && lastTriggerWord == "copy from")
                        triggerWord = "to (copy)";

                    if (!triggerWord.Contains("("))
                        lastTriggerWord = triggerWord;

                    TriggerWordEvent message = new TriggerWordEvent();
                    message.triggerWord = triggerWord;
                    string[] content = new string[text.Length - 1];
                    Array.Copy(text, 1, content, 0, text.Length - 1);
                    if (triggerWord == "delete from" || triggerWord == "copy from"
                        || triggerWord == "paste before" || triggerWord == "paste after")
                    {
                        string[] tmpContent = new string[content.Length - 1];
                        Array.Copy(content, 1, tmpContent, 0, content.Length - 1);
                        content = tmpContent;
                    }
                    else if (triggerWord == "add")
                    {
                        string[] tmpContenct = new string[content.Length - 1];
                        tmpContenct[0] = content[content.Length - 1];
                        int tmpContentIndex = 1;
                        for (int i = 0; i < content.Length; i++)
                        {
                            if (content[i].Equals("after")) break;
                            tmpContenct[tmpContentIndex] = content[i];
                            tmpContentIndex++;
                        }
                        content = tmpContenct;
                    }
                    message.content = content;
                    if (!(triggerWord == "done" && !prevReplaceAll))
                        sendMessage(triggerWord, content);
                    return message;
                }
                else if (text.Length > 0 && (text[1] == "before" || text[1] == "after") && LevenshteinDistance(text[0], "paste")<=3)
                {
                    string triggerWord = "paste " + text[1];
                    lastTriggerWord = triggerWord;

                    TriggerWordEvent message = new TriggerWordEvent();
                    message.triggerWord = triggerWord;
                    string[] content = new string[text.Length - 1];
                    Array.Copy(text, 1, content, 0, text.Length - 1);
                    string[] tmpContent = new string[content.Length - 1];
                    Array.Copy(content, 1, tmpContent, 0, content.Length - 1);
                    content = tmpContent;
                    message.content = content;
                    sendMessage(triggerWord, content);
                    return message;
                }
                return null;
            }
            catch(Exception e)
            {
                SystemLogger.getErrorLog().Error("Error in parse result" + e.Message);
                return null;
            }
            
        }

        private void sendMessage(String triggerWord, String[] content)
        {
            try
            {
                string sentence = String.Join(" ", content);
                if (triggerWord.Equals("fix word"))
                    triggerWord = "fix";
                MessageEvent message = new MessageEvent();
                message.message = triggerWord + " " + sentence;
                if (triggerWord == "move" || triggerWord == "change")
                    message.message = triggerWord;
                if (triggerWord == "fix" && content.Length > 1)
                    message.message = "fix " + content[0];
                if (triggerWord == "replace" && (content[0] == "all" || content[0] == "all,") && content.Length > 3)
                    message.message = "replace all " + content[1] + " " + content[2];
                else if (triggerWord == "replace" && content[0] != "all" && content.Length > 2)
                    message.message = "replace " + content[0] + " " + content[1];
                if (isReplaceAll)
                {
                    message.message = "Waiting for done trigger word";
                }
                message.type = MessageEvent.messageType.TriggerWord;
                sendMessageToEngine(this, message);
                if (triggerWord == "replace" && (content[0] == "all" || content[0] == "all,"))
                    isReplaceAll = true;
            }
            catch(Exception e)
            {
                SystemLogger.getErrorLog().Error("Exception in send message " + e.Message);
            }
        }

        public virtual void switchToSystemLib()
        {
            this.speechToText.disconnect();
            SystemLogger.getEventLog().Info("connect to system lib speech to text");
            Type speechToTextType = Type.GetType("EyeGaze.SpeechToText.SystemLibSpeechToText");
            speechToText = (InterfaceSpeechToText)Activator.CreateInstance(speechToTextType);
        }

        private bool checkConnection()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void changeBackToCloud(string key, string keyInfo)
        {
            while (!checkConnection())
            {
                Thread.Sleep(5000);
            }
            if (!this.terminate)
                stopToSwitchCloud = true;
        }


        public void disconnect()
        {
            speechToText.disconnect();
        }

        public void finishListen()
        {
            this.terminate = true;
        }

        private string[] GetSenteceWithoutPunctuation(string[] sentence)
        {
            for (int i = 0; i < sentence.Length; i++)
            {
                if (sentence[i].ElementAt(sentence[i].Length - 1) == ',' || sentence[i].ElementAt(sentence[i].Length - 1) == '.')
                {
                    sentence[i] = sentence[i].Substring(0, sentence[i].Length - 1);
                }
            }
            return sentence;
        }

        private int LevenshteinDistance(string left, string right)
        {
            left = left.ToLower();
            right = right.ToLower();

            if (left == null || right == null)
            {
                return -1;
            }

            if (left.Length == 0)
            {
                return right.Length;
            }

            if (right.Length == 0)
            {
                return left.Length;
            }

            int[,] distance = new int[left.Length + 1, right.Length + 1];

            for (int i = 0; i <= left.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= right.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= left.Length; i++)
            {
                for (int j = 1; j <= right.Length; j++)
                {
                    if (right[j - 1] == left[i - 1])
                    {
                        distance[i, j] = distance[i - 1, j - 1];
                    }
                    else
                    {
                        distance[i, j] = Math.Min(distance[i - 1, j], Math.Min(distance[i, j - 1], distance[i - 1, j - 1])) + 1;
                    }
                }
            }

            return distance[left.Length, right.Length];
        }

        private bool checkIfTriggerWordLevDisFromActions(string triggerWord)
        {
            foreach(string action in actions)
            {
                if (LevenshteinDistance(triggerWord, action) <= 2)
                    return true;
            }
            return false;
        }

        private bool checkLevinshteinDistance(string wordFromActions, string triggerWord)
        {
            return LevenshteinDistance(wordFromActions, triggerWord) <= 2 && !actions.Contains(triggerWord);
        }

    }


}
