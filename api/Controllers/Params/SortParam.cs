using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace api.Controllers.Params
{
    public class SortParam
    {
        public string Column { get; private set; }
        public bool Desc { get; private set; }

        public static SortParam ParseParam(string queryParam)
        {
            if (string.IsNullOrEmpty(queryParam)) return null;
            var reps = JsonConvert.DeserializeObject<List<string>>(queryParam);
            var desc = reps[1].ToUpper() switch
            {
                "ASC" => false,
                "DESC" => true,
                _ => throw new ArgumentException(queryParam)
            };
            return new SortParam { Column = reps[0].ToLower(), Desc = desc};
        }
    }
}
