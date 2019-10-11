using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HongHeng.UnityReferenceTracker {

    public delegate bool PropertyFilter(SerializedProperty serializedProperty);

    public static class RefApi {

        public static ReferenceFile GetReference(string path, PropertyFilter filter,
            bool containsEmpty = false) {
            if (UnityApi.IsSceneFile(path)) {
                return GetSceneReference(path, filter, containsEmpty);
            }
            return GetResourceReference(path, filter, containsEmpty);
        }

        private static ReferenceFile GetResourceReference(string path,
            PropertyFilter filter, bool containsEmpty = false) {
            // AssetDatabase.LoadAllAssetsAtPath(path) can not find parent object
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            ReferenceObject[] referenceObjects = null;
            if (obj == null) {
                if (containsEmpty) {
                    referenceObjects = new[] {ReferenceObject.Empty};
                }
            } else if (obj is GameObject go) {
                referenceObjects = UnityApi.GetAllComponentsInChildren(go)
                    .Select(t => t.component == null
                        ? containsEmpty
                            ? ReferenceObject.EmptyComponent(
                                UnityApi.GetTransformPath(t.gameObject))
                            : null
                        : GetReferenceObject(t.component, filter))
                    .Where(refObj => refObj != null)
                    .ToArray();
            } else {
                referenceObjects = AssetDatabase.LoadAllAssetsAtPath(path)
                    .Select(t => t == null
                        ? containsEmpty ? ReferenceObject.Empty : null
                        : GetReferenceObject(t, filter))
                    .Where(refObj => refObj != null)
                    .ToArray();
            }
            return referenceObjects?.Length > 0
                ? new ReferenceFile {
                    ReferenceFilePath = path,
                    ReferenceObjects = referenceObjects,
                }
                : null;
        }

        private static ReferenceFile GetSceneReference(string scenePath, PropertyFilter filter,
            bool containsEmpty = false) {
            var scene = UnityApi.MakeSureSceneOpened(scenePath);
            var referenceObjects = scene.GetRootGameObjects()
                .SelectMany(go => UnityApi.GetAllComponentsInChildren(go)
                    .Select(t => t.component == null
                        ? containsEmpty
                            ? ReferenceObject.EmptyComponent(
                                UnityApi.GetTransformPath(t.gameObject))
                            : null
                        : GetReferenceObject(t.component, filter, true))
                    .Append(go == null
                        ? containsEmpty
                            ? ReferenceObject.EmptyGameObject
                            : null
                        : GetReferenceObject(go, filter, true))
                )
                .Where(refObj => refObj != null)
                .ToArray();
            return referenceObjects.Length > 0
                ? new ReferenceFile {
                    ReferenceFilePath = scenePath,
                    ReferenceObjects = referenceObjects,
                }
                : null;
        }

        private static ReferenceObject GetReferenceObject(Object obj, PropertyFilter filter,
            bool infoInScene = false) {
            var referenceProperties = GetReferenceProperties(obj, filter);
            return referenceProperties.Count > 0
                ? new ReferenceObject(infoInScene
                    ? UnityApi.GetTransformPath(obj)
                    : obj.ToString()) {
                    ReferenceProperties = referenceProperties,
                }
                : null;
        }

        private static List<ReferenceProperty> GetReferenceProperties(Object obj,
            PropertyFilter filter) {
            var referenceProperties = new List<ReferenceProperty>();
            var so = new SerializedObject(obj);
            var sp = so.GetIterator();
            while (sp.Next(true)) {
                if (filter(sp)) {
                    referenceProperties.Add(new ReferenceProperty(sp));
                }
            }
            so.Dispose();
            return referenceProperties;
        }

    }

}
