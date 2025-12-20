using UnityEditor;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
	[CustomEditor(typeof(FoliageGeneratorSetSettings))]
	public class FoliageGeneratorSetSettingsEditor : Editor
	{
        protected FoliageGeneratorSetSettings setSettings;

        public void Awake()
        {
            setSettings = target as FoliageGeneratorSetSettings;
        }

        public override void OnInspectorGUI()
        {
            if (FoliageGeneratorEditor.LastSelectedFoliageGenerator != null)
            {
                if (GUILayout.Button("Back to Foliage Generator"))
                {
                    Selection.objects = new GameObject[] { FoliageGeneratorEditor.LastSelectedFoliageGenerator.gameObject };
                }

                if (GUILayout.Button("Generate Foliage"))
                {
                    FoliageGeneratorEditor.LastSelectedFoliageGenerator.Generate();
                }

                var set = FoliageGeneratorEditor.LastSelectedFoliageGenerator.GetSet(setSettings);
                if(set != null)
                {
                    var s = new GUIStyle(GUI.skin.label);
                    s.fontStyle = FontStyle.Bold;
                    GUILayout.Label("Generate Single Set:", s);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Generate " + set.Name))
                    {
                        FoliageGeneratorEditor.LastSelectedFoliageGenerator.GenerateSet(set);
                    }
                    if (GUILayout.Button("Destroy " + set.Name))
                    {
                        FoliageGeneratorEditor.LastSelectedFoliageGenerator.DestroySet(set);
                    }
                    GUILayout.EndHorizontal();
                }   
            }

            base.OnInspectorGUI();
        }
    }
}