using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HongHeng.UnityReferenceTracker {

    public static class UnityApi {

        public static bool IsSceneFile(string scenePath) {
            return scenePath.EndsWith(".unity");
        }

        public static IEnumerable<(GameObject gameObject, Component component)>
            GetAllComponentsInChildren(GameObject go) {
            if (go == null) {
                yield break;
            }
            foreach (var comp in go.GetComponents<Component>()) {
                yield return (go, comp);
            }
            var childCount = go.transform.childCount;
            for (var i = 0; i < childCount; i++) {
                var child = go.transform.GetChild(i).gameObject;
                foreach (var result in GetAllComponentsInChildren(child)) {
                    yield return result;
                }
            }
        }

        public static Scene MakeSureSceneOpened(string scenePath) {
            var scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.isLoaded) {
                scene = EditorSceneManager.OpenScene(scenePath);
            }
            if (!scene.IsValid() || !scene.isLoaded) {
                Debug.LogError(
                    $"scene: IsValid = {scene.IsValid()}; isLoaded = {scene.isLoaded}. {scenePath}");
            }
            //Debug.Log($"scene: {scenePath}, {scene.isLoaded}, {scene.GetRootGameObjects().Length}");
            return scene;
        }

        public static string GetTransformPath(Object obj) {
            return AnimationUtility.CalculateTransformPath(
                (obj is Component component ? component.gameObject : obj as GameObject)
                ?.transform, null);
        }

    }

}
