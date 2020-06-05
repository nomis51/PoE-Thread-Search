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
        private readonly PoETSDbContext _context;
        public PoETSDatabase() {
            _context = new PoETSDbContext();

            Init();
        }

        private async void Init() {
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task<List<T>> Get<T>() where T : Model {
            _context.Lock.WaitOne();
            var entities = await _context.Set<T>().ToListAsync();
            _context.Lock.ReleaseMutex();

            return entities;
        }

        public async Task<List<T>> Get<T>(Predicate<T> predicate) where T : Model {
            var entities = (await Get<T>()).FindAll(predicate);

            return entities;
        }

        public async Task<T> Get<T>(int id) where T : Model {
            var entity = (await Get<T>()).Find(t => t.Id == id);

            return entity;
        }

        public async Task<T> Insert<T>(T entity) where T : Model {
            _context.Lock.WaitOne();
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            _context.Lock.ReleaseMutex();

            return entity;
        }

        public async Task<List<T>> Insert<T>(List<T> entities) where T : Model {
            foreach (var entity in entities) {
                await Insert(entity);
            }

            return entities;
        }

        public async Task<T> Update<T>(T entity) where T : Model {
            _context.Lock.WaitOne();
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            _context.Lock.ReleaseMutex();

            return entity;
        }

        public async Task<T> Delete<T>(int id) where T : Model {
            var entity = await Get<T>(id);

            _context.Lock.WaitOne();
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            _context.Lock.ReleaseMutex();

            return entity;
        }



    }
}
