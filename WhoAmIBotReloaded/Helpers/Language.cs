using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoAmIBotReloaded.Helpers
{
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class Language
    {
        public LanguageInfo Info { get; set; }
        public Dictionary<string, string> Strings { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class LanguageInfo
    {
        public string Name { get; set; }
        public string Base { get; set; }
        public string Variant { get; set; }
        public string LanguageCode { get; set; }
    }
}
