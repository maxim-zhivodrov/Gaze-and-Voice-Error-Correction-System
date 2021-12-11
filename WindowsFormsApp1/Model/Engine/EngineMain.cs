using EyeGaze.SpeechToText;
using System;
using System.Collections.Generic;
using System.Linq;
using SpeechToTextClass = EyeGaze.SpeechToText.SpeechToText;
using System.Windows.Forms;
//using WindowsFormsApp1;
using TriggerWordEvent = EyeGaze.SpeechToText.TriggerWordEvent;
using System.Drawing;
using eyeGaze = EyeGaze.EyeTracking.MousePoint;
using EyeGaze.TextEditor;
using EyeGaze.SpellChecker;
using EyeGaze.Logger;
using System.Threading;
using EyeGaze.EyeTracking;
using System.IO;
using Microsoft.Win32;
using EyeGaze.GazeTracker;
using System.Timers;
using Experiment;

namespace EyeGaze.Engine
{
    public class EngineMain
    {
        //private List<String> fixing = null;
        private (List<String> list,int x,int y) fixing = (null,0,0);

        private AbstractTextEditor<CoordinateRange> textEditor;
        private SpellCheckerAbstract spellChecker;
        public event TriggerHandlerMessage messageToForm;
        private SpeechToTextClass speechToText;
        private ManualResetEvent completedEvent;
        private EyeGazeInterface eyeGaze;
        private CoordinateRange deleteLastCoordinate;
        private CoordinateRange copyLastCoordinate;
        private (String trigger, CoordinateRange prevCoord, String changed) lastOperation;
        public MainClass mainExperiment;

        //[System.Runtime.InteropServices.DllImport("DpiHelper.dll")]
        //static public extern void PrintDpiInfo();

        //[System.Runtime.InteropServices.DllImport("DpiHelper.dll")]
        //static public extern int SetDPIScaling(Int32 adapterIDHigh, UInt32 adapterIDlow, UInt32 sourceID, UInt32 dpiPercentToSet);
        //[System.Runtime.InteropServices.DllImport("DpiHelper.dll")]
        //static public extern void RestoreDPIScaling();


        [STAThread]
        static public void Main(String[] args)
        {
            //List<String> words = new List<string>();
            //words.Add("word one");
            //words.Add("word two");
            //words.Add("word three");
            //words.Add("word four");
            //words.Add("word five");
            //suggestionPopup sugg = new suggestionPopup(900,500,words);
            //sugg.Show();
            //sugg.TopMost = true;
            //Application.Run(sugg);

            
            EngineMain engine = new EngineMain();
            SystemLogger.getEventLog().Info("----------------------Starting System-------------------------");
            SystemLogger.getErrorLog().Info("----------------------Starting System-------------------------");
            //Application.Run(new Form1(engine));
            Controller c = new Controller(engine);
        }

        public EngineMain()
        {
            SystemLogger.configureLogs();
        }

        public void SetTextEditor(WordTextEditor textEditor)
        {
            this.textEditor = textEditor;

        }
        public void SetSpellChecker(NHunspellSpellChecker spellChecker)
        {
            this.spellChecker = spellChecker;
        }
        public void Start(string textEditorPath, string speechToTextNamespace, string key, string keyInfo, string eyeGazeNamespace, string spellChecker, int expnum=0)
        {
            GazeTracker.GazeTracker GT = GazeTracker.GazeTracker.getInstance();
            if(eyeGazeNamespace== "EyeGaze.EyeTracking.GazePoint" && !GT.initialized)
            {
                GT.connect();
                GT.listen();
            }
            completedEvent = new ManualResetEvent(false);
            SystemLogger.getEventLog().Info("Starting initialization of the system");
            Type eyeGazeType = Type.GetType(eyeGazeNamespace);
            this.eyeGaze = (EyeGazeInterface)Activator.CreateInstance(eyeGazeType);
            Type spellCheckerType = Type.GetType(spellChecker);
            this.spellChecker = (SpellCheckerAbstract)Activator.CreateInstance(spellCheckerType);
            Type textEditorType = Type.GetType("EyeGaze.TextEditor.WordTextEditor");
            this.textEditor = (AbstractTextEditor<CoordinateRange>)Activator.CreateInstance(textEditorType, textEditorPath);
            this.speechToText = new SpeechToTextClass(speechToTextNamespace);
            WireEventHandlersTriggerWord(speechToText);
            WireEventHandlersException(speechToText);
            WireEventHandlersCloseFile(this.textEditor);
            this.speechToText.FindActionFromSpeech(key, keyInfo);
            completedEvent.Set();
            this.End();
        }

        public void StartReg(string textEditorPath, string speechToTextNamespace, string key, string keyInfo, string eyeGazeNamespace, string spellChecker)
        {
            this.mainExperiment = new MainClass();
            completedEvent = new ManualResetEvent(false);
            SystemLogger.getEventLog().Info("Starting initialization of the system");
            Type textEditorType = Type.GetType("EyeGaze.TextEditor.WordTextEditor");
            this.textEditor = (AbstractTextEditor<CoordinateRange>)Activator.CreateInstance(textEditorType, textEditorPath);
            WireEventHandlersCloseFile(this.textEditor);
            completedEvent.Set();
        }

        public void setSpellChecker(string spellchecker)
        {
            if (spellchecker.Equals("Hunspell"))
            {
                this.spellChecker = new NHunspellSpellChecker();
            }
            else if (spellchecker.Equals("Word"))
            {
                this.spellChecker = new WordSpell();
            }
        }

        public void End()
        {
            this.finishListen();
            if(this.completedEvent != null)
            {
                this.completedEvent.WaitOne();
                this.completedEvent.Reset();
            }


            if(this.speechToText!=null) this.speechToText.disconnect();
            if (this.spellChecker != null) this.spellChecker.CloseSpellChecker();
            if (this.textEditor != null) this.textEditor.CloseFile();
        }

        public void finishListen()
        {
            if(this.speechToText != null)
                this.speechToText.finishListen();
        }
        private void WireEventHandlersTriggerWord(SpeechToTextClass stt)
        {
            TriggerWordHandler handler = new TriggerWordHandler(triggerWordHandler);
            stt.triggerHandler += handler;
        }
        private void WireEventHandlersException(SpeechToTextClass stt)
        {
            TriggerHandlerMessage handler = new TriggerHandlerMessage(handleMessageFromSpeechToText);
            stt.sendMessageToEngine += handler;
        }
        
        private void WireEventHandlersCloseFile(AbstractTextEditor<CoordinateRange> file)
        {
            TriggerHandlerMessage handler = new TriggerHandlerMessage(handleMessageFromTextEditor);
            file.sendMessageToEngine += handler;
        }

        public void handleMessageFromTextEditor(object sender, MessageEvent e)
        {
            if (e.type == MessageEvent.messageType.closeFile)
            {
                this.finishListen();
                if(messageToForm != null)
                    messageToForm(this, e);
                this.completedEvent.WaitOne();
                this.completedEvent.Reset();

            }
        }
        public void handleMessageFromSpeechToText(object sender, MessageEvent e)
        {
            try
            {
                if (e.type == MessageEvent.messageType.ConnectionFail || e.type == MessageEvent.messageType.WrongAuthentication)
                    messageToForm(this, e);
                else if (e.type == MessageEvent.messageType.TriggerWord)
                {
                    if (this.textEditor.fileReadOnly())
                        e.message = "Editing cannot be done because the file is read-only";
                    messageToForm(this, e);
                }

            }
            catch { }

        }

        public void triggerWordHandler(object sender, TriggerWordEvent e)
        {

            if (e.triggerWord.Equals("fix"))
                Fix(eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("fix word"))
                FixWord(e.content[1], eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("change"))
                Change();

            else if (e.triggerWord.Equals("move"))
                Move();

            else if (e.triggerWord.Equals("add"))
                Add(e.content, eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("replace") && (e.content[0].Equals("all") || e.content[0].Equals("all,")))
                ReplaceAll(e.content);

            else if (e.triggerWord.Equals("replace"))
                Replace(e.content, eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("done"))
                ReplaceAllDone();

            else if (e.triggerWord.Equals("options"))
                MoreSuggestions();

            else if (e.triggerWord.Equals("1") || e.triggerWord.Equals("2") || e.triggerWord.Equals("3") || e.triggerWord.Equals("4") || e.triggerWord.Equals("5"))
                FixFromSuggestions(e.triggerWord);

            else if (e.triggerWord.Equals("delete"))
                DeleteOneWord(e.content,eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("delete from"))
                DeleteFrom(e.content, eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("to (delete)"))
                DeleteFromTo(e.content, eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("copy from"))
                CopyFrom(e.content, eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Equals("to (copy)"))
                CopyFromTo(e.content, eyeGaze.GetEyeGazePosition());

            else if (e.triggerWord.Contains("paste"))
                Paste(e.content, e.triggerWord.Substring(e.triggerWord.IndexOf(' ')+1), eyeGaze.GetEyeGazePosition());
            else if (e.triggerWord.Equals("cancel"))
                Cancel();

        }

        private void Cancel()
        {
            try
            {
                if (lastOperation.trigger == "fix" || lastOperation.trigger == "fixTo" || lastOperation.trigger == "change"
                    || lastOperation.trigger == "replace" || lastOperation.trigger == "add" || lastOperation.trigger == "delete"||lastOperation.trigger=="paste")
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    CoordinateRange newCoords = new CoordinateRange(lastOperation.prevCoord.X, lastOperation.prevCoord.Y, lastOperation.prevCoord.range, lastOperation.changed);
                    textEditor.ReplaceWord(newCoords, lastOperation.prevCoord.word);
                    mainExperiment.EventCommand("cancel", "cancel", textBeforeChange, textEditor.getDoc().Content.Text, false,DateTime.Now);
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void Fix(Point position)
        {
            try
            {
                SystemLogger.getEventLog().Info("Trigger word Fix");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<CoordinateRange> misspelledWords = GetMisspelledWords(wordsInSight);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(misspelledWords, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                FixClosestMisspelledWord(sortedPoints);
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void DeleteOneWord(string[] sentence,Point position)
        {
            try
            {
                string wordToDelete = sentence[0];
                SystemLogger.getEventLog().Info("Trigger word Delete One Word");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                //textEditor.ReplaceWord(sortedPoints.First().Key,"");

                if (sortedPoints.Count > 0)
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, wordToDelete.ToLower());


                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(wordToDelete.ToLower()))
                        if (LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), wordToDelete.ToLower()) <= 2)
                        {
                            textEditor.HighlightWordForSpecificTime(wordToReplaceCoordinateRange, 3000);
                            Thread.Sleep(3000);
                            textEditor.ReplaceWord(wordToReplaceCoordinateRange, "");

                            //for cancel
                            lastOperation = ("delete", sortedPoints.First().Key, "");
                            string deletedWord = sortedPoints.First().Key.word;
                            mainExperiment.EventCommand("delete", "delete " + deletedWord, textBeforeChange, textEditor.getDoc().Content.Text, true, DateTime.Now);
                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }


                
            }

            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }
        public void Change()
        {
            try
            {
                SystemLogger.getEventLog().Info("Trigger word Change");
                List<CoordinateRange> wordsInSight = textEditor.GetCursorPositionWordsInArea();
                List<CoordinateRange> misspelledWords = GetMisspelledWords(wordsInSight);
                FixLatestMisspelledWord(misspelledWords);
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void FixWord(string word, Point position) 
        {
            try
            {
                SystemLogger.getEventLog().Info("Trigger word Fix + Word");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<CoordinateRange> similarWords = getSimilarLexicographicWords(wordsInSight, word);
                if (similarWords.Count == 0)
                    return;
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(similarWords, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    CoordinateRange wordToFix = sortedPoints.First().Key;
                    textEditor.ReplaceWord(wordToFix, word);

                    //for cancel
                    lastOperation = ("fixTo", wordToFix, word);
                    mainExperiment.EventCommand("fix to", "fix to " + wordToFix, textBeforeChange, textEditor.getDoc().Content.Text, true,DateTime.Now);
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void Move()
        {
            try { 
                SystemLogger.getEventLog().Info("Trigger word Move");
                Point position = eyeGaze.GetEyeGazePosition();
                textEditor.MoveCursor(position);
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void Add(string[] sentence, Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string firstWord = sentence[0];
                string wordsToAdd = GetSentceFromStringArray(sentence);
                SystemLogger.getEventLog().Info("Trigger word add");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<CoordinateRange> exactWords = GetExactWords(wordsInSight, firstWord);
                if (exactWords.Count == 0)
                    return;
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(exactWords, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                CoordinateRange firstPoint = sortedPoints.First().Key;
                wordsToAdd = wordsToAdd.Replace(firstWord, firstPoint.word);
                AddWords(firstPoint, wordsToAdd);
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void Replace(string[] sentence, Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string wordToReplace = sentence[0];
                string replaceToWord = sentence[2];
                SystemLogger.getEventLog().Info("Trigger word Replace");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, wordToReplace.ToLower());

                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(wordToReplace.ToLower()))
                        if (LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), wordToReplace.ToLower()) <= 2)
                        {
                            textEditor.ReplaceWord(wordToReplaceCoordinateRange, replaceToWord);

                            //for cancel
                            lastOperation = ("replace", wordToReplaceCoordinateRange, replaceToWord);
                            mainExperiment.EventCommand("replace", "replace " + wordToReplace + " to " + replaceToWord, textBeforeChange, textEditor.getDoc().Content.Text,
                                true, DateTime.Now);
                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }
        private void DeleteFrom(string[] sentence, Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string startWord = sentence[0];
                SystemLogger.getEventLog().Info("Trigger word Delete From");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, startWord.ToLower());

                        
                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(startWord.ToLower()))
                        if (LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), startWord.ToLower()) <= 2)
                        {
                            deleteLastCoordinate = wordToReplaceCoordinateRange;
                            int timeHighlighted = 2000;
                            textEditor.HighlightWordForSpecificTime(wordToReplaceCoordinateRange, timeHighlighted);
                            //Thread.Sleep(timeHighlighted);
                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        private void DeleteFromTo(string[] sentence, Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string stopWord = sentence[0];
                SystemLogger.getEventLog().Info("Trigger word Delete From To");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, stopWord.ToLower());


                        
                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(stopWord.ToLower()))
                        if (LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), stopWord.ToLower()) <= 2)
                        {

                            deleteLastCoordinate.range.End = wordToReplaceCoordinateRange.range.End;
                            CoordinateRange wholeSentenceCoordinates = new CoordinateRange(deleteLastCoordinate.X, deleteLastCoordinate.Y, deleteLastCoordinate.range, deleteLastCoordinate.range.Text);
                            int timeHighlighted = 3000;
                            textEditor.HighlightWordForSpecificTime(wholeSentenceCoordinates, timeHighlighted);
                            Thread.Sleep(timeHighlighted);


                            CoordinateRange deleted=textEditor.DeleteSentence(deleteLastCoordinate, wordToReplaceCoordinateRange);

                            //for cancel
                            lastOperation = ("delete", deleted, "");
                            mainExperiment.EventCommand("delete from to", "delete from " + deleteLastCoordinate.word + " to " + stopWord, 
                                textBeforeChange, textEditor.getDoc().Content.Text, true, DateTime.Now);
                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void CopyFrom(string[] sentence, Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string startWord = sentence[0];
                SystemLogger.getEventLog().Info("Trigger word Copy From");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, startWord.ToLower());


                        
                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(startWord.ToLower()))
                        if(LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), startWord.ToLower()) <= 2)
                        {
                            if (((WordTextEditor)this.textEditor).isSentenceHighlighted)
                                ((WordTextEditor)this.textEditor).StopHightlightLastCopiedSentece();
                            copyLastCoordinate = wordToReplaceCoordinateRange;
                            int timeHighlighted = 2000;
                            textEditor.HighlightWordForSpecificTime(wordToReplaceCoordinateRange, timeHighlighted);
                            //Thread.Sleep(timeHighlighted);
                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }

        }

        private void CopyFromTo(string[] sentence, Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string stopWord = sentence[0];
                SystemLogger.getEventLog().Info("Trigger word Copy From To");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, stopWord.ToLower());

                        
                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(stopWord.ToLower()))
                        if(LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), stopWord.ToLower()) <= 2)
                        {
                            CoordinateRange copied = textEditor.SaveSentence(copyLastCoordinate, wordToReplaceCoordinateRange);
                            textEditor.HighlightLastCopiedSentence(copied);
                            mainExperiment.EventCommand("copy from to", "copy from " + copyLastCoordinate.word + " to " + stopWord, 
                                textBeforeChange, textEditor.getDoc().Content.Text, true,DateTime.Now);
                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void Paste(string[] sentence, string pastePlacement ,Point position)
        {
            try
            {
                sentence = GetSenteceWithoutPunctuation(sentence);
                string stopWord = sentence[0];
                SystemLogger.getEventLog().Info("Trigger word Paste");
                List<CoordinateRange> wordsInSight = textEditor.GetAllWordsInArea(position);
                List<KeyValuePair<CoordinateRange, double>> distanceFromCoordinate = FindDistanceFromCoordinate(wordsInSight, position).ToList();
                List<KeyValuePair<CoordinateRange, double>> sortedPoints = SortByDistance(distanceFromCoordinate);
                if (sortedPoints.Count > 0)
                {
                    string textBeforeChange = textEditor.getDoc().Content.Text;
                    while (sortedPoints.Count > 0)
                    {
                        CoordinateRange wordToReplaceCoordinateRange = getLevDistanceClosest(sortedPoints, stopWord.ToLower());

                        
                        //if (wordToReplaceCoordinateRange.word.ToLower().Equals(stopWord.ToLower()))
                        if(LevenshteinDistance(wordToReplaceCoordinateRange.word.ToLower(), stopWord.ToLower()) <= 2)
                        {
                            //textEditor.HighlightWordForSpecificTime(wordToReplaceCoordinateRange, 1000);
                            //textEditor.SaveSentence(copyLastCoordinate, wordToReplaceCoordinateRange);
                            CoordinateRange pasted= textEditor.PasteSentence(wordToReplaceCoordinateRange, pastePlacement);
                            textEditor.StopHightlightLastCopiedSentece();

                            //for cancel
                            lastOperation = ("paste", pasted, wordToReplaceCoordinateRange.word);
                            if (pastePlacement.Equals("before"))
                                mainExperiment.EventCommand("paste", "paste " + pastePlacement + " " + stopWord, textBeforeChange, textEditor.getDoc().Content.Text, true, DateTime.Now);
                            else if (pastePlacement.Equals("after"))
                                mainExperiment.EventCommand("paste", "paste " + pastePlacement + " " + stopWord, textBeforeChange, textEditor.getDoc().Content.Text, true, DateTime.Now);

                            return;
                        }
                        return;
                        sortedPoints.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void ReplaceAll(string[] sentence)
        {
            try
            {
                if (sentence.Length < 3)
                {
                    SystemLogger.getErrorLog().Info("Trying to replace all, there are not enough words in the command");
                    return;
                }
                sentence = GetSenteceWithoutPunctuation(sentence);
                string wordToReplace = sentence[1];
                string replaceToWord = sentence[2];
                SystemLogger.getEventLog().Info("Trigger word ReplaceAll");
                List<CoordinateRange> wordsCoordinateRanges = textEditor.GetRangesForAllSameWords(wordToReplace);
                textEditor.ReplaceAllWords(wordsCoordinateRanges, wordToReplace, replaceToWord);
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void ReplaceAllDone()
        {
            try
            {
                textEditor.ReplaceAllDone();
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }

        public void MoreSuggestions()
        {
            try
            {
                new Thread(() =>
                {
                    textEditor.ShowMoreSuggestions();

                }).Start();
                //textEditor.ShowMoreSuggestions();
                textEditor.choosingSuggestion = true;
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
            //if (fixing.list != null && fixing.list.Count>0)
            //{
            //    suggestionPopup sp = new suggestionPopup(fixing.x - 90, fixing.y - 50, fixing.list);
            //    sp.Refresh();
            //    sp.Show();
            //    sp.TopMost = true;
            //    Application.DoEvents();
            //}
        }

        public void FixFromSuggestions(String trigger)
        {
            if (!textEditor.choosingSuggestion) return;
            //List<string> numbers = new List<string>();
            //numbers.Add("zero");
            //numbers.Add("one");
            //numbers.Add("two");
            //numbers.Add("three");
            //numbers.Add("four");
            //numbers.Add("five");
            //int index = numbers.IndexOf(trigger);
            string textBeforeChange = textEditor.getDoc().Content.Text;
            int index= Int32.Parse(trigger)-1;
            String fixedWord = textEditor.fixedWord.list[index];
            textEditor.ReplaceWord(textEditor.fixedWord.coord, fixedWord.Trim());
            textEditor.HideMoreSuggestions();
            mainExperiment.EventCommand("options", "options " + trigger, textBeforeChange, textEditor.getDoc().Content.Text,
                false, DateTime.Now);
            //textEditor.FixFromSuggestions(index);

        }

        private string[] GetSenteceWithoutPunctuation(string[] sentence)
        {
            for(int i=0; i < sentence.Length; i++)
            {
                if(sentence[i].ElementAt(sentence[i].Length - 1) == ',' || sentence[i].ElementAt(sentence[i].Length - 1) == '.')
                {
                    sentence[i] = sentence[i].Substring(0, sentence[i].Length - 1);
                }
            }
            return sentence;
        }

        private string GetSentceFromStringArray(string[] array)
        {
            string str = "";
            for (int i = 0; i < array.Length; i++)
            {
                str += array[i] + " ";
            }

            return str.Trim();
        }

        private void FixClosestMisspelledWord(List<KeyValuePair<CoordinateRange, double>> sortedMisspelledWordsByDistance)
        {
            if (sortedMisspelledWordsByDistance.Count > 0)
            {
                string textBeforeChange = textEditor.getDoc().Content.Text;
                CoordinateRange wordToFix = sortedMisspelledWordsByDistance.First().Key;
                List<string> suggestions = spellChecker.GetSpellingSuggestions(wordToFix.word);
                if (suggestions.Count > 0)
                {
                    String fixedWord = suggestions.First().Trim();
                    textEditor.ReplaceWord(wordToFix, suggestions.First().Trim());
                    suggestions.RemoveAt(0);
                    //fixing = (suggestions, wordToFix.X, wordToFix.Y);
                    textEditor.fixedWord= (suggestions, wordToFix);

                    //for cancel
                    lastOperation = ("fix", wordToFix, fixedWord);
                    mainExperiment.EventCommand("fix", "fix", textBeforeChange, textEditor.getDoc().Content.Text,true, DateTime.Now);
                }
                return;
            }
            SystemLogger.getEventLog().Info("No misspelled word found close to eye gaze");
        }

        private void AddWords(CoordinateRange firstWordCoordinate, string wordsToAdd)
        {
            string textBeforeChange = textEditor.getDoc().Content.Text;
            textEditor.ReplaceWord(firstWordCoordinate, wordsToAdd);

            //for cancel
            lastOperation = ("add", firstWordCoordinate, wordsToAdd);

            String[] wordsToAddArray = wordsToAdd.Split();
            String[] wordsForCommand = new string[wordsToAddArray.Length + 1];

            for (int i = 1, index = 0; i < wordsToAddArray.Length; i++, index++)
                wordsForCommand[index] = wordsToAddArray[i];

            wordsForCommand[wordsForCommand.Length - 2] = "after";
            wordsForCommand[wordsForCommand.Length - 1] = wordsToAddArray[0];
            mainExperiment.EventCommand("add", "add " + String.Join(" ", wordsForCommand), textBeforeChange, textEditor.getDoc().Content.Text, true, DateTime.Now);
        }

        private void FixLatestMisspelledWord(List<CoordinateRange> misspelledWords)
        {
            if (misspelledWords.Count > 0)
            {
                CoordinateRange wordToFix = misspelledWords.Last();
                List<string> suggestions = spellChecker.GetSpellingSuggestions(wordToFix.word);
                if (suggestions.Count > 0)
                {
                    textEditor.ReplaceWord(wordToFix, suggestions.First());

                    //for cancel
                    lastOperation = ("change", wordToFix, suggestions.First());
                }
                return;
            }
            SystemLogger.getEventLog().Info("No misspelled word found close to cursor");
        }
        public List<CoordinateRange> GetMisspelledWords(List<CoordinateRange> allWords)
        {
            List<CoordinateRange> misspelledWords = new List<CoordinateRange>();
            foreach (CoordinateRange word in allWords)
            {
                if (spellChecker.IsMisspelled(word.word))
                {
                    misspelledWords.Add(word);
                }
            }
            return misspelledWords;
        }

        public List<CoordinateRange> GetExactWords(List<CoordinateRange> allWords, string firstWord)
        {
            List<CoordinateRange> sameWord = new List<CoordinateRange>();
            Dictionary<CoordinateRange, int> disDict = new Dictionary<CoordinateRange, int>();
            foreach (CoordinateRange word in allWords)
            {
                int levDistance = this.LevenshteinDistance(word.word.ToLower(), firstWord.ToLower());
                if(levDistance == 0)
                {
                    sameWord.Add(word);
                    return sameWord;
                }
                disDict.Add(word, levDistance);
                //if (LevenshteinDistance(word.word.ToLower(), firstWord.ToLower()) <= 2)
                ////if (word.word.ToLower().Equals(firstWord.ToLower()))
                //{
                //    sameWord.Add(word);
                //}
            }
            sameWord.Add(disDict.Aggregate((l, r) => l.Value < r.Value ? l : r).Key);
            return sameWord;
        }

        public Dictionary<CoordinateRange, double> FindDistanceFromCoordinate(List<CoordinateRange> misspelledWords, Point coordinate)
        {
            Dictionary<CoordinateRange, double> result = new Dictionary<CoordinateRange, double>();
            foreach (CoordinateRange word in misspelledWords)
            {
                double distance = Math.Sqrt(Math.Pow(coordinate.X - word.X, 2) + Math.Pow(coordinate.Y - word.Y, 2));
                if(distance < 170)
                    result.Add(word, distance);
            }
            return result;
        }

        public List<KeyValuePair<T1, T2>> SortByDistance<T1, T2>(List<KeyValuePair<T1, T2>> WordsAndDistance) where T2 : IComparable
        {
            WordsAndDistance.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            return WordsAndDistance;
        }

        public List<CoordinateRange> getSimilarLexicographicWords(List<CoordinateRange> wordsInRange, string word)
        {
            List<CoordinateRange> result = new List<CoordinateRange>();
            List<KeyValuePair<CoordinateRange, int>> similarWordsAndDistance = new List<KeyValuePair<CoordinateRange, int>>();
            foreach (CoordinateRange wordInRange in wordsInRange)
            {
                if (!IsPunctuation(wordInRange.word))
                    similarWordsAndDistance.Add(new KeyValuePair<CoordinateRange, int>(wordInRange, LevenshteinDistance(wordInRange.word, word)));
            }
            similarWordsAndDistance = SortByDistance(similarWordsAndDistance);
            int i = 0;
            while (i< similarWordsAndDistance.Count && similarWordsAndDistance[i].Value == 0)
                i++;
            if (i == similarWordsAndDistance.Count)
                return result;
            int min = similarWordsAndDistance[i].Value;
            if (min <= 3)
            {
                foreach (KeyValuePair<CoordinateRange, int> pair in similarWordsAndDistance)
                    if (pair.Value == min)
                        result.Add(pair.Key);
            }
            return result;
        }

        public int LevenshteinDistance(string left, string right)
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
        private Boolean IsPunctuation(string word)
        {
            if (word.Equals("\r") || word.Equals("\t") || word.Equals("\n"))
            {
                return true;
            }
            return false;
        }

        private CoordinateRange getLevDistanceClosest(List<KeyValuePair<CoordinateRange, double>> sortedPoints, string compareToWord)
        {
            Dictionary<CoordinateRange, int> disDict = new Dictionary<CoordinateRange, int>();
            foreach(KeyValuePair<CoordinateRange, double> pair in sortedPoints)
            {
                string candidateWord = pair.Key.word;
                int levDistance = this.LevenshteinDistance(compareToWord, candidateWord);
                if (levDistance == 0) return pair.Key;
                disDict.Add(pair.Key, levDistance);
            }


            return disDict.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
        }

    }
}
