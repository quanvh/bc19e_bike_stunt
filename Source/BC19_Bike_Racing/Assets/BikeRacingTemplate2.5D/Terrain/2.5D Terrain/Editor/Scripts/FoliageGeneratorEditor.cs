using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    [CustomEditor(typeof(FoliageGenerator))]
    public class FoliageGeneratorEditor : Editor
    {
        protected static bool foldoutDefaults = false;
        public static FoliageGenerator LastSelectedFoliageGenerator;

        protected FoliageGenerator generator;

        public SerializedProperty startProperty;
        public SerializedProperty endProperty;

        protected Terrain25D terrain;
        public Terrain25D Terrain
        {
            get
            {
                if (terrain == null && target != null)
                {
                    terrain = ((FoliageGenerator)target).GetComponentInParent<Terrain25D>();
                }
                return terrain;
            }
        }

        public void Awake()
        {
            generator = target as FoliageGenerator;
        }

        void OnEnable()
        {
            startProperty = serializedObject.FindProperty("Start");
            endProperty = serializedObject.FindProperty("End");

            LastSelectedFoliageGenerator = generator;
        }

        public override void OnInspectorGUI()
        {
            Terrain25DEditor.DrawInspectorGUI(Terrain);

            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("F Foliage Generator (BETA)", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            FoliageGenerator generator = (this.target as FoliageGenerator);

            if (generator != null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "foliage generate");
                    generator.Generate();
                }
                if (GUILayout.Button("Destroy"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "foliage destroy");
                    generator.Destroy();
                }
                if (GUILayout.Button("Destroy All"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "foliage destroy all");
                    generator.DestroyAll();
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Save Folliage"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "save folliage position");
                    generator.SaveFolliage();
                }
            }

            DrawGenerateSetButtons(generator);

            // Draw default gui and check for updates.
            EditorGUI.indentLevel++;
            serializedObject.Update();
            SerializedProperty serializedProperty = serializedObject.GetIterator();
            serializedProperty.NextVisible(enterChildren: true); // skip script
            while (serializedProperty.NextVisible(enterChildren: false))
            {
                if (!serializedProperty.name.StartsWith("Default"))
                    EditorGUILayout.PropertyField(serializedProperty);
            }

            // Default settings
            EditorGUI.indentLevel++;
            foldoutDefaults = EditorGUILayout.Foldout(foldoutDefaults, new GUIContent("Defaults"));
            if (foldoutDefaults)
            {
                serializedProperty.Reset();
                serializedProperty.NextVisible(enterChildren: true); // skip script
                while (serializedProperty.NextVisible(enterChildren: false))
                {
                    if (serializedProperty.name.StartsWith("Default"))
                        EditorGUILayout.PropertyField(serializedProperty);
                }
            }
            EditorGUI.indentLevel--;

            if (serializedObject.hasModifiedProperties)
            {
                bool startOrEndChanged = startProperty.floatValue != generator.Start || endProperty.floatValue != generator.End;
                serializedObject.ApplyModifiedProperties();

                if (startOrEndChanged)
                {
                    generator.SetStartAndSend(startProperty.floatValue, endProperty.floatValue);
                }
            }
            EditorGUI.indentLevel--;

            GUILayout.EndVertical();
        }

        public static void DrawGenerateSetButtons(FoliageGenerator generator)
        {
            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("Generate Single Sets:", s);
            foreach (var set in generator.GeneratorSets)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate " + set.Name))
                {
                    Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "generate " + set.Name);
                    generator.GenerateSet(set);
                }
                if (GUILayout.Button("Destroy " + set.Name))
                {
                    Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "destroy " + set.Name);
                    generator.DestroySet(set);
                }
                GUILayout.EndHorizontal();
            }
        }

        [MenuItem("Tools/2.5D Terrain/Place Selected", priority = 102)]
        public static void PlaceSelected()
        {
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
            {
                FoliageGenerator.PlaceObjects(Selection.gameObjects);
            }
        }
    }
}