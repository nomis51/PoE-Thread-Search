using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Configuration {
    public class Config {
        public string PoETSServer { get; set; }
        public string DatabaseFileName { get; set; }
        public string DatabaseFilePath { get; set; }
        public bool NoDatabase { get; set; }
        public List<Synonyms> Synonyms { get; set; } = new List<Synonyms>();
        public int NbPostPerPage { get; set; }
        public string PoEWebSiteForumUrlTemplate { get; set; }
        public string PostsHtmlTableXPath { get; set; }
        public string PostAuthorHtmlXPath { get; set; }
        public string PostAvatarHtmlXPath { get; set; }
        public string ForumThreadTitleHtmlXPath { get; set; }
        public string PostContentHtmlXPath { get; set; }
        public string PostTimeHtmlXPath { get; set; }
        public string ForumThreadPaginationHtmlXPath { get; set; }
        public int SearchFuzziness { get; set; }
        public List<string> LoadingMessages { get; set; }

    }
}
