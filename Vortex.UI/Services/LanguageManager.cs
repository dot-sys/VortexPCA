using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Vortex.UI.Services
{
    public class LanguageManager
    {
        private static readonly Dictionary<string, string> LanguageResourcePaths = new Dictionary<string, string>
        {
            { "RU", "Resources/Strings.ru-RU.xaml" },
            { "UA", "Resources/Strings.uk-UA.xaml" },
            { "DE", "Resources/Strings.de-DE.xaml" },
            { "PT", "Resources/Strings.pt-PT.xaml" },
            { "ES", "Resources/Strings.es-ES.xaml" },
            { "EN", "Resources/Strings.en-US.xaml" }
        };

        private const string DefaultLanguageResourcePath = "Resources/Strings.en-US.xaml";

        public void ChangeLanguage(string languageCode)
        {
            string resourcePath = LanguageResourcePaths.TryGetValue(languageCode, out var path)
                ? path
                : DefaultLanguageResourcePath;

            var existingLanguageDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null &&
                    (d.Source.OriginalString.Contains("Resources/Strings.") ||
                     d.Source.OriginalString.Contains("/Resources/Strings.")));

            if (existingLanguageDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(existingLanguageDict);
            }

            var newDict = new ResourceDictionary
            {
                Source = new Uri(resourcePath, UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Insert(0, newDict);
        }
    }
}
