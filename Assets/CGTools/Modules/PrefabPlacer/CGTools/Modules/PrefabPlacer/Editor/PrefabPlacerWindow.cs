using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CGTools.Core;
using UnityEditorInternal;

namespace CGTools.Modules.PrefabPlacer
{
    /// <summary>
    /// Main editor window for the Prefab Placer module.
    /// </summary>
    public class PrefabPlacerWindow : EditorWindow
    {
        #region Static Instance

        private static PrefabPlacerWindow instance;
        public static PrefabPlacerWindow Instance => instance;

        #endregion

        #region Data

        private PrefabPlacerSettings settings;
        private List<GameObject> loadedPrefabs = new List<GameObject>();
        private Vector2 scrollPosition;
        private SystemLanguage currentLanguage;
        private string presetNameInput = "New Preset";

        // Dirty flag — settings are written to disk only on disable or explicit user actions,
        // not on every MouseDrag or keyboard event.
        private bool settingsDirty = false;

        #endregion

        #region Window Management

        [MenuItem("Tools/CGTools/Prefab Placer 🎨", false, 10)]
        public static void ShowWindow()
        {
            PrefabPlacerWindow window = GetWindow<PrefabPlacerWindow>("Prefab Placer");
            window.minSize = new Vector2(350, 600);
            window.Show();
        }

        private void OnEnable()
        {
            instance = this;

            settings = PrefabPlacerModule.LoadSettings() ?? new PrefabPlacerSettings();
            settings.Validate();

            currentLanguage = CGToolsSettings.Instance.Language;

            LoadPrefabsFromPaths();

            SceneView.duringSceneGui += OnSceneGUI;

            settings.totalObjectsPainted = 0;
            settings.totalObjectsErased = 0;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            FlushSettings();

            instance = null;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawModeButtons();
            DrawPrefabSlots();
            DrawBrushSettings();
            DrawRotationSettings();
            DrawScaleSettings();
            DrawAdvancedSettings();
            DrawPresets();
            DrawStatistics();

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        #endregion

        #region Header

        private void DrawHeader()
        {
            GUILayout.Space(10);

            GUILayout.Label(
                GetLocalizedText("🎨 PREFAB PLACER", "🎨 COLOCADOR DE PREFABS"),
                EditorStyles.boldLabel
            );

            EditorGUILayout.LabelField($"v{PrefabPlacerModule.Instance.Version}", EditorStyles.miniLabel);

            GUILayout.Space(5);

            EditorGUILayout.HelpBox(
                GetLocalizedText(
                    "Click/Drag in Scene View to paint or erase prefabs.",
                    "Clic/Arrastra en la Vista de Escena para pintar o borrar prefabs."
                ),
                MessageType.Info
            );

            GUILayout.Space(5);
            DrawSeparator();
        }

        #endregion

        #region Mode Buttons

        private void DrawModeButtons()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = settings.currentMode == PaintMode.Paint ? Color.green : Color.white;
            if (GUILayout.Button(GetLocalizedText("🖌️ Paint Mode", "🖌️ Modo Pintar"), GUILayout.Height(30)))
            {
                settings.currentMode = PaintMode.Paint;
                MarkDirty();
            }

            GUI.backgroundColor = settings.currentMode == PaintMode.Erase ? Color.red : Color.white;
            if (GUILayout.Button(GetLocalizedText("🧹 Erase Mode", "🧹 Modo Borrar"), GUILayout.Height(30)))
            {
                settings.currentMode = PaintMode.Erase;
                MarkDirty();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.LabelField(
                GetLocalizedText(
                    "Shortcuts: [P] Paint | [E] Erase | [ / ] Brush Size",
                    "Atajos: [P] Pintar | [E] Borrar | [ / ] Tamaño Pincel"
                ),
                EditorStyles.centeredGreyMiniLabel
            );

            GUILayout.Space(5);
            DrawSeparator();
        }

        #endregion

        #region Prefab Slots

        private void DrawPrefabSlots()
        {
            GUILayout.Space(10);

            GUILayout.Label(GetLocalizedText("📦 PREFABS", "📦 PREFABS"), EditorStyles.boldLabel);
            GUILayout.Space(5);

            for (int i = 0; i < settings.prefabPaths.Count; i++)
                DrawPrefabSlot(i);

            GUILayout.Space(5);

            if (GUILayout.Button(GetLocalizedText("+ Add Prefab Slot", "+ Agregar Slot de Prefab")))
            {
                settings.prefabPaths.Add("");
                loadedPrefabs.Add(null);
            }

            GUILayout.Space(5);
            DrawSeparator();
        }

        private void DrawPrefabSlot(int index)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField(
                loadedPrefabs[index],
                typeof(GameObject),
                false
            );

            if (EditorGUI.EndChangeCheck())
            {
                loadedPrefabs[index] = newPrefab;
                settings.prefabPaths[index] = newPrefab != null ? AssetDatabase.GetAssetPath(newPrefab) : "";
                FlushSettings();
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                settings.prefabPaths.RemoveAt(index);
                loadedPrefabs.RemoveAt(index);
                FlushSettings();
            }

            EditorGUI.BeginDisabledGroup(index == 0);
            if (GUILayout.Button("↑", GUILayout.Width(25)))
            {
                string tempPath = settings.prefabPaths[index];
                GameObject tempPrefab = loadedPrefabs[index];
                settings.prefabPaths[index] = settings.prefabPaths[index - 1];
                loadedPrefabs[index] = loadedPrefabs[index - 1];
                settings.prefabPaths[index - 1] = tempPath;
                loadedPrefabs[index - 1] = tempPrefab;
                FlushSettings();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(index == settings.prefabPaths.Count - 1);
            if (GUILayout.Button("↓", GUILayout.Width(25)))
            {
                string tempPath = settings.prefabPaths[index];
                GameObject tempPrefab = loadedPrefabs[index];
                settings.prefabPaths[index] = settings.prefabPaths[index + 1];
                loadedPrefabs[index] = loadedPrefabs[index + 1];
                settings.prefabPaths[index + 1] = tempPath;
                loadedPrefabs[index + 1] = tempPrefab;
                FlushSettings();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        #endregion

        #region Brush Settings

        private void DrawBrushSettings()
        {
            GUILayout.Space(10);

            GUILayout.Label(GetLocalizedText("🖌️ BRUSH SETTINGS", "🖌️ CONFIGURACIÓN DE PINCEL"), EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            float newBrushSize = EditorGUILayout.Slider(
                GetLocalizedText("Brush Size", "Tamaño Pincel"),
                settings.brushSize, 0.1f, 100f
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.brushSize = newBrushSize;
                MarkDirty();
            }

            EditorGUI.BeginChangeCheck();
            int newQuantity = EditorGUILayout.IntSlider(
                GetLocalizedText("Quantity", "Cantidad"),
                settings.quantity, 1, 100
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.quantity = newQuantity;
                MarkDirty();
            }

            EditorGUI.BeginChangeCheck();
            BrushShape newShape = (BrushShape)EditorGUILayout.EnumPopup(
                GetLocalizedText("Brush Shape", "Forma Pincel"),
                settings.brushShape
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.brushShape = newShape;
                MarkDirty();
            }

            GUILayout.Space(5);
            DrawSeparator();
        }

        #endregion

        #region Rotation Settings

        private void DrawRotationSettings()
        {
            GUILayout.Space(10);

            GUILayout.Label(GetLocalizedText("🔄 ROTATION", "🔄 ROTACIÓN"), EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            RotationMode newMode = (RotationMode)EditorGUILayout.EnumPopup(
                GetLocalizedText("Mode", "Modo"),
                settings.rotationMode
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.rotationMode = newMode;
                MarkDirty();
            }

            if (settings.rotationMode == RotationMode.Fixed)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newRotation = EditorGUILayout.Vector3Field(
                    GetLocalizedText("Fixed Rotation", "Rotación Fija"),
                    settings.fixedRotation
                );
                if (EditorGUI.EndChangeCheck())
                {
                    settings.fixedRotation = newRotation;
                    MarkDirty();
                }
            }

            EditorGUI.BeginChangeCheck();
            bool newRandomRotation = EditorGUILayout.Toggle(
                GetLocalizedText("Random Rotation", "Rotación Aleatoria"),
                settings.randomRotation
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.randomRotation = newRandomRotation;
                MarkDirty();
            }

            if (settings.randomRotation || settings.rotationMode == RotationMode.Random)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                Vector3 newMin = EditorGUILayout.Vector3Field(
                    GetLocalizedText("Min", "Mín"),
                    settings.randomRotationMin
                );
                Vector3 newMax = EditorGUILayout.Vector3Field(
                    GetLocalizedText("Max", "Máx"),
                    settings.randomRotationMax
                );
                if (EditorGUI.EndChangeCheck())
                {
                    settings.randomRotationMin = newMin;
                    settings.randomRotationMax = newMax;
                    MarkDirty();
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);
            DrawSeparator();
        }

        #endregion

        #region Scale Settings

        private void DrawScaleSettings()
        {
            GUILayout.Space(10);

            GUILayout.Label(GetLocalizedText("📏 SCALE", "📏 ESCALA"), EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            Vector3 newScale = EditorGUILayout.Vector3Field(
                GetLocalizedText("Base Scale", "Escala Base"),
                settings.baseScale
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.baseScale = newScale;
                MarkDirty();
            }

            EditorGUI.BeginChangeCheck();
            bool newRandomScale = EditorGUILayout.Toggle(
                GetLocalizedText("Random Scale", "Escala Aleatoria"),
                settings.randomScale
            );
            if (EditorGUI.EndChangeCheck())
            {
                settings.randomScale = newRandomScale;
                MarkDirty();
            }

            if (settings.randomScale)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                float newVariation = EditorGUILayout.Slider(
                    GetLocalizedText("Variation %", "Variación %"),
                    settings.scaleVariation, 0f, 100f
                );
                bool newUniform = EditorGUILayout.Toggle(
                    GetLocalizedText("Uniform", "Uniforme"),
                    settings.uniformRandomScale
                );
                if (EditorGUI.EndChangeCheck())
                {
                    settings.scaleVariation = newVariation;
                    settings.uniformRandomScale = newUniform;
                    MarkDirty();
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);
            DrawSeparator();
        }

        #endregion

        #region Advanced Settings

        private void DrawAdvancedSettings()
        {
            GUILayout.Space(10);

            settings.showAdvancedSettings = EditorGUILayout.Foldout(
                settings.showAdvancedSettings,
                GetLocalizedText("⚙️ ADVANCED SETTINGS", "⚙️ CONFIGURACIÓN AVANZADA"),
                true,
                EditorStyles.foldoutHeader
            );

            if (settings.showAdvancedSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();

                float newMinDistance = EditorGUILayout.Slider(
                    GetLocalizedText("Min Distance", "Distancia Mínima"),
                    settings.minDistance, 0f, 10f
                );

                LayerMask tempMask = settings.layerMask;
                LayerMask newLayerMask = EditorGUILayout.MaskField(
                    GetLocalizedText("Layer Mask", "Máscara de Capas"),
                    InternalEditorUtility.LayerMaskToConcatenatedLayersMask(tempMask),
                    InternalEditorUtility.layers
                );
                int finalLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(newLayerMask);

                float newMaxSlope = EditorGUILayout.Slider(
                    GetLocalizedText("Max Slope Angle", "Ángulo Máx. Pendiente"),
                    settings.maxSlopeAngle, 0f, 90f
                );

                float newSurfaceOffset = EditorGUILayout.FloatField(
                    GetLocalizedText("Surface Offset", "Desplazamiento Superficie"),
                    settings.surfaceOffset
                );

                bool newAlignToSurface = EditorGUILayout.Toggle(
                    GetLocalizedText("Align to Surface", "Alinear a Superficie"),
                    settings.alignToSurface
                );

                if (EditorGUI.EndChangeCheck())
                {
                    settings.minDistance = newMinDistance;
                    settings.layerMask = finalLayerMask;
                    settings.maxSlopeAngle = newMaxSlope;
                    settings.surfaceOffset = newSurfaceOffset;
                    settings.alignToSurface = newAlignToSurface;
                    MarkDirty();
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(5);
            DrawSeparator();
        }

        #endregion

        #region Presets

        private void DrawPresets()
        {
            GUILayout.Space(10);

            GUILayout.Label(GetLocalizedText("💾 PRESETS", "💾 PREAJUSTES"), EditorStyles.boldLabel);
            GUILayout.Space(5);

            if (settings.presets.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();

                foreach (var preset in settings.presets)
                {
                    if (GUILayout.Button(preset.presetName, GUILayout.Height(25)))
                        LoadPreset(preset);
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            EditorGUILayout.BeginHorizontal();
            presetNameInput = EditorGUILayout.TextField(presetNameInput);

            if (GUILayout.Button(GetLocalizedText("Save", "Guardar"), GUILayout.Width(60)))
                SaveNewPreset(presetNameInput);

            EditorGUILayout.EndHorizontal();

            if (settings.presets.Count > 0)
            {
                if (GUILayout.Button(GetLocalizedText("Clear All Presets", "Limpiar Todos los Preajustes")))
                {
                    if (EditorUtility.DisplayDialog(
                        GetLocalizedText("Clear Presets", "Limpiar Preajustes"),
                        GetLocalizedText("Are you sure?", "¿Estás seguro?"),
                        GetLocalizedText("Yes", "Sí"),
                        GetLocalizedText("Cancel", "Cancelar")))
                    {
                        settings.presets.Clear();
                        FlushSettings();
                    }
                }
            }

            GUILayout.Space(5);
            DrawSeparator();
        }

        private void SaveNewPreset(string presetName)
        {
            if (string.IsNullOrWhiteSpace(presetName))
            {
                EditorUtility.DisplayDialog(
                    GetLocalizedText("Invalid Name", "Nombre Inválido"),
                    GetLocalizedText("Please enter a valid preset name.", "Por favor ingresa un nombre válido."),
                    "OK"
                );
                return;
            }

            settings.presets.Add(PrefabPlacerPreset.CreateFromSettings(settings, presetName));
            FlushSettings();
            presetNameInput = "New Preset";
        }

        private void LoadPreset(PrefabPlacerPreset preset)
        {
            preset.ApplyToSettings(settings);
            LoadPrefabsFromPaths();
            FlushSettings();
            Repaint();
        }

        #endregion

        #region Statistics

        private void DrawStatistics()
        {
            GUILayout.Space(10);

            settings.showStatistics = EditorGUILayout.Foldout(
                settings.showStatistics,
                GetLocalizedText("📊 STATISTICS", "📊 ESTADÍSTICAS"),
                true,
                EditorStyles.foldoutHeader
            );

            if (settings.showStatistics)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField(
                    GetLocalizedText("Total in Scene", "Total en Escena"),
                    PrefabPlacerCore.GetPlacedObjectCount().ToString()
                );
                EditorGUILayout.LabelField(
                    GetLocalizedText("Painted (Session)", "Pintados (Sesión)"),
                    settings.totalObjectsPainted.ToString()
                );
                EditorGUILayout.LabelField(
                    GetLocalizedText("Erased (Session)", "Borrados (Sesión)"),
                    settings.totalObjectsErased.ToString()
                );

                GUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(GetLocalizedText("Select All", "Seleccionar Todos")))
                    PrefabPlacerCore.SelectAllPlacedObjects();

                if (GUILayout.Button(GetLocalizedText("Clear All", "Limpiar Todos")))
                {
                    if (EditorUtility.DisplayDialog(
                        GetLocalizedText("Clear All Objects", "Limpiar Todos los Objetos"),
                        GetLocalizedText(
                            "This will delete all placed prefabs. Are you sure?",
                            "Esto eliminará todos los prefabs colocados. ¿Estás seguro?"
                        ),
                        GetLocalizedText("Yes", "Sí"),
                        GetLocalizedText("Cancel", "Cancelar")))
                    {
                        settings.totalObjectsErased += PrefabPlacerCore.ClearAllPlacedObjects();
                        FlushSettings();
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
        }

        #endregion

        #region Scene View

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            HandleKeyboardShortcuts(e);
            PrefabPlacerCore.DrawBrushPreview(e, settings);

            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                if (e.button == 0 && !e.alt)
                {
                    HandleSceneViewInteraction(e);
                    e.Use();
                }
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        private void HandleKeyboardShortcuts(Event e)
        {
            if (e.type != EventType.KeyDown)
                return;

            switch (e.keyCode)
            {
                case KeyCode.P:
                    settings.currentMode = PaintMode.Paint;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.E:
                    settings.currentMode = PaintMode.Erase;
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.LeftBracket:
                    settings.brushSize = Mathf.Max(0.1f, settings.brushSize - 1f);
                    MarkDirty();
                    Repaint();
                    e.Use();
                    break;

                case KeyCode.RightBracket:
                    settings.brushSize = Mathf.Min(100f, settings.brushSize + 1f);
                    MarkDirty();
                    Repaint();
                    e.Use();
                    break;
            }
        }

        private void HandleSceneViewInteraction(Event e)
        {
            if (settings.currentMode == PaintMode.Paint)
            {
                if (!PrefabPlacerCore.ValidateSettings(settings, loadedPrefabs))
                    return;

                settings.totalObjectsPainted += PrefabPlacerCore.PaintPrefabs(e, settings, loadedPrefabs);
                MarkDirty();
            }
            else if (settings.currentMode == PaintMode.Erase)
            {
                settings.totalObjectsErased += PrefabPlacerCore.ErasePrefabs(e, settings);
                MarkDirty();
            }

            SceneView.RepaintAll();
            Repaint();
        }

        #endregion

        #region Settings Management

        /// <summary>
        /// Marks settings as modified. Actual disk write happens in FlushSettings().
        /// </summary>
        private void MarkDirty()
        {
            settingsDirty = true;
        }

        /// <summary>
        /// Writes settings to disk immediately. Use for explicit user actions (preset save, prefab change).
        /// </summary>
        private void FlushSettings()
        {
            PrefabPlacerModule.SaveSettings(settings);
            settingsDirty = false;
        }

        public void OnSettingsChanged()
        {
            currentLanguage = CGToolsSettings.Instance.Language;
            Repaint();
        }

        private void LoadPrefabsFromPaths()
        {
            loadedPrefabs.Clear();

            foreach (string path in settings.prefabPaths)
            {
                loadedPrefabs.Add(
                    !string.IsNullOrEmpty(path)
                        ? AssetDatabase.LoadAssetAtPath<GameObject>(path)
                        : null
                );
            }
        }

        #endregion

        #region UI Helpers

        private string GetLocalizedText(string english, string spanish)
        {
            return currentLanguage == SystemLanguage.Spanish ? spanish : english;
        }

        private void DrawSeparator()
        {
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        #endregion
    }
}