using Microsoft.EntityFrameworkCore;
using PoETS.Data.Database;
using PoETS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data {
    public class PoETSDatabase : IPoETSDatabase {
        #region Singleton
        private static PoETSDatabase _instance = null;
        public static PoETSDatabase Instance() {
            if (_instance == null) {
                _instance = new PoETSDatabase();
            }

            return _instance;
        }
        #endregion

        private readonly PoETSDbContext _context;
        private PoETSDatabase() {
            _context = new PoETSDbContext();
        }

        public async Task<List<T>> Get<T>() where T : Model {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> Get<T>(int id) where T : Model {
            return (await Get<T>()).Find(t => t.Id == id);
        }

        public async Task<T> Insert<T>(T entity) where T : Model {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<T> Update<T>(T entity) where T : Model {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<T> Delete<T>(int id) where T : Model {
            var entity = await Get<T>(id);
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }



    }
}
