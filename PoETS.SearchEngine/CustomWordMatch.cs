using Ganss.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.SearchEngine {
    public class CustomWordMatch {
        public int Index { get; set; }
        public string Word { get; set; }
        public string SynonymOf { get; set; } = null;

        public CustomWordMatch(WordMatch wordMatch) {
            Index = wordMatch.Index;
            Word = wordMatch.Word;
        }

        public CustomWordMatch(WordMatch wordMatch, string synonymOf) {
            Index = wordMatch.Index;
            Word = wordMatch.Word;
            SynonymOf = synonymOf;
        }
    }
}
