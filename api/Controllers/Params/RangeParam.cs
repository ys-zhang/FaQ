using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace api.Controllers.Params
{
    public class RangeParam
    {
        public int Start {get; private set;}
        public int End {get; private set;}
        public int OffSet => Start;
        public int Limit => End - Start + 1;
        public static RangeParam ParseParam(string queryParam)
        {
            if (string.IsNullOrEmpty(queryParam)) return null;
            var values = JsonConvert.DeserializeObject<List<int>>(queryParam);
            var lo = values.Min();
            var hi = values.Max();
            return new RangeParam { Start = lo, End = hi };
        }
    }
}
