using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    [Table("Page")]
    public class Page : Model {
        public int No { get; set; }
        public int ThreadId { get; set; }
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
