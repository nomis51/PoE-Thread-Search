using Microsoft.EntityFrameworkCore;
using PoETS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PoETS.Data.Database {
    public class PoETSDbContext : DbContext {
        public Mutex Lock { get; }


        public PoETSDbContext() {
            Lock = new Mutex();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            options.UseSqlite("Data Source=.\\data.db");
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<Page> Pages { get; set; }

    }
}
