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

        [MenuItem("Tools/ReferenceTracker/FindMissingReferenceFiles (not mat)", false, 1)]
        public static void FindMissingReferenceFiles() {
            CountTime(() => FindAndLogMissingReferenceFiles(path => !path.EndsWith(".mat")));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (all)", false, 101)]
        public static void FindAllUselessFile() {
            CountTime(() => FindUselessFile());
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (code)", false, 102)]
        public static void FindUselessCodeFile() {
            CountTime(() => FindUselessFile(path => path.EndsWith(".cs")));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (not code)", false, 103)]
        public static void FindUselessResourceFile() {
            CountTime(() => FindUselessFile(path => !path.EndsWith(".cs")));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (prefab)", false, 104)]
        public static void FindUselessPrefabFile() {
            CountTime(() => FindUselessFile(path => path.EndsWith(".prefab")));
        }

        [MenuItem("Tools/ReferenceTracker/FindReferences (Selection)", false, 202)]
        [MenuItem("Assets/- FindReferences", false, 39)]
        public static void FindReferences() {
            CountTime(() => FindReferences(Selection.activeObject));
        }

        public static IEnumerable<ReferenceFile> GetMissingReferenceFiles(
            Func<string, bool> filter = null) {
            filter = filter ?? (path => true);
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets" + Path.DirectorySeparatorChar))
                .Where(File.Exists)
                .Where(filter)
                .Select(MissingReferenceTracker.FindMissingReferenceFiles)
                .Where(file => file != null);
        }

        private static void FindAndLogMissingReferenceFiles(Func<string, bool> filter = null) {
            var fileRefs = GetMissingReferenceFiles(filter).ToArray();
            Debug.Log($"====\nFindMissingReferenceFiles.");
            DebugLog(fileRefs);
            Debug.Log($"FindMissingReferenceFiles. result = {fileRefs.Length}");
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

        private static void CountTime(Action action) {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            Debug.Log($"Time cost: {sw.ElapsedMilliseconds} ms");
        }

    }

}
