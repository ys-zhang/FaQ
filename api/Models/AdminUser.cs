using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace api.Models
{
    public class AdminUser
    {
        [Key]
        public string Username { get; set; }
        public string Password { get; set; }
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<AdminUserRole> Roles { get; set; }
    }

    public enum AdminUserRole
    {
        Admin, Readonly
    }
}