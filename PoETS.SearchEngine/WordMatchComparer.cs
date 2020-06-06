using Ganss.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.SearchEngine {
    class WordMatchComparer : IEqualityComparer<WordMatch> {
        public static readonly WordMatchComparer Instance = new WordMatchComparer();

        public bool Equals(WordMatch x, WordMatch y) {
            return x.Index.Equals(y.Index);
        }

        public int GetHashCode(WordMatch obj) {
            return obj.Index.GetHashCode();
        }
    }
}
