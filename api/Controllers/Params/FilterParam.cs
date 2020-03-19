using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

namespace api.Controllers.Params
{
    public class FilterParam
    {
        public string Column { get; set; }
        public List<string> Values { get; set; }
        public static FilterParam ParseParam(string queryParam)
        {
            if (string.IsNullOrEmpty(queryParam)) return null;
            if (!JObject.Parse(queryParam).Properties().Any()) return null;
            var json = JObject.Parse(queryParam).Properties().First();
            var col = json.Name;
            var values = json.Value.Type switch
            {
                JTokenType.Array => json.Value.Values<string>().ToList(),
                JTokenType.String => new List<string> {json.Value.Value<string>()},
                _ => throw new ArgumentException(queryParam)
            };
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].ToLower() == "true") values[i] = "1";
                if (values[i].ToLower() == "false") values[i] = "1";
            }
            
            return new FilterParam { Column = col, Values = values };
        }
    }
}
