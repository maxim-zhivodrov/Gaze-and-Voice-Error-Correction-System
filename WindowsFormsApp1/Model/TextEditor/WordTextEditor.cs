using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Word;
using System.Threading;
using EyeGaze.Logger;
using Application = Microsoft.Office.Interop.Word.Application;
using EyeGaze.Engine;
using System.IO;
using System.Windows.Forms;

namespace EyeGaze.TextEditor
{
    public struct CoordinateRange
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Range range { get; set; }
        public string word { get; set; }

        public CoordinateRange(int x, int y, Range r, string w)
        {
            X = x;
            Y = y;
            range = r;
            word = w;
        }
    }
    public class WordTextEditor : AbstractTextEditor<CoordinateRange>
    {
        public Application application;
        Document document;
        Window window;
        private Range cursorPosition;
        private bool fileIsOpen;
        private string replaceAllOldWord;
        private string replaceAllNewWord;
        private List<CoordinateRange> replaceAllWordsToChange;
        private bool isReplaceAll;
        private string lastCopiedSentence;
        private (Range range, WdColorIndex color) lastCopiedSentenceInfo = (null, 0);
        public bool isSentenceHighlighted = false;

        public WordTextEditor(string path)
        {
            try
            {
                application = new Application();
                SystemLogger.getEventLog().Info("Trying to open documtent");
                application.DocumentBeforeClose += new ApplicationEvents4_DocumentBeforeCloseEventHandler(word_DocumentBeforeClose);
                application.Visible = true;
                application.OpenAttachmentsInFullScreen = true;
                document = application.Documents.Open(path);
                window = application.ActiveWindow;
                //application.ActiveWindow.View.FullScreen = true;
                //document.ActiveWindow.View.FullScreen = true;
                SystemLogger.getEventLog().Info("Word text editor has been initialized and document is open");
                fileIsOpen = true;
                isReplaceAll = false;
                lastCopiedSentence = "";


            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Error(String.Format("Word text editor did not open successfully {0}", e.Message));
            }
        }

        private Boolean IsSeperator(char c)
        {
            if (Char.IsPunctuation(c) || c == ' ' || c == '\n' || c == '\r' || c == '\t')
                return true;
            return false;
        }

        public Document GetDocument()
        {
            return this.document;
        }
        public Application GetApplication()
        {
            return this.application;
        }
        public override void SetCursorPosition(Range cursorPosition)
        {
            this.cursorPosition = cursorPosition;
        }
        /// <summary>
        /// The range of the specific point is a dot on the screen.
        /// To get the text surrounding the dot we will need to move
        /// the start point and the end point to get a wider range
        /// In that range we will look for the closest word
        /// </summary>
        /// <param name="cursorPosition"></param>
        /// <returns></returns>
        public List<Range> getRangeFromPoint(System.Drawing.Point cursorPosition)
        {
            Range rangeOrg = (Range)window.RangeFromPoint(cursorPosition.X, cursorPosition.Y);
            Range rangeDown = (Range)window.RangeFromPoint(cursorPosition.X, cursorPosition.Y + 30);
            Range rangeUp = (Range)window.RangeFromPoint(cursorPosition.X, cursorPosition.Y - 30);
            List<Range> ranges = new List<Range>();
            ranges.Add(rangeOrg);
            ranges.Add(rangeDown);
            ranges.Add(rangeUp);

            foreach (Range r in ranges)
            {
                r.Start = r.Start - 10;
                r.End = r.End + 10;
            }
            //int start = rangeFromPoint.Start;
            //rangeFromPoint.Start = start - 40;
            //rangeFromPoint.MoveEnd(WdUnits.wdWord, 5);
            return ranges;
        }

        public Range getRangeFromCursor(System.Drawing.Point cursorPosition)
        {
            Range rangeFromPoint = (Range)window.RangeFromPoint(cursorPosition.X, cursorPosition.Y);
            int start = rangeFromPoint.Start;
            rangeFromPoint.Start = start - 170;
            return rangeFromPoint;
        }
        public override List<CoordinateRange> GetCursorPositionWordsInArea()
        {
            SetCursorPosition(GetTextEditorCursorRange().Duplicate);
            Range cursorPositionExpanded = cursorPosition.Duplicate;
            int start = cursorPositionExpanded.Start;
            cursorPositionExpanded.Start = start - 170;
            List<CoordinateRange> wordsInRange = GetAllWordsInRange(cursorPositionExpanded);
            if (wordsInRange[wordsInRange.Count - 1].range.End == cursorPositionExpanded.End)
                wordsInRange.RemoveAt(wordsInRange.Count - 1);
            return wordsInRange;
        }

        public Range GetTextEditorCursorRange()
        {
            return application.Selection.Range;
        }

        public override System.Drawing.Point GetPointOfCursorPosition()
        {
            int left, top, width, height;
            window.GetPoint(out left, out top, out width, out height, cursorPosition);
            System.Drawing.Point point = new System.Drawing.Point(left, top);
            return point;
        }

        private Tuple<int, int> RangeOfTableOfContent()
        {
            int minRange = 100000000;
            int maxRange = -1;
            foreach (TableOfContents toc in document.TablesOfContents)
            {
                if (toc.Range.Start < minRange)
                {
                    minRange = toc.Range.Start;
                }
                if (toc.Range.End > maxRange)
                {
                    maxRange = toc.Range.End;
                }
            }
            return Tuple.Create(minRange, maxRange);
        }
        public override List<CoordinateRange> GetRangesForAllSameWords(string word)
        {
            Range documentRange = document.Range(0, 1);
            documentRange.WholeStory();
            int lastChar = documentRange.End;
            List<CoordinateRange> coordinateRanges = new List<CoordinateRange>();
            Range range = documentRange.Duplicate;
            Tuple<int, int> tableOfContentRange = RangeOfTableOfContent();
            bool existsTableOfContent = tableOfContentRange.Item2 > 0 ? true : false;

            while (range.End != range.Start)
            {
                range.Find.Execute(word);
                // words inside the table of content cannot be replaced
                if (existsTableOfContent && range.Start >= tableOfContentRange.Item1 && range.End <= tableOfContentRange.Item2)
                {
                    range.Start = tableOfContentRange.Item2 + 1;
                    range.End = lastChar;
                    continue;
                }
                if (range.Start == documentRange.Start && range.End == documentRange.End) // the word does not exist in the text
                {
                    return coordinateRanges;
                }
                Range checkRange = range.Duplicate;
                bool entireWord;
                if (checkRange.Start == 0)
                {
                    checkRange.MoveEnd(WdUnits.wdCharacter, 1);
                    string wordInRange = checkRange.Text;
                    entireWord = Char.IsLetterOrDigit(wordInRange[wordInRange.Length - 1]) ? false : true;
                }
                else if (checkRange.End == lastChar)
                {
                    checkRange.MoveStart(WdUnits.wdCharacter, -1);
                    string wordInRange = checkRange.Text;
                    entireWord = Char.IsLetterOrDigit(wordInRange[0]) ? false : true;
                }
                else
                {
                    checkRange.MoveStart(WdUnits.wdCharacter, -1);
                    checkRange.MoveEnd(WdUnits.wdCharacter, 1);
                    string wordInRange = checkRange.Text;
                    entireWord = Char.IsLetterOrDigit(wordInRange[0]) || Char.IsLetterOrDigit(wordInRange[wordInRange.Length - 1]) ? false : true;
                }
                if (!entireWord)
                {
                    documentRange.Start = range.End;
                    range = documentRange.Duplicate;
                    continue;
                }
                try
                {
                    coordinateRanges.Add(new CoordinateRange(0, 0, range, range.Text));
                    documentRange.Start = range.End;
                }
                catch (Exception)
                {
                }
                range = documentRange.Duplicate;
            }
            return coordinateRanges;
        }
        public override void CloseFile()
        {
            try
            {
                if (fileIsOpen)
                {
                    document.Save();
                    document.Close();
                    application.Quit(false);
                    SystemLogger.getEventLog().Info("Document is successfully closed");
                    fileIsOpen = false;
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Error(e.Message);
            }
        }

        public override List<CoordinateRange> GetAllWordsInArea(System.Drawing.Point position)
        {
            List<Range> ranges = getRangeFromPoint(position);
            List<CoordinateRange> cr = new List<CoordinateRange>();
            Dictionary<(int, int), CoordinateRange> dict = new Dictionary<(int, int), CoordinateRange>();
            foreach (Range r in ranges)
            {
                List<CoordinateRange> currRange = GetAllWordsInRange(r);
                //cr.AddRange(currRange);
                foreach (CoordinateRange coor in currRange)
                {
                    if (!dict.ContainsKey((coor.X, coor.Y)))
                    {
                        dict.Add((coor.X, coor.Y), coor);
                    }
                }
            }
            List<CoordinateRange> result = new List<CoordinateRange>(dict.Values);
            return result;
        }

        public override List<CoordinateRange> GetAllWordsInRange(Range range)
        {
            int left, top, width, height;
            List<CoordinateRange> result = new List<CoordinateRange>();
            int wordsInRangeCount = range.Words.Count;
            for (int i = 1; i <= wordsInRangeCount; i++)
            {
                Range wordRange = range.Words[i];
                string word = range.Words[i].Text;
                word = word.Replace(" ", "");
                wordRange.End = wordRange.Start + word.Length;
                try
                {
                    char c = Convert.ToChar(word);
                    if (!Char.IsPunctuation(c))
                    {
                        window.GetPoint(out left, out top, out width, out height, wordRange);
                        result.Add(new CoordinateRange(left + width / 2, top, wordRange, word));
                    }
                }
                catch (FormatException)
                {
                    window.GetPoint(out left, out top, out width, out height, wordRange);
                    result.Add(new CoordinateRange(left + width / 2, top, wordRange, word));
                }
                catch (Exception) { }
            }
            return result;
        }
        public override void MoveCursor(System.Drawing.Point position)
        {
            try
            {
                Range rangeFromPoint = (Range)window.RangeFromPoint(position.X, position.Y);
                document.Range(rangeFromPoint.Start, rangeFromPoint.End).Select();
                SystemLogger.getEventLog().Info(String.Format("Cursor has successfully moved to point: x={0} y={1} range: {2}",
                    position.X, position.Y, rangeFromPoint));
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Info(e.Message);
            }
        }
        private bool IsUpper(string str)
        {
            foreach (Char c in str)
            {
                if (c <= 'A' || c >= 'Z')
                    return false;
            }
            return true;
        }
        public override void ReplaceWord(CoordinateRange wordToChange, string newWord)
        {
            if (isReplaceAll)
                return;
            if(wordToChange.word.Length > 0)
            {
                if (IsUpper(wordToChange.word))
                {
                    newWord = newWord.ToUpper();
                }
                else if ((wordToChange.word[0] >= 'A' && wordToChange.word[0] <= 'Z')&&newWord.Length!=0) // first letter is upper case
                {
                    newWord = newWord[0].ToString().ToUpper() + newWord.Substring(1);
                }
            }
            wordToChange.range.Text = newWord;
            wordToChange.range.End = wordToChange.range.Start + newWord.Length;
            Range rangeToHighlight = wordToChange.range.Duplicate;
            Thread thread = new Thread(() => HighlightWord(rangeToHighlight));
            thread.Start();
            SystemLogger.getEventLog().Info(String.Format("Changed {0} to {1} in point x={2} y={3} range start={4} end={5}",
                wordToChange.word, newWord, wordToChange.X, wordToChange.Y, wordToChange.range.Start, wordToChange.range.End));
        }
        private void word_DocumentBeforeClose(Document Doc, ref bool Cancel)
        {
            MessageEvent message = new MessageEvent();
            message.message = "close word";
            message.type = MessageEvent.messageType.closeFile;
            fileIsOpen = false;
            base.CloseFile(message);
            if (!document.ReadOnly)
                document.Save();
        }

        private void oWord_WindowBeforeDoubleClick(Selection sel, ref bool Cancel)
        {
            if (!isReplaceAll)
                return;
            Range range = sel.Range;
            Range dupRange = sel.Range.Duplicate;
            dupRange.Start -= 1;
            string textInRange = dupRange.Text;
            while (range.Start > 0 && !IsSeperator(textInRange[0]))
            {
                range.Start -= 1;
                dupRange.Start -= 1;
                textInRange = dupRange.Text;
            }
            dupRange.End += 1;
            Range documentRange = document.Range(0, 1);
            documentRange.WholeStory();
            int lastChar = documentRange.End;
            while (range.End <= lastChar && !IsSeperator(textInRange[textInRange.Length - 1]))
            {
                range.End += 1;
                dupRange.End += 1;
                textInRange = dupRange.Text;
            }
            if (!range.HighlightColorIndex.Equals(WdColorIndex.wdYellow))
                return;
            foreach (CoordinateRange wordToChange in replaceAllWordsToChange)
            {
                if (wordToChange.range.Start == range.Start && wordToChange.range.End == range.End)
                {
                    string str = replaceAllOldWord;
                    if (wordToChange.word[0] >= 'A' && wordToChange.word[0] <= 'Z') // first letter is upper case
                    {
                        str = str[0].ToString().ToUpper() + str.Substring(1);
                    }
                    wordToChange.range.Text = str;
                    wordToChange.range.End = wordToChange.range.Start + str.Length;
                    Range rangeToHighlight = wordToChange.range.Duplicate;
                    rangeToHighlight.HighlightColorIndex = WdColorIndex.wdNoHighlight;
                    SystemLogger.getEventLog().Info(String.Format("Changed Back {0} to {1} in point x={2} y={3} range start={4} end={5}",
                        wordToChange.word, str, wordToChange.X, wordToChange.Y, wordToChange.range.Start, wordToChange.range.End));
                }
            }
            SystemLogger.getEventLog().Info(String.Format("double clicked " + range.Text));
        }
        public override void ReplaceAllWords(List<CoordinateRange> wordsToChange, string oldWord, string newWord)
        {
            if (isReplaceAll)
                return;
            isReplaceAll = true;
            this.replaceAllWordsToChange = wordsToChange;
            replaceAllNewWord = newWord;
            replaceAllOldWord = oldWord;
            application.WindowBeforeDoubleClick +=
            new ApplicationEvents4_WindowBeforeDoubleClickEventHandler(
               oWord_WindowBeforeDoubleClick);
            foreach (CoordinateRange wordToChange in wordsToChange)
            {
                string str = newWord;
                if (IsUpper(wordToChange.word))
                {
                    newWord = newWord.ToUpper();
                }
                else if (wordToChange.word[0] >= 'A' && wordToChange.word[0] <= 'Z') // first letter is upper case
                {
                    str = str[0].ToString().ToUpper() + str.Substring(1);
                }
                wordToChange.range.Text = str;
                wordToChange.range.End = wordToChange.range.Start + str.Length;
                Range rangeToHighlight = wordToChange.range.Duplicate;
                rangeToHighlight.HighlightColorIndex = WdColorIndex.wdYellow;
                SystemLogger.getEventLog().Info(String.Format("Changed {0} to {1} in point x={2} y={3} range start={4} end={5}",
                    wordToChange.word, str, wordToChange.X, wordToChange.Y, wordToChange.range.Start, wordToChange.range.End));
            }
        }

        public void HighlightWord(Range rangeToHighlight)
        {
            WdColorIndex prevColor = rangeToHighlight.HighlightColorIndex;
            if (prevColor == WdColorIndex.wdYellow)
                prevColor = WdColorIndex.wdWhite;
            rangeToHighlight.HighlightColorIndex = WdColorIndex.wdYellow;
            Thread.Sleep(3000);
            if (fileIsOpen)
                try { rangeToHighlight.HighlightColorIndex = prevColor; }
                catch(Exception e) { }
                
        }

        public void UnHighlightWord(List<CoordinateRange> wordsToChange)
        {
            foreach (CoordinateRange wordToChange in wordsToChange)
            {
                if (fileIsOpen)
                    wordToChange.range.HighlightColorIndex = WdColorIndex.wdNoHighlight;
            }
        }
        //done trigger word is meant to unhighlight all words that were replaced
        //using the replace all trigger word
        //if the system is not in replace all mode then return;
        public override void ReplaceAllDone()
        {
            if (!isReplaceAll)
                return;
            UnHighlightWord(this.replaceAllWordsToChange);
            isReplaceAll = false;
        }

        public override bool fileReadOnly()
        {
            return document.ReadOnly;
        }

        public override CoordinateRange DeleteSentence(CoordinateRange startRange, CoordinateRange endRange)
        {
            startRange.range.End = endRange.range.End;
            CoordinateRange toReturn = new CoordinateRange(startRange.X,startRange.Y,startRange.range,startRange.range.Text);
            startRange.range.Text = "";
            //lastCoordinate.range.End = wordToChange.range.Start + newWord.Length;
            SystemLogger.getEventLog().Info(String.Format("Changed {0} to {1} in point x={2} y={3} range start={4} end={5}",
                startRange.word, "", startRange.X, startRange.Y, startRange.range.Start, startRange.range.End));

            //toReturn.word = toReturn.range.Text;
            return toReturn;

        }

        public override CoordinateRange SaveSentence(CoordinateRange startRange, CoordinateRange endRange)
        {
            startRange.range.End = endRange.range.End;
            lastCopiedSentence = startRange.range.Text;
            CoordinateRange toReturn = new CoordinateRange(startRange.X, startRange.Y, startRange.range, startRange.range.Text);
            SystemLogger.getEventLog().Info(String.Format("Changed {0} to {1} in point x={2} y={3} range start={4} end={5}",
                startRange.word, "", startRange.X, startRange.Y, startRange.range.Start, startRange.range.End));
            return toReturn;
        }

        public override CoordinateRange PasteSentence(CoordinateRange startRange, string pastePlacement)
        {
            CoordinateRange toReturn = new CoordinateRange();
            if (pastePlacement == "after")
            {
                toReturn = new CoordinateRange(startRange.X, startRange.Y, startRange.range, startRange.range.Text);
                startRange.range.Text = startRange.range.Text + " " + lastCopiedSentence;
                HighlightWordForSpecificTime(startRange, 3000);
                //startRange.range.End = startRange.range.End + lastCopiedSentence.Length;
            }
            else if (pastePlacement == "before")
            {
                toReturn = new CoordinateRange(startRange.X, startRange.Y, startRange.range, startRange.range.Text);
                startRange.range.Text = lastCopiedSentence + " " + startRange.range.Text;
                HighlightWordForSpecificTime(startRange, 3000);
                //startRange.range.Start = startRange.range.Start - lastCopiedSentence.Length;
            }
            return toReturn;
        }

        public override void HighlightWordForSpecificTime(CoordinateRange startRange, int milSecs)
        {
            Range rangeToHighlight = startRange.range.Duplicate;
            Thread thread = new Thread(() => {
                WdColorIndex prevColor = rangeToHighlight.HighlightColorIndex;
                if (prevColor == WdColorIndex.wdYellow)
                    prevColor = WdColorIndex.wdWhite;
                rangeToHighlight.HighlightColorIndex = WdColorIndex.wdYellow;
                Thread.Sleep(milSecs);
                if (fileIsOpen)
                    try { rangeToHighlight.HighlightColorIndex = prevColor; }
                    catch(Exception e) { }
                    
            });
            thread.Start();
 
        }
        public override Document getDoc()
        {
            return document;
        }

        public override string getLastCopiedSentence()
        {
            return lastCopiedSentence;
        }

        public override void HighlightLastCopiedSentence(CoordinateRange range)
        {
            try
            {
                if (this.lastCopiedSentenceInfo.range != null) this.StopHightlightLastCopiedSentece();
                Range rangeToHighlight = range.range.Duplicate;
                WdColorIndex prevColor = rangeToHighlight.HighlightColorIndex;
                if (prevColor == WdColorIndex.wdYellow)
                    prevColor = WdColorIndex.wdWhite;
                rangeToHighlight.HighlightColorIndex = WdColorIndex.wdYellow;
                this.lastCopiedSentenceInfo = (rangeToHighlight, prevColor);
                this.isSentenceHighlighted = true;
            }
            catch(Exception e) { }
            
        }

        public override void StopHightlightLastCopiedSentece()
        {
            try
            {
                if (fileIsOpen)
                {
                    lastCopiedSentenceInfo.range.HighlightColorIndex = lastCopiedSentenceInfo.color;
                    this.isSentenceHighlighted = false;
                }
            }
            catch(Exception e) { }
            
                
        }
    }

}