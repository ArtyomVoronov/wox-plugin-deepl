using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RestEase;
using Wox.Infrastructure.Storage;


namespace Wox.Plugin.DeepL
{
    public class Main : IPlugin, ISettingProvider, ISavable
    {
        private PluginInitContext _context;
        private IDeepLApi _client;
        private Settings _settings;
        private PluginJsonStorage<Settings> _storage;


        private Dictionary<string, string> _supportedLng = new Dictionary<string, string>
        {
            { "BG", "Bulgarian" },
            { "CS", "Czech" },
            { "DA", "Danish" },
            { "DE", "German" },
            { "EL", "Greek" },
            { "EN", "English" },
            { "ES", "Spanish" },
            { "ET", "Estonian" },
            { "FI", "Finnish" },
            { "FR", "French" },
            { "HU", "Hungarian" },
            { "ID", "Indonesian" },
            { "IT", "Italian" },
            { "JA", "Japanese" },
            { "LT", "Lithuanian" },
            { "LV", "Latvian" },
            { "NL", "Dutch" },
            { "PL", "Polish" },
            { "PT", "Portuguese (all Portuguese varieties mixed)" },
            { "RO", "Romanian" },
            { "RU", "Russian" },
            { "SK", "Slovak" },
            { "SL", "Slovenian" },
            { "SV", "Swedish" },
            { "TR", "Turkish" },
            { "UK", "Ukrainian" },
            { "ZH", "Chinese" }
        };

        //TODO Implement copy to clipboard action as similiar as http://www.wox.one/plugin/60
        public void Init(PluginInitContext context)
        {
            _context = context;
            _storage = new PluginJsonStorage<Settings>();
            _settings = _storage.Load();

            _client = new RestClient("https://api-free.deepl.com/v2/").For<IDeepLApi>();

            if (!string.IsNullOrWhiteSpace(_settings.AuthKey))
            {
                _client.AuthKey = $"DeepL-Auth-Key {_settings.AuthKey}";
            }
        }

        public List<Result> Query(Query query)
        {
            if (string.IsNullOrWhiteSpace(_client.AuthKey))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "Auth Key is not set",
                        IcoPath = "deepl_logo.png",
                        Action = _ =>
                        {
                            return true;
                        }
                    }
                };
            }

            if (query.Terms.Length < 3)
            {
                var term = query.Terms[query.Terms.Length-1]?.ToUpper() ?? string.Empty;
                if (query.Terms.Length == 1 || string.IsNullOrWhiteSpace(term) || _supportedLng.ContainsKey(term))
                {
                    return _supportedLng.Select(lng => new Result
                    {
                        Title = lng.Key,
                        SubTitle = lng.Value,
                        IcoPath = "deepl_logo.png",
                        Action = _ =>
                        {
                            var newQuery = query.RawQuery.Trim();
                            _context.API.ChangeQuery($"{newQuery} {lng.Key} ");
                            return false;
                        }
                    }).ToList();
                }

                return _supportedLng.Where(p => p.Key.StartsWith(term)).Select(lng => new Result
                {
                    Title = lng.Key,
                    SubTitle = lng.Value,
                    IcoPath = "deepl_logo.png",
                    Action = _ =>
                    {
                        var newQuery = query.RawQuery.Remove(query.RawQuery.LastIndexOf(query.SecondSearch)).Trim();
                        _context.API.ChangeQuery($"{newQuery} {lng.Key} ");
                        return false;
                    }
                }).ToList();
            }

            //TODO Introduce cache to reduce API calls number
            var text = query.SecondToEndSearch.Replace($"{query.SecondSearch} ","");
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<Result>();
            }

            var request = new Dictionary<string, string>
            {
                {"source_lang", query.FirstSearch},
                {"target_lang", query.SecondSearch},
                {"text", text}
            };

            TranslationResponse translations = null;

            try
            {
                translations = _client.Translate(request).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = e.Message,
                        IcoPath = "deepl_logo.png",
                        Action = _ =>
                        {
                            return true;
                        }
                    }
                };
            }

            if (translations?.Translations == null || !translations.Translations.Any())
            {
                return new List<Result>
                {
                    new Result
                    {
                            Title = "No results",
                            IcoPath = "deepl_logo.png",
                            Action = _ =>
                            {
                                return true;
                            }
                    }
                };
            }

            return translations.Translations.Select(t => new Result
            {
                Title = t.Text,
                IcoPath = "deepl_logo.png",
                Action = _ =>
                {
                    return true;
                }
            }).ToList();
        }

        Control ISettingProvider.CreateSettingPanel()
        {
            return new PluginSettings(_settings, _client);
        }

        public void Save()
        {
            _storage.Save();
        }
    }
}
