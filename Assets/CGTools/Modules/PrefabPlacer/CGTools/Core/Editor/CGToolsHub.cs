using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CGTools.Core
{
    /// <summary>
    /// Main hub window for CGTools. Central access point for all modules.
    /// </summary>
    public class CGToolsHub : EditorWindow
    {
        private Vector2 scrollPosition;
        private Texture2D logoTexture;
        private CGToolsSettings settings;

        private bool showSettingsSection = false;
        private bool showStatisticsSection = false;

        private List<ICGModule> cachedModules;
        private double lastModuleRefreshTime;
        private const double MODULE_REFRESH_INTERVAL = 1.0;

        #region Window Management

        [MenuItem("Tools/CGTools/Hub ⚡", false, 0)]
        public static void ShowWindow()
        {
            CGToolsHub window = GetWindow<CGToolsHub>("CGTools Hub");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            settings = CGToolsSettings.Instance;
            ModuleManager.Initialize();
            LoadLogo();
            settings.RecordUsage();
            RefreshModuleCache();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawModulesSection();
            DrawSettingsSection();
            DrawStatisticsSection();
            DrawLinksSection();
            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Header

        private void DrawHeader()
        {
            GUILayout.Space(10);

            if (logoTexture != null)
            {
                float aspectRatio = (float)logoTexture.height / logoTexture.width;
                Rect logoRect = GUILayoutUtility.GetRect(position.width - 20, (position.width - 20) * aspectRatio);
                GUI.DrawTexture(logoRect, logoTexture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUILayout.Label("🎮 CGTOOLS", EditorStyles.boldLabel);
            }

            GUILayout.Space(5);

            EditorGUILayout.LabelField(
                GetLocalizedText(
                    "Professional Unity Editor Extensions by Cubas Games",
                    "Extensiones Profesionales de Unity Editor por Cubas Games"
                ),
                EditorStyles.centeredGreyMiniLabel
            );

            GUILayout.Space(10);
            DrawSeparator();
        }

        #endregion

        #region Modules Section

        private void DrawModulesSection()
        {
            GUILayout.Space(10);

            GUILayout.Label(
                GetLocalizedText("📦 INSTALLED MODULES", "📦 MÓDULOS INSTALADOS"),
                EditorStyles.boldLabel
            );

            GUILayout.Space(5);

            if (EditorApplication.timeSinceStartup - lastModuleRefreshTime > MODULE_REFRESH_INTERVAL)
                RefreshModuleCache();

            if (cachedModules == null || cachedModules.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    GetLocalizedText(
                        "No modules found. Please install a module package.",
                        "No se encontraron módulos. Por favor instala un paquete de módulo."
                    ),
                    MessageType.Warning
                );
            }
            else
            {
                foreach (var module in cachedModules)
                    DrawModuleCard(module);
            }

            GUILayout.Space(5);

            if (GUILayout.Button(GetLocalizedText("🔄 Refresh Modules", "🔄 Refrescar Módulos")))
            {
                ModuleManager.RefreshModules();
                RefreshModuleCache();
            }

            GUILayout.Space(10);
            DrawSeparator();
        }

        private void DrawModuleCard(ICGModule module)
        {
            string moduleName = module.GetLocalizedName(settings.Language);
            string moduleDesc = module.GetLocalizedDescription(settings.Language);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (module.Icon != null)
                GUILayout.Label(module.Icon, GUILayout.Width(48), GUILayout.Height(48));
            else
                GUILayout.Label("📦", GUILayout.Width(48), GUILayout.Height(48));

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical();

            GUILayout.Label(moduleName, EditorStyles.boldLabel);
            GUILayout.Label(moduleDesc, EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"v{module.Version}", EditorStyles.miniLabel);

            if (!module.IsInstalled)
                GUILayout.Label(GetLocalizedText("❌ Not Installed", "❌ No Instalado"), EditorStyles.miniLabel);
            else if (!module.IsCompatible)
                GUILayout.Label(GetLocalizedText("⚠️ Incompatible", "⚠️ Incompatible"), EditorStyles.miniLabel);
            else
                GUILayout.Label(GetLocalizedText("✅ Ready", "✅ Listo"), EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(!module.IsInstalled || !module.IsCompatible);
            if (GUILayout.Button(GetLocalizedText("Open", "Abrir"), GUILayout.Width(80), GUILayout.Height(48)))
                ModuleManager.OpenModule(module.ModuleID);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        #endregion

        #region Settings Section

        private void DrawSettingsSection()
        {
            GUILayout.Space(10);

            showSettingsSection = EditorGUILayout.Foldout(
                showSettingsSection,
                GetLocalizedText("⚙️ SETTINGS", "⚙️ CONFIGURACIÓN"),
                true,
                EditorStyles.foldoutHeader
            );

            if (showSettingsSection)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);

                EditorGUI.BeginChangeCheck();
                SystemLanguage newLanguage = (SystemLanguage)EditorGUILayout.EnumPopup(
                    GetLocalizedText("Language", "Idioma"),
                    settings.Language
                );
                if (EditorGUI.EndChangeCheck())
                    settings.Language = newLanguage;

                EditorGUI.BeginChangeCheck();
                bool newShowOnStartup = EditorGUILayout.Toggle(
                    GetLocalizedText("Show Hub on Startup", "Mostrar Hub al Iniciar"),
                    settings.ShowHubOnStartup
                );
                if (EditorGUI.EndChangeCheck())
                    settings.ShowHubOnStartup = newShowOnStartup;

                EditorGUI.BeginChangeCheck();
                bool newDebugLogs = EditorGUILayout.Toggle(
                    GetLocalizedText("Enable Debug Logs", "Habilitar Logs de Depuración"),
                    settings.EnableDebugLogs
                );
                if (EditorGUI.EndChangeCheck())
                    settings.EnableDebugLogs = newDebugLogs;

                EditorGUI.BeginChangeCheck();
                ThemeType newTheme = (ThemeType)EditorGUILayout.EnumPopup(
                    GetLocalizedText("Theme", "Tema"),
                    settings.Theme
                );
                if (EditorGUI.EndChangeCheck())
                    settings.Theme = newTheme;

                GUILayout.Space(10);

                if (GUILayout.Button(GetLocalizedText("Reset to Defaults", "Restablecer Valores Predeterminados")))
                {
                    bool confirm = EditorUtility.DisplayDialog(
                        GetLocalizedText("Reset Settings", "Restablecer Configuración"),
                        GetLocalizedText(
                            "Are you sure you want to reset all settings to defaults?",
                            "¿Estás seguro de que quieres restablecer toda la configuración?"
                        ),
                        GetLocalizedText("Yes", "Sí"),
                        GetLocalizedText("Cancel", "Cancelar")
                    );

                    if (confirm)
                    {
                        settings.ResetToDefaults();
                        RefreshModuleCache();
                    }
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
            DrawSeparator();
        }

        #endregion

        #region Statistics Section

        private void DrawStatisticsSection()
        {
            GUILayout.Space(10);

            showStatisticsSection = EditorGUILayout.Foldout(
                showStatisticsSection,
                GetLocalizedText("📊 STATISTICS", "📊 ESTADÍSTICAS"),
                true,
                EditorStyles.foldoutHeader
            );

            if (showStatisticsSection)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);

                var stats = ModuleManager.GetStatistics();

                EditorGUILayout.LabelField(
                    GetLocalizedText("Total Modules", "Total de Módulos"),
                    stats.TotalModules.ToString()
                );
                EditorGUILayout.LabelField(
                    GetLocalizedText("Available Modules", "Módulos Disponibles"),
                    stats.AvailableModules.ToString()
                );
                EditorGUILayout.LabelField(
                    GetLocalizedText("Times Opened", "Veces Abierto"),
                    settings.TotalOpenCount.ToString()
                );

                if (settings.FirstUseDate != System.DateTime.MinValue)
                    EditorGUILayout.LabelField(
                        GetLocalizedText("First Used", "Primer Uso"),
                        settings.FirstUseDate.ToShortDateString()
                    );

                if (settings.LastUseDate != System.DateTime.MinValue)
                    EditorGUILayout.LabelField(
                        GetLocalizedText("Last Used", "Último Uso"),
                        settings.LastUseDate.ToShortDateString()
                    );

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
            DrawSeparator();
        }

        #endregion

        #region Links Section

        private void DrawLinksSection()
        {
            GUILayout.Space(10);

            GUILayout.Label(
                GetLocalizedText("📚 RESOURCES", "📚 RECURSOS"),
                EditorStyles.boldLabel
            );

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(GetLocalizedText("📖 Documentation", "📖 Documentación")))
                Application.OpenURL("https://github.com/FredoCubap/Cubas-Games-Tools");

            if (GUILayout.Button(GetLocalizedText("🐛 Report Bug", "🐛 Reportar Error")))
                Application.OpenURL("https://github.com/FredoCubap/Cubas-Games-Tools/issues");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(GetLocalizedText("📧 Contact", "📧 Contacto")))
                Application.OpenURL("mailto:support@cubasgames.com");

            if (GUILayout.Button(GetLocalizedText("🎥 Video Tutorials", "🎥 Tutoriales en Video")))
                Application.OpenURL("https://youtube.com/@cubasgames");

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            DrawSeparator();
        }

        #endregion

        #region Footer

        private void DrawFooter()
        {
            GUILayout.Space(10);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("CGTools v1.0.0 | © 2026 Cubas Games", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);
        }

        #endregion

        #region Utilities

        private void LoadLogo()
        {
            string[] possiblePaths =
            {
                "Assets/CGTools/Core/Resources/Images/Logo.png",
                "Assets/CGTools/Core/Resources/Images/Banner.png",
                "Assets/CGTools/Core/Resources/Logo.png"
            };

            foreach (string path in possiblePaths)
            {
                logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (logoTexture != null)
                    break;
            }
        }

        private void RefreshModuleCache()
        {
            cachedModules = new List<ICGModule>(ModuleManager.AvailableModules);
            lastModuleRefreshTime = EditorApplication.timeSinceStartup;
        }

        private string GetLocalizedText(string english, string spanish)
        {
            return settings != null && settings.Language == SystemLanguage.Spanish ? spanish : english;
        }

        private void DrawSeparator()
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        #endregion
    }
}