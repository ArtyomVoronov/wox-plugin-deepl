using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestEase;

namespace Wox.Plugin.DeepL
{
    public interface IDeepLApi
    {
        [Header("Authorization")]
        string AuthKey { get; set; }

        [Post("translate")]
        Task<TranslationResponse> Translate([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> data);
    }


    public class TranslationResponse
    {
        public List<Translation> Translations { get; set; }
    }

    public class Translation
    {
        [JsonProperty("detected_source_language")]
        public string DetectedSourceLanguage { get; set; }

        public string Text { get; set; }
    }
}
