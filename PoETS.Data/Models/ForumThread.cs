using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    public class ForumThread : Model {
        public string Name { get; set; }
        public Player Author { get; set; }
        public int ThreadId { get; set; }
        public int NbPage { get; set; } = 1;
    }
}
