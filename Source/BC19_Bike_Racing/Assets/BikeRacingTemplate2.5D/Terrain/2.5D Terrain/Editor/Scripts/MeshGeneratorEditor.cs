using Kamgam.Terrain25DLib.Helpers;
using System;
using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    [CustomEditor(typeof(MeshGenerator))]
    public class MeshGeneratorEditor : Editor
    {
        protected MeshGenerator generator;

        public static bool IsGenerating = false;
        public static bool IsSmoothing = false;
        public static bool AutoUpdate = false;
        public static bool LastGenerationFailed = false;
        public static string LastGenerationFailMsg = "";
        public static double LastAutoGenerationTime;

        protected Terrain25D terrain;
        public Terrain25D Terrain
        {
            get
            {
                if (terrain == null && target != null)
                {
                    terrain = ((MeshGenerator)target).GetComponentInParent<Terrain25D>();
                }
                return terrain;
            }
        }

        public void Awake()
        {
            generator = target as MeshGenerator;
#if UNITY_EDITOR
            if(generator.Material == null)
            {
                generator.Material = Terrain25DSettings.GetOrCreateSettings().DefaultMaterial;
            }
#endif
        }

        public void OnEnable()
        {
            IsGenerating = false;
            IsSmoothing = false;
        }

        public override void OnInspectorGUI()
        {
            Terrain25DEditor.DrawInspectorGUI(Terrain);

            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("# Mesh Generator", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (IsGenerating)
            {
                GUILayout.Label("generating ..");
            }
            else
            {
                if (GUILayout.Button("Generate Mesh"))
                {
                    StartGeneratingMeshAsync(generator);
                }
            }

            if (IsSmoothing)
            {
                GUILayout.Label("smoothing ..");
            }
            else
            {
                if (GUILayout.Button("Smooth Mesh"))
                {
                    IsSmoothing = true;
                    UtilsEditor.ExecuteDelayed(0.05f, () =>
                    {
                        generator.SmoothMesh();
                        IsSmoothing = false;
                    });
                }
            }

            if (GUILayout.Button("Add Vertex"))
            {
                var vertex = generator.AddVertex();

                // guess a reasonable default position for the new vertex
                var cam = SceneView.lastActiveSceneView.camera;
                if (cam != null)
                {
                    var rect = cam.pixelRect;
                    var ray = cam.ScreenPointToRay(
                        new Vector2(
                            rect.width * 0.5f, rect.height * 0.5f
                            )
                        );

                    bool hitOnMesh = false;
                    var meshes = generator.transform.GetComponentsInChildren<MeshFilter>();
                    foreach (var mesh in meshes)
                    {
                        RaycastHit hit;
                        if (UtilsEditor.IntersectRayMesh(ray, mesh, out hit) == 1)
                        {
                            vertex.transform.position = hit.point;
                            hitOnMesh = true;
                            break;
                        }
                    }

                    if (!hitOnMesh)
                    {
                        var plane = new Plane(generator.transform.TransformDirection(Vector3.forward), generator.transform.TransformPoint(Vector3.zero));
                        float enter;
                        if (plane.Raycast(ray, out enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);
                            vertex.transform.position = hitPoint;
                        }
                    }
                }

                // select
                EditorGUIUtility.PingObject(vertex);
                Selection.objects = new GameObject[] { vertex };
            }

            // Draw default gui and check for updates.
            serializedObject.Update();
            SerializedProperty serializedProperty = serializedObject.GetIterator();
            serializedProperty.NextVisible(enterChildren: true); // skip script
            while (serializedProperty.NextVisible(enterChildren: true))
            {
                GUI.enabled = true;
                if (serializedProperty.name == "MergeSmoothedVertices" && generator.SmoothNormals == false)
                    GUI.enabled = false;
                EditorGUILayout.PropertyField(serializedProperty);
            }
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                if (AutoUpdate)
                    GenerateMesh(generator, ignoreSmoothCombineAndErosion: true);
            }

            EditorGUILayout.Space();
            AutoUpdate = EditorGUILayout.Toggle(new GUIContent("Auto Update", "Automatically regenrate the mesh if a setting is changed (slow for big meshes). This also ignores the Combine and Smooth setting to gain performance."), AutoUpdate);

            EditorGUILayout.Space();
            // Failure Info
            if (LastGenerationFailed)
            {
                var l = new GUIStyle(GUI.skin.label);
                l.fontStyle = FontStyle.Bold;
                GUILayout.Label("Log", l);

                var bgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(2f, 1f, 0f);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUIStyle textStyle = new GUIStyle(EditorStyles.label);
                textStyle.wordWrap = true;
                EditorGUILayout.LabelField("Mesh generation failed. It happens (working on it).\nUsually a slight adjustment of the settings helps.\nError: " + LastGenerationFailMsg, textStyle);
                GUILayout.EndVertical();
                GUI.backgroundColor = bgColor;
            }

            // TODO: Does not trigger if the mouse leaves the inspector while waiting because OnInspectorGUI is not called.
            if (AutoUpdate && LastAutoGenerationTime > 0 && EditorApplication.timeSinceStartup - LastAutoGenerationTime > 0.5)
            {
                LastAutoGenerationTime = -1;
                StartGeneratingMeshAsync(generator);
            }

            GUILayout.EndVertical();
        }

        public static void StartGeneratingMeshAsync(MeshGenerator generator, bool ignoreSmoothCombineAndErosion = false)
        {
            IsGenerating = true;
            UtilsEditor.ExecuteDelayed(0.05f, () => {
                GenerateMesh(generator, ignoreSmoothCombineAndErosion);
                IsGenerating = false;
            });
        }

        public static void StartSmoothingMeshAsync(MeshGenerator generator)
        {
            IsSmoothing = true;
            UtilsEditor.ExecuteDelayed(0.05f, () => {
                generator.SmoothMesh();
                IsSmoothing = false;
            });
        }

        public static void GenerateMesh(MeshGenerator generator, bool ignoreSmoothCombineAndErosion = false)
        {
            try
            {
                LastGenerationFailed = false;
                generator.GenerateMesh(ignoreSmoothCombineAndErosion);
                if (ignoreSmoothCombineAndErosion)
                    LastAutoGenerationTime = EditorApplication.timeSinceStartup;
            }
            catch(Poly2Tri.PointOnEdgeException e)
            {
                LastGenerationFailed = true;
                LastGenerationFailMsg = e.Message;
            }
        }
    }
}
