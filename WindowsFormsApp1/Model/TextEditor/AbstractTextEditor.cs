using System;
using System.Collections.Generic;
using EyeGaze.Engine;
using Microsoft.Office.Interop.Word;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace EyeGaze.TextEditor
{
    public abstract class AbstractTextEditor<T>
    {
        //public (List<String> list, int x, int y) fixedWord = (null, 0, 0);
        Thread popup_thread;
        public (List<String> list, CoordinateRange coord) fixedWord = (null, new CoordinateRange(0,0,null,""));
        public Boolean choosingSuggestion = false;
        private suggestionPopup sp = null;

        public event TriggerHandlerMessage sendMessageToEngine;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cursorPosition"></param>
        /// <param name="word"> after this word add the sentence</param>
        /// <param name="wordsToAdd"> the sentence to add</param>
        /// <returns></returns>
        protected virtual void CloseFile(MessageEvent m)
        {
            sendMessageToEngine(this, m);
        }
        public abstract void SetCursorPosition(Range cursorPosition);
        public abstract void ReplaceWord(T wordToChange, string newWord);
        public abstract void CloseFile();
        public abstract List<CoordinateRange> GetCursorPositionWordsInArea();
        public abstract System.Drawing.Point GetPointOfCursorPosition();
        public abstract List<CoordinateRange> GetAllWordsInArea(System.Drawing.Point position);
        public abstract void MoveCursor(System.Drawing.Point position);
        public abstract List<CoordinateRange> GetAllWordsInRange(Range range);
        public abstract List<CoordinateRange> GetRangesForAllSameWords(string word);
        public abstract void ReplaceAllWords(List<CoordinateRange> wordsToChange, string oldWord, string newWord);
        public abstract void ReplaceAllDone();
        public abstract bool fileReadOnly();

        public abstract CoordinateRange DeleteSentence(CoordinateRange startRange, CoordinateRange endRange);
        public abstract CoordinateRange SaveSentence(CoordinateRange startRange, CoordinateRange endRange);
        public abstract CoordinateRange PasteSentence(CoordinateRange startRange, string pastePlacement);
        public abstract void HighlightWordForSpecificTime(CoordinateRange startRange, int milSecs);
        public abstract Document getDoc();
        public abstract string getLastCopiedSentence();
        public abstract void HighlightLastCopiedSentence(CoordinateRange range);
        public abstract void StopHightlightLastCopiedSentece();

         public void ShowMoreSuggestions()
        {
            List<string> emptyList = new List<string>();
            emptyList.Add("NO MATCH");
            if (fixedWord.list != null && fixedWord.list.Count > 0)
            {
                sp = new suggestionPopup(fixedWord.coord.X - 90, fixedWord.coord.Y - 50, fixedWord.list);
                sp.Refresh();
                sp.Show();
                sp.TopMost = true;
                System.Windows.Forms.Application.DoEvents();
            }
            else
            {
                sp = new suggestionPopup(fixedWord.coord.X - 90, fixedWord.coord.Y - 50, emptyList);

                sp.Refresh();
                sp.Show();
                sp.TopMost = true;
                System.Windows.Forms.Application.DoEvents();
            }

            popup_thread = Thread.CurrentThread;
            Thread.Sleep(5000);
            popup_thread.Abort();
        }


        public void HideMoreSuggestions()
        {
            popup_thread.Abort();
        }

    }
}
