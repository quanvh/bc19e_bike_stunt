using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    [CustomEditor(typeof(Terrain25D))]
    [CanEditMultipleObjects]
    public class Terrain25DEditor : Editor
    {
        Terrain25D terrain;

        void OnEnable()
        {
            terrain = (Terrain25D)target;
        }

        public override void OnInspectorGUI()
        {

            DrawInspectorGUI(terrain);
            if (terrain.SplineController != null)
                SplineControllerEditor.DrawInspectorGUI(terrain.SplineController);
        }

        public static void DrawInspectorGUI(Terrain25D terrain)
        {
            if (terrain == null)
                return;

            EditorGUILayout.Space();

            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("T 2.5D Terrain", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.enabled = !Selection.Contains(terrain.gameObject);
            if (GUILayout.Button("Go to Terrain T"))
            {
                Selection.objects = new GameObject[] { terrain.gameObject };
            }


            // Spline Controller

            GUI.enabled = true;
            bool hasSplineController = terrain.SplineController;
            if (hasSplineController)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Spline:");
                GUI.enabled = !terrain.SplineController.EditMode;
                if (GUILayout.Button("Start Editing"))
                {
                    terrain.SplineController.EditMode = true;
                    SplineControllerEditor.PrepareToEdit(terrain.SplineController);
                }
                GUI.enabled = hasSplineController && terrain.SplineController.EditMode;
                if (GUILayout.Button("Stop Editing"))
                {
                    SplineControllerEditor.StopEditing(terrain.SplineController);
                }
                GUI.enabled = hasSplineController;
                if (GUILayout.Button("Go to Spline Controller S"))
                {
                    Selection.objects = new GameObject[] { terrain.SplineController.gameObject };
                    EditorGUIUtility.PingObject(terrain.SplineController.gameObject);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("add Spline Controller C"))
                {
                    terrain.AddSplineController();
                }
            }


            // Mesh Generator

            GUI.enabled = true;
            bool hasMeshGenerator = terrain.MeshGenerator != null;
            if (hasMeshGenerator)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mesh:");
                if (MeshGeneratorEditor.IsGenerating)
                {
                    GUI.enabled = false;
                    GUILayout.Button("generating ..");
                    GUI.enabled = hasMeshGenerator;
                }
                else
                {
                    if (GUILayout.Button("Generate Mesh"))
                    {
                        MeshGeneratorEditor.StartGeneratingMeshAsync(terrain.MeshGenerator);
                    }
                }
                if (MeshGeneratorEditor.IsSmoothing)
                {
                    GUI.enabled = false;
                    GUILayout.Button("smoothing ..");
                    GUI.enabled = hasMeshGenerator;
                }
                else
                {
                    if (GUILayout.Button("Smooth"))
                    {
                        MeshGeneratorEditor.StartSmoothingMeshAsync(terrain.MeshGenerator);
                    }
                }
                if (GUILayout.Button("Go to Mesh Generator #"))
                {
                    Selection.objects = new GameObject[] { terrain.MeshGenerator.gameObject };
                    EditorGUIUtility.PingObject(terrain.MeshGenerator.gameObject);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("add Mesh Generator #"))
                {
                    terrain.AddMeshGenerator();
                }
            }


            // Collider Generator

            GUI.enabled = true;
            bool hasColliderGenerator = terrain.Collider2DGenerator != null;
            if (hasColliderGenerator)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Collider:");
                GUI.enabled = hasColliderGenerator;
                if (GUILayout.Button("Generate Collider"))
                    terrain.Collider2DGenerator.GenerateColliders();
                if (GUILayout.Button("Go to Collider Generator []"))
                {
                    Selection.objects = new GameObject[] { terrain.Collider2DGenerator.gameObject };
                    EditorGUIUtility.PingObject(terrain.Collider2DGenerator.gameObject);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("add Collider Generator []"))
                {
                    terrain.AddCollider2DGenerator();
                }
            }


            // Foliage Generator

            GUI.enabled = true;
            bool hasFoliageGenerator = terrain.FoliageGenerator != null;
            if (hasFoliageGenerator)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Foliage:");
                if (GUILayout.Button("Generate Foliage"))
                    terrain.FoliageGenerator.Generate();
                if (GUILayout.Button("Go to Foliage Generator F"))
                {
                    Selection.objects = new GameObject[] { terrain.FoliageGenerator.gameObject };
                    EditorGUIUtility.PingObject(terrain.FoliageGenerator.gameObject);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("add Foliage Generator F"))
                {
                    terrain.AddFoliageGenerator();
                }
            }

            GUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        [MenuItem("GameObject/Create 2.5D Terrain", false, 10)]
        [MenuItem("Tools/2.5D Terrain/Create 2.5D Terrain", false, 10)]
        static void CreateTerrain25D(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;

            if (parent != null)
            {
                var terrainInParent = parent.GetComponentInChildren<Terrain25D>();
                if (terrainInParent != null)
                {
                    Debug.LogWarning("Can not add 2.5D Terrain inside another 2.5D Terrain. Will add to parent instead.");
                    if (parent.transform.parent != null)
                        parent = parent.transform.parent.gameObject;
                    else
                        parent = null;
                }
            }

            GameObject go = new GameObject("2.5D Terrain", typeof(Terrain25D));
            GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;

            var terrain = go.GetComponent<Terrain25D>();
            terrain.AddSplineController();
            terrain.AddCollider2DGenerator();
            terrain.AddMeshGenerator();
            terrain.AddFoliageGenerator();
        }

        [MenuItem("Tools/2.5D Terrain/Manual (v" + Terrain25DSettings.Version + ")", false, 501)]
        static void OpenManual(MenuCommand menuCommand)
        {
            Application.OpenURL("https://kamgam.com/unity/25d-terrain#manual");
        }

        [MenuItem("Tools/2.5D Terrain/Please write a review :-)", false, 601)]
        static void OpenAssetStore(MenuCommand menuCommand)
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/220783");
        }
    }
}