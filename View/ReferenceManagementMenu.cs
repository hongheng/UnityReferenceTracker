using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HongHeng.UnityReferenceTracker {

    public static class ReferenceManagementMenu {

        private static readonly FileReferenceTracker Tracker = new FileReferenceTracker();

        [MenuItem("Assets/- FindMissingReferenceFiles", false, 31)]
        public static void FindMissingReferenceFiles() {
            var fileRefs = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets" + Path.DirectorySeparatorChar))
                .Where(File.Exists)
                .Select(MissingReferenceTracker.FindMissingReferenceFiles)
                .Where(file => file != null)
                .ToArray();
            Debug.Log($"====\nFindMissingReferenceFiles.");
            DebugLog(fileRefs);
            Debug.Log($"FindMissingReferenceFiles. result = {fileRefs.Length}");
        }

        [MenuItem("Assets/- FindReferences", false, 32)]
        public static void FindReferences() {
            CountTime(() => FindReferences(Selection.activeObject));
        }

        [MenuItem("Assets/- FindUselessFiles (all)", false, 33)]
        public static void FindAllUselessFile() {
            CountTime(() => FindUselessFile());
        }

        [MenuItem("Assets/- FindUselessFiles (code)", false, 34)]
        public static void FindUselessCodeFile() {
            CountTime(() => FindUselessFile(path => path.EndsWith(".cs")));
        }

        [MenuItem("Assets/- FindUselessFiles (not code)", false, 35)]
        public static void FindUselessResourceFile() {
            CountTime(() => FindUselessFile(path => !path.EndsWith(".cs")));
        }

        [MenuItem("Assets/- FindUselessFiles (prefab)", false, 36)]
        public static void FindUselessPrefabFile() {
            CountTime(() => FindUselessFile(path => path.EndsWith(".prefab")));
        }

        private static void CountTime(Action action) {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            Debug.Log($"Time cost: {sw.ElapsedMilliseconds} ms");
        }

        private static void FindReferences(Object obj) {
            var path = AssetDatabase.GetAssetPath(obj);
            Debug.Log($"====\nFindReferences. {path}", obj);
            var fileRefs = Tracker.FindReferences(obj).ToArray();
            DebugLog(fileRefs);
            Debug.Log($"FindReferences. result = {fileRefs.Length}", obj);
        }

        private static void DebugLog(IEnumerable<ReferenceFile> fileRefs) {
            foreach (var fileRef in fileRefs) {
                var filePath = fileRef.ReferenceFilePath;
                Debug.Log($"\t{filePath}", AssetDatabase.LoadAssetAtPath<Object>(filePath));
                foreach (var refRes in fileRef.ReferenceObjects) {
                    Debug.Log($"\t\t{refRes.ReferenceObjInfo}");
                    foreach (var prop in refRes.ReferenceProperties) {
                        Debug.Log($"\t\t\t{prop}");
                    }
                }
            }
        }

        private static void FindUselessFile(Func<string, bool> filter = null) {
            var selection = Selection.assetGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            filter = filter ?? (path => true);
            var selectFiles = AssetDatabase.FindAssets("", selection)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(File.Exists)
                .Where(filter)
                .Where(NeedReference)
                .Select(AssetDatabase.LoadAssetAtPath<Object>)
                .ToArray();
            var uselessFiles = selectFiles
                .Where(obj => !Tracker.FindReferences(obj).Any())
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();
            Debug.Log($"====\nFindUselessFile. From {selectFiles.Length} files.");
            if (uselessFiles.Length > 0) {
                UselessFilesWindow.Create(uselessFiles);
                foreach (var filePath in uselessFiles) {
                    Debug.Log($"\t{filePath}", AssetDatabase.LoadAssetAtPath<Object>(filePath));
                }
            }
            Debug.Log($"FindUselessFile. Find {uselessFiles.Length} files.");
        }

        private static readonly HashSet<string> IgnoredDirectory = new HashSet<string> {
            "Resources",
            "Plugins"
        };

        private static bool NeedReference(string path) {
            return !path.Split(Path.DirectorySeparatorChar)
                .Any(IgnoredDirectory.Contains);
        }

    }

}
