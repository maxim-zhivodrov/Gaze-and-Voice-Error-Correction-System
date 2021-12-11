using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeGaze.SpellChecker
{
    public abstract class SpellCheckerAbstract
    {
        public abstract List<string> GetSpellingSuggestions(string word);
        public abstract bool IsMisspelled(string word);
        public abstract void CloseSpellChecker();
    }
}
