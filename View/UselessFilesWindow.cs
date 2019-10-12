using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HongHeng.UnityReferenceTracker.View {

    public class UselessFilesWindow : EditorWindow {

        public static void Create(string[] uselessFiles) {
            if (uselessFiles.Length == 0) {
                return;
            }
            CreateInstance<UselessFilesWindow>()
                .SetData(uselessFiles)
                .Show();
        }

        private UselessFilesWindow SetData(string[] uselessFiles) {
            _uselessFiles = uselessFiles;
            return this;
        }

        private string[] _uselessFiles;
        private Vector2 _scrollPos;

        private void OnGUI() {
            var uselessFiles = _uselessFiles;
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("删除所有非cs文件")) {
                foreach (var file in uselessFiles.Where(file => !file.EndsWith(".cs"))) {
                    AssetDatabase.DeleteAsset(file);
                }
            }
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.Space();
            foreach (var file in uselessFiles) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.Label(file);
                    if (File.Exists(file)) {
                        if (GUILayout.Button("删除")) {
                            AssetDatabase.DeleteAsset(file);
                        }
                        if (GUILayout.Button("打开")) {
                            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(file));
                        }
                    } else {
                        GUILayout.Label("已删除");
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

    }

}