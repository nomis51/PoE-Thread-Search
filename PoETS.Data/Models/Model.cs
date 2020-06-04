using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    public abstract class Model {
        [Column("Id")]
        [PrimaryKey]
        [NotNull]
        [AutoIncrement]
        public int Id { get; set; }
    }
}
