using System.Collections.Generic;

namespace Lowque.DataAccess.Internationalization.Interfaces
{
    public interface ILocalizationContext
    {
        string TryTranslate(string key);
        string TryTranslate(string key, params string[] args);
        string TryTranslate(string key, IEnumerable<string> args);

        Dictionary<string, string> GetLocalizedPhrases();
    }
}
