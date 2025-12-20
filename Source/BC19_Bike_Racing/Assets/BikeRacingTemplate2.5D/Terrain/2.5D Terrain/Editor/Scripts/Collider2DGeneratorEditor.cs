using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    [CustomEditor(typeof(Collider2DGenerator))]
    public class Collider2DGeneratorEditor : Editor
    {
        protected Collider2DGenerator generator;

        protected Terrain25D terrain;
        public Terrain25D Terrain
        {
            get
            {
                if (terrain == null && target != null)
                {
                    terrain = ((Collider2DGenerator)target).GetComponentInParent<Terrain25D>();
                }
                return terrain;
            }
        }

        public void Awake()
        {
            generator = target as Collider2DGenerator;
        }

        public override void OnInspectorGUI()
        {
            Terrain25DEditor.DrawInspectorGUI(Terrain);

            var s = new GUIStyle(GUI.skin.label);
            s.fontStyle = FontStyle.Bold;
            GUILayout.Label("[] Collider Generator", s);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Generate Collider"))
            {
                generator.GenerateColliders();
            }
            GUILayout.EndVertical();

            base.OnInspectorGUI();
        }
    }
}
