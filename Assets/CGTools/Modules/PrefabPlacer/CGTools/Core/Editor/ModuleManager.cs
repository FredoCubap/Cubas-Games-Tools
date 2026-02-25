using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CGTools.Core
{
    /// <summary>
    /// Manages module registration, detection, and lifecycle.
    /// Gestiona el registro, detección y ciclo de vida de los módulos.
    /// </summary>
    public static class ModuleManager
    {
        private static List<ICGModule> registeredModules = new List<ICGModule>();
        private static bool isInitialized = false;

        /// <summary>
        /// All registered modules / Todos los módulos registrados
        /// </summary>
        public static IReadOnlyList<ICGModule> RegisteredModules => registeredModules.AsReadOnly();

        /// <summary>
        /// Only installed and compatible modules / Solo módulos instalados y compatibles
        /// </summary>
        public static IReadOnlyList<ICGModule> AvailableModules
        {
            get
            {
                return registeredModules
                    .Where(m => m.IsInstalled && m.IsCompatible)
                    .ToList()
                    .AsReadOnly();
            }
        }

        /// <summary>
        /// Initialize the module manager and auto-detect modules
        /// Inicializar el gestor de módulos y auto-detectar módulos
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            registeredModules.Clear();
            AutoDetectModules();
            isInitialized = true;

            Debug.Log($"[CGTools] ModuleManager initialized. Found {registeredModules.Count} module(s).");
        }

        /// <summary>
        /// Manually register a module
        /// Registrar un módulo manualmente
        /// </summary>
        public static void RegisterModule(ICGModule module)
        {
            if (module == null)
            {
                Debug.LogError("[CGTools] Attempted to register null module.");
                return;
            }

            if (registeredModules.Any(m => m.ModuleID == module.ModuleID))
            {
                Debug.LogWarning($"[CGTools] Module '{module.ModuleID}' is already registered.");
                return;
            }

            registeredModules.Add(module);
            module.OnModuleRegistered();

            Debug.Log($"[CGTools] Module registered: {module.ModuleID} v{module.Version}");
        }

        /// <summary>
        /// Unregister a module by ID
        /// Desregistrar un módulo por ID
        /// </summary>
        public static void UnregisterModule(string moduleID)
        {
            var module = registeredModules.FirstOrDefault(m => m.ModuleID == moduleID);
            if (module != null)
            {
                registeredModules.Remove(module);
                Debug.Log($"[CGTools] Module unregistered: {moduleID}");
            }
        }

        /// <summary>
        /// Get a specific module by ID
        /// Obtener un módulo específico por ID
        /// </summary>
        public static ICGModule GetModule(string moduleID)
        {
            return registeredModules.FirstOrDefault(m => m.ModuleID == moduleID);
        }

        /// <summary>
        /// Check if a module is registered and available
        /// Verificar si un módulo está registrado y disponible
        /// </summary>
        public static bool IsModuleAvailable(string moduleID)
        {
            var module = GetModule(moduleID);
            return module != null && module.IsInstalled && module.IsCompatible;
        }

        /// <summary>
        /// Open a module's window by ID
        /// Abrir la ventana de un módulo por ID
        /// </summary>
        public static void OpenModule(string moduleID)
        {
            var module = GetModule(moduleID);
            if (module == null)
            {
                Debug.LogError($"[CGTools] Module '{moduleID}' not found.");
                return;
            }

            if (!module.IsInstalled)
            {
                Debug.LogError($"[CGTools] Module '{moduleID}' is not installed.");
                return;
            }

            if (!module.IsCompatible)
            {
                Debug.LogError($"[CGTools] Module '{moduleID}' is not compatible with this Unity version.");
                return;
            }

            try
            {
                module.OpenWindow();
            }
            catch (Exception e)
            {
                Debug.LogError($"[CGTools] Error opening module '{moduleID}': {e.Message}");
            }
        }

        /// <summary>
        /// Notify all modules that settings have been saved
        /// Notificar a todos los módulos que se han guardado los ajustes
        /// </summary>
        public static void NotifySettingsSaved()
        {
            foreach (var module in registeredModules)
            {
                try
                {
                    module.OnSettingsSaved();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CGTools] Error notifying module '{module.ModuleID}': {e.Message}");
                }
            }
        }

        /// <summary>
        /// Auto-detect modules using reflection
        /// Auto-detectar módulos usando reflexión
        /// </summary>
        private static void AutoDetectModules()
        {
            try
            {
                // Get all assemblies in the current domain
                // Obtener todos los ensamblados del dominio actual
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // Find all types that implement ICGModule
                        // Buscar todos los tipos que implementan ICGModule
                        var moduleTypes = assembly.GetTypes()
                            .Where(t => typeof(ICGModule).IsAssignableFrom(t) 
                                     && !t.IsInterface 
                                     && !t.IsAbstract);

                        foreach (var moduleType in moduleTypes)
                        {
                            try
                            {
                                // Create instance and register
                                // Crear instancia y registrar
                                var module = (ICGModule)Activator.CreateInstance(moduleType);
                                RegisterModule(module);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"[CGTools] Could not instantiate module '{moduleType.Name}': {e.Message}");
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Some assemblies might fail to load types, skip them
                        // Algunos ensamblados pueden fallar al cargar tipos, omitirlos
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CGTools] Error during module auto-detection: {e.Message}");
            }
        }

        /// <summary>
        /// Get statistics about registered modules
        /// Obtener estadísticas sobre los módulos registrados
        /// </summary>
        public static ModuleStatistics GetStatistics()
        {
            return new ModuleStatistics
            {
                TotalModules = registeredModules.Count,
                InstalledModules = registeredModules.Count(m => m.IsInstalled),
                CompatibleModules = registeredModules.Count(m => m.IsCompatible),
                AvailableModules = registeredModules.Count(m => m.IsInstalled && m.IsCompatible),
                IncompatibleModules = registeredModules.Count(m => m.IsInstalled && !m.IsCompatible)
            };
        }

        /// <summary>
        /// Refresh module detection (useful after importing new modules)
        /// Refrescar detección de módulos (útil después de importar nuevos módulos)
        /// </summary>
        public static void RefreshModules()
        {
            isInitialized = false;
            Initialize();
        }

        /// <summary>
        /// Get all modules sorted by name
        /// Obtener todos los módulos ordenados por nombre
        /// </summary>
        public static List<ICGModule> GetModulesSorted(SystemLanguage language)
        {
            return registeredModules
                .OrderBy(m => language == SystemLanguage.Spanish ? m.ModuleNameES : m.ModuleNameEN)
                .ToList();
        }

        /// <summary>
        /// Check if any modules need updates (placeholder for future update system)
        /// Verificar si algún módulo necesita actualizaciones (placeholder para sistema futuro)
        /// </summary>
        public static bool HasUpdatesAvailable()
        {
            // TODO: Implement update checking system
            // TODO: Implementar sistema de verificación de actualizaciones
            return false;
        }
    }

    /// <summary>
    /// Statistics about registered modules
    /// Estadísticas sobre los módulos registrados
    /// </summary>
    public struct ModuleStatistics
    {
        public int TotalModules;
        public int InstalledModules;
        public int CompatibleModules;
        public int AvailableModules;
        public int IncompatibleModules;
    }
}