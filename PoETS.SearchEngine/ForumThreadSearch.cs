using Ganss.Text;
using PoETS.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.SearchEngine {
    public class ForumThreadSearch {

        public ForumThreadSearch() {
        }

        public IEnumerable<CustomWordMatch> Search(List<string> keywords, List<string> inputs) {
            List<string> words = new List<string>();
            List<Synonyms> synonyms = new List<Synonyms>();

            foreach (var k in keywords) {
                var syns = GetSynonyms(k);
                if (syns.Count == 0) {
                    words.Add(k);
                } else {
                    synonyms.AddRange(syns);

                    words.AddRange(syns.Select(s => s.Words)
                        .SelectMany(s => s)
                        .Distinct());
                }
            }

            var query = new AhoCorasick(words);

            foreach (var i in inputs) {
                var matches = query.Search(i.ToLower());

                if (matches.Any()) {
                    return ToCustomWordMatch(matches
                        //.Select(m => {
                        //    if (!keywords.Contains(m.Word)) {
                        //        var syns = synonyms.FindAll(s => s.Words.Contains(m.Word));
                        //        var keyword = syns.Select(e => e.Words.Find(s => keywords.Contains(s))).First();

                        //        if (keyword != null) {
                        //            m.Word = keyword;
                        //        }

                        //        return m;
                        //    }

                        //    return m;
                        //})
                        .Distinct(WordMatchComparer.Instance)
                        .OrderBy(m => m.Index), keywords, synonyms);
                } else {
                    List<CustomWordMatch> fuzzyMatches = new List<CustomWordMatch>();
                    var i_split = i.Split(' ');

                    for (int k = 0; k < i_split.Length; ++k) {
                        var w = i_split[k];
                        foreach (var keyword in keywords) {
                            if (LevenshteinDistance(w, keyword) <= ConfigManager.GetConfig().SearchFuzziness) {
                                // TODO: return a wordMatch with the word and index
                                fuzzyMatches.Add(
                                    new CustomWordMatch(new WordMatch() {
                                        Index = FindWordIndexFromSplit(k, w, i_split),
                                        Word = w
                                    })
                                );
                            }
                        }
                    }

                    return fuzzyMatches;
                }
            }

            return new List<CustomWordMatch>();
        }

        private int FindWordIndexFromSplit(int wordSplitIndex, string word, string[] split_input) {
            int index = 0;

            for (int i = 0; i < wordSplitIndex; ++i) {
                index += split_input[i].Length + 1;
            }

            return index;
        }

        private int LevenshteinDistance(string s, string t) {
            var d = new List<List<int>>();

            for (int i = 0; i < s.Length + 1; ++i) {
                List<int> r = new List<int>();
                for (int j = 0; j < t.Length + 1; ++j) {
                    r.Add(0);
                }

                d.Add(r);
            }

            for (int i = 1; i < s.Length + 1; ++i) {
                d[i][0] = i;
            }

            for (int j = 1; j < t.Length + 1; ++j) {
                d[0][j] = j;
            }

            for (int j = 1; j < t.Length + 1; ++j) {
                for (int i = 1; i < s.Length + 1; ++i) {
                    int substitutionCost = s[i - 1] == t[j - 1] ? 0 : 1;

                    d[i][j] = Math.Min(d[i - 1][j] + 1, Math.Min(d[i][j - 1] + 1, d[i - 1][j - 1] + substitutionCost));
                }
            }

            return d[s.Length][t.Length];
        }

        private IEnumerable<CustomWordMatch> ToCustomWordMatch(IEnumerable<WordMatch> wordMatches, List<string> keywords, List<Synonyms> synonyms) {
            if (synonyms is null) {
                throw new ArgumentNullException(nameof(synonyms));
            }

            return wordMatches
                  .Select(m => {
                      if (!keywords.Contains(m.Word)) {
                          var syns = synonyms.FindAll(s => s.Words.Contains(m.Word));
                          var keyword = syns.Select(e => e.Words.Find(s => keywords.Contains(s))).First();

                          if (keyword != null) {
                              return new CustomWordMatch(m, keyword);
                          }
                      }

                      return new CustomWordMatch(m);
                  });
        }

        private List<Synonyms> GetSynonyms(string keyword) {
            string loweredKeyword = keyword.ToLower();
            return ConfigManager.GetConfig().Synonyms.FindAll(s => s.Words.Contains(loweredKeyword));
        }

        private bool _Search(List<string> keywords, string content) {
            string contentLowered = content.ToLower();

            foreach (var k in keywords) {
                if (contentLowered.Contains(k.ToLower())) {
                    return true;
                }

                //for (int i = 1; i < k.Length / 2; ++i) {
                //    if (contentLowered.Contains(k.Substring(1))) {
                //        return true;
                //    }

                //    if (contentLowered.Contains(k.Substring(0, k.Length - i))) {
                //        return true;
                //    }
                //}
            }

            return false;
        }
    }
}
