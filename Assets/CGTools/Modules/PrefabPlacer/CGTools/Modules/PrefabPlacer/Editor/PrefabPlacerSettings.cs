using System;
using System.Collections.Generic;
using UnityEngine;

namespace CGTools.Modules.PrefabPlacer
{
    /// <summary>
    /// Persistent settings for Prefab Placer module.
    /// Configuración persistente para el módulo Prefab Placer.
    /// </summary>
    [Serializable]
    public class PrefabPlacerSettings
    {
        #region Prefab Slots / Slots de Prefabs

        [Tooltip("List of prefab paths to paint / Lista de rutas de prefabs para pintar")]
        public List<string> prefabPaths = new List<string>();

        #endregion

        #region Brush Settings / Configuración de Pincel

        [Tooltip("Brush radius in units / Radio del pincel en unidades")]
        [Range(0.1f, 100f)]
        public float brushSize = 5.0f;

        [Tooltip("Number of prefabs to place per stroke / Número de prefabs a colocar por trazo")]
        [Range(1, 100)]
        public int quantity = 10;

        [Tooltip("Brush shape type / Tipo de forma del pincel")]
        public BrushShape brushShape = BrushShape.Circle;

        #endregion

        #region Rotation Settings / Configuración de Rotación

        [Tooltip("Rotation mode / Modo de rotación")]
        public RotationMode rotationMode = RotationMode.SurfaceAligned;

        [Tooltip("Fixed rotation values / Valores de rotación fijos")]
        public Vector3 fixedRotation = Vector3.zero;

        [Tooltip("Enable random rotation / Habilitar rotación aleatoria")]
        public bool randomRotation = false;

        [Tooltip("Minimum random rotation per axis / Rotación aleatoria mínima por eje")]
        public Vector3 randomRotationMin = new Vector3(-15f, -180f, -15f);

        [Tooltip("Maximum random rotation per axis / Rotación aleatoria máxima por eje")]
        public Vector3 randomRotationMax = new Vector3(15f, 180f, 15f);

        #endregion

        #region Scale Settings / Configuración de Escala

        [Tooltip("Base scale for all prefabs / Escala base para todos los prefabs")]
        public Vector3 baseScale = Vector3.one;

        [Tooltip("Enable random scale variation / Habilitar variación de escala aleatoria")]
        public bool randomScale = false;

        [Tooltip("Random scale variation percentage / Porcentaje de variación de escala aleatoria")]
        [Range(0f, 100f)]
        public float scaleVariation = 20f;

        [Tooltip("Keep uniform scale when randomizing / Mantener escala uniforme al aleatorizar")]
        public bool uniformRandomScale = true;

        #endregion

        #region Advanced Settings / Configuración Avanzada

        [Tooltip("Minimum distance between placed objects / Distancia mínima entre objetos colocados")]
        [Range(0f, 10f)]
        public float minDistance = 1.0f;

        [Tooltip("Layer mask for surface detection / Máscara de capas para detección de superficie")]
        public int layerMask = -1; // Everything

        [Tooltip("Maximum slope angle in degrees / Ángulo de pendiente máximo en grados")]
        [Range(0f, 90f)]
        public float maxSlopeAngle = 45f;

        [Tooltip("Offset from surface / Desplazamiento desde la superficie")]
        public float surfaceOffset = 0f;

        [Tooltip("Align to surface normal / Alinear a la normal de la superficie")]
        public bool alignToSurface = true;

        #endregion

        #region Paint Mode / Modo de Pintado

        [Tooltip("Current painting mode / Modo de pintado actual")]
        public PaintMode currentMode = PaintMode.Paint;

        #endregion

        #region UI State / Estado de UI

        [Tooltip("Show advanced settings section / Mostrar sección de configuración avanzada")]
        public bool showAdvancedSettings = false;

        [Tooltip("Show statistics section / Mostrar sección de estadísticas")]
        public bool showStatistics = true;

        #endregion

        #region Statistics / Estadísticas

        [Tooltip("Total objects painted in current session / Total de objetos pintados en sesión actual")]
        public int totalObjectsPainted = 0;

        [Tooltip("Total objects erased in current session / Total de objetos borrados en sesión actual")]
        public int totalObjectsErased = 0;

        #endregion

        #region Presets / Preajustes

        [Tooltip("Saved presets / Preajustes guardados")]
        public List<PrefabPlacerPreset> presets = new List<PrefabPlacerPreset>();

        #endregion

        #region Constructor / Constructor

        /// <summary>
        /// Default constructor with sensible defaults
        /// Constructor por defecto con valores sensatos
        /// </summary>
        public PrefabPlacerSettings()
        {
            // Defaults are set via field initializers
            // Los valores por defecto se establecen mediante inicializadores de campo
        }

        #endregion

        #region Utility Methods / Métodos Auxiliares

        /// <summary>
        /// Reset all settings to default values
        /// Restablecer toda la configuración a valores predeterminados
        /// </summary>
        public void ResetToDefaults()
        {
            prefabPaths.Clear();
            brushSize = 5.0f;
            quantity = 10;
            brushShape = BrushShape.Circle;
            rotationMode = RotationMode.SurfaceAligned;
            fixedRotation = Vector3.zero;
            randomRotation = false;
            randomRotationMin = new Vector3(-15f, -180f, -15f);
            randomRotationMax = new Vector3(15f, 180f, 15f);
            baseScale = Vector3.one;
            randomScale = false;
            scaleVariation = 20f;
            uniformRandomScale = true;
            minDistance = 1.0f;
            layerMask = -1;
            maxSlopeAngle = 45f;
            surfaceOffset = 0f;
            alignToSurface = true;
            currentMode = PaintMode.Paint;
            showAdvancedSettings = false;
            showStatistics = true;
            totalObjectsPainted = 0;
            totalObjectsErased = 0;
        }

        /// <summary>
        /// Create a deep copy of these settings
        /// Crear una copia profunda de esta configuración
        /// </summary>
        public PrefabPlacerSettings Clone()
        {
            PrefabPlacerSettings clone = new PrefabPlacerSettings();
            clone.prefabPaths = new List<string>(prefabPaths);
            clone.brushSize = brushSize;
            clone.quantity = quantity;
            clone.brushShape = brushShape;
            clone.rotationMode = rotationMode;
            clone.fixedRotation = fixedRotation;
            clone.randomRotation = randomRotation;
            clone.randomRotationMin = randomRotationMin;
            clone.randomRotationMax = randomRotationMax;
            clone.baseScale = baseScale;
            clone.randomScale = randomScale;
            clone.scaleVariation = scaleVariation;
            clone.uniformRandomScale = uniformRandomScale;
            clone.minDistance = minDistance;
            clone.layerMask = layerMask;
            clone.maxSlopeAngle = maxSlopeAngle;
            clone.surfaceOffset = surfaceOffset;
            clone.alignToSurface = alignToSurface;
            clone.currentMode = currentMode;
            clone.showAdvancedSettings = showAdvancedSettings;
            clone.showStatistics = showStatistics;
            clone.totalObjectsPainted = totalObjectsPainted;
            clone.totalObjectsErased = totalObjectsErased;
            clone.presets = new List<PrefabPlacerPreset>(presets);
            return clone;
        }

        /// <summary>
        /// Validate settings and fix any invalid values
        /// Validar configuración y corregir valores inválidos
        /// </summary>
        public void Validate()
        {
            brushSize = Mathf.Clamp(brushSize, 0.1f, 100f);
            quantity = Mathf.Clamp(quantity, 1, 100);
            minDistance = Mathf.Clamp(minDistance, 0f, 10f);
            maxSlopeAngle = Mathf.Clamp(maxSlopeAngle, 0f, 90f);
            scaleVariation = Mathf.Clamp(scaleVariation, 0f, 100f);

            // Ensure prefab paths list exists
            // Asegurar que existe la lista de rutas de prefabs
            if (prefabPaths == null)
                prefabPaths = new List<string>();

            // Ensure presets list exists
            // Asegurar que existe la lista de preajustes
            if (presets == null)
                presets = new List<PrefabPlacerPreset>();
        }

        #endregion
    }

    #region Enums / Enumeraciones

    /// <summary>
    /// Brush shape types / Tipos de forma de pincel
    /// </summary>
    [Serializable]
    public enum BrushShape
    {
        Circle,
        Square
    }

    /// <summary>
    /// Rotation modes / Modos de rotación
    /// </summary>
    [Serializable]
    public enum RotationMode
    {
        [Tooltip("Align to surface normal / Alinear a la normal de la superficie")]
        SurfaceAligned,

        [Tooltip("Use fixed rotation values / Usar valores de rotación fijos")]
        Fixed,

        [Tooltip("Random rotation within range / Rotación aleatoria dentro de rango")]
        Random,

        [Tooltip("Surface aligned + random offset / Alineado a superficie + desplazamiento aleatorio")]
        SurfaceAlignedWithRandomOffset
    }

    /// <summary>
    /// Paint modes / Modos de pintado
    /// </summary>
    [Serializable]
    public enum PaintMode
    {
        Paint,
        Erase
    }

    #endregion

    #region Preset System / Sistema de Preajustes

    /// <summary>
    /// Preset configuration for quick switching
    /// Configuración de preajuste para cambio rápido
    /// </summary>
    [Serializable]
    public class PrefabPlacerPreset
    {
        public string presetName = "New Preset";
        public List<string> prefabPaths = new List<string>();
        public float brushSize = 5.0f;
        public int quantity = 10;
        public BrushShape brushShape = BrushShape.Circle;
        public RotationMode rotationMode = RotationMode.SurfaceAligned;
        public Vector3 fixedRotation = Vector3.zero;
        public bool randomRotation = false;
        public Vector3 randomRotationMin = new Vector3(-15f, -180f, -15f);
        public Vector3 randomRotationMax = new Vector3(15f, 180f, 15f);
        public Vector3 baseScale = Vector3.one;
        public bool randomScale = false;
        public float scaleVariation = 20f;
        public bool uniformRandomScale = true;
        public float minDistance = 1.0f;
        public float maxSlopeAngle = 45f;

        /// <summary>
        /// Create preset from current settings
        /// Crear preajuste desde configuración actual
        /// </summary>
        public static PrefabPlacerPreset CreateFromSettings(PrefabPlacerSettings settings, string name)
        {
            PrefabPlacerPreset preset = new PrefabPlacerPreset();
            preset.presetName = name;
            preset.prefabPaths = new List<string>(settings.prefabPaths);
            preset.brushSize = settings.brushSize;
            preset.quantity = settings.quantity;
            preset.brushShape = settings.brushShape;
            preset.rotationMode = settings.rotationMode;
            preset.fixedRotation = settings.fixedRotation;
            preset.randomRotation = settings.randomRotation;
            preset.randomRotationMin = settings.randomRotationMin;
            preset.randomRotationMax = settings.randomRotationMax;
            preset.baseScale = settings.baseScale;
            preset.randomScale = settings.randomScale;
            preset.scaleVariation = settings.scaleVariation;
            preset.uniformRandomScale = settings.uniformRandomScale;
            preset.minDistance = settings.minDistance;
            preset.maxSlopeAngle = settings.maxSlopeAngle;
            return preset;
        }

        /// <summary>
        /// Apply this preset to settings
        /// Aplicar este preajuste a la configuración
        /// </summary>
        public void ApplyToSettings(PrefabPlacerSettings settings)
        {
            settings.prefabPaths = new List<string>(prefabPaths);
            settings.brushSize = brushSize;
            settings.quantity = quantity;
            settings.brushShape = brushShape;
            settings.rotationMode = rotationMode;
            settings.fixedRotation = fixedRotation;
            settings.randomRotation = randomRotation;
            settings.randomRotationMin = randomRotationMin;
            settings.randomRotationMax = randomRotationMax;
            settings.baseScale = baseScale;
            settings.randomScale = randomScale;
            settings.scaleVariation = scaleVariation;
            settings.uniformRandomScale = uniformRandomScale;
            settings.minDistance = minDistance;
            settings.maxSlopeAngle = maxSlopeAngle;
        }
    }

    #endregion
}