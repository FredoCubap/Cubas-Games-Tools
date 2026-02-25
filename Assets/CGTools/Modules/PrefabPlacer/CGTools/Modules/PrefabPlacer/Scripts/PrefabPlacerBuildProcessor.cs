#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CGTools.Modules.PrefabPlacer
{
    /// <summary>
    /// Ensures PrefabPlacerTag components are stripped from all scenes before a build.
    /// </summary>
    public class PrefabPlacerBuildProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            PrefabPlacerTag[] tags = Object.FindObjectsOfType<PrefabPlacerTag>();

            foreach (PrefabPlacerTag tag in tags)
            {
                if (tag != null)
                    Object.DestroyImmediate(tag);
            }

            if (tags.Length > 0)
                Debug.Log($"[CGTools] Removed {tags.Length} PrefabPlacerTag component(s) from build.");
        }
    }
}
#endif