using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CGTools.Core
{
    /// <summary>
    /// Global settings for CGTools. Persists across Unity sessions as a ScriptableObject.
    /// </summary>
    [Serializable]
    public class CGToolsSettings : ScriptableObject
    {
        private static CGToolsSettings instance;
        private const string SettingsPath = "Assets/CGTools/Core/Resources/CGToolsSettings.asset";
        private const string ResourcesPath = "CGToolsSettings";

        #region General Settings

        [Header("General")]
        [Tooltip("Preferred interface language.")]
        [SerializeField] private SystemLanguage language = SystemLanguage.English;

        [Tooltip("Show CGTools Hub automatically on Unity startup.")]
        [SerializeField] private bool showHubOnStartup = true;

        [Tooltip("Show welcome screen on first install.")]
        [SerializeField] private bool showWelcomeScreen = true;

        [Tooltip("Enable debug logging in the Console.")]
        [SerializeField] private bool enableDebugLogs = false;

        #endregion

        #region Theme Settings

        [Header("Theme")]
        [Tooltip("Current UI theme.")]
        [SerializeField] private ThemeType theme = ThemeType.Dark;

        #endregion

        #region Usage Statistics

        [Header("Statistics")]
        [Tooltip("Total number of times CGTools has been opened.")]
        [SerializeField] private int totalOpenCount = 0;

        [Tooltip("Date CGTools was first used.")]
        [SerializeField] private string firstUseDateString = "";

        [Tooltip("Date CGTools was last used.")]
        [SerializeField] private string lastUseDateString = "";

        #endregion

        #region Module Settings

        [Header("Module Settings")]
        [Tooltip("Per-module settings stored as JSON.")]
        [SerializeField] private List<ModuleSettingsEntry> moduleSettings = new List<ModuleSettingsEntry>();

        #endregion

        #region Properties

        public SystemLanguage Language
        {
            get => language;
            set { language = value; Save(); }
        }

        public bool ShowHubOnStartup
        {
            get => showHubOnStartup;
            set { showHubOnStartup = value; Save(); }
        }

        public bool ShowWelcomeScreen
        {
            get => showWelcomeScreen;
            set { showWelcomeScreen = value; Save(); }
        }

        public bool EnableDebugLogs
        {
            get => enableDebugLogs;
            set { enableDebugLogs = value; Save(); }
        }

        public ThemeType Theme
        {
            get => theme;
            set { theme = value; Save(); }
        }

        public int TotalOpenCount
        {
            get => totalOpenCount;
            private set { totalOpenCount = value; Save(); }
        }

        public DateTime FirstUseDate
        {
            get => ParseDate(firstUseDateString);
            private set { firstUseDateString = value.ToString("o"); Save(); }
        }

        public DateTime LastUseDate
        {
            get => ParseDate(lastUseDateString);
            private set { lastUseDateString = value.ToString("o"); Save(); }
        }

        #endregion

        #region Singleton

        public static CGToolsSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = LoadOrCreateSettings();
                return instance;
            }
        }

        #endregion

        #region Initialization

        private static CGToolsSettings LoadOrCreateSettings()
        {
            CGToolsSettings settings = Resources.Load<CGToolsSettings>(ResourcesPath);

            if (settings == null)
            {
                settings = CreateInstance<CGToolsSettings>();
                settings.InitializeDefaults();

#if UNITY_EDITOR
                string directory = System.IO.Path.GetDirectoryName(SettingsPath);
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                string resourcesFolder = "Assets/CGTools/Core/Resources";
                if (!System.IO.Directory.Exists(resourcesFolder))
                    System.IO.Directory.CreateDirectory(resourcesFolder);

                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[CGTools] Settings file created at: " + SettingsPath);
#endif
            }

            return settings;
        }

        private void InitializeDefaults()
        {
            language = SystemLanguage.English;
            showHubOnStartup = true;
            showWelcomeScreen = true;
            enableDebugLogs = false;
            theme = ThemeType.Dark;
            totalOpenCount = 0;
            firstUseDateString = DateTime.Now.ToString("o");
            lastUseDateString = DateTime.Now.ToString("o");
            moduleSettings = new List<ModuleSettingsEntry>();
        }

        #endregion

        #region Usage Tracking

        /// <summary>
        /// Records that CGTools was opened and updates usage statistics.
        /// </summary>
        public void RecordUsage()
        {
            TotalOpenCount++;
            LastUseDate = DateTime.Now;

            if (string.IsNullOrEmpty(firstUseDateString))
                FirstUseDate = DateTime.Now;
        }

        #endregion

        #region Module Settings Management

        /// <summary>
        /// Saves module-specific settings as JSON.
        /// </summary>
        public void SaveModuleSettings(string moduleID, object settings)
        {
            if (settings == null)
                return;

            string json = JsonUtility.ToJson(settings);
            var entry = moduleSettings.Find(e => e.moduleID == moduleID);

            if (entry != null)
            {
                entry.settingsJson = json;
            }
            else
            {
                moduleSettings.Add(new ModuleSettingsEntry
                {
                    moduleID = moduleID,
                    settingsJson = json
                });
            }

            Save();
        }

        /// <summary>
        /// Loads module-specific settings from JSON. Returns a new instance if not found.
        /// </summary>
        public T LoadModuleSettings<T>(string moduleID) where T : new()
        {
            var entry = moduleSettings.Find(e => e.moduleID == moduleID);

            if (entry != null && !string.IsNullOrEmpty(entry.settingsJson))
            {
                try
                {
                    return JsonUtility.FromJson<T>(entry.settingsJson);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CGTools] Failed to load settings for module '{moduleID}': {e.Message}");
                }
            }

            return new T();
        }

        /// <summary>
        /// Returns true if the module has saved settings.
        /// </summary>
        public bool HasModuleSettings(string moduleID)
        {
            return moduleSettings.Exists(e => e.moduleID == moduleID);
        }

        /// <summary>
        /// Clears saved settings for a specific module.
        /// </summary>
        public void ClearModuleSettings(string moduleID)
        {
            moduleSettings.RemoveAll(e => e.moduleID == moduleID);
            Save();
        }

        #endregion

        #region Save / Reset

        /// <summary>
        /// Persists settings to disk and notifies all registered modules.
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            ModuleManager.NotifySettingsSaved();
#endif
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        public void ResetToDefaults()
        {
            InitializeDefaults();
            moduleSettings.Clear();
            Save();

            Debug.Log("[CGTools] Settings reset to defaults.");
        }

        #endregion

        #region Utilities

        private DateTime ParseDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            try
            {
                return DateTime.Parse(dateString);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Returns the appropriate text based on the current language setting.
        /// </summary>
        public string GetLocalizedText(string englishText, string spanishText)
        {
            return language == SystemLanguage.Spanish ? spanishText : englishText;
        }

        #endregion
    }

    /// <summary>
    /// Available UI theme options.
    /// </summary>
    public enum ThemeType
    {
        Dark,
        Light
    }

    /// <summary>
    /// Stores per-module settings as serialized JSON.
    /// </summary>
    [Serializable]
    public class ModuleSettingsEntry
    {
        public string moduleID;
        public string settingsJson;
    }
}