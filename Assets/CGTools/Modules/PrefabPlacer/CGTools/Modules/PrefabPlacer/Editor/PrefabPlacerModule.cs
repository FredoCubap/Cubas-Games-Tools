using UnityEngine;
using UnityEditor;
using CGTools.Core;

namespace CGTools.Modules.PrefabPlacer
{
    /// <summary>
    /// Prefab Placer module registration and metadata.
    /// Registro y metadata del módulo Prefab Placer.
    /// </summary>
    public class PrefabPlacerModule : CGModuleBase
    {
        private static PrefabPlacerModule instance;
        private Texture2D cachedIcon;

        #region ICGModule Implementation / Implementación de ICGModule

        public override string ModuleID => "PrefabPlacer";

        public override string ModuleNameEN => "Prefab Placer";

        public override string ModuleNameES => "Colocador de Prefabs";

        public override string DescriptionEN => "Paint prefabs on any surface with advanced brush controls";

        public override string DescriptionES => "Pinta prefabs en cualquier superficie con controles avanzados de pincel";

        public override string Version => "1.0.0";

        public override string MinUnityVersion => "2021.3";

        public override Texture2D Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    LoadIcon();
                }
                return cachedIcon;
            }
        }

        public override bool IsInstalled => true;

        public override bool IsCompatible
        {
            get
            {
#if UNITY_2021_3_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Module Lifecycle / Ciclo de Vida del Módulo

        /// <summary>
        /// Static constructor - auto-registers the module
        /// Constructor estático - auto-registra el módulo
        /// </summary>
        static PrefabPlacerModule()
        {
            // Module will be auto-detected by ModuleManager using reflection
            // El módulo será auto-detectado por ModuleManager usando reflexión
        }

        public override void OnModuleRegistered()
        {
            if (CGToolsSettings.Instance.EnableDebugLogs)
            {
                Debug.Log($"[CGTools] {ModuleNameEN} module registered successfully.");
            }
        }

        public override void OnSettingsSaved()
        {
            // Reload settings when global settings are saved
            // Recargar configuración cuando se guardan los ajustes globales
            if (PrefabPlacerWindow.Instance != null)
            {
                PrefabPlacerWindow.Instance.OnSettingsChanged();
            }
        }

        #endregion

        #region Window Management / Gestión de Ventana

        public override void OpenWindow()
        {
            PrefabPlacerWindow.ShowWindow();
        }

        /// <summary>
        /// Menu item to open Prefab Placer window
        /// Elemento de menú para abrir la ventana de Prefab Placer
        /// </summary>
        [MenuItem("Tools/CGTools/Prefab Placer 🎨", false, 10)]
        public static void OpenFromMenu()
        {
            PrefabPlacerWindow.ShowWindow();
        }

        #endregion

        #region Settings Management / Gestión de Configuración

        public override object GetModuleSettings()
        {
            return CGToolsSettings.Instance.LoadModuleSettings<PrefabPlacerSettings>(ModuleID);
        }

        public override void SetModuleSettings(object settings)
        {
            if (settings is PrefabPlacerSettings placerSettings)
            {
                CGToolsSettings.Instance.SaveModuleSettings(ModuleID, placerSettings);
            }
        }

        /// <summary>
        /// Load settings for this module
        /// Cargar configuración para este módulo
        /// </summary>
        public static PrefabPlacerSettings LoadSettings()
        {
            return CGToolsSettings.Instance.LoadModuleSettings<PrefabPlacerSettings>("PrefabPlacer");
        }

        /// <summary>
        /// Save settings for this module
        /// Guardar configuración para este módulo
        /// </summary>
        public static void SaveSettings(PrefabPlacerSettings settings)
        {
            CGToolsSettings.Instance.SaveModuleSettings("PrefabPlacer", settings);
        }

        #endregion

        #region Icon Loading / Carga de Ícono

        private void LoadIcon()
        {
            // Try to load icon from Resources
            // Intentar cargar ícono desde Resources
            string[] possiblePaths = new string[]
            {
                "Assets/CGTools/Modules/PrefabPlacer/Resources/Icons/PrefabPlacerIcon.png",
                "Assets/CGTools/Modules/PrefabPlacer/Resources/PrefabPlacerIcon.png",
                "Assets/CGTools/Modules/PrefabPlacer/Editor/Icons/PrefabPlacerIcon.png"
            };

            foreach (string path in possiblePaths)
            {
                cachedIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (cachedIcon != null)
                {
                    if (CGToolsSettings.Instance.EnableDebugLogs)
                    {
                        Debug.Log($"[CGTools] {ModuleNameEN} icon loaded from: {path}");
                    }
                    return;
                }
            }

            // Icon not found - will use default emoji in Hub
            // Ícono no encontrado - usará emoji por defecto en el Hub
            if (CGToolsSettings.Instance.EnableDebugLogs)
            {
                Debug.LogWarning($"[CGTools] {ModuleNameEN} icon not found. Using default.");
            }
        }

        #endregion

        #region Helper Methods / Métodos Auxiliares

        /// <summary>
        /// Get singleton instance
        /// Obtener instancia singleton
        /// </summary>
        public static PrefabPlacerModule Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PrefabPlacerModule();
                }
                return instance;
            }
        }

        /// <summary>
        /// Check if module is ready to use
        /// Verificar si el módulo está listo para usar
        /// </summary>
        public static bool IsReady()
        {
            return Instance.IsInstalled && Instance.IsCompatible;
        }

        #endregion
    }
}