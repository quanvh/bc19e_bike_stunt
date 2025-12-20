using Kamgam.Terrain25DLib.ClipperLib;
using Kamgam.Terrain25DLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kamgam.Terrain25DLib
{
    /// <summary>
    /// Creates the front and back meshes.<br />
    /// It does so by shrinking a curve via ClipperLib offset.
    /// </summary>
    public partial class MeshGenerator : MonoBehaviour
    {
        public enum BevelType { Circular, Linear /*, Custom*/ }

        public const string CombinedTerrainObjectName = "CombinedMesh";
        public const string VertexName = "Vertex";

        public delegate void PreMeshGenerationHandler(MeshGenerator meshGenerator, List<Vector3[]> shapes, List<Vector3[]> holes, List<MeshPointInfo> infos, bool ignoreSmoothCombineAndErosion);

        /// <summary>
        /// Called before the mesh generation is started. Use this to change things programmatically. Randomize some things for example.
        /// </summary>
        public event PreMeshGenerationHandler OnPreMeshGeneration;

        /// <summary>
        /// Called after mesh generation finished. Receives a list of all generated MeshFilters. Use to execute some action after the mesh has been generated.
        /// </summary>
        public event Action<List<MeshFilter>> OnPostMeshGenerated;

        [Header("Front")]

        /* Results are too unpredictable, shelved for now. TODO: investigate for future updates.
		public AnimationCurve BevelCurve = new AnimationCurve(
			new Keyframe(0f, 1f,  0f, 0f), new Keyframe(1f, 0f, -3f, 0f)
			);
		//*/

        [Tooltip("How thick the bevel should be in the direction of the z axis.")]
        [Range(0.2f, 30)]
        public float FrontBevelWidth = 3f;

        [Range(0.1f, 30)]
        public float FrontBevelScale = 1f;

        [Tooltip("Set to 0 to remove the bevel.")]
        [Range(0, 10)]
        public int FrontBevelDivisions = 3;

        public BevelType FrontBevelType = BevelType.Circular;

        public bool FrontClosed = true;


        [Header("Middle")]

        public float FrontMiddleWidth = 1f;
        public float BackMiddleWidth = 1f;

        [Tooltip("Should the UVs of the middle parts be front projected (like a decal) or layouted separately?")]
        public bool MiddleFrontProjectUVs = false;

        [Tooltip("The sacle of the middle UVs if front projection is not used.")]
        public float MiddleUVScale = 1f;


        [Header("Back")]

        [Tooltip("How thick the bevel should be in the direction of the z axis.")]
        [Range(0.2f, 30)]
        public float BackBevelWidth = 3f;

        [Range(0.1f, 30)]
        public float BackBevelScale = 1.5f;

        [Tooltip("Set to 0 to remove the bevel.")]
        [Range(0, 10)]
        public int BackBevelDivisions = 3;

        public BevelType BackBevelType = BevelType.Circular;

        public bool BackClosed = false;


        [Header("Mesh Generation")]

        [Tooltip("Average the normals after generation to smoothen the surface?")]
        public bool SmoothNormals = true;

        [Tooltip("Merge smoothed vertices in combined meshes?\nNOTICE: this will mess-up the UV layout.")]
        public bool MergeSmoothedVertices = false;

        [Tooltip("Combine into one mesh?")]
        public bool CombineMeshes = true;

        [Header("Snow")]
        [Tooltip("Actually it's just an offset of the vertices on top of the mesh. I named it 'snow' since that makes it more understandable what it does.")]
        [Range(0f, 2f)]
        public float SnowThickness = 0;

        [Tooltip("Snow is only be applied to parts of the terrain which have a slope below this angle.")]
        [Range(0f, 90f)]
        public float SnowSlopeLimit = 50f;


        [Header("Erosion (experimental, slow)")]

        [Tooltip("Simulate erosion on the bevel to generate a more organic looking shape?")]
        public bool Erosion = true;

        [Tooltip("There may be holes if this value is too high. Try to reduce ErosionSegmentLength (!very slow!). Experiment with it.")]
        [Range(0.5f, 5f)]
        public float ErosionStrength = 1f;

        [Range(0.2f, 1f)]
        [Tooltip("Segment length for curve shortening used to simluate erosion (reduce for filigree shapes).")]
        public float ErosionSegmentLength = 0.5f;

        [Header("Mesh Properties")]

        [Tooltip("Mark the generated  mesh as static?")]
        public bool StaticMesh = true;

        [Tooltip("Should the terrain mesh cast shadows?")]
        public bool CastShadows = false;

        [Tooltip("Should the generated mesh have a special tag?")]
        [Helpers.TagSelector]
        public string MeshTag = "";

        public Material Material;

        public enum MeshCompressions { Off = 0, Low = 1, Mid = 2, High = 3 }
        public MeshCompressions MeshCompression = MeshCompressions.Mid;

        [Header("Clipper")]

        [Tooltip("Do NOT disable! Disabling this will increase the resolution of the mesh by a lot. Disable only if you are having issues (holes in geometry).")]
        public bool AutoCleanClipperPolygons = true;

        [Tooltip("Use this to control the clipping beahviour if 'AutoCleanClipperPolygons' is turned OFF.")]
        [Range(0.01f, 0.5f)]
        public float CleanClipperDistance = 0.1f;

        protected MeshFilter _combinedMesh;
        public MeshFilter CombinedMesh
        {
            get
            {
                if (_combinedMesh != null)
                {
                    return _combinedMesh;
                }
                else
                {
                    var obj = transform.Find(CombinedTerrainObjectName);
                    if (obj != null)
                    {
                        _combinedMesh = obj.GetComponent<MeshFilter>();
                    }
                    return _combinedMesh;
                }
            }

            set
            {
                _combinedMesh = value;
            }
        }

        [System.NonSerialized]
        public List<MeshMiddleSegment> MiddleSegments;

        [System.NonSerialized]
        public List<MeshFrontSegment> FrontSegments;

        public void GenerateMesh(bool ignoreSmoothCombineAndErosion = false)
        {
            var splineController = transform.parent.GetComponentInChildren<SplineController>(includeInactive: true);
            if (splineController.SplineCount > 0)
            {
                splineController.CombineAndRememberInfo();

                // match transform to spline controller
                transform.parent = splineController.transform.parent;
                transform.position = splineController.transform.position;
                transform.rotation = splineController.transform.rotation;
                transform.localScale = splineController.transform.localScale;

                var shapes = splineController.CombinationResult.GetShapesCopy();
                var holes = splineController.CombinationResult.GetHolesCopy();
                var splineInfos = splineController.CombinationResult.Infos;

                // apply snow?
                if (Mathf.Abs(SnowThickness) > 0.01f)
                {
                    float slopeAngle, offsetTop;
                    foreach (var shape in shapes)
                    {
                        float[] offsets = new float[shape.Length];
                        for (int i = 0; i < shape.Length; i++)
                        {
                            var p0 = shape[Utils.Mod(i - 1, shape.Length)];
                            var p1 = shape[Utils.Mod(i + 1, shape.Length)];
                            slopeAngle = Utils.AngleX(p0, p1);
                            if (slopeAngle < SnowSlopeLimit && UtilsPolygons.ContainsPoint(shape, new Vector3(shape[i].x, shape[i].y - 0.1f)))
                            {
                                offsetTop = SnowThickness * (1 - (slopeAngle / 90f));
                                offsets[i] = offsetTop;
                            }
                        }
                        for (int i = 0; i < shape.Length; i++)
                        {
                            if (offsets[i] != 0)
                            {
                                // avoid pushing offset points into the shape
                                if (!UtilsPolygons.ContainsPoint(shape, new Vector3(shape[i].x, shape[i].y + offsets[i])))
                                {
                                    shape[i].y += offsets[i];
                                }
                            }
                        }
                    }
                    foreach (var shape in holes)
                    {
                        float[] offsets = new float[shape.Length];
                        for (int i = 0; i < shape.Length; i++)
                        {
                            var p0 = shape[Utils.Mod(i - 1, shape.Length)];
                            var p1 = shape[Utils.Mod(i + 1, shape.Length)];
                            slopeAngle = Utils.AngleX(p0, p1);
                            if (slopeAngle < SnowSlopeLimit && !UtilsPolygons.ContainsPoint(shape, new Vector3(shape[i].x, shape[i].y - 0.1f)))
                            {
                                offsetTop = SnowThickness * (1 - (slopeAngle / 90f));
                                offsets[i] = offsetTop;
                            }
                        }
                        for (int i = 0; i < shape.Length; i++)
                        {
                            if (offsets[i] != 0)
                            {
                                // avoid pushing offset points into the shape
                                if (UtilsPolygons.ContainsPoint(shape, new Vector3(shape[i].x, shape[i].y + offsets[i])))
                                {
                                    shape[i].y += offsets[i];
                                }
                            }
                        }
                    }
                }

                // find all mesh infos
                List<MeshPointInfo> meshPointInfos = new List<MeshPointInfo>();
                foreach (var path in splineInfos)
                {
                    foreach (var info in path)
                    {
                        var meshBezierPoint0 = info.Point0.transform.GetComponent<MeshBezierPointInfo>();
                        var meshBezierPoint1 = info.Point1.transform.GetComponent<MeshBezierPointInfo>();
                        var meshPointInfo = new MeshPointInfo(meshBezierPoint0, meshBezierPoint1, info);
                        meshPointInfos.Add(meshPointInfo);
                    }
                }

                OnPreMeshGeneration?.Invoke(this, shapes, holes, meshPointInfos, ignoreSmoothCombineAndErosion);
                GenerateMesh(shapes, holes, meshPointInfos, ignoreSmoothCombineAndErosion);
            }
        }

        public void GenerateMesh(List<Vector3[]> shapes, List<Vector3[]> holes, List<MeshPointInfo> infos, bool ignoreSmoothCombineAndErosion = false)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name.Contains("Mesh"))
                    Utils.SmartDestroy(transform.GetChild(i).gameObject);
            }

            var customVertices = new List<Vector3>();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name.StartsWith("Vertex"))
                    customVertices.Add(transform.GetChild(i).position);
            }

            MiddleSegments = GenerateMiddleMesh(shapes, holes, infos, MiddleFrontProjectUVs, MiddleUVScale);
            FrontSegments = GenerateFrontMesh(shapes, holes, infos, customVertices, isFront: true, erosion: Erosion && !ignoreSmoothCombineAndErosion);
            FrontSegments.AddRange(GenerateFrontMesh(shapes, holes, infos, customVertices, isFront: false, erosion: Erosion && !ignoreSmoothCombineAndErosion));

            if (CombineMeshes && !ignoreSmoothCombineAndErosion)
            {
                CombinedMesh = CombineGeometry(MiddleSegments, FrontSegments);
            }

            if (SmoothNormals && !ignoreSmoothCombineAndErosion)
            {
                if (CombineMeshes)
                {
                    if (MergeSmoothedVertices)
                        MeshUtils.MergeVertices(CombinedMesh.sharedMesh, 0.01f);
                    SmoothMeshNormals(null, null, CombinedMesh);
                }
                else
                    SmoothMeshNormals(MiddleSegments, FrontSegments, null);
            }

            var generatedMeshes = new List<MeshFilter>();
            if (CombineMeshes)
            {
                generatedMeshes.Add(CombinedMesh);
                OnPostMeshGenerated?.Invoke(generatedMeshes);
            }
            else
            {
                foreach (var middle in MiddleSegments)
                    generatedMeshes.Add(middle.GetComponent<MeshFilter>());
                foreach (var front in FrontSegments)
                    generatedMeshes.Add(front.GetComponent<MeshFilter>());
                OnPostMeshGenerated?.Invoke(generatedMeshes);
            }
        }

        public List<MeshMiddleSegment> GenerateMiddleMesh(List<Vector3[]> shapes, List<Vector3[]> holes, List<MeshPointInfo> infos, bool frontProjectUVs, float middleUVScale)
        {
            var meshMiddleSegments = new List<MeshMiddleSegment>();
            MeshMiddleSegment segment;

            foreach (var points in shapes)
            {
                segment = MeshMiddleSegment.Create(transform, points, FrontMiddleWidth, BackMiddleWidth, infos, Material, CastShadows, frontProjectUVs, middleUVScale);
                meshMiddleSegments.Add(segment);
            }

            foreach (var points in holes)
            {
                segment = MeshMiddleSegment.Create(transform, points, FrontMiddleWidth, BackMiddleWidth, infos, Material, CastShadows, frontProjectUVs, middleUVScale);
                meshMiddleSegments.Add(segment);
            }

            return meshMiddleSegments;
        }

        public List<MeshFrontSegment> GenerateFrontMesh(
            List<Vector3[]> shapes, List<Vector3[]> holes, List<MeshPointInfo> infos, List<Vector3> vertices,
            bool isFront, bool erosion)
        {
            var meshSegments = new List<MeshFrontSegment>();
            MeshFrontSegment segment;

            int numOfSegments = isFront ? FrontBevelDivisions : BackBevelDivisions;

            // remember to original holes in clipper space to discard meshes generated inside them.
            var originalHoles = ClipperHelper.ToClipperPointLists(toLocalSpace(holes));

            var previousShapes = new List<Vector3[]>(shapes);
            previousShapes.AddRange(holes);

            float offset;
            List<List<List<IntPoint>>> shapesAndHoles;
            List<List<IntPoint>> previousPaths;
            List<List<IntPoint>> newPaths = null;
            var nextHoles = new List<List<IntPoint>>();
            var nextShapes = new List<List<IntPoint>>();

            if (numOfSegments == 0)
            {
                nextHoles = ClipperHelper.ToClipperPointLists(toLocalSpace(holes));
                nextShapes = ClipperHelper.ToClipperPointLists(toLocalSpace(shapes));
            }

            float startZ = isFront ? FrontMiddleWidth : BackMiddleWidth;
            float stepZ = numOfSegments == 0 ? 0f : 1f / numOfSegments;
            var bevelType = isFront ? FrontBevelType : BackBevelType;
            var bevelWidth = isFront ? FrontBevelWidth : BackBevelWidth;
            var bevelScale = isFront ? FrontBevelScale : BackBevelScale;

            for (int s = 0; s <= numOfSegments; s++)
            {
                float bevelFuncX = Mathf.Clamp01((float)s / (float)numOfSegments);
                float bevelFuncValue = bevelFuncY(bevelType, bevelFuncX);
                float nextBevelFuncX = Mathf.Clamp01((float)(s + 1) / (float)numOfSegments);
                float nextBevelFuncValue = bevelFuncY(bevelType, nextBevelFuncX);

                offset = nextBevelFuncValue - bevelFuncValue;
                offset *= bevelScale;
                offset *= -1f;
                if (Mathf.Abs(offset) < 0.03f)
                    offset = Mathf.Sign(offset) * 0.03f;

                float currentZ = -(startZ + stepZ * s * bevelWidth);
                float nextZ = -(startZ + stepZ * (s + 1) * bevelWidth);

                if (s < numOfSegments)
                {
                    if (s == 0)
                    {
                        previousPaths = ClipperHelper.ToClipperPointLists(toLocalSpace(previousShapes));
                        newPaths = offsetPolygon(shapes, holes, offset);
                    }
                    else
                    {
                        previousPaths = newPaths;
                        newPaths = offsetPolygon(nextShapes, nextHoles, offset);
                    }

                    if (AutoCleanClipperPolygons)
                    {
                        // ensure clean distance is below offset.
                        float finalCleanDistance = Mathf.Abs(offset) * 0.3f;
                        // merge close points to reduce number of triangles
                        newPaths = Clipper.CleanPolygons(newPaths, finalCleanDistance * ClipperHelper.VecToClipFactor, true);
                    }
                    else
                    {
                        // ensure clean distance is below offset.
                        float finalCleanDistance = Mathf.Abs(offset) * 0.3f;
                        // merge close points to reduce number of triangles
                        newPaths = Clipper.CleanPolygons(newPaths, Mathf.Min(finalCleanDistance, CleanClipperDistance) * ClipperHelper.VecToClipFactor, false);
                    }

                    List<List<IntPoint>> solutions = new List<List<IntPoint>>();

                    Clipper c = new Clipper();
                    c.AddPaths(previousPaths, PolyType.ptSubject, closed: true);
                    c.AddPaths(newPaths, PolyType.ptClip, closed: true);
                    c.Execute(
                        ClipType.ctDifference, solutions,
                        PolyFillType.pftNonZero, PolyFillType.pftNonZero
                        );

                    nextHoles.Clear();
                    nextShapes.Clear();

                    shapesAndHoles = ClipperHelper.SplitIntoShapesAndHoles(solutions);
                    var offsetShapes = shapesAndHoles[0];
                    var offsetHoles = shapesAndHoles[1];

                    // If a shape is inside an original hole then it should be dropped.
                    for (int i = offsetShapes.Count - 1; i >= 0; i--)
                    {
                        foreach (var hole in originalHoles)
                        {
                            if (Clipper.PointInPolygon(offsetShapes[i][0], hole) != 0)
                            {
                                offsetShapes.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    // If a shape contains an original hole but no other holes then it should be dropped.
                    for (int i = offsetShapes.Count - 1; i >= 0; i--)
                    {
                        bool containsOriginalHole = false;
                        bool containsOtherHoles = false;
                        foreach (var hole in originalHoles)
                        {
                            if (Clipper.PointInPolygon(hole[0], offsetShapes[i]) != 0)
                            {
                                containsOriginalHole = true;
                                break;
                            }
                        }
                        foreach (var hole in offsetHoles)
                        {
                            if (Clipper.PointInPolygon(hole[0], offsetShapes[i]) != 0)
                            {
                                containsOtherHoles = true;
                                break;
                            }
                        }
                        if (containsOriginalHole && !containsOtherHoles)
                        {
                            offsetShapes.RemoveAt(i);
                        }
                    }

                    // If a shape is inside another shape in this iteration then it's an actual hole and should be a hole in the next iteration.
                    // All other shapes should be dropped.
                    for (int i = offsetShapes.Count - 1; i >= 0; i--)
                    {
                        for (int k = offsetShapes.Count - 1; k >= 0; k--)
                        {
                            // ignore self
                            if (i == k)
                                continue;

                            if (Clipper.PointInPolygon(offsetShapes[i][0], offsetShapes[k]) != 0)
                                nextHoles.Add(offsetShapes[i]);
                        }
                    }

                    // z for holes
                    var meshHoles = ClipperHelper.ToVector3Arrays(offsetHoles);
                    for (int h = 0; h < meshHoles.Count; h++)
                    {
                        var hole = meshHoles[h];

                        bool containsOriginalHole = false;
                        foreach (var originalHole in originalHoles)
                        {
                            if (Clipper.PointInPolygon(originalHole[0], offsetHoles[h]) != 0)
                            {
                                containsOriginalHole = true;
                                break;
                            }
                        }

                        bool containsOtherHoles = false;
                        for (int j = 0; j < offsetHoles.Count; j++)
                        {
                            if (j == h)
                                continue;

                            if (Clipper.PointInPolygon(offsetHoles[j][0], offsetHoles[h]) != 0)
                            {
                                containsOtherHoles = true;
                                break;
                            }
                        }

                        for (int i = 0; i < hole.Length; i++)
                        {
                            hole[i].z = (containsOriginalHole && !containsOtherHoles) ? currentZ : nextZ;
                            if (!isFront)
                                hole[i].z = -hole[i].z;
                        }
                    }

                    // z for shapes
                    foreach (var shape in offsetShapes)
                    {
                        var meshShape = ClipperHelper.ToVector3Array(shape);
                        bool isInsideOtherHole = false;
                        for (int j = 0; j < offsetHoles.Count; j++)
                        {
                            if (Clipper.PointInPolygon(shape[0], offsetHoles[j]) != 0)
                            {
                                isInsideOtherHole = true;
                                break;
                            }
                        }
                        for (int i = 0; i < meshShape.Length; i++)
                        {
                            if (isInsideOtherHole)
                                meshShape[i].z = nextZ;
                            else
                                meshShape[i].z = currentZ;

                            if (!isFront)
                                meshShape[i].z = -meshShape[i].z;
                        }

                        // Draw for debugging (set s to the iteration you want to observe)
                        /*
						if (s == 0)
						{
							var color = Color.green;
							var shapeDbg = meshShape;
							for (int o = 1; o < shapeDbg.Length - 1; o++)
							{
								var p0 = transform.TransformPoint(shapeDbg[o - 1]);
								var p1 = transform.TransformPoint(shapeDbg[o]);
								Debug.DrawLine(p0, p1, color, 10f);
								Debug.DrawLine(p0, p0 + Vector3.up * 0.1f, color, 10f);
							}

							// direction
							var pd0 = transform.TransformPoint(shapeDbg[0]);
							var pd1 = transform.TransformPoint(shapeDbg[1]);
							var v = pd1 - pd0;
							Debug.DrawLine(pd0, pd0 + (new Vector3(v.y, -v.x, v.z)).normalized, color, 10f);

							foreach (var holeDbg in meshHoles)
							{
								color = Color.red;
								for (int o = 1; o < holeDbg.Length - 1; o++)
								{
									var p0 = transform.TransformPoint(holeDbg[o - 1]);
									var p1 = transform.TransformPoint(holeDbg[o]);
									Debug.DrawLine(p0, p1, color, 10f);
									Debug.DrawLine(p0, p0 + Vector3.up * 0.1f, color, 10f);
								}

								// direction
								pd0 = transform.TransformPoint(holeDbg[0]);
								pd1 = transform.TransformPoint(holeDbg[1]);
								v = pd1 - pd0;
								Debug.DrawLine(pd0, pd0 + (new Vector3(v.y, -v.x, v.z)).normalized, color, 10f);
							}
						}//*/

                        // create mesh
                        segment = MeshFrontSegment.Create(transform, meshShape, meshHoles, FrontMiddleWidth, BackMiddleWidth, infos, vertices, Material, CastShadows, isFront);
                        meshSegments.Add(segment);
                    }

                    // Holes in the current iteration should be shapes in the next iteration.
                    foreach (var hole in offsetHoles)
                    {
                        hole.Reverse();
                        nextShapes.Add(hole);
                    }

                    // Shapes inside of holes need to be removed.
                    for (int i = nextShapes.Count - 1; i >= 0; i--)
                    {
                        if (ClipperHelper.PointInPolygons(nextShapes[i][0], nextHoles))
                        {
                            nextShapes.RemoveAt(i);
                        }
                    }

                    // reverse shapes (they stem from current holes and their orientation needs to be reverted to be valid shapes for clipper).
                    foreach (var shape in nextShapes)
                    {
                        shape.Reverse();
                    }

                    // erosion (curve shortening)
                    if (erosion && s > 0 && s < numOfSegments - 1)
                    {
                        var shortenedShapes = new List<List<IntPoint>>();
                        foreach (var shape in nextShapes)
                        {
                            var newShape = CurveShorteningHelper.ShortenClipperCurve(shape, bevelScale * ErosionStrength, ErosionSegmentLength);
                            shortenedShapes.Add(newShape);

                            // Draw for debugging (set s to the iteration you want to observe)
                            /*
							if (s == 2)
							{
								var color = Color.green;
								var shapeOrigDbg = ClipperHelper.ToVector2Array(shape);
								for (int o = 1; o < shapeOrigDbg.Length - 1; o++)
								{
									var p0 = transform.TransformPoint(shapeOrigDbg[o - 1]);
									var p1 = transform.TransformPoint(shapeOrigDbg[o]);
									Debug.DrawLine(p0, p1, Color.green, 10f);
								}

								var shapeShortDbg = ClipperHelper.ToVector2Array(newShape);
								for (int o = 1; o < shapeShortDbg.Length - 1; o++)
								{
									var p0 = transform.TransformPoint(shapeShortDbg[o - 1]);
									var p1 = transform.TransformPoint(shapeShortDbg[o]);
									Debug.DrawLine(p0, p1, Color.yellow, 10f);
								}
							}//*/
                        }
                        nextShapes = shortenedShapes;
                    }
                }
                else
                {
                    // Shapes containing original holes which are themselves not contained in holes need to be removed.
                    foreach (var originalHole in originalHoles)
                    {
                        for (int i = nextShapes.Count - 1; i >= 0; i--)
                        {
                            // Contains original hole.
                            if (Clipper.PointInPolygon(originalHole[0], nextShapes[i]) != 0)
                            {
                                // Only remove if not covered by a hole inside the shape
                                if (!ClipperHelper.PointInPolygons(originalHole[0], nextHoles))
                                {
                                    nextShapes.RemoveAt(i);
                                }
                            }
                        }
                    }

                    // Last segment, fill the rest
                    var meshHoles = ClipperHelper.ToVector3Arrays(nextHoles);
                    foreach (var meshHole in meshHoles)
                    {
                        for (int i = 0; i < meshHole.Length; i++)
                        {
                            meshHole[i].z = currentZ;
                            if (!isFront)
                                meshHole[i].z = -meshHole[i].z;
                        }
                    }
                    var meshShapes = ClipperHelper.ToVector3Arrays(nextShapes);
                    foreach (var meshShape in meshShapes)
                    {
                        // calc final z coordinates
                        for (int i = 0; i < meshShape.Length; i++)
                        {
                            meshShape[i].z = currentZ;
                            if (!isFront)
                                meshShape[i].z = -meshShape[i].z;
                        }
                        if ((isFront && FrontClosed) || (!isFront && BackClosed))
                        {
                            segment = MeshFrontSegment.Create(transform, meshShape, meshHoles, FrontMiddleWidth, BackMiddleWidth, infos, vertices, Material, CastShadows, isFront);
                            meshSegments.Add(segment);
                        }
                    }
                }
            }

            return meshSegments;
        }

        protected float bevelFuncY(BevelType type, float x)
        {
            switch (type)
            {
                case BevelType.Circular:
                    return 1f - Mathf.Sqrt(1 - x * x);

                /*
            case BevelType.Custom:
                return 1f - BevelCurve.Evaluate(x);
                */

                default:
                case BevelType.Linear:
                    return x;
            }
        }

        /// <summary>
        /// Offsets the given polygon and returns the results.
        /// </summary>
        /// <param name="shapes">float value * VecToClipFactor</param>
        /// <param name="offsetTSD">float value * VecToClipFactor</param>
        /// <returns>Result in float values (int values divided by VecToClipFactor)</returns>
        protected List<List<IntPoint>> offsetPolygon(List<Vector3[]> shapes, List<Vector3[]> holes, float offset)
        {
            var shapesTSD = ClipperHelper.ToClipperPointLists(toLocalSpace(shapes));
            var holesTSD = ClipperHelper.ToClipperPointLists(toLocalSpace(holes));

            return offsetPolygon(shapesTSD, holesTSD, offset);
        }

        protected List<List<IntPoint>> offsetPolygon(List<List<IntPoint>> shapes, List<List<IntPoint>> holes, float offset)
        {
            ClipperOffset o = new ClipperOffset();
            o.AddPaths(shapes, JoinType.jtSquare, EndType.etClosedPolygon);
            if (holes != null)
                o.AddPaths(holes, JoinType.jtSquare, EndType.etClosedPolygon);

            List<List<IntPoint>> solutions = new List<List<IntPoint>>();
            o.Execute(ref solutions, offset * ClipperHelper.VecToClipFactor);

            return solutions;
        }

        protected List<Vector3[]> toLocalSpace(IList<Vector3[]> vectorLists)
        {
            var result = new List<Vector3[]>();

            foreach (var vectors in vectorLists)
            {
                result.Add(toLocalSpace(vectors));
            }

            return result;
        }

        protected Vector3[] toLocalSpace(IList<Vector3> vectors)
        {
            var result = new Vector3[vectors.Count];

            for (int i = 0; i < vectors.Count; i++)
            {
                result[i] = transform.InverseTransformPoint(vectors[i]);
            }

            return result;
        }

        public MeshFilter[] GetMeshFilters()
        {
            return transform.GetComponentsInChildren<MeshFilter>();
        }

        public MeshRenderer[] GetMeshRenderers()
        {
            return transform.GetComponentsInChildren<MeshRenderer>();
        }

        public void SmoothMesh()
        {
            SmoothMeshNormals(MiddleSegments, FrontSegments, CombinedMesh);
        }

        public void SmoothMeshNormals(List<MeshMiddleSegment> middleSegments, List<MeshFrontSegment> frontSegments, MeshFilter combinedMesh)
        {
            var meshes = new List<Mesh>();

            if (middleSegments != null)
            {
                foreach (var seg in middleSegments)
                {
                    if (seg != null)
                        meshes.Add(seg.MeshFilter.sharedMesh);
                }
            }

            if (frontSegments != null)
            {
                foreach (var seg in frontSegments)
                {
                    if (seg != null)
                        meshes.Add(seg.MeshFilter.sharedMesh);
                }
            }

            if (combinedMesh != null)
            {
                meshes.Add(combinedMesh.sharedMesh);
            }

            // Try fetching mesh filter in child transforms if all the lists were empty
            if (meshes.Count == 0)
            {
                meshes = transform.GetComponentsInChildren<MeshFilter>().Select(mf => mf.sharedMesh).ToList();
            }

            MeshUtils.SmoothMeshes(meshes);
        }

        public GameObject AddVertex()
        {
            int numOfVertices = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains("Vertex"))
                    numOfVertices++;
            }
            var vertex = new GameObject(VertexName + " " + numOfVertices, typeof(MeshVertex));
            vertex.transform.SetParent(transform);
            vertex.transform.localPosition = Vector3.zero;
            vertex.transform.localScale = Vector3.one;
            vertex.transform.localRotation = Quaternion.identity;

            return vertex;
        }

        public MeshFilter CombineGeometry(List<MeshMiddleSegment> middleSegments, List<MeshFrontSegment> frontSegments)
        {
            // destroy existing combined terrain
            var combinedTerrainTransform = this.transform.Find(CombinedTerrainObjectName);
            if (combinedTerrainTransform != null)
            {
                Utils.SmartDestroy(combinedTerrainTransform.gameObject);
            }


            MeshFilter[] meshFilters = new MeshFilter[middleSegments.Count + frontSegments.Count];
            int i = 0;
            foreach (var seg in middleSegments)
            {
                meshFilters[i] = seg.MeshFilter;
                i++;
            }
            foreach (var seg in frontSegments)
            {
                meshFilters[i] = seg.MeshFilter;
                i++;
            }
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            var combinedGameObject = new GameObject(CombinedTerrainObjectName, typeof(MeshRenderer), typeof(MeshFilter));
            var meshRenderer = combinedGameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Material;
            meshRenderer.shadowCastingMode = CastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            meshRenderer.allowOcclusionWhenDynamic = false;
            combinedGameObject.transform.SetParent(this.transform);
            combinedGameObject.transform.localPosition = Vector3.zero;
            combinedGameObject.transform.localScale = Vector3.one;
            combinedGameObject.transform.localRotation = Quaternion.identity;
            combinedGameObject.tag = getMeshTag();

            var meshFilter = combinedGameObject.GetComponent<MeshFilter>();
            MeshUtils.CombineMesh(meshFilters, meshFilter, false, true);
            combinedGameObject.SetActive(true);

#if UNITY_EDITOR
            combinedGameObject.isStatic = StaticMesh;
            UnityEditor.MeshUtility.SetMeshCompression(
                meshFilter.sharedMesh,
                (UnityEditor.ModelImporterMeshCompression)MeshCompression
            );
#endif

            // destroy segments
            foreach (var seg in middleSegments)
                Utils.SmartDestroy(seg.gameObject);
            foreach (var seg in frontSegments)
                Utils.SmartDestroy(seg.gameObject);
            middleSegments.Clear();
            frontSegments.Clear();

            return meshFilter;
        }

        protected string getMeshTag()
        {
            if (!string.IsNullOrEmpty(MeshTag))
            {
                return MeshTag;
            }
            if (!string.IsNullOrEmpty(tag))
            {
                return tag;
            }
            return "";
        }

#if UNITY_EDITOR
        protected void debugDraw(List<Vector3[]> pointLists, Color color, float duration)
        {
            foreach (var points in pointLists)
            {
                for (int p = 0; p < points.Length; p++)
                {
                    var a = points[p];
                    var b = points[(p + 1) % points.Length];
                    Debug.DrawLine(
                        transform.TransformPoint(new Vector3(a.x, a.y, a.z)),
                        transform.TransformPoint(new Vector3(b.x, b.y, b.z)),
                        color, duration);
                }
            }
        }
#endif
    }
}
