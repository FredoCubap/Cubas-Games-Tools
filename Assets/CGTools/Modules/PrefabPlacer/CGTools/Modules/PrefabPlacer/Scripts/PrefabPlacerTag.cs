#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace CGTools.Modules.PrefabPlacer
{
    /// <summary>
    /// Editor-only component that identifies prefabs placed by Prefab Placer.
    /// Automatically removed from builds via PrefabPlacerBuildProcessor.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    public class PrefabPlacerTag : MonoBehaviour
    {
        #region Fields

        [HideInInspector]
        [Tooltip("Unique identifier for this placed instance.")]
        public string instanceID;

        [HideInInspector]
        [Tooltip("Asset path of the original prefab.")]
        public string prefabPath;

        [HideInInspector]
        [Tooltip("Timestamp of when this prefab was placed.")]
        public string placedDateString;

        [HideInInspector]
        [Tooltip("ID of the module that placed this prefab.")]
        public string moduleID = "PrefabPlacer";

        #endregion

        #region Lifecycle

        private void Awake()
        {
            if (!Application.isEditor)
            {
                DestroyImmediate(this);
                return;
            }

            if (string.IsNullOrEmpty(instanceID))
            {
                instanceID = System.Guid.NewGuid().ToString();
                placedDateString = System.DateTime.Now.ToString("o");
            }
        }

        private void OnValidate()
        {
            if (!Application.isEditor)
                DestroyImmediate(this);
        }

        #endregion

        #region Static Utilities

        /// <summary>
        /// Adds a PrefabPlacerTag to the given GameObject. Returns existing tag if already present.
        /// </summary>
        public static PrefabPlacerTag AddTagToObject(GameObject obj, string prefabPath)
        {
            if (obj == null)
                return null;

            PrefabPlacerTag existingTag = obj.GetComponent<PrefabPlacerTag>();
            if (existingTag != null)
                return existingTag;

            PrefabPlacerTag tag = obj.AddComponent<PrefabPlacerTag>();
            tag.prefabPath = prefabPath;
            tag.instanceID = System.Guid.NewGuid().ToString();
            tag.placedDateString = System.DateTime.Now.ToString("o");
            tag.moduleID = "PrefabPlacer";
            tag.hideFlags = HideFlags.HideInInspector;

            return tag;
        }

        /// <summary>
        /// Returns true if the GameObject has a PrefabPlacerTag component.
        /// </summary>
        public static bool HasTag(GameObject obj)
        {
            return obj != null && obj.GetComponent<PrefabPlacerTag>() != null;
        }

        /// <summary>
        /// Returns the PrefabPlacerTag on the given GameObject, or null if not found.
        /// </summary>
        public static PrefabPlacerTag GetTag(GameObject obj)
        {
            return obj != null ? obj.GetComponent<PrefabPlacerTag>() : null;
        }

        /// <summary>
        /// Removes the PrefabPlacerTag from the given GameObject.
        /// </summary>
        public static void RemoveTag(GameObject obj)
        {
            if (obj == null)
                return;

            PrefabPlacerTag tag = obj.GetComponent<PrefabPlacerTag>();
            if (tag != null)
                DestroyImmediate(tag);
        }

        /// <summary>
        /// Finds all tagged objects in the current scene.
        /// </summary>
        public static PrefabPlacerTag[] FindAllTagsInScene()
        {
            return FindObjectsOfType<PrefabPlacerTag>();
        }

        /// <summary>
        /// Returns the count of tagged objects in the current scene.
        /// </summary>
        public static int CountTaggedObjects()
        {
            return FindObjectsOfType<PrefabPlacerTag>().Length;
        }

        /// <summary>
        /// Destroys all tagged objects in the scene. Returns the count removed.
        /// </summary>
        public static int RemoveAllTagsFromScene()
        {
            PrefabPlacerTag[] tags = FindAllTagsInScene();

            foreach (PrefabPlacerTag tag in tags)
            {
                if (tag != null)
                    DestroyImmediate(tag);
            }

            return tags.Length;
        }

        #endregion

        #region Custom Inspector

        [CustomEditor(typeof(PrefabPlacerTag))]
        public class PrefabPlacerTagEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                PrefabPlacerTag tag = (PrefabPlacerTag)target;

                EditorGUILayout.HelpBox(
                    "This component identifies prefabs placed by CGTools Prefab Placer. " +
                    "It is automatically removed from builds.",
                    MessageType.Info
                );

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Instance ID", tag.instanceID);
                EditorGUILayout.TextField("Prefab Path", tag.prefabPath);
                EditorGUILayout.TextField("Module ID", tag.moduleID);

                if (!string.IsNullOrEmpty(tag.placedDateString))
                {
                    try
                    {
                        System.DateTime placedDate = System.DateTime.Parse(tag.placedDateString);
                        EditorGUILayout.TextField("Placed Date", placedDate.ToString());
                    }
                    catch
                    {
                        EditorGUILayout.TextField("Placed Date", "Invalid date");
                    }
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();

                if (GUILayout.Button("Remove Tag"))
                    DestroyImmediate(tag);
            }
        }

        #endregion
    }
}
#endif