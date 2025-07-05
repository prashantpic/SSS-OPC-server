using Microsoft.Extensions.Localization;

namespace ui.webapp.Services
{
    /// <summary>
    /// Provides services for application localization. Manages loading of translation files 
    /// and provides access to localized strings throughout the application.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the localized string for the specified key.
        /// </summary>
        /// <param name="key">The key of the string resource.</param>
        /// <returns>The localized string.</returns>
        string this[string key] { get; }
    }

    /// <summary>
    /// An implementation of ILocalizationService that uses IStringLocalizer.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer<App> _localizer;

        /// <summary>
        /// Initializes a new instance of the LocalizationService.
        /// </summary>
        /// <param name="localizerFactory">The factory to create localizers.</param>
        public LocalizationService(IStringLocalizerFactory localizerFactory)
        {
            _localizer = localizerFactory.Create(typeof(App));
        }

        /// <summary>
        /// Gets the localized string for the specified key.
        /// </summary>
        /// <param name="key">The key of the string resource.</param>
        /// <returns>The localized string.</returns>
        public string this[string key] => _localizer[key];
    }
}