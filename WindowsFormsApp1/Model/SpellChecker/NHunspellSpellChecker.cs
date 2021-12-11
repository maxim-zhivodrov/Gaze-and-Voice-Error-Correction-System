using System;
using System.Collections.Generic;
using NHunspell;
using EyeGaze.Logger;
using System.IO;
using System.Reflection;


namespace EyeGaze.SpellChecker
{
    public class NHunspellSpellChecker : SpellCheckerAbstract
    {
        Hunspell hunspell;
        public NHunspellSpellChecker()
        {
            try
            {
                var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                var combinedPath1 = Path.Combine(outPutDirectory, "en_us.aff");
                var combinedPath2 = Path.Combine(outPutDirectory, "en_us.dic");
                string path1 = new Uri(combinedPath1).LocalPath;
                string path2 = new Uri(combinedPath2).LocalPath;
                hunspell = new Hunspell(path1, path2);
                SystemLogger.getEventLog().Info("Hunspell spell checker opened successfully");
            }
            catch (Exception e)
            {
                SystemLogger.getErrorLog().Error(String.Format("Hunspell spell checker did not open successfully: {0}", e.Message));
            }
        }
        public override bool IsMisspelled(string word)
        {
                List<string> suggestions = hunspell.Suggest(word);
                if (hunspell.Spell(word) || IsPunctuation(word))
                {
                    return false;
                }
                else if (suggestions.Count != 0)
                {
                    return true;
                }
            return false;
        }
        private Boolean IsPunctuation(string word)
        {
            if (word.Equals("\r") || word.Equals("\t") || word.Equals("\n"))
            {
                return true;
            }
            return false;
        }

        public override List<string> GetSpellingSuggestions(string word)
        {
            return hunspell.Suggest(word);
        }

        public override void CloseSpellChecker()
        {
            SystemLogger.getEventLog().Info("Spell checker closed successfully");
        }
    }
}
