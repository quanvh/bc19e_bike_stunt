#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    // Create a new type of Settings Asset.
    public class Terrain25DSettings : ScriptableObject
    {
        public const string Version = "1.0.2";

        public const string SettingsFilePath = "Assets/BikeRacingTemplate2.5D/Terrain/2.5DTerrainSettings.asset";

        public static Color DefaultAddColor = Color.white;
        public static Color DefaultSubtractColor = new Color(1f, 0.6f, 0.6f);
        public static Color DefaultCombinedColor = Color.green;

        [SerializeField]
        public Material DefaultMaterial;

        [SerializeField]
        public Color AddColor;

        [SerializeField]
        public Color SubtractColor;

        [SerializeField]
        public Color CombinedColor;

        [SerializeField]
        public bool ShowLabels;

        [SerializeField]
        public KeyCode AddCurvePointKey = KeyCode.N;

        [SerializeField]
        public KeyCode HandleTypeNoneKey = KeyCode.None;

        [SerializeField]
        public KeyCode HandleTypeBrokenKey = KeyCode.None;

        [SerializeField]
        public KeyCode HandleTypeMirroredKey = KeyCode.None;

        [SerializeField]
        public bool ShowIcon = true;

        [SerializeField]
        public bool ShowLogs = false;

        [SerializeField]
        public bool CollapseHierarchyAfterSplineEdit = true;

        static Terrain25DSettings cachedSettings;

        public static Terrain25DSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<Terrain25DSettings>(SettingsFilePath);

                // Still not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:Terrain25DSettings");
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<Terrain25DSettings>(path);
                    }
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    Material defaultMaterial = null;
                    defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Terrain/Materials/2.5D Terrain TriPlanar.mat");

                    cachedSettings = ScriptableObject.CreateInstance<Terrain25DSettings>();
                    cachedSettings.DefaultMaterial = defaultMaterial;
                    cachedSettings.AddColor = Terrain25DSettings.DefaultAddColor;
                    cachedSettings.SubtractColor = Terrain25DSettings.DefaultSubtractColor;
                    cachedSettings.CombinedColor = Terrain25DSettings.DefaultCombinedColor;
                    cachedSettings.AddCurvePointKey = KeyCode.N;
                    cachedSettings.HandleTypeNoneKey = KeyCode.None;
                    cachedSettings.HandleTypeBrokenKey = KeyCode.None;
                    cachedSettings.HandleTypeMirroredKey = KeyCode.None;
                    cachedSettings.ShowIcon = true;
                    cachedSettings.ShowLogs = false;
                    cachedSettings.CollapseHierarchyAfterSplineEdit = true;
                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);

#if UNITY_EDITOR
                    // Fix URP Materials
                    int urpMaterialsNeedFixing = 0;
                    if (getUsedRenderPipeline() == null)
                        urpMaterialsNeedFixing = 1; // Standard
                    else if (!getUsedRenderPipeline().GetType().Name.Contains("Universal"))
                        urpMaterialsNeedFixing = 2; // HDRP

                    if (urpMaterialsNeedFixing > 0)
                    {
                        Dictionary<string, Color> materials = new Dictionary<string, Color> {
                        { "Examples/Foliage/Materials/BushPurple", new Color(0.801f, 0f, 0.710f) },
                        { "Examples/Foliage/Materials/BushTeal" , new Color(0.02f, 0.92f, 0.9f) },
                        { "Examples/Foliage/Materials/GrassRed", new Color(0.153f, 0.470f, 0.180f) },
                        { "Examples/Foliage/Materials/GrassYellow", new Color(0.905f, 0.744f, 0.064f) },
                        { "Examples/Foliage/Materials/Sky", Color.white },
                        { "Examples/Foliage/Materials/TreeLeavesGreen", new Color(0.153f, 0.470f, 0.180f) },
                        { "Examples/Foliage/Materials/TreeLeavesYellow", new Color(0.905f, 0.744f, 0.064f) },
                        { "Examples/Foliage/Materials/TreeTrunk" , new Color(0.547f, 0.25f, 0f) },
                        { "Materials/2.5D Terrain URP" , Color.white }
                    };

                        Shader shader = getDefaultShader();
                        foreach (var kv in materials)
                        {
                            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Terrain/" + kv.Key + ".mat");
                            if (material == null)
                                continue;
                            material.shader = shader;
                            material.color = kv.Value;
                        }

                        Debug.LogWarning("2.5D Terrain: Changed material shaders from URP to " + (urpMaterialsNeedFixing == 1 ? "Standard" : "HDRP") + ".");
                    }

                    AssetDatabase.SaveAssets();

                    CheckPackages.CheckForPackage("com.unity.shadergraph", (found) =>
                    {
                        if (found == false)
                        {
                            string msg = "2.5D Terrain: Shader Graph Package is not installed.\n\nTo use the provided Tri-Planar shader you'll have to install it: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/ \n\nIf you do NOT want to use Shader Graph then the 'Standard' shader will be assigned to all Tri-Planar materials and you will have to make your own Tri-Planar standard shader.\n\nAre you planning to install and use Shader Graph?";
                            bool keepShaders = EditorUtility.DisplayDialog("2.5D Terrain Shader Graph Package is not installed!", msg, "Yes (keep broken shaders as they are)", "No (fix by assigning 'Standard' shader)");

                        // Fix shaders in all Materials
                            var shader = getDefaultShader();
                            if (shader != null)
                            {
                                if (keepShaders)
                                {
                                    Debug.LogWarning("2.5D Terrain: Shader Graph Package is not installed. Keeping broken shaders. They will appear PINK! Time to download and install Shader Graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/. \nAfter installation you may have to open the Graph (Shaders/TriPlanarProjection and Shaders/Terrain) and save it once.");
                                }
                                else
                                {
                                    Debug.LogWarning("2.5D Terrain: Shader Graph Package is not installed. Fixing shaders.");

                                    Material material;
                                    material = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Terrain/Examples/2.5D Terrain TriPlanar Rock Sand.mat");
                                    material.shader = shader;
                                    material.color = new Color(0.905f, 0.744f, 0.064f);

                                    material = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Terrain/Examples/2.5D Terrain TriPlanar Rock Grass.mat");
                                    material.shader = shader;
                                    material.color = new Color(0.153f, 0.470f, 0.180f);

                                    material = AssetDatabase.LoadAssetAtPath<Material>("Assets/2.5D Terrain/Materials/2.5D Terrain TriPlanar.mat");
                                    material.shader = shader;
                                    material.color = new Color(0.5f, 0.5f, 0.5f);

                                    AssetDatabase.SaveAssets();
                                }
                            }
                            else
                            {
                                Debug.LogError("No default shader found!");
                            }

                        // Fix Settings default material
                        /* // no longer needes since we now fix the shaders in the materials directly.
                        var mat = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset?.defaultMaterial;
                        if (mat == null)
                            mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

                        if (mat != null)
                        {
                            settings.DefaultMaterial = mat;
                            Debug.Log("2.5D Terrain: Applied default material as replacement for Shader Graph Material.");

                            // update materials in selected children
                            foreach (var go in Selection.gameObjects)
                            {
                                var terrain = go.GetComponent<Terrain25D>();
                                if (terrain != null && terrain.MeshGenerator != null)
                                    terrain.MeshGenerator.Material = mat;
                            }
                        }*/
                        }
                    });
#endif
                }
            }

            return cachedSettings;
        }

        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

        static Shader getDefaultShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Standard");
            else
                return getUsedRenderPipeline().defaultShader;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        [MenuItem("Tools/2.5D Terrain/Settings", priority = 500)]
        public static void SelectSettings()
        {
            var settings = Terrain25DSettings.GetOrCreateSettings();
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

    static class Terrain25DSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateTerrain25DSettingsProvider()
        {
            var provider = new SettingsProvider("Project/2.5D Terrain", SettingsScope.Project)
            {
                label = "2.5D Terrain",
                guiHandler = (searchContext) =>
                {
                    var settings = Terrain25DSettings.GetSerializedSettings();

                    EditorGUILayout.LabelField("Version: " + Terrain25DSettings.Version);

                    EditorGUILayout.PropertyField(settings.FindProperty("DefaultMaterial"), new GUIContent("Default Material:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("AddColor"), new GUIContent("Spline add color:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("SubtractColor"), new GUIContent("Spline subtract color:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("CombinedColor"), new GUIContent("Spline combined color:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowLabels"), new GUIContent("Show Labels:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("AddCurvePointKey"), new GUIContent("Add curve point key:"));
                    EditorGUILayout.HelpBox("Press and hold this key to enable the add point mode.", MessageType.None);

                    EditorGUILayout.PropertyField(settings.FindProperty("HandleTypeNoneKey"), new GUIContent("Set handle to 'None' key:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("HandleTypeBrokenKey"), new GUIContent("Set handle to 'Broken' key:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("HandleTypeMirroredKey"), new GUIContent("Set handle to 'Mirrored' key:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("CollapseHierarchyAfterSplineEdit"), new GUIContent("Collapse Hierarchy after spline edit:"));

                    EditorGUILayout.PropertyField(settings.FindProperty("ShowIcon"), new GUIContent("Show the gizmo icon:"));
                    EditorGUILayout.PropertyField(settings.FindProperty("ShowLogs"), new GUIContent("Show log messages:"));

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "2.5D", "2.5d", "terrain", "terrain25d", "25d", "2.5d" })
            };

            return provider;
        }
    }
}
#endif