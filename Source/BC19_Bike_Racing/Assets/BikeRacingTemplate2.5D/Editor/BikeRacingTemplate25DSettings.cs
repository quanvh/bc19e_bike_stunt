using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    public class BikeRacingTemplate25DSettings : ScriptableObject
    {
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            GetOrCreateSettings();
        }

        public const string Version = "1.0.0";
        public const string SettingsFilePath = "Assets/2.5D BikeRacingTemplateSettings.asset";
		
        protected static BikeRacingTemplate25DSettings cachedSettings;

        public static BikeRacingTemplate25DSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<BikeRacingTemplate25DSettings>(SettingsFilePath);

                // Still not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:BikeRacingTemplate25DSettings");
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<BikeRacingTemplate25DSettings>(path);
                    }
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<BikeRacingTemplate25DSettings>();


                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();

                    SetupTools.SetupLayers();
                    SetupTools.SetupBuildScenesDelayed();
                    MaterialShaderFixer.FixMaterialsDelayed(MaterialShaderFixer.RenderPiplelineType.Standard, onSetupComplete );
                }
            }
            return cachedSettings;
        }

        static void onSetupComplete()
        {
            EditorUtility.DisplayDialog(
                   "Import finished",
                   "The Bike Racing Template has been imported.\n\nYou can find some setup helpers under: Tools > Bike Racing Template > Setup ...\n\nPlease start by reading the manual.",
                   "Okay (open manual)"
                   );

            OpenManual();
            EditorSceneManager.OpenScene("Assets/BikeRacingTemplate2.5D/Scenes/Main.unity");
        }

        [MenuItem("Tools/Bike Racing Template/Open Manual", priority = 600)]
        public static void OpenManual()
        {
            EditorUtility.OpenWithDefaultApp("Assets/BikeRacingTemplate2.5D/BikeRacingTemplate2.5DManual.pdf");
        }

        [MenuItem("Tools/Bike Racing Template/Please leave a review :-)", priority = 700)]
        public static void RateAsset()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/229066");
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        public static void SelectSettings()
        {
            var settings = BikeRacingTemplate25DSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Looping 2.5D settings could not be found or created.", "Ok");
            }
        }
    }
}