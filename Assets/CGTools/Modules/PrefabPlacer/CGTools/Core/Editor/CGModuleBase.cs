using UnityEngine;

namespace CGTools.Core
{
    /// <summary>
    /// Base abstract class for CGTools modules.
    /// Provides default implementations for common functionality.
    /// Inherit from this instead of implementing ICGModule directly.
    /// </summary>
    public abstract class CGModuleBase : ICGModule
    {
        // --- Required ---

        public abstract string ModuleID { get; }
        public abstract string ModuleNameEN { get; }
        public abstract string ModuleNameES { get; }
        public abstract string DescriptionEN { get; }
        public abstract string DescriptionES { get; }
        public abstract string Version { get; }
        public abstract void OpenWindow();

        // --- Optional overrides ---

        public virtual Texture2D Icon => null;
        public virtual bool IsInstalled => true;
        public virtual bool IsCompatible => CheckUnityVersionCompatibility();
        public virtual string MinUnityVersion => "2021.3";

        public virtual void OnModuleRegistered() { }
        public virtual void OnSettingsSaved() { }
        public virtual object GetModuleSettings() => null;
        public virtual void SetModuleSettings(object settings) { }

        // --- Localization ---

        public string GetLocalizedName(SystemLanguage language)
        {
            return language == SystemLanguage.Spanish ? ModuleNameES : ModuleNameEN;
        }

        public string GetLocalizedDescription(SystemLanguage language)
        {
            return language == SystemLanguage.Spanish ? DescriptionES : DescriptionEN;
        }

        // --- Helpers ---

        /// <summary>
        /// Returns true if the current Unity version meets the minimum requirement.
        /// </summary>
        protected bool CheckUnityVersionCompatibility()
        {
#if UNITY_2021_3_OR_NEWER
            return true;
#else
            return false;
#endif
        }
    }
}