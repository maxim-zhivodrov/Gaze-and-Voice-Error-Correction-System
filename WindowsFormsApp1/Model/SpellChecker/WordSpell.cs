using System;
using System.Collections.Generic;
using Application = Microsoft.Office.Interop.Word.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using System.IO;
using EyeGaze.Logger;
using System.Reflection;

namespace EyeGaze.SpellChecker
{
    class WordSpell : SpellCheckerAbstract
    {
        public Application application;
        Document document;
        bool fileIsOpen;
        String fileName;

        public WordSpell()
        {
            reload();
        }
        public void reload()
        {
            application = new Application();
            application.Visible = false;
            fileName = Path.GetTempFileName();
            File.WriteAllBytes(fileName, Properties.Resources.wordSpell);
            document = application.Documents.Open(fileName);
            fileIsOpen = true;
        }

        public override List<string> GetSpellingSuggestions(string word)
        {
            List<string> spellSuggestions = new List<string>();
            try
            {
                SpellingSuggestions suggestions = application.GetSpellingSuggestions(word);
                foreach (SpellingSuggestion suggestion in suggestions)
                {
                    spellSuggestions.Add(suggestion.Name);
                }
            }
            catch { }
            return spellSuggestions;
        }

        /// <summary>
        /// Returns true if there is a misspelling in the word. Otherwise false
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public override bool IsMisspelled(string word)
        {
            try
            {
                return !application.CheckSpelling(word);
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Error("WordSpell exception" + e.Message);
                CloseSpellChecker();
                reload();
                return false;
            }
        }

        public override void CloseSpellChecker()
        {
            try
            {
                if (fileIsOpen)
                {
                    document.Save();
                    document.Close();
                    application.Quit(false);
                    SystemLogger.getEventLog().Info("Spell checker closed successfully");
                    fileIsOpen = false;
                    File.Delete(fileName);
                }
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Error(e.Message);
            }
        }
    }
}