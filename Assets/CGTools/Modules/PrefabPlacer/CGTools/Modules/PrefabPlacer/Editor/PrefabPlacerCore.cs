using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CGTools.Modules.PrefabPlacer
{
    /// <summary>
    /// Core logic for Prefab Placer - painting, erasing, and placement algorithms.
    /// Lógica central para Prefab Placer - pintado, borrado y algoritmos de colocación.
    /// </summary>
    public static class PrefabPlacerCore
    {
        #region Paint Logic / Lógica de Pintado

        /// <summary>
        /// Paint prefabs at mouse position
        /// Pintar prefabs en la posición del mouse
        /// </summary>
        public static int PaintPrefabs(Event currentEvent, PrefabPlacerSettings settings, List<GameObject> loadedPrefabs)
        {
            if (loadedPrefabs == null || loadedPrefabs.Count == 0)
            {
                Debug.LogWarning("[CGTools] No prefabs loaded to paint.");
                return 0;
            }

            Vector3? hitPoint = GetMouseWorldPosition(currentEvent, settings.layerMask);
            if (!hitPoint.HasValue)
                return 0;

            int paintedCount = 0;

            for (int i = 0; i < settings.quantity; i++)
            {
                Vector3? spawnPosition = GetValidSpawnPosition(hitPoint.Value, settings);
                
                if (spawnPosition.HasValue)
                {
                    GameObject prefab = GetRandomPrefab(loadedPrefabs);
                    if (prefab != null)
                    {
                        GameObject instance = InstantiatePrefab(prefab, spawnPosition.Value, settings);
                        if (instance != null)
                        {
                            paintedCount++;
                        }
                    }
                }
            }

            return paintedCount;
        }

        #endregion

        #region Erase Logic / Lógica de Borrado

        /// <summary>
        /// Erase prefabs at mouse position
        /// Borrar prefabs en la posición del mouse
        /// </summary>
        public static int ErasePrefabs(Event currentEvent, PrefabPlacerSettings settings)
        {
            Vector3? hitPoint = GetMouseWorldPosition(currentEvent, settings.layerMask);
            if (!hitPoint.HasValue)
                return 0;

            int erasedCount = 0;
            Collider[] hitColliders = Physics.OverlapSphere(hitPoint.Value, settings.brushSize);

            foreach (Collider col in hitColliders)
            {
                if (col != null && PrefabPlacerTag.HasTag(col.gameObject))
                {
                    Undo.DestroyObjectImmediate(col.gameObject);
                    erasedCount++;
                }
            }

            return erasedCount;
        }

        #endregion

        #region Position & Raycast / Posición y Raycast

        /// <summary>
        /// Get world position from mouse using raycast
        /// Obtener posición mundial desde el mouse usando raycast
        /// </summary>
        public static Vector3? GetMouseWorldPosition(Event currentEvent, int layerMask)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return hit.point;
            }

            return null;
        }

        /// <summary>
        /// Get valid spawn position within brush area
        /// Obtener posición válida de spawn dentro del área del pincel
        /// </summary>
        private static Vector3? GetValidSpawnPosition(Vector3 centerPoint, PrefabPlacerSettings settings)
        {
            const int MAX_ATTEMPTS = 10;

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                Vector3 randomOffset = GetRandomOffset(settings.brushSize, settings.brushShape);
                Vector3 targetPosition = centerPoint + randomOffset;

                // Raycast down from above to find surface
                // Raycast hacia abajo desde arriba para encontrar superficie
                Ray ray = new Ray(targetPosition + Vector3.up * 100f, Vector3.down);
                
                if (Physics.Raycast(ray, out RaycastHit hit, 200f, settings.layerMask))
                {
                    // Check slope angle
                    // Verificar ángulo de pendiente
                    float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if (slopeAngle > settings.maxSlopeAngle)
                        continue;

                    Vector3 spawnPosition = hit.point + hit.normal * settings.surfaceOffset;

                    // Check minimum distance to other objects
                    // Verificar distancia mínima a otros objetos
                    if (settings.minDistance > 0f && IsPositionTooClose(spawnPosition, settings.minDistance))
                        continue;

                    return spawnPosition;
                }
            }

            return null;
        }

        /// <summary>
        /// Get random offset based on brush shape
        /// Obtener desplazamiento aleatorio basado en la forma del pincel
        /// </summary>
        private static Vector3 GetRandomOffset(float brushSize, BrushShape shape)
        {
            switch (shape)
            {
                case BrushShape.Circle:
                    return GetRandomCircleOffset(brushSize);
                
                case BrushShape.Square:
                    return GetRandomSquareOffset(brushSize);
                
                default:
                    return GetRandomCircleOffset(brushSize);
            }
        }

        /// <summary>
        /// Get random position within a circle
        /// Obtener posición aleatoria dentro de un círculo
        /// </summary>
        private static Vector3 GetRandomCircleOffset(float radius)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0f, radius);
            
            return new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );
        }

        /// <summary>
        /// Get random position within a square
        /// Obtener posición aleatoria dentro de un cuadrado
        /// </summary>
        private static Vector3 GetRandomSquareOffset(float size)
        {
            return new Vector3(
                Random.Range(-size, size),
                0f,
                Random.Range(-size, size)
            );
        }

        /// <summary>
        /// Check if position is too close to existing tagged objects
        /// Verificar si la posición está muy cerca de objetos etiquetados existentes
        /// </summary>
        private static bool IsPositionTooClose(Vector3 position, float minDistance)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(position, minDistance);
            
            foreach (Collider col in nearbyColliders)
            {
                if (PrefabPlacerTag.HasTag(col.gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Prefab Instantiation / Instanciación de Prefabs

        /// <summary>
        /// Instantiate prefab with proper settings
        /// Instanciar prefab con configuración adecuada
        /// </summary>
        private static GameObject InstantiatePrefab(GameObject prefab, Vector3 position, PrefabPlacerSettings settings)
        {
            // Instantiate using PrefabUtility to maintain prefab connection
            // Instanciar usando PrefabUtility para mantener conexión de prefab
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            
            if (instance == null)
            {
                Debug.LogError($"[CGTools] Failed to instantiate prefab: {prefab.name}");
                return null;
            }

            // Set position
            // Establecer posición
            instance.transform.position = position;

            // Apply rotation
            // Aplicar rotación
            Quaternion rotation = CalculateRotation(position, settings);
            instance.transform.rotation = rotation;

            // Apply scale
            // Aplicar escala
            Vector3 scale = CalculateScale(settings);
            instance.transform.localScale = scale;

            // Add tag for identification
            // Agregar tag para identificación
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            PrefabPlacerTag.AddTagToObject(instance, prefabPath);

            // Register undo
            // Registrar deshacer
            Undo.RegisterCreatedObjectUndo(instance, "Paint Prefab");

            return instance;
        }

        /// <summary>
        /// Get random prefab from list
        /// Obtener prefab aleatorio de la lista
        /// </summary>
        private static GameObject GetRandomPrefab(List<GameObject> prefabs)
        {
            if (prefabs == null || prefabs.Count == 0)
                return null;

            int randomIndex = Random.Range(0, prefabs.Count);
            return prefabs[randomIndex];
        }

        #endregion

        #region Rotation Calculation / Cálculo de Rotación

        /// <summary>
        /// Calculate rotation based on settings
        /// Calcular rotación basada en configuración
        /// </summary>
        private static Quaternion CalculateRotation(Vector3 position, PrefabPlacerSettings settings)
        {
            Quaternion baseRotation = Quaternion.identity;

            // Calculate base rotation based on mode
            // Calcular rotación base según el modo
            switch (settings.rotationMode)
            {
                case RotationMode.SurfaceAligned:
                    baseRotation = GetSurfaceRotation(position, settings.layerMask);
                    break;

                case RotationMode.Fixed:
                    baseRotation = Quaternion.Euler(settings.fixedRotation);
                    break;

                case RotationMode.Random:
                    baseRotation = GetRandomRotation(settings.randomRotationMin, settings.randomRotationMax);
                    break;

                case RotationMode.SurfaceAlignedWithRandomOffset:
                    Quaternion surfaceRot = GetSurfaceRotation(position, settings.layerMask);
                    Quaternion randomRot = GetRandomRotation(settings.randomRotationMin, settings.randomRotationMax);
                    baseRotation = surfaceRot * randomRot;
                    break;
            }

            // Apply additional random rotation if enabled
            // Aplicar rotación aleatoria adicional si está habilitada
            if (settings.randomRotation && settings.rotationMode != RotationMode.Random)
            {
                Quaternion randomOffset = GetRandomRotation(settings.randomRotationMin, settings.randomRotationMax);
                baseRotation *= randomOffset;
            }

            return baseRotation;
        }

        /// <summary>
        /// Get surface-aligned rotation
        /// Obtener rotación alineada a la superficie
        /// </summary>
        private static Quaternion GetSurfaceRotation(Vector3 position, int layerMask)
        {
            Ray ray = new Ray(position + Vector3.up * 10f, Vector3.down);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 20f, layerMask))
            {
                return Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            return Quaternion.identity;
        }

        /// <summary>
        /// Get random rotation within range
        /// Obtener rotación aleatoria dentro de rango
        /// </summary>
        private static Quaternion GetRandomRotation(Vector3 min, Vector3 max)
        {
            Vector3 randomEuler = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );

            return Quaternion.Euler(randomEuler);
        }

        #endregion

        #region Scale Calculation / Cálculo de Escala

        /// <summary>
        /// Calculate scale based on settings
        /// Calcular escala basada en configuración
        /// </summary>
        private static Vector3 CalculateScale(PrefabPlacerSettings settings)
        {
            Vector3 scale = settings.baseScale;

            if (settings.randomScale)
            {
                float variation = settings.scaleVariation / 100f;

                if (settings.uniformRandomScale)
                {
                    // Uniform random scale (same for all axes)
                    // Escala aleatoria uniforme (igual para todos los ejes)
                    float randomFactor = Random.Range(1f - variation, 1f + variation);
                    scale *= randomFactor;
                }
                else
                {
                    // Independent random scale per axis
                    // Escala aleatoria independiente por eje
                    scale.x *= Random.Range(1f - variation, 1f + variation);
                    scale.y *= Random.Range(1f - variation, 1f + variation);
                    scale.z *= Random.Range(1f - variation, 1f + variation);
                }
            }

            return scale;
        }

        #endregion

        #region Scene View Visualization / Visualización en Vista de Escena

        /// <summary>
        /// Draw brush preview in scene view
        /// Dibujar vista previa del pincel en la vista de escena
        /// </summary>
        public static void DrawBrushPreview(Event currentEvent, PrefabPlacerSettings settings)
        {
            Vector3? hitPoint = GetMouseWorldPosition(currentEvent, settings.layerMask);
            
            if (!hitPoint.HasValue)
                return;

            // Choose color based on mode
            // Elegir color según el modo
            Color brushColor = settings.currentMode == PaintMode.Paint 
                ? new Color(0f, 1f, 0f, 0.3f)  // Green for paint / Verde para pintar
                : new Color(1f, 0f, 0f, 0.4f);  // Red for erase / Rojo para borrar

            Handles.color = brushColor;

            // Draw based on brush shape
            // Dibujar según la forma del pincel
            switch (settings.brushShape)
            {
                case BrushShape.Circle:
                    DrawCircleBrush(hitPoint.Value, settings.brushSize);
                    break;

                case BrushShape.Square:
                    DrawSquareBrush(hitPoint.Value, settings.brushSize);
                    break;
            }

            // Draw normal indicator if aligning to surface
            // Dibujar indicador de normal si se alinea a la superficie
            if (settings.alignToSurface && settings.rotationMode == RotationMode.SurfaceAligned)
            {
                Ray ray = new Ray(hitPoint.Value + Vector3.up * 10f, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, 20f, settings.layerMask))
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(hit.point, hit.point + hit.normal * 2f);
                }
            }
        }

        /// <summary>
        /// Draw circle brush preview
        /// Dibujar vista previa de pincel circular
        /// </summary>
        private static void DrawCircleBrush(Vector3 center, float radius)
        {
            Handles.DrawSolidDisc(center, Vector3.up, radius);
            Handles.DrawWireDisc(center, Vector3.up, radius);
        }

        /// <summary>
        /// Draw square brush preview
        /// Dibujar vista previa de pincel cuadrado
        /// </summary>
        private static void DrawSquareBrush(Vector3 center, float size)
        {
            Vector3[] corners = new Vector3[5];
            corners[0] = center + new Vector3(-size, 0, -size);
            corners[1] = center + new Vector3(size, 0, -size);
            corners[2] = center + new Vector3(size, 0, size);
            corners[3] = center + new Vector3(-size, 0, size);
            corners[4] = corners[0]; // Close the square

            Handles.DrawPolyLine(corners);
            
            // Draw filled quad (using multiple lines for fill effect)
            // Dibujar cuadrilátero relleno (usando múltiples líneas para efecto de relleno)
            for (int i = 0; i < 10; i++)
            {
                float t = i / 10f;
                Vector3 start = Vector3.Lerp(corners[0], corners[3], t);
                Vector3 end = Vector3.Lerp(corners[1], corners[2], t);
                Handles.DrawLine(start, end);
            }
        }

        #endregion

        #region Statistics & Utilities / Estadísticas y Utilidades

        /// <summary>
        /// Get total count of placed objects in scene
        /// Obtener conteo total de objetos colocados en la escena
        /// </summary>
        public static int GetPlacedObjectCount()
        {
            return PrefabPlacerTag.CountTaggedObjects();
        }

        /// <summary>
        /// Select all placed objects in scene
        /// Seleccionar todos los objetos colocados en la escena
        /// </summary>
        public static void SelectAllPlacedObjects()
        {
            PrefabPlacerTag[] tags = PrefabPlacerTag.FindAllTagsInScene();
            GameObject[] objects = new GameObject[tags.Length];

            for (int i = 0; i < tags.Length; i++)
            {
                objects[i] = tags[i].gameObject;
            }

            Selection.objects = objects;
        }

        /// <summary>
        /// Clear all placed objects from scene
        /// Limpiar todos los objetos colocados de la escena
        /// </summary>
        public static int ClearAllPlacedObjects()
        {
            PrefabPlacerTag[] tags = PrefabPlacerTag.FindAllTagsInScene();
            
            Undo.SetCurrentGroupName("Clear All Placed Prefabs");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (PrefabPlacerTag tag in tags)
            {
                if (tag != null)
                {
                    Undo.DestroyObjectImmediate(tag.gameObject);
                }
            }

            Undo.CollapseUndoOperations(undoGroup);

            return tags.Length;
        }

        #endregion

        #region Validation / Validación

        /// <summary>
        /// Validate settings before painting
        /// Validar configuración antes de pintar
        /// </summary>
        public static bool ValidateSettings(PrefabPlacerSettings settings, List<GameObject> loadedPrefabs)
        {
            if (settings == null)
            {
                Debug.LogError("[CGTools] Settings is null.");
                return false;
            }

            if (loadedPrefabs == null || loadedPrefabs.Count == 0)
            {
                Debug.LogWarning("[CGTools] No prefabs assigned.");
                return false;
            }

            if (settings.brushSize <= 0f)
            {
                Debug.LogWarning("[CGTools] Brush size must be greater than 0.");
                return false;
            }

            if (settings.quantity <= 0)
            {
                Debug.LogWarning("[CGTools] Quantity must be greater than 0.");
                return false;
            }

            return true;
        }

        #endregion
    }
}