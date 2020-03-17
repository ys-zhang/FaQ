using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    public class RangeSelection
    {
        public int _start { get; set; }
        public int _end { get; set; }
        public string _sort { get; set; }
        public string _order { get; set; }

        public int Start => _start;
        public int End => _end;
        public string Sort => _sort;
        public string Order => _order;

    }
}
