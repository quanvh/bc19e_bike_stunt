#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kamgam.BridgeBuilder25D
{
    // Create a new type of Settings Asset.
    public class BridgeBuilder25DSettings : ScriptableObject
    {
        public const string Version = "1.0.1";

        public const string SettingsFilePath = "Assets/BikeRacingTemplate2.5D/BridgeBuilder/BridgeBuilder2.5DSettings.asset";

        public static Color DefaultColor = Color.white;

        [SerializeField]
        public GameObject DefaultBridgePart;
        const string DefaultBridgePartPath = "Assets/BikeRacingTemplate2.5D/BridgeBuilder/Prefabs/BridgePartTexturedPrefab.prefab";

        [SerializeField]
        public GameObject DefaultBridgeEdgePart;
        const string DefaultBridgeEdgePartPath = "Assets/BikeRacingTemplate2.5D/BridgeBuilder/Prefabs/BridgeEdgePartTexturedPrefab.prefab";

        [SerializeField]
        public PhysicsMaterial2D DefaultBridgePhysicsMaterial;
        const string DefaultBridgePhysicsMaterialPath = "Assets/BikeRacingTemplate2.5D/BridgeBuilder/Materials/BridgePhysicsMaterial.physicsMaterial2D";

        [SerializeField]
        public Color Color;

        [SerializeField]
        public bool ShowIcon = true;

        [SerializeField]
        public bool ShowLogs = false;


        protected static BridgeBuilder25DSettings cachedSettings;

        public static BridgeBuilder25DSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<BridgeBuilder25DSettings>(SettingsFilePath);
				
				if (cachedSettings == null)
                {
                    var guids = AssetDatabase.FindAssets("t:BridgeBuilder25DSettings");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<BridgeBuilder25DSettings>(path);
                    }
                }
				
                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<BridgeBuilder25DSettings>();
                    cachedSettings.Color = BridgeBuilder25DSettings.DefaultColor;
                    cachedSettings.ShowIcon = true;
                    cachedSettings.ShowLogs = false;

                    var bridgePart = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBridgePartPath);
                    if (bridgePart != null)
                        cachedSettings.DefaultBridgePart = bridgePart;

                    var bridgeEdgePart = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBridgeEdgePartPath);
                    if (bridgeEdgePart != null)
                        cachedSettings.DefaultBridgeEdgePart = bridgeEdgePart;

                    var physicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(DefaultBridgePhysicsMaterialPath);
                    if (physicsMaterial != null)
                        cachedSettings.DefaultBridgePhysicsMaterial = physicsMaterial;

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                }
            }
            return cachedSettings;
        }

        static Shader getDefaultShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Standard");
            else
                return getUsedRenderPipeline().defaultShader;
        }

        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        [MenuItem("Tools/2.5D Bridge/Settings", priority = 500)]
        public static void SelectSettings()
        {
            var settings = BridgeBuilder25DSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "2.5D Terrain settings could not be found or created.", "Ok");
            }
        }
    }

    static class BridgeBuilder25DSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateBridgeBuilder25DSettingsProvider()
        {
            var provider = new SettingsProvider("Project/2.5D Bridge Builder", SettingsScope.Project)
            {
                label = "2.5D Bridge Builder",
                guiHandler = (searchContext) =>
                {
                    var settings = BridgeBuilder25DSettings.GetSerializedSettings();

                    EditorGUILayout.LabelField("Version: " + BridgeBuilder25DSettings.Version);

                    EditorGUILayout.PropertyField(settings.FindProperty("DefaultBridgePart"), new GUIContent("Default Part:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("DefaultBridgeEdgePart"), new GUIContent("Default Edge Part:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("DefaultBridgePhysicsMaterial"), new GUIContent("Default Physics Material:"));
                    

                    EditorGUILayout.PropertyField(settings.FindProperty("Color"), new GUIContent("Spline add color:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("ShowIcon"), new GUIContent("Show the gizmo icon:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowLogs"), new GUIContent("Show log messages:"));

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "2.5D", "2.5d", "bridge", "BridgeBuilder25D", "25d", "2.5d" })
            };

            return provider;
        }
    }
}
#endif