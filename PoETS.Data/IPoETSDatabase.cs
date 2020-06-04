using PoETS.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data {
    interface IPoETSDatabase {
        Task<List<T>> Get<T>() where T : Model;
        Task<T> Get<T>(int id) where T : Model;
        Task<T> Insert<T>(T entity) where T : Model;
        Task<T> Update<T>(T entity) where T : Model;
        Task<T> Delete<T>(int id) where T : Model;
    }
}
