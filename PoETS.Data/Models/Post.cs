using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    public class Post : Model {
        public int PostId { get; set; }
        public int PageNo { get; set; }
        public int ThreadId { get; set; }
        public string Content { get; set; }
        public Player Author { get; set; }
        public DateTime Time { get; set; }
    }
}
