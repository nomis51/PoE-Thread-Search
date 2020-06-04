using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    public class Post : Model {
        public int PostId { get; set; }
        public int Page { get; set; }
        public ForumThread ForumThread { get; set; }
        public string Content { get; set; }
        public Player Author { get; set; }
        public DateTime Time { get; set; }
    }
}
