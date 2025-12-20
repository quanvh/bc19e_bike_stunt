#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Linq;

namespace Kamgam.BikeRacing25D
{
    /// <summary>
    /// The SaveGameProviderEditor contains some handy functions for backup and restore
    /// of local savegame Scriptable Objects.
    /// </summary>
    [CustomEditor(typeof(SaveGameProvider))]
    public class SaveGameProviderEditor : Editor
    {
        bool showFileContent = true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SaveGameProvider provider = this.target as SaveGameProvider;

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = provider.UseFileFromDisk == false;
            if (GUILayout.Button("Save Asset to File"))
            {
                provider._EditorSaveAssetToJson();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New " + (provider.UseFileFromDisk ? "JSON file" : "Asset")))
            {
                provider.CreateNew();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("");
            showFileContent = EditorGUILayout.BeginFoldoutHeaderGroup(showFileContent, "Persistent File");
            if (showFileContent)
            {
                //EditorGUILayout.LabelField("In Editor Infos");

                // File path
                var path = SaveGameProvider.GetPersistentFilePath(provider.FileName);
                bool fileExists = File.Exists(path);
                GUI.enabled = fileExists;
                EditorGUILayout.LabelField("Persistent file path " + (fileExists ? "" : "(does not exist yet)") + ":");
                EditorGUILayout.SelectableLabel(path);

                // buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open File"))
                {
                    if (fileExists)
                    {
                        Application.OpenURL("file://" + path);
                    }
                    else
                    {
                        Debug.Log("No such file: '" + path + "'.");
                    }
                }
                if (GUILayout.Button("Delete File"))
                {
                    File.Delete(path);
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
                if (GUILayout.Button("Open Folder"))
                {
                    Application.OpenURL("file://" + path.Replace(provider.FileName, ""));
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public static string ReplaceLast(string source, string oldValue, string newValue)
        {
            int place = source.LastIndexOf(oldValue);

            if (place == -1)
            {
                return source;
            }

            string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
            return result;
        }

    }
}
#endif