using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.BikeRacing25D
{
    public static class MaterialShaderFixer
    {
        public enum RenderPiplelineType
        {
            URP, HDRP, Standard
        }

        static Dictionary<RenderPiplelineType, string> Packages = new Dictionary<RenderPiplelineType, string>
        {
			{ RenderPiplelineType.HDRP, "Assets/BikeRacingTemplate2.5D/Packages/HDRP.unitypackage" },
            { RenderPiplelineType.URP, "Assets/BikeRacingTemplate2.5D/Packages/URP.unitypackage" }
        };

        static RenderPiplelineType _createdForRenderPipleline;
        static System.Action _onComplete;
        static double importPackageTime;

        public static void FixMaterialsDelayed(RenderPiplelineType createdForRenderPipleline, System.Action onComplete)
        {
            // Materials may not be loaded at this time. Thus we wait for them to be imported.
            _createdForRenderPipleline = createdForRenderPipleline;
            _onComplete = onComplete;
            EditorApplication.update -= onEditorUpdate;
            EditorApplication.update += onEditorUpdate;
            importPackageTime = EditorApplication.timeSinceStartup + 3;
        }

        static void onEditorUpdate()
        {
            if (importPackageTime - EditorApplication.timeSinceStartup < 0)
            {
                EditorApplication.update -= onEditorUpdate;
                FixMaterials(_createdForRenderPipleline);
                return;
            }
        }

        [MenuItem("Tools/Bike Racing Template/Fix Materials", priority = 501)]
        public static void FixMaterials()
        {
            Debug.Log("Fixing materials.");
            FixMaterials(RenderPiplelineType.Standard);
        }

        public static void FixMaterials(RenderPiplelineType createdForRenderPipleline)
        {
            var currentRenderPipline = GetCurrentRenderPiplelineType();
            if (currentRenderPipline != createdForRenderPipleline)
            {
                /*
                EditorUtility.DisplayDialog(
                    "Render pipeline mismatch detected.", 
                    "The materials in this asset have been created with the " + createdForRenderPipleline.ToString() + " Pipeline. You are using '" + currentRenderPipline.ToString() + "'.\n\nThe materials for your render pipeline will now be imported. In case some are still broken afterwards please fix those manually.",
                    "Understood"
                    );
                */
                Debug.LogWarning("Bike Racing Template: The materials in this asset have been created with the " + createdForRenderPipleline.ToString() + " Pipeline. You are using '" + currentRenderPipline.ToString() + "'.\n\nThe materials for your render pipeline will now be imported. In case some are still broken afterwards please fix those manually.");

                AssetDatabase.importPackageCompleted -= onPackageImported;
                AssetDatabase.importPackageCompleted += onPackageImported;

                string packagePath = Packages[currentRenderPipline];
                AssetDatabase.ImportPackage(packagePath, interactive: false);

                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log("Nothing to fix.");
                _onComplete?.Invoke();
            }
        }

        static void onPackageImported(string packageName)
        {
            if (!packageName.StartsWith("URP") && !packageName.StartsWith("HDRP"))
                return;

            AssetDatabase.importPackageCompleted -= onPackageImported;

            CheckPackages.CheckForPackage("com.unity.shadergraph", (found) =>
            {
                if (!found)
                {
                    // Not shader graph installed -> Fix tri planar shader by applying the default shader.
                    string msg = "Shader Graph Package is not installed.\n\nTo use the provided Tri-Planar shader you'll have to install shader graph: https://docs.unity3d.com/Packages/com.unity.shadergraph@latest/ \n\nFor now the 'Standard' shader will be assigned to all Tri-Planar materials";
                    EditorUtility.DisplayDialog("Shader Graph Package is not installed!", msg, "Okay");

                    Debug.LogWarning("Shader Graph Package is not installed. Falling back to default shader.");

                    // Revert shadergraph shader to default shader if shadergraph package is not installed
                    var shader = GetDefaultShader();
                    if (shader != null)
                    {
                        var currentRenderPipeline = GetCurrentRenderPiplelineType();
                        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/BikeRacingTemplate2.5D/Terrain/Materials/Terrain.mat");
                        if (material != null)
                        {
                            material.shader = shader;
                            material.color = new Color(0.1f, 0.600f, 0.1f);
                        }
                    }
                    else
                    {
                        Debug.LogError("No default shader found!");
                    }
                }

                AssetDatabase.SaveAssets();
                _onComplete?.Invoke();
            });
        }

        public static RenderPiplelineType GetCurrentRenderPiplelineType()
        {
            // Assume URP as default
            var renderPipeline = RenderPiplelineType.URP;

            // check if Standard or HDRP
            if (getUsedRenderPipeline() == null)
                renderPipeline = RenderPiplelineType.Standard; // Standard
            else if (!getUsedRenderPipeline().GetType().Name.Contains("Universal"))
                renderPipeline = RenderPiplelineType.HDRP; // HDRP

            return renderPipeline;
        }

        public static Shader GetDefaultShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Standard");
            else
                return getUsedRenderPipeline().defaultShader;
        }

        public static Shader GetDefaultParticleShader()
        {
            if (getUsedRenderPipeline() == null)
                return Shader.Find("Particles/Standard Unlit");
            else
                return getUsedRenderPipeline().defaultParticleMaterial.shader;
        }

        /// <summary>
        /// Returns the current pipline. Returns NULL if it's the standard render pipeline.
        /// </summary>
        /// <returns></returns>
        static UnityEngine.Rendering.RenderPipelineAsset getUsedRenderPipeline()
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            else
                return UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        }

    }
}