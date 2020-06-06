using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoETS.Data.Models {
    public class Player : Model {
        public string Name { get; set; }
        public string AvatarUri { get; set; }
    }
}
