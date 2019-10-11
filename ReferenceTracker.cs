using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HongHeng.UnityReferenceTracker.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HongHeng.UnityReferenceTracker {

    public static class ReferenceTracker {

        private static readonly FileReferenceTracker Tracker = new FileReferenceTracker();

        #region Missing Reference

        public static ReferenceFile[] LogMissingReferenceFiles(IEnumerable<string> filePaths) {
            Debug.Log($"====\nFindMissingReferenceFiles.");
            var fileRefs = CountTime(() => GetMissingReferenceFiles(filePaths));
            DebugLog(fileRefs);
            Debug.Log($"FindMissingReferenceFiles. result = {fileRefs.Length}");
            return fileRefs;
        }

        public static ReferenceFile[] GetMissingReferenceFiles(IEnumerable<string> filePaths) {
            return filePaths
                .Where(File.Exists)
                .Select(MissingReferenceTracker.FindMissingReferenceFiles)
                .Where(file => file != null)
                .ToArray();
        }

        #endregion

        #region Useless File

        public static string[] LogUselessFiles(IEnumerable<string> filePaths) {
            Debug.Log($"====\nFindUselessFile.");
            var uselessFiles = CountTime(() => GetUselessFiles(filePaths));
            if (uselessFiles.Length > 0) {
                foreach (var filePath in uselessFiles) {
                    Debug.Log($"\t{filePath}", AssetDatabase.LoadAssetAtPath<Object>(filePath));
                }
            }
            Debug.Log($"FindUselessFile. Find {uselessFiles.Length} files.");
            return uselessFiles;
        }

        public static string[] GetUselessFiles(IEnumerable<string> filePaths) {
            var selectObjects = filePaths
                .Where(File.Exists)
                .Select(AssetDatabase.LoadAssetAtPath<Object>)
                .ToArray();
            return selectObjects
                .Where(obj => !Tracker.FindReferences(obj).Any())
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();
        }

        #endregion

        #region File References

        public static ReferenceFile[] LogReferences(Object obj) {
            var path = AssetDatabase.GetAssetPath(obj);
            Debug.Log($"====\nFindReferences. {path}", obj);
            var fileRefs = CountTime(() => GetReferences(obj));
            DebugLog(fileRefs);
            Debug.Log($"FindReferences. result = {fileRefs.Length}", obj);
            return fileRefs;
        }

        public static ReferenceFile[] GetReferences(Object obj) {
            return Tracker.FindReferences(obj).ToArray();
        }

        #endregion

        #region Tool

        private static void DebugLog(IEnumerable<ReferenceFile> fileRefs) {
            foreach (var fileRef in fileRefs.OrderBy(fr => fr.ReferenceFilePath)) {
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

        private static T CountTime<T>(Func<T> func) {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var result = func();
            sw.Stop();
            Debug.Log($"Time cost: {sw.ElapsedMilliseconds} ms");
            return result;
        }

        #endregion

    }

}
