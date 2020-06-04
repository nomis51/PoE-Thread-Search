﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    [Table("ForumThread")]
    public class ForumThread : Model {
        public string Name { get; set; }
        public Player Author { get; set; }
        public int NbPage { get; set; } = 1;
        public List<Page> Pages { get; } = new List<Page>();
    }
}