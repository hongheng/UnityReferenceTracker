using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace HongHeng.UnityReferenceTracker.Core {

    public class FileReferenceTracker {

        private class DependenceCache {

            private readonly Dictionary<string, string[]> _cache =
                new Dictionary<string, string[]>();

            public IEnumerable<string> GetDependencies(string path) {
                if (!_cache.TryGetValue(path, out var res)) {
                    res = AssetDatabase.GetDependencies(path, true);
                    _cache.Add(path, res);
                }
                return res;
            }

        }

        private readonly DependenceCache _cache = new DependenceCache();

        #region api

        public IEnumerable<ReferenceFile> FindReferences(Object targetObj) {
            Assert.IsNotNull(targetObj, "Target object is null");
            var targetPath = AssetDatabase.GetAssetPath(targetObj);
            var targetObjects = (AssetDatabase.IsSubAsset(targetObj) || targetObj is SceneAsset)
                ? new[] {targetObj}
                : AssetDatabase.LoadAllAssetsAtPath(targetPath);
            var fileRefs = AssetDatabase.GetAllAssetPaths()
                .Where(path => path != targetPath)
                .Where(path => _cache.GetDependencies(path).Contains(targetPath))
                .Select(path => new ReferenceFile {
                    ReferenceFilePath = path,
                })
                .ToArray();

            var countResourceRefs = 0;
            foreach (var resourceRef in fileRefs
                .Where(r => !UnityApi.IsSceneFile(r.ReferenceFilePath))) {
                countResourceRefs++;
                var realResRef = RefApi.GetReference(resourceRef.ReferenceFilePath,
                    GetFilter(targetObjects));
                if (realResRef != null) {
                    yield return realResRef;
                }
            }
            if (fileRefs.Length == countResourceRefs) {
                yield break;
            }

            var targetComponentsOrObject = targetObj is GameObject go
                ? go.GetComponents<Component>().Append(targetObj).ToArray()
                : new[] {targetObj};
            var sceneRefs = fileRefs
                .Where(r => UnityApi.IsSceneFile(r.ReferenceFilePath))
                .Select(sceneRef => RefApi.GetReference(sceneRef.ReferenceFilePath,
                    GetFilter(targetComponentsOrObject)))
                .Where(sceneRef => sceneRef != null);
            foreach (var sceneRef in sceneRefs) {
                yield return sceneRef;
            }
        }

        #endregion

        private static PropertyFilter GetFilter(IEnumerable<Object> targetObjects) {
            return sp => {
                if (sp.propertyType != SerializedPropertyType.ObjectReference)
                    return false;
                var objRef = sp.objectReferenceValue;
                if (objRef == null) {
                    return false;
                }
                var go = objRef is MonoBehaviour behaviour ? behaviour.gameObject : null;
                return targetObjects.Any(o => (o == objRef) || (go != null && o == go));
            };
        }

    }

}
