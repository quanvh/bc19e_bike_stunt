using UnityEditor;
using UnityEngine;

namespace Kamgam.BikeAndCharacter25D
{
    public class BikeAndCharacter25DSettings : ScriptableObject
    {
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            BikeAndCharacter25DSettings.GetOrCreateSettings();
        }

        public const string Version = "1.0.1";
        public const string SettingsFilePath = "Assets/BikeRacingTemplate2.5D/BikeAndChar/BikeAndCharacter25DSettings.asset";
		
        protected static BikeAndCharacter25DSettings cachedSettings;

        public static BikeAndCharacter25DSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<BikeAndCharacter25DSettings>(SettingsFilePath);

                if (cachedSettings == null)
                {
                    var guids = AssetDatabase.FindAssets("t:BikeAndCharacter25DSettings");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<BikeAndCharacter25DSettings>(path);
                    }
                }

                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<BikeAndCharacter25DSettings>();


                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();
                }
            }
            return cachedSettings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        public static void SelectSettings()
        {
            var settings = BikeAndCharacter25DSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "BikeAndCharacter25D settings could not be found or created.", "Ok");
            }
        }
    }
}