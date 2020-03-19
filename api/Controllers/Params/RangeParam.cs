using System.Collections.Generic;
using Newtonsoft.Json;

namespace api.Controllers.Params
{
    public class RangeParam
    {
        public int Start {get; private set;}
        public int End {get; private set;}
        public int OffSet => Start - 1;
        public int Limit => End - Start + 1;
        public static RangeParam ParseParam(string queryParam)
        {
            if (string.IsNullOrEmpty(queryParam)) return null;
            var values = JsonConvert.DeserializeObject<List<int>>(queryParam);
            return new RangeParam { Start = values[0], End = values[1] };
        }
    }
}
