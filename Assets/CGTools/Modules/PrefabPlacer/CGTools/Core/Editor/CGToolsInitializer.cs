using UnityEditor;
using UnityEngine;

namespace CGTools.Core
{
    /// <summary>
    /// Automatically initializes CGTools on Unity startup and after package import.
    /// </summary>
    [InitializeOnLoad]
    public static class CGToolsInitializer
    {
        private const string FIRST_LAUNCH_KEY = "CGTools_FirstLaunch";
        private const string LAST_VERSION_KEY = "CGTools_LastVersion";
        private const string CURRENT_VERSION = "1.0.0";

        static CGToolsInitializer()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            bool isFirstLaunch = !EditorPrefs.HasKey(FIRST_LAUNCH_KEY);
            string lastVersion = EditorPrefs.GetString(LAST_VERSION_KEY, "");
            bool isNewVersion = lastVersion != CURRENT_VERSION;

            InitializeCoreSystems();

            if (isFirstLaunch)
                HandleFirstLaunch();
            else if (isNewVersion)
                HandleVersionUpdate(lastVersion);
            else
                HandleRegularStartup();

            EditorPrefs.SetString(LAST_VERSION_KEY, CURRENT_VERSION);
        }

        private static void InitializeCoreSystems()
        {
            try
            {
                var settings = CGToolsSettings.Instance;
                ModuleManager.Initialize();

                if (settings.EnableDebugLogs)
                    Debug.Log("[CGTools] Core systems initialized successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CGTools] Error initializing core systems: {e.Message}");
            }
        }

        private static void HandleFirstLaunch()
        {
            EditorPrefs.SetBool(FIRST_LAUNCH_KEY, true);
            Debug.Log("[CGTools] First launch detected. Welcome to CGTools!");

            var settings = CGToolsSettings.Instance;

            if (settings.ShowWelcomeScreen)
                EditorApplication.delayCall += CGToolsWelcome.ShowWindow;

            var stats = ModuleManager.GetStatistics();
            Debug.Log($"[CGTools] Found {stats.AvailableModules} available module(s).");
        }

        private static void HandleVersionUpdate(string previousVersion)
        {
            Debug.Log($"[CGTools] Updated from version {previousVersion} to {CURRENT_VERSION}");

            var settings = CGToolsSettings.Instance;

            if (settings.EnableDebugLogs)
                Debug.Log("[CGTools] Version update detected. Refreshing modules...");

            ModuleManager.RefreshModules();
        }

        private static void HandleRegularStartup()
        {
            var settings = CGToolsSettings.Instance;

            if (settings.EnableDebugLogs)
                Debug.Log("[CGTools] Regular startup.");

            if (settings.ShowHubOnStartup)
                EditorApplication.delayCall += CGToolsHub.ShowWindow;
        }

        #region Menu Items

        /// <summary>
        /// Verifies CGTools installation integrity and reports any issues.
        /// </summary>
        [MenuItem("Tools/CGTools/Verify Installation", false, 100)]
        public static void VerifyInstallation()
        {
            bool hasErrors = false;

            Debug.Log("[CGTools] Verifying installation...");

            if (!System.IO.Directory.Exists("Assets/CGTools/Core"))
            {
                Debug.LogError("[CGTools] Core folder not found at Assets/CGTools/Core");
                hasErrors = true;
            }

            try
            {
                var settings = CGToolsSettings.Instance;
                Debug.Log("[CGTools] Settings loaded successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CGTools] Failed to load settings: {e.Message}");
                hasErrors = true;
            }

            try
            {
                ModuleManager.Initialize();
                var stats = ModuleManager.GetStatistics();
                Debug.Log($"[CGTools] Module Manager: {stats.TotalModules} module(s) registered.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CGTools] Module Manager error: {e.Message}");
                hasErrors = true;
            }

            if (hasErrors)
            {
                EditorUtility.DisplayDialog(
                    "CGTools Installation",
                    "Installation verification found errors. Check the Console for details.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "CGTools Installation",
                    $"✅ Installation verified successfully!\n\nVersion: {CURRENT_VERSION}\nModules: {ModuleManager.GetStatistics().AvailableModules}",
                    "OK"
                );
            }
        }

        /// <summary>
        /// Forces a full reinitialization of all CGTools systems.
        /// </summary>
        [MenuItem("Tools/CGTools/Force Reinitialize", false, 101)]
        public static void ForceReinitialize()
        {
            Debug.Log("[CGTools] Force reinitializing...");

            ModuleManager.RefreshModules();
            CGToolsSettings.Instance.Save();

            Debug.Log("[CGTools] Reinitialization complete.");

            EditorUtility.DisplayDialog(
                "CGTools",
                "Reinitialization complete. All systems refreshed.",
                "OK"
            );
        }

        /// <summary>
        /// Resets CGTools to first launch state. Useful for testing the onboarding flow.
        /// </summary>
        [MenuItem("Tools/CGTools/Reset to First Launch (Debug)", false, 102)]
        public static void ResetToFirstLaunch()
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Reset CGTools",
                "This will reset CGTools to first launch state. All settings will be lost.\n\nAre you sure?",
                "Yes, Reset",
                "Cancel"
            );

            if (confirm)
            {
                EditorPrefs.DeleteKey(FIRST_LAUNCH_KEY);
                EditorPrefs.DeleteKey(LAST_VERSION_KEY);
                CGToolsSettings.Instance.ResetToDefaults();

                Debug.Log("[CGTools] Reset to first launch state. Restart Unity to see welcome screen.");

                EditorUtility.DisplayDialog(
                    "CGTools Reset",
                    "CGTools has been reset to first launch state.\n\nRestart Unity to see the welcome screen.",
                    "OK"
                );
            }
        }

        [MenuItem("Tools/CGTools/Documentation", false, 200)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/FredoCubap/Cubas-Games-Tools");
        }

        [MenuItem("Tools/CGTools/Contact Support", false, 201)]
        public static void ContactSupport()
        {
            Application.OpenURL("mailto:support@cubasgames.com");
        }

        #endregion
    }
}