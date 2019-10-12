using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace HongHeng.UnityReferenceTracker.View {

    public static class ReferenceManagementMenu {

        [MenuItem("Tools/ReferenceTracker/FindMissingReferenceFiles (not mat)", false, 1)]
        private static void FindMissingReferenceFiles() {
            ReferenceTracker.LogMissingReferenceFiles(
                AssetDatabase.GetAllAssetPaths()
                    .Where(path =>
                        path.StartsWith("Assets" + Path.DirectorySeparatorChar)
                        && !path.EndsWith(".mat")));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (all)", false, 101)]
        private static void FindAllUselessFile() {
            UselessFilesWindow.Create(
                ReferenceTracker.LogUselessFiles(
                    SelectionFilePaths(null)));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (code)", false, 102)]
        private static void FindUselessCodeFile() {
            UselessFilesWindow.Create(
                ReferenceTracker.LogUselessFiles(
                    SelectionFilePaths(path => path.EndsWith(".cs"))));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (not code)", false, 103)]
        private static void FindUselessResourceFile() {
            UselessFilesWindow.Create(
                ReferenceTracker.LogUselessFiles(
                    SelectionFilePaths(path => !path.EndsWith(".cs"))));
        }

        [MenuItem("Tools/ReferenceTracker/FindUselessFiles (prefab)", false, 104)]
        private static void FindUselessPrefabFile() {
            UselessFilesWindow.Create(
                ReferenceTracker.LogUselessFiles(
                    SelectionFilePaths(path => path.EndsWith(".prefab"))));
        }

        [MenuItem("Tools/ReferenceTracker/FindReferences (Selection)", false, 202)]
        [MenuItem("Assets/- FindReferences", false, 39)]
        private static void FindReferences() {
            ReferenceTracker.LogReferences(Selection.activeObject);
        }

        private static readonly HashSet<string> IgnoredDirectory = new HashSet<string> {
            "Resources",
            "Plugins"
        };

        private static IEnumerable<string> SelectionFilePaths(Func<string, bool> filter) {
            var selection = Selection.assetGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            return AssetDatabase.FindAssets("", selection)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(filter)
                .Where(path => !path.Split(Path.DirectorySeparatorChar)
                    .Any(IgnoredDirectory.Contains));
        }

    }

}
