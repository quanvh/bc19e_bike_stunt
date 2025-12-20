using Kamgam.Terrain25DLib.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    [System.Serializable]
    public class FoliageGeneratorSet
    {
        [Tooltip("The of the set. This is also the name of the root object in the hierarchy.")]
        public string Name = "Trees";

        [Tooltip("Turn off to skip this set during generation.")]
        public bool Enabled = true;

        [Tooltip("Settings for the set. Create one in the Project with: Assets > Create > 2.5D Terrain > FoliageSet Settings")]
        public FoliageGeneratorSetSettings Settings;

        [Tooltip("Mark the placed objects as static?")]
        public bool StaticMeshes = false;

        [Range(0, 100), Tooltip("Start position in percentage along the terrain X axis.")]
        public float Start = 0f;

        [Range(0, 100), Tooltip("End position in percentage along the terrain X axis.")]
        public float End = 100f;

        [Tooltip("How many objects should be generated along the local X axis. Useful to get organic looking incrase or decrease of foliage.")]
        public AnimationCurve Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });

        protected Transform root;

        public List<Vector3> FoliagePos = new List<Vector3> { };

        public Transform GetRoot(FoliageGenerator generator)
        {
            if (root == null)
            {
                if (generator != null)
                {
                    root = generator.transform.Find(Name);
                    if (root == null)
                    {
                        var obj = new GameObject(Name);
                        obj.transform.parent = generator.transform;
                        obj.transform.localPosition = Vector3.zero;
                        obj.transform.localRotation = Quaternion.identity;
                        obj.transform.localScale = Vector3.one;
                        root = obj.transform;
#if UNITY_EDITOR
                        UnityEditor.Undo.RegisterCreatedObjectUndo(obj, Name + " created");
#endif
                    }
                    return root;
                }
                else
                {
                    Debug.LogError("generator is null.");
                }
            }

            // Update name while we are at it.
            if (root != null && root.name != Name)
                root.name = Name;

            return root;
        }

        public void Destroy(FoliageGenerator generator)
        {
            if (GetRoot(generator) != null)
            {
                Utils.SmartDestroy(root.gameObject);
            }
        }


        public static int IntersectRayMesh(Ray ray, MeshFilter meshFilter, List<RaycastHit> hits)
        {
#if UNITY_EDITOR
            return UtilsEditor.IntersectRayMeshMultiple(ray, meshFilter, hits);
#else
            // TODO: Use this at runtime https://forum.unity.com/threads/editor-raycast-against-scene-meshes-without-collider-editor-select-object-using-gui-coordinate.485502/#post-7246292
            //       Has no normal info though.
            Debug.LogError("FoliageGenerator.IntersectRayMesh not implementend for runtime use.");
            return -1;
#endif
        }

        protected static void AssignMaterialFromRatios(GameObject obj, FoliageGeneratorSetSettings.PrefabRatio prefabRatio)
        {
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
                {
                    if (prefabRatio.HasMaterialForIndex(i))
                    {
                        var sharedMaterials = meshRenderer.sharedMaterials;
                        var marterial = prefabRatio.GetRandomMaterial(i);
                        sharedMaterials[i] = marterial;
                        meshRenderer.sharedMaterials = sharedMaterials;
                    }
                }
            }
            else if (obj.transform.childCount > 0)
            {
                // maybe the meshes are in children, if yes, then interpret the materialIndex as childIndex and assign the material to the childs first material
                for (int c = 0; c < obj.transform.childCount; c++)
                {
                    meshRenderer = obj.transform.GetChild(c).GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        if (prefabRatio.HasMaterialForIndex(c))
                        {
                            meshRenderer.sharedMaterial = prefabRatio.GetRandomMaterial(c);
                        }
                    }
                }
            }
        }

        public void DestroyInSelectedArea(FoliageGenerator generator)
        {
            if (generator == null)
                return;

            if (GetRoot(generator) == null)
                return;

            (float minX, float maxX, _, _) = CalculateMinMaxXZ(generator, Settings.Margin, inLocalSpace: true);
            float distanceX = maxX - minX;
            destroyInsideMinMaxX(generator, minX + distanceX * (Start / 100f), minX + distanceX * (End / 100f));
        }

        protected void destroyInsideMinMaxX(FoliageGenerator generator, float minX, float maxX)
        {
            var root = GetRoot(generator);
            for (int t = root.childCount - 1; t >= 0; t--)
            {
                var pos = root.GetChild(t).localPosition;
                if (pos.x >= minX && pos.x <= maxX)
                {
                    Utils.SmartDestroy(root.GetChild(t).gameObject);
                }
            }
        }

        public static (float, float, float, float) CalculateMinMaxXZ(FoliageGenerator generator, float margin, bool inLocalSpace)
        {
            var bounds = generator.GetBounds();

            float fullMinX = bounds.min.x;
            float fullMaxX = bounds.max.x;
            float fullDistance = fullMaxX - fullMinX;

            float minX = Mathf.Clamp(fullMinX, fullMinX + margin, fullMaxX - margin);
            float maxX = Mathf.Clamp(fullMinX + fullDistance, fullMinX + margin, fullMaxX - margin);

            if (!inLocalSpace)
            {
                return (minX, maxX, bounds.min.z, bounds.max.z);
            }
            else
            {
                var min = generator.transform.InverseTransformPoint(bounds.min);
                var max = generator.transform.InverseTransformPoint(bounds.max);
                return (min.x + margin, max.x - margin, min.z, max.z);
            }
        }

        public void Generate(FoliageGenerator generator, MeshRenderer[] meshRenderers, bool replace = false)
        {
            if (!Enabled)
                return;

            if (Settings == null)
            {
                Debug.LogError("Foliage Generator Set '" + Name + "' has no Settings. Skipping this set.");
                return;
            }

            if (generator.MeshRenderers == null || generator.MeshRenderers.Length == 0)
                return;

            (float minX, float maxX, _, _) = CalculateMinMaxXZ(generator, Settings.Margin, inLocalSpace: true);
            float distanceX = maxX - minX;
            destroyInsideMinMaxX(generator, minX + distanceX * (Start / 100f), minX + distanceX * (End / 100f));

#if UNITY_EDITOR
            if (meshRenderers.Length > 1)
                Debug.Log("Using the FoliageGenerator on multiple small meshes is slower than using it on one mesh. Try using the 'Combine Meshes' setting in the MeshGenerator.");
#endif


            foreach (var renderer in meshRenderers)
            {
#if UNITY_EDITOR
                if (replace)
                    RePlaceMesh(generator, renderer);
                else
                    GenerateForMesh(generator, renderer);
#else
                RePlaceMesh(generator, renderer);
#endif
            }
        }

        public void RePlaceMesh(FoliageGenerator generator, MeshRenderer meshRenderer)
        {
            if (!Enabled)
                return;

            if (Settings == null)
            {
                Debug.LogError("Foliage Generator Set '" + Name + "' has no Settings. Skipping this set.");
                return;
            }

            if (!Settings.HasValidPrefabRatios())
            {
                Debug.LogWarning("Foliage Generator Set '" + Name + "' Settings do not have any Prefabs assigned. Skipping this set.");
                return;
            }

            MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;

            if (Start >= End)
                return;

            foreach (Vector3 pos in FoliagePos)
            {
                var prefabSettings = Settings.GetRandomPrefab();
                var obj = Utils.SmartInstantiate(prefabSettings.Prefab, GetRoot(generator));
                obj.isStatic = StaticMeshes;
                obj.transform.localScale = Vector3.one * Random.Range(Settings.MinScale, Settings.MaxScale);
                obj.transform.SetPositionAndRotation(pos, Quaternion.identity);

                obj.transform.Rotate(0, Random.Range(-0.5f * Settings.RotYVarianceInDeg, 0.5f * Settings.RotYVarianceInDeg),
                    Random.Range(-0.5f * Settings.RotZVarianceInDeg, 0.5f * Settings.RotZVarianceInDeg), Space.Self);
                AssignMaterialFromRatios(obj, prefabSettings);
            }
        }

        public void SaveFoliagePos(FoliageGenerator generator)
        {
            FoliagePos.Clear();
            foreach (Transform obj in GetRoot(generator))
            {
                FoliagePos.Add(obj.transform.position);
            }
        }

        public void GenerateForMesh(FoliageGenerator generator, MeshRenderer meshRenderer)
        {
            if (!Enabled)
                return;

            if (Settings == null)
            {
                Debug.LogError("Foliage Generator Set '" + Name + "' has no Settings. Skipping this set.");
                return;
            }

            if (!Settings.HasValidPrefabRatios())
            {
                Debug.LogWarning("Foliage Generator Set '" + Name + "' Settings do not have any Prefabs assigned. Skipping this set.");
                return;
            }

            MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;

            if (Start >= End)
                return;

            FoliagePos.Clear();

            var bounds = generator.GetBounds();
            (float minX, float maxX, float minZ, float maxZ) = CalculateMinMaxXZ(generator, Settings.Margin, inLocalSpace: true);
            float xDistance = maxX - minX;

            float distanceFront = Settings.DensityMinDistance + (1f - Settings.DensityFront) * (Settings.DensityMaxDistance - Settings.DensityMinDistance);
            float distanceBack = Settings.DensityMinDistance + (1f - Settings.DensityBack) * (Settings.DensityMaxDistance - Settings.DensityMinDistance);

            // create
            var triangles = meshFilter.sharedMesh.triangles;
            var vertices = meshFilter.sharedMesh.vertices;
            float progress = 0; // 0 to 1
            Vector3 normal, inPlane, v0, v1, v2;
            int counter = 0;
            List<RaycastHit> hits = new List<RaycastHit>();
            for (int i = 0; i < 2; i++)
            {
                bool isFront = i == 0;

                float distance = isFront ? distanceFront : distanceBack;
                float middleWidth = isFront ? generator.FrontMiddleWidth : generator.BackMiddleWidth;
                float middleWithOverlap = Settings.MiddleWidthOverlap;
                float fullBevelWidth = isFront ? generator.FrontBevelWidth : generator.BackBevelWidth;
                float bevelWidth = Random.Range(0, fullBevelWidth);
                float currentX = minX;
                while (currentX < maxX)
                {
                    progress = (currentX - minX) / xDistance;

                    // skip if out of gen limits
                    if (progress < Start / 100f || progress > End / 100f)
                    {
                        currentX += distance;
                        continue;
                    }

                    float rayX = currentX + Random.Range(-distance * 0.4f, distance * 0.4f);
                    float rayY = generator.transform.InverseTransformPoint(bounds.max).y;
                    float rayZ = Random.Range(0, isFront ? minZ : maxZ);

                    // In the middle
                    if (!Settings.PlaceInMiddle && Mathf.Abs(rayZ) < Mathf.Abs(middleWidth) - Settings.MiddleWidthOverlap)
                    {
                        currentX += distance;
                        continue;
                    }

                    // Outside of depth limits?
                    if ((isFront && Settings.DepthLimitFront >= 0f && rayZ < -Settings.DepthLimitFront)
                        || (!isFront && Settings.DepthLimitBack >= 0f && rayZ > Settings.DepthLimitBack))
                    {
                        currentX += distance;
                        continue;
                    }

                    Vector3 rayStartPosLocal = new Vector3(rayX, rayY, rayZ);
                    var rayStartPos = generator.transform.TransformPoint(rayStartPosLocal);
                    rayStartPos.y += 1f + generator.RayStartOffsetY;

                    currentX += distance;

                    // fall of in z direction
                    float likelyhoodForZAxis = Settings.DepthCurve.Evaluate(Mathf.Abs(rayStartPosLocal.z) / Mathf.Max(Mathf.Abs(minZ), Mathf.Abs(maxZ)));
                    float likelyhoodForXAxis = Curve.Evaluate(progress);
                    bool spawn = Utils.RandomResult(likelyhoodForZAxis) && Utils.RandomResult(Curve.Evaluate(progress));
                    if (!spawn)
                        continue;

                    // Draw for debugging
                    //Debug.DrawRay(rayStartPos, Vector3.down * 200, Color.blue, 10f);

                    // Check if there is a mesh
                    Ray ray = new Ray(rayStartPos, Vector3.down);
                    hits.Clear();
                    int hitCount = IntersectRayMesh(ray, meshFilter, hits);
                    if (hitCount == 0)
                        continue;

                    foreach (var hit in hits)
                    {
                        // Draw for debugging
                        //Debug.DrawRay(hit.point, Vector3.up * 0.5f, Color.blue, 10f);
                        //Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.cyan, 10f);

                        if (Settings.UseMeshNormalsForAlignment)
                        {
                            normal = hit.normal;
                            inPlane = Utils.CreateNormal(hit.normal);
                        }
                        else
                        {
                            // calc none smoothed normal for the hit triangle
                            v0 = meshFilter.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3]]);
                            v1 = meshFilter.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 1]]);
                            v2 = meshFilter.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 2]]);
                            normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                            inPlane = v0 - v1;
                        }

                        var hitOrientation = Vector3.Dot(Vector3.up, normal.normalized);
                        if (!Settings.AllowUpsideDown && hitOrientation < 0)
                            continue;


                        // Draw for debugging
                        //var m = 1 - Vector3.Project(normal.normalized, Vector3.up).magnitude;
                        //UtilsDebug.DrawVectorLERP(hit.point, Vector3.up, m, m < Settings.GrassSlopeLimit ? Color.green : Color.red, 15);

                        if (1 - Vector3.Project(normal.normalized, Vector3.up).magnitude < Settings.SlopeLimit)
                        {
                            var prefabSettings = Settings.GetRandomPrefab();
                            var obj = Utils.SmartInstantiate(prefabSettings.Prefab, GetRoot(generator));
                            obj.isStatic = StaticMeshes;
                            obj.transform.localScale = Vector3.one * Random.Range(Settings.MinScale, Settings.MaxScale);
                            obj.transform.position = hit.point + new Vector3(0, -Settings.GroundIntersection * obj.transform.localScale.y, 0); // stick them in the earth a little
                            obj.transform.rotation = Quaternion.identity;

                            if (Settings.AlignRotationWithTerrain)
                            {
                                var rot = obj.transform.rotation;
                                rot.SetLookRotation(inPlane, normal);
                                obj.transform.rotation = rot;
                            }

                            obj.transform.Rotate(0, Random.Range(-0.5f * Settings.RotYVarianceInDeg, 0.5f * Settings.RotYVarianceInDeg),
                                Random.Range(-0.5f * Settings.RotZVarianceInDeg, 0.5f * Settings.RotZVarianceInDeg), Space.Self);
                            AssignMaterialFromRatios(obj, prefabSettings);
                            counter++;

                            FoliagePos.Add(obj.transform.position);
#if UNITY_EDITOR
                            UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Foliage Obj " + obj.name);

                            //UtilsDebug.DrawVector(hit.point, normal * 10, Color.blue, 10); 
                            //UtilsDebug.DrawVector(hit.point, inPlane, Color.yellow, 10);
#endif
                        }
                        //UtilsDebug.DrawVector(ray.origin, ray.direction * 100, Color.blue, 10);

                        // stop after first hit
                        if (!Settings.PlaceInCaves)
                            break;
                    }
                }
            }

#if UNITY_EDITOR
            if (Terrain25DSettings.GetOrCreateSettings().ShowLogs)
                Debug.Log("Generated " + counter + " objects in " + Name + ".");
#endif
        }

        /// <summary>
        /// Returns the distance to the mesh from the raycast origin. Returns float.NaN if no hit was found.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="objectWithMeshFilter"></param>
        /// <param name="maxDistance"></param>
        /// <returns>Distance as float or float.NaN</returns>
        private static float RayCastMesh(Ray ray, GameObject objectWithMeshFilter, float maxDistance = 1000f)
        {
            if (objectWithMeshFilter != null)
            {
                float distance = float.NaN;
                // check all triangles for hit with raycast
                var meshFilter = objectWithMeshFilter.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    int[] triangles = meshFilter.sharedMesh.triangles;
                    Vector3[] vertices = meshFilter.sharedMesh.vertices;
                    int a, b, c;
                    for (int i = 0; i < triangles.Length; i = i + 3)
                    {
                        a = triangles[i];
                        b = triangles[i + 1];
                        c = triangles[i + 2];
                        /*
                        Debug.DrawLine(meshFilter.transform.TransformPoint(vertices[a]), meshFilter.transform.TransformPoint(vertices[b]), Color.red, 3.0f);
                        Debug.DrawLine(meshFilter.transform.TransformPoint(vertices[b]), meshFilter.transform.TransformPoint(vertices[c]), Color.red, 3.0f);
                        Debug.DrawLine(meshFilter.transform.TransformPoint(vertices[c]), meshFilter.transform.TransformPoint(vertices[a]), Color.red, 3.0f); //*/
                        distance = IntersectRayTriangle(
                            ray,
                            meshFilter.transform.TransformPoint(vertices[a]),
                            meshFilter.transform.TransformPoint(vertices[b]),
                            meshFilter.transform.TransformPoint(vertices[c]));
                        if (!float.IsNaN(distance))
                        {
                            break;
                        }
                    }
                }
                if (float.IsNaN(distance) == false)
                {
                    if (distance < maxDistance)
                    {
                        return distance;
                    }
                }
            }

            return float.NaN;
        }

        const float kEpsilon = 0.000001f;

        /// <summary>
        /// Thanks to: https://answers.unity.com/questions/861719/a-fast-triangle-triangle-intersection-algorithm-fo.html
        /// Ray-versus-triangle intersection test suitable for ray-tracing etc.
        /// Port of Möller–Trumbore algorithm c++ version from:
        /// https://en.wikipedia.org/wiki/Möller–Trumbore_intersection_algorithm
        /// </summary>
        /// <returns><c>The distance along the ray to the intersection</c> if one exists, <c>NaN</c> if one does not.</returns>
        /// <param name="ray">the ray</param>
        /// <param name="v0">A vertex 0 of the triangle.</param>
        /// <param name="v1">A vertex 1 of the triangle.</param>
        /// <param name="v2">A vertex 2 of the triangle.</param>
        public static float IntersectRayTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            // edges from v1 & v2 to v0.
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 h = Vector3.Cross(ray.direction, e2);
            float a = Vector3.Dot(e1, h);
            if ((a > -kEpsilon) && (a < kEpsilon))
            {
                return float.NaN;
            }

            float f = 1.0f / a;

            Vector3 s = ray.origin - v0;
            float u = f * Vector3.Dot(s, h);
            if ((u < 0.0f) || (u > 1.0f))
            {
                return float.NaN;
            }

            Vector3 q = Vector3.Cross(s, e1);
            float v = f * Vector3.Dot(ray.direction, q);
            if ((v < 0.0f) || (u + v > 1.0f))
            {
                return float.NaN;
            }

            float t = f * Vector3.Dot(e2, q);
            if (t > kEpsilon)
            {
                return t;
            }
            else
            {
                return float.NaN;
            }
        }
    }

}