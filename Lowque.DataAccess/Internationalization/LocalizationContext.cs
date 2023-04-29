using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lowque.DataAccess.Internationalization.Interfaces;
using Newtonsoft.Json;

namespace Lowque.DataAccess.Internationalization
{
    public class LocalizationContext : ILocalizationContext
    {
        public string TryTranslate(string key)
        {
            var localizedPhrases = GetLocalizedPhrases();
            return localizedPhrases.ContainsKey(key)
                ? localizedPhrases[key]
                : key;
        }

        public string TryTranslate(string key, params string[] args)
        {
            var localizedPhrases = GetLocalizedPhrases();
            return localizedPhrases.ContainsKey(key)
                ? string.Format(localizedPhrases[key], args)
                : key;
        }

        public string TryTranslate(string key, IEnumerable<string> args)
        {
            return TryTranslate(key, args.ToArray());
        }

        public Dictionary<string, string> GetLocalizedPhrases()
        {
            return LoadLocalizedPhrases().Pl;
        }

        private LocalizedPhrasesContainer LoadLocalizedPhrases()
        {
            var appExecutionDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var localizedPhrasesFilePath = Path.Combine(appExecutionDirectory, "Internationalization", "Resources", "LocalizedPhrases.json");
            var localizedPhrasesFileContent = File.ReadAllText(localizedPhrasesFilePath);
            var localizedPhrases = JsonConvert.DeserializeObject<LocalizedPhrasesContainer>(localizedPhrasesFileContent);
            return localizedPhrases;
        }
    }
}
