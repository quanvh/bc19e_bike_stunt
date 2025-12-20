using Kamgam.Terrain25DLib.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    public class FoliageGenerator : MonoBehaviour
    {
        [Range(0, 100), Tooltip("Start position in percentage along the terrain X axis.")]
        public float Start = 0f;

        [Range(0, 100), Tooltip("End position in percentage along the terrain X axis.")]
        public float End = 100f;

        [Tooltip("Usually the raycasts start at the top of the terrain. Use this to change the start position.")]
        public float RayStartOffsetY = 0f;

#if UNITY_2022_1_OR_NEWER
        // Bug in 2021.1 beta (first element layout is broken)
		// https://forum.unity.com/threads/case-1412863-inspector-renders-data-of-first-element-of-array-over-the-others-when-expanded.1258950/
		[NonReorderable]
#endif
        public FoliageGeneratorSet[] GeneratorSets = new FoliageGeneratorSet[]
        {
            new FoliageGeneratorSet()
        };

        protected Terrain25D terrain;
        public Terrain25D Terrain
        {
            get
            {
                if (terrain == null)
                {
                    terrain = transform.GetComponentInParent<Terrain25D>();
                }
                return terrain;
            }
        }

        protected MeshGenerator meshGenerator;
        public MeshGenerator MeshGenerator
        {
            get
            {
                if (meshGenerator == null)
                {
                    meshGenerator = transform.parent.GetComponentInChildren<MeshGenerator>();
                }
                return meshGenerator;
            }
        }

        public MeshRenderer[] MeshRenderers
        {
            get
            {
                if (MeshGenerator != null)
                {
                    return MeshGenerator.GetMeshRenderers();
                }
                return DefaultMeshRenderers;
            }
        }

        [Header("Settings used if there is no 2.5D Terrain.")]
        public float DefaultFrontBevelWidth = 3f;
        public float DefaultFrontMiddleWidth = 2f;
        public float DefaultBackBevelWidth = 3f;
        public float DefaultBackMiddleWidth = 2f;
        public MeshRenderer[] DefaultMeshRenderers;

        public float FrontBevelWidth
        {
            get
            {
                if (MeshGenerator != null)
                {
                    return MeshGenerator.FrontBevelWidth;
                }
                else
                {
                    return DefaultFrontBevelWidth;
                }
            }
        }

        public float BackBevelWidth
        {
            get
            {
                if (MeshGenerator != null)
                {
                    return MeshGenerator.BackBevelWidth;
                }
                else
                {
                    return DefaultBackBevelWidth;
                }
            }
        }

        public float FrontMiddleWidth
        {
            get
            {
                if (MeshGenerator != null)
                {
                    return MeshGenerator.FrontMiddleWidth;
                }
                else
                {
                    return DefaultFrontMiddleWidth;
                }
            }
        }

        public float BackMiddleWidth
        {
            get
            {
                if (MeshGenerator != null)
                {
                    return MeshGenerator.BackMiddleWidth;
                }
                else
                {
                    return DefaultBackMiddleWidth;
                }
            }
        }

        /// <summary>
        /// This is the axis-aligned bounding box fully enclosing the child meshes in world space.
        /// </summary>
        /// <returns></returns>
        public Bounds GetBounds()
        {
            var meshRenderers = meshGenerator.GetMeshRenderers();
            Bounds bounds = meshRenderers[0].bounds;
            for (int i = 1; i < meshRenderers.Length; i++)
            {
                bounds.Encapsulate(meshRenderers[i].bounds);
            }
            return bounds;
        }

        public void SetStartAndSend(float start, float end)
        {
            foreach (var set in GeneratorSets)
            {
                set.Start = start;
                set.End = end;
            }
        }

        public void Generate(bool replace = false)
        {
            foreach (var set in GeneratorSets)
            {
                set.Generate(this, MeshRenderers, replace);
            }
        }

        public void SaveFolliage()
        {
            foreach (var set in GeneratorSets)
            {
                set.SaveFoliagePos(this);
            }
        }

        public void GenerateSet(FoliageGeneratorSet set)
        {
            set.Generate(this, MeshRenderers);
        }

        public void GenerateSet(string name)
        {
            foreach (var set in GeneratorSets)
            {
                if (set.Name == name)
                    set.Generate(this, MeshRenderers);
            }
        }

        public void GenerateSet(FoliageGeneratorSetSettings setSettings)
        {
            var set = GetSet(setSettings);
            if (set != null)
                GenerateSet(set);
        }

        public FoliageGeneratorSet GetSet(FoliageGeneratorSetSettings setSettings)
        {
            foreach (var set in GeneratorSets)
            {
                if (set.Settings == setSettings)
                    return set;
            }
            return null;
        }

        public void DestroySet(FoliageGeneratorSet set)
        {
            set.Destroy(this);
        }

        public void DestroyAll()
        {
            foreach (var set in GeneratorSets)
            {
                set.Destroy(this);
            }
        }

        public void Destroy()
        {
            foreach (var set in GeneratorSets)
            {
                set.DestroyInSelectedArea(this);
            }
        }

        public static void PlaceObjects(IList<GameObject> gameObjects)
        {
            if (gameObjects == null || gameObjects.Count == 0)
                return;

#if !UNITY_EDITOR
            Debug.LogError("Not supported at runtime.");
			return;
#else
            foreach (var go in gameObjects)
            {
                RaycastHit2D[] hits = new RaycastHit2D[10];
                int results = Physics2D.RaycastNonAlloc(go.transform.position + Vector3.up * 2, Vector3.down, hits, 200f);
                int i = hits.FirstIndexNonAlloc(h => h.transform != null && h.transform.parent.parent.gameObject.GetComponentInChildren<FoliageGenerator>() != null);
                if (results > 0 && i >= 0)
                {
                    // interset with actual mesh
                    var foliageGenerator = hits[i].transform.parent.parent.gameObject.GetComponentInChildren<FoliageGenerator>();
                    if (foliageGenerator != null)
                    {
                        Ray ray = new Ray(go.transform.position + Vector3.up * 2, Vector3.down);
                        RaycastHit hit;
                        foreach (var meshRenderer in foliageGenerator.MeshRenderers)
                        {
                            var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                            if (UtilsEditor.IntersectRayMesh(ray, meshFilter, out hit) > 0)
                            {
                                UnityEditor.Undo.RegisterCompleteObjectUndo(go.transform, "Place selected");
                                go.transform.position = hit.point + Vector3.down * 0.1f;
                            }
                        }
                    }
                }
            }
#endif
        }
    }
}