using Kamgam.Terrain25DLib.Helpers;
using Poly2Tri;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kamgam.Terrain25DLib
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class MeshFrontSegment : MonoBehaviour
	{
		public MeshFilter MeshFilter;
		public MeshRenderer MeshRenderer;

		public static MeshFrontSegment Create(Transform parent, Vector3[] shapeInLocalSpace, List<Vector3[]> holesInLocalSpace, float middleWidthFront, float middleWidthBack, List<MeshPointInfo> infos, List<Vector3> customVerticesInWorldSpace, Material material, bool castShadows, bool isFront)
		{
			int nr = parent.GetComponentsInChildren<MeshFrontSegment>().Length;
			var segment = new GameObject("Mesh " + (isFront ? "Front" : "Back") + " " + nr, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshFrontSegment)).GetComponent<MeshFrontSegment>();
			segment.transform.SetParent(parent);
			segment.transform.localPosition = Vector3.zero;
			segment.transform.localScale = Vector3.one;
			segment.transform.localRotation = Quaternion.identity;
			segment.Reset();

			segment.MeshRenderer.sharedMaterial = material;
			segment.MeshRenderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
			segment.MeshRenderer.lightProbeUsage = LightProbeUsage.Off;
			segment.MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			segment.MeshRenderer.allowOcclusionWhenDynamic = false;

			segment.GenerateGeometry(shapeInLocalSpace, holesInLocalSpace, middleWidthFront, middleWidthBack, infos, customVerticesInWorldSpace, 1f, isFront);

			return segment;
		}

		public void Reset()
		{
			MeshFilter = this.gameObject.GetComponent<MeshFilter>();
			if (MeshFilter == null)
			{
				MeshFilter = this.gameObject.AddComponent<MeshFilter>();
			}

			MeshRenderer = this.gameObject.GetComponent<MeshRenderer>();
			if (MeshRenderer == null)
			{
				MeshRenderer = this.gameObject.AddComponent<MeshRenderer>();
			}
		}

		public void SetMaterial(Material material)
		{
			MeshRenderer.sharedMaterial = material;
		}

		/// <summary>
		/// Generates a mesh from the given data.
		/// </summary>
		/// <param name="shape">pos in local space</param>
		/// <param name="holes">pos in local space</param>
		/// <param name="middleWidthFront"></param>
		/// <param name="middleWidthBack"></param>
		/// <param name="infos"></param>
		/// <param name="vertices">pos in world space</param>
		/// <param name="textureScale"></param>
		/// <param name="isFront"></param>
		public void GenerateGeometry(Vector3[] shape, List<Vector3[]> holes, float middleWidthFront, float middleWidthBack, List<MeshPointInfo> infos, List<Vector3> vertices, float textureScale, bool isFront)
		{
			if (shape == null)
				return;

			var shapePoints = toPolygonPoints(shape);
			var polygon = new Polygon(shapePoints);

			// match holes to polygons
			if (holes != null)
			{
				foreach (var hole in holes)
				{
					if (UtilsPolygons.ContainsPoint(shape, hole[0]))
					{
						var holePoints = toPolygonPoints(hole);
						var holePolygon = new Polygon(holePoints);
						polygon.AddHole(holePolygon);
					}
				}
			}

			// add custom vertices as steiner points
			for (int v = 0; v < vertices.Count; v++)
			{
				var vertex = transform.InverseTransformPoint(vertices[v]);

				// is this vertex for the front or the back?
				if (!isFront && vertex.z < 0)
					continue;

				if (isFront && vertex.z > 0)
					continue;

				bool insideShape = UtilsPolygons.ContainsPoint(shape, vertex);
				if (insideShape)
				{
					bool isInHole = false;
					foreach (var hole in holes)
					{
						if (UtilsPolygons.ContainsPoint(hole, vertex))
						{
							isInHole = true;
							break;
						}
					}
					if (isInHole == false)
					{
						polygon.AddSteinerPoint(new TriangulationPoint(vertex.x, vertex.y));
					}
				}
			}

			var verticesInLocalSpace = new List<Vector3>();
            for (int i = 0; i < vertices.Count; i++)
            {
				verticesInLocalSpace.Add(transform.InverseTransformPoint(vertices[i]));
            }

			GenerateGeometryWithPoly2Tri(polygon, shape, holes, verticesInLocalSpace, middleWidthFront, middleWidthBack, infos, textureScale, isFront);
		}

		/// <summary>
		/// Generate Mesh using Poly2Tri.
		/// </summary>
		/// <param name="pointLists">First is outline, rest are holes.</param>
		/// <param name="textureScale"></param>
		public void GenerateGeometryWithPoly2Tri(Polygon polygon, Vector3[] shape, List<Vector3[]> holes, List<Vector3> vertices, float middleWidthFront, float middleWidthBack, List<MeshPointInfo> infos, float textureScale, bool isFront)
		{
			if (polygon == null)
				return;

			P2T.Triangulate(polygon);

			MeshPointInfo info0;
			MeshPointInfo info1;
			MeshPointInfo info2;
			float offsetTop0, frontBevelMultiplier0, backBevelMultiplier0, frontMiddleMultiplier0, backMiddleMultiplier0;
			float offsetTop1, frontBevelMultiplier1, backBevelMultiplier1, frontMiddleMultiplier1, backMiddleMultiplier1;
			float offsetTop2, frontBevelMultiplier2, backBevelMultiplier2, frontMiddleMultiplier2, backMiddleMultiplier2;
			float middleWidth = isFront ? middleWidthFront : middleWidthBack;

			int triCount = polygon.Triangles.Count;
			Vector3[] meshVertices = new Vector3[triCount * 3];
			Vector2[] meshUVs = new Vector2[triCount * 3];
			int[] meshTriangles = new int[triCount * 3];
			Vector3 p0, p1, p2;
			Vector3 uvPoint03D, uvPoint13D, uvPoint23D;
			int steinerIndex0, steinerIndex1, steinerIndex2;
			for (int i = 0; i < triCount; i++)
			{
				p2 = new Vector3(polygon.Triangles[i].Points[0].Xf, polygon.Triangles[i].Points[0].Yf, 0);
				p1 = new Vector3(polygon.Triangles[i].Points[1].Xf, polygon.Triangles[i].Points[1].Yf, 0);
				p0 = new Vector3(polygon.Triangles[i].Points[2].Xf, polygon.Triangles[i].Points[2].Yf, 0);
				p0.z = poly2triZPos(p0, shape, holes);
				p1.z = poly2triZPos(p1, shape, holes);
				p2.z = poly2triZPos(p2, shape, holes);

				steinerIndex0 = findVertexIndex2D(vertices, p0);
				steinerIndex1 = findVertexIndex2D(vertices, p1);
				steinerIndex2 = findVertexIndex2D(vertices, p2);

				info0 = MeshPointInfo.FindClosest(infos, transform.TransformPoint(p0));
				offsetTop0 = MeshPointInfo.CalculateTopOffset(info0);
				frontBevelMultiplier0 = MeshPointInfo.CalculateFrontBevelMultiplier(info0);
				backBevelMultiplier0 = MeshPointInfo.CalculateBackBevelMultiplier(info0);
				frontMiddleMultiplier0 = MeshPointInfo.CalculateFrontMiddleMultiplier(info0);
				backMiddleMultiplier0 = MeshPointInfo.CalculateBackMiddleMultiplier(info0);

				info1 = MeshPointInfo.FindClosest(infos, transform.TransformPoint(p1));
				offsetTop1 = MeshPointInfo.CalculateTopOffset(info1);
				frontBevelMultiplier1 = MeshPointInfo.CalculateFrontBevelMultiplier(info1);
				backBevelMultiplier1 = MeshPointInfo.CalculateBackBevelMultiplier(info1);
				frontMiddleMultiplier1 = MeshPointInfo.CalculateFrontMiddleMultiplier(info1);
				backMiddleMultiplier1 = MeshPointInfo.CalculateBackMiddleMultiplier(info1);

				info2 = MeshPointInfo.FindClosest(infos, transform.TransformPoint(p2));
				offsetTop2 = MeshPointInfo.CalculateTopOffset(info2);
				frontBevelMultiplier2 = MeshPointInfo.CalculateFrontBevelMultiplier(info2);
				backBevelMultiplier2 = MeshPointInfo.CalculateBackBevelMultiplier(info2);
				frontMiddleMultiplier2 = MeshPointInfo.CalculateFrontMiddleMultiplier(info2);
				backMiddleMultiplier2 = MeshPointInfo.CalculateBackMiddleMultiplier(info2);

				// assume a steiner point with z > 0 is meant for the front, for the back if < 0
				if (steinerIndex0 == -1 || (isFront && vertices[steinerIndex0].z > 0) || (!isFront && vertices[steinerIndex0].z < 0))
				{
					p0.y += offsetTop0;
					// middle width and bevel multiplier are separate value, thus we remove middle, then multiply bevels, and then add it again and multiply
					p0.z += isFront ? middleWidth : -middleWidth;
					p0.z *= isFront ? frontBevelMultiplier0 : backBevelMultiplier0;
					p0.z -= isFront ? middleWidth * frontMiddleMultiplier0 : -middleWidth * backMiddleMultiplier0;
				}
				else
                {
					p0.z = vertices[steinerIndex0].z;
                }

				if (steinerIndex1 == -1 || (isFront && vertices[steinerIndex1].z > 0) || (!isFront && vertices[steinerIndex1].z < 0))
				{
					p1.y += offsetTop1;
					p1.z += isFront ? middleWidth : -middleWidth;
					p1.z *= isFront ? frontBevelMultiplier1 : backBevelMultiplier1;
					p1.z -= isFront ? middleWidth * frontMiddleMultiplier1 : -middleWidth * backMiddleMultiplier1;
				}
				else
				{
					p1.z = vertices[steinerIndex1].z;
				}

				if (steinerIndex2 == -1 || (isFront && vertices[steinerIndex2].z > 0) || (!isFront && vertices[steinerIndex2].z < 0))
				{
					p2.y += offsetTop2;
					p2.z += isFront ? middleWidth : -middleWidth;
					p2.z *= isFront ? frontBevelMultiplier2 : backBevelMultiplier2;
					p2.z -= isFront ? middleWidth * frontMiddleMultiplier2 : -middleWidth * backMiddleMultiplier2;
				}
				else
				{
					p2.z = vertices[steinerIndex2].z;
				}

				meshVertices[i * 3 + 0] = p0;
				meshVertices[i * 3 + 1] = p1;
				meshVertices[i * 3 + 2] = p2;

				if (isFront)
				{
					meshTriangles[i * 3 + 0] = i * 3 + 0;
					meshTriangles[i * 3 + 1] = i * 3 + 1;
					meshTriangles[i * 3 + 2] = i * 3 + 2;
				}
				else
                {
					meshTriangles[i * 3 + 0] = i * 3 + 2;
					meshTriangles[i * 3 + 1] = i * 3 + 1;
					meshTriangles[i * 3 + 2] = i * 3 + 0;
				}

				uvPoint03D = this.transform.TransformPoint(new Vector3(p0.x, p0.y, 0));
				uvPoint13D = this.transform.TransformPoint(new Vector3(p1.x, p1.y, 0));
				uvPoint23D = this.transform.TransformPoint(new Vector3(p2.x, p2.y, 0));
				meshUVs[i * 3] = new Vector2(uvPoint03D.x / textureScale, uvPoint03D.y / textureScale);
				meshUVs[i * 3 + 1] = new Vector2(uvPoint13D.x / textureScale, uvPoint13D.y / textureScale);
				meshUVs[i * 3 + 2] = new Vector2(uvPoint23D.x / textureScale, uvPoint23D.y / textureScale);
			}

			UnityEngine.Mesh mesh;
			if (MeshFilter.sharedMesh == null)
			{
				mesh = new UnityEngine.Mesh();
			}
			else
			{
				mesh = MeshFilter.sharedMesh;
				mesh.Clear();
			}
			mesh.vertices = meshVertices;
			mesh.uv = meshUVs;
			mesh.triangles = meshTriangles;
			mesh.RecalculateNormals();
			//mesh.RecalculateBounds();
			MeshFilter.sharedMesh = mesh;
		}

		protected int findVertexIndex2D(List<Vector3> vertices, Vector3 vertex)
		{
			if (vertices == null || vertices.Count == 0)
				return -1;

            for (int i = 0; i < vertices.Count; i++)
				if (Mathf.Abs(vertices[i].x - vertex.x) < 0.0001f && Mathf.Abs(vertices[i].y - vertex.y) < 0.0001f)
					return i;

			return -1;
		}

		protected float poly2triZPos(Vector2 point, Vector3[] shape, List<Vector3[]> holes)
		{
			for (int i = 0; i < shape.Length; i++)
			{
				if (
					// perforamce optimization (thanks profiler), possible because we only look for exact matches. Maybe a native lookup hashmap would be even faster
					   shape[i].x - point.x < 0.01f
					&& shape[i].x - point.x > -0.01f
					&& shape[i].y - point.y < 0.01f
					&& shape[i].y - point.y > -0.01f
					// the actual test
					&& Vector2.SqrMagnitude((Vector2)shape[i] - point) < 0.0001f
					)
				{
					return shape[i].z;
				}
			}

			for (int h = 0; h < holes.Count; h++)
			{
				for (int p = 0; p < holes[h].Length; p++)
				{
					if (
						// perforamce optimization (thanks profiler), possible because we only look for exact matches. Maybe a native lookup hashmap would be even faster
						   holes[h][p].x - point.x < 0.1f
						&& holes[h][p].x - point.x > -0.1f
						&& holes[h][p].y - point.y < 0.1f
						&& holes[h][p].y - point.y > -0.1f
						// the actual test
						&& Vector2.SqrMagnitude((Vector2)holes[h][p] - point) < 0.0001f
						)
					{
						return holes[h][p].z;
					}
				}
			}

			return holes[0][0].z;
		}

		protected PolygonPoint[] toPolygonPoints(IList<Vector3> vectors)
		{
			var result = new PolygonPoint[vectors.Count];
            
			for (int i = 0; i < vectors.Count; i++)
            {
				result[i] = toPolygonPoint(vectors[i]);
			}

			return result;
		}

		protected PolygonPoint toPolygonPoint(Vector3 vector)
        {
			return new PolygonPoint(vector.x, vector.y);
        }

		protected float poly2triPolyPointDistance(PolygonPoint a, PolygonPoint b)
		{
			return Mathf.Sqrt((a.Xf - b.Xf) * (a.Xf - b.Xf) + (a.Yf - b.Yf) * (a.Yf - b.Yf));
		}
	}
}