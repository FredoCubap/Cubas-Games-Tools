using UnityEngine;

namespace CGTools.Core
{
    /// <summary>
    /// Interface that all CGTools modules must implement.
    /// </summary>
    public interface ICGModule
    {
        /// <summary>
        /// Unique identifier for the module (e.g., "PrefabPlacer")
        /// </summary>
        string ModuleID { get; }

        /// <summary>
        /// Display name in English.
        /// </summary>
        string ModuleNameEN { get; }

        /// <summary>
        /// Display name in Spanish.
        /// </summary>
        string ModuleNameES { get; }

        /// <summary>
        /// Short description in English.
        /// </summary>
        string DescriptionEN { get; }

        /// <summary>
        /// Short description in Spanish.
        /// </summary>
        string DescriptionES { get; }

        /// <summary>
        /// Module version (e.g., "1.0.0")
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Module icon (48x48 recommended).
        /// </summary>
        Texture2D Icon { get; }

        /// <summary>
        /// Is the module currently installed and ready to use?
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Is the module compatible with the current Unity version?
        /// </summary>
        bool IsCompatible { get; }

        /// <summary>
        /// Minimum Unity version required (e.g., "2021.3")
        /// </summary>
        string MinUnityVersion { get; }

        /// <summary>
        /// Open the module's main window.
        /// </summary>
        void OpenWindow();

        /// <summary>
        /// Called when the module is first registered.
        /// </summary>
        void OnModuleRegistered();

        /// <summary>
        /// Called when CGTools settings are saved.
        /// </summary>
        void OnSettingsSaved();

        /// <summary>
        /// Get module-specific settings. Can return null if unused.
        /// </summary>
        object GetModuleSettings();

        /// <summary>
        /// Set module-specific settings.
        /// </summary>
        void SetModuleSettings(object settings);

        /// <summary>
        /// Get the localized display name based on language.
        /// </summary>
        string GetLocalizedName(SystemLanguage language);

        /// <summary>
        /// Get the localized description based on language.
        /// </summary>
        string GetLocalizedDescription(SystemLanguage language);
    }
}