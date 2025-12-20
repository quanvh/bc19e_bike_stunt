using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    public static class SetupTools
    {
        [MenuItem("Tools/Bike Racing Template/Setup Layers", priority = 500)]
        public static void SetupLayers()
        {

            Debug.Log("Adding Layers.");

            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

            if (asset != null && asset.Length > 0)
            {
                SerializedObject serializedObject = new SerializedObject(asset[0]);
                SerializedProperty layers = serializedObject.FindProperty("layers");

                // ADD layers
                // Keep in mind: indices below 6 are the built-in layers and should not be changed.
                AddLayerAt(layers, 8, "Bike");
                AddLayerAt(layers, 9, "BikeWheels");
                AddLayerAt(layers, 10, "BikeTrigger");
                AddLayerAt(layers, 11, "Ground");

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                // SET COLLISION MASKS
                
                // "Ground" collides with everything
                Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Ground"), Physics2D.AllLayers);

                // "Bike" collides with everything except "BikeTriggers" and "BikeWheels"
                Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Bike"), Physics2D.AllLayers);
                // Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Bike"), LayerMask.NameToLayer("BikeTriggers")); // done by BikeWheels below
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Bike"), LayerMask.NameToLayer("BikeWheels"));

                // "BikeWheels" collide with everything except the "Bike" layer.
                Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("BikeWheels"), Physics2D.AllLayers);
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("BikeWheels"), LayerMask.NameToLayer("Bike"));

                // "BikeTrigger" collides only with the "Default" layer (this assumes your triggers are on the default layer).
                Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("BikeTrigger"), 1 << LayerMask.NameToLayer("Default"));
            }
        }

        static void AddLayerAt(SerializedProperty layers, int index, string layerName, bool tryOtherIndex = true)
        {
            // Skip if a layer with the name already exists.
            for (int i = 0; i < layers.arraySize; ++i)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    Debug.Log("Skipping layer '" + layerName + "' because it already exists.");
                    return;
                }
            }

            // Extend layers if necessary
            if (index >= layers.arraySize)
                layers.arraySize = index + 1;

            // set layer name at index
            var element = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
                Debug.Log("Added layer '" + layerName + "' at index " + index + ".");
            }
            else
            {
                Debug.LogWarning("Could not add layer at index " + index + " because there already is another layer '" + element.stringValue + "'." );

                if (tryOtherIndex)
                {
                    // Go up in layer indices and try to find an empty spot.
                    for (int i = index + 1; i < 32; ++i)
                    {
                        // Extend layers if necessary
                        if (i >= layers.arraySize)
                            layers.arraySize = i + 1;

                        // Try to add layer at the incremented index
                        element = layers.GetArrayElementAtIndex(i);
                        if (string.IsNullOrEmpty(element.stringValue))
                        {
                            element.stringValue = layerName;
                            Debug.Log("Added layer '" + layerName + "' at index " + i + " instead of " + index + ".");
                            return;
                        }
                    }

                    Debug.LogError("Could not add layer " + layerName + " because there is no space left in the layers array.");
                }
            }
        }

        static double addScenesTime;

        public static void SetupBuildScenesDelayed()
        {
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;
            addScenesTime = EditorApplication.timeSinceStartup + 1;
        }

        static void onEditorUpdate()
        {
            if (addScenesTime - EditorApplication.timeSinceStartup < 0)
            {
                EditorApplication.update -= onEditorUpdate;
                SetupBuildScenes();
                return;
            }
        }


        [MenuItem("Tools/Bike Racing Template/Setup Build Scenes", priority = 500)]
        public static void SetupBuildScenes()
        {
            Debug.Log("Adding scenes to build");

            string[] scenePaths = new string[]
            {
                "Assets/BikeRacingTemplate2.5D/Scenes/Main.unity",
                "Assets/BikeRacingTemplate2.5D/Scenes/UIs.unity",
                "Assets/BikeRacingTemplate2.5D/Scenes/Level1.unity",
                "Assets/BikeRacingTemplate2.5D/Scenes/Level2.unity",
                "Assets/BikeRacingTemplate2.5D/Scenes/Level3.unity",
                "Assets/BikeRacingTemplate2.5D/Scenes/DialogAlert.unity"
            };

            List<EditorBuildSettingsScene> editorBuildSettingsScenes = EditorBuildSettings.scenes.ToList();
            foreach (var path in scenePaths)
            {
                // Skip if already in the list.
                var existingScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.path == path);
                if (existingScene != null)
                {
                    Debug.Log("Skipping scene '" + path + "' because it already is part of the build.");
                    continue;
                }

                // Add (make sure Main.unity is the first scene).
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (asset != null)
                {
                    if(path.EndsWith("Main.unity"))
                        editorBuildSettingsScenes.Insert(0, new EditorBuildSettingsScene(path, enabled: true));
                    else
                        editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(path, enabled: true));
                    Debug.Log("Added scene '" + path + "'.");
                }
            }
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
    }
}