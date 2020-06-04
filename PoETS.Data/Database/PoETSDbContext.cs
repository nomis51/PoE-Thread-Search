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

        public int NextId(Type type) {
            switch (type) {
                case typeof(Player):
                    if (PlayersNextId == -1) {
                        PlayersNextId = GetNextId(Players);
                    }
                    return PlayersNextId++;
                case "Posts":
                    if (PostsNextId == -1) {
                        PostsNextId = GetNextId(Posts);
                    }
                    return PostsNextId++;
                case "ForumThreads":
                    if (ForumThreadsNextId == -1) {
                        ForumThreadsNextId = GetNextId(ForumThreads);
                    }
                    return ForumThreadsNextId++;
                case "Pages":
                    if (PageNextId == -1) {
                        PageNextId = GetNextId(Pages);
                    }
                    return PageNextId++;
                default:
                    return 0;
            }
        }

        private int GetNextId<T>(DbSet<T> set) where T : Model {
            return set.Count() > 0 ? set.Max(p => p.Id) + 1 : 1;
        }

        private int PlayersNextId = -1;
        public DbSet<Player> Players { get; set; }
        private int PostsNextId = -1;
        public DbSet<Post> Posts { get; set; }
        private int ForumThreadsNextId = -1;
        public DbSet<ForumThread> ForumThreads { get; set; }
        private int PageNextId = -1;
        public DbSet<Page> Pages { get; set; }

    }
}
