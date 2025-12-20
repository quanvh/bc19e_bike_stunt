using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kamgam.Terrain25DLib
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class MeshMiddleSegment : MonoBehaviour
	{
		public MeshFilter MeshFilter;
		public MeshRenderer MeshRenderer;

		/// <summary>
		/// Creates the flat middle segments of the mesh.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="pointsInWorldSpace"></param>
		/// <param name="widthFront"></param>
		/// <param name="widthBack"></param>
		/// <param name="infos"></param>
		/// <param name="material"></param>
		/// <param name="castShadows"></param>
		/// <returns></returns>
		public static MeshMiddleSegment Create(
			Transform parent, Vector3[] pointsInWorldSpace, float widthFront, float widthBack, List<MeshPointInfo> infos,
			Material material, bool castShadows, bool frontProjectUVs = false, float middleUVScale = 1f)
		{
			int nr = parent.GetComponentsInChildren<MeshMiddleSegment>().Length;
			var segment = new GameObject("Mesh Middle " + nr, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshMiddleSegment)).GetComponent<MeshMiddleSegment>();
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

			segment.GenerateGeometry(pointsInWorldSpace, infos, widthFront, widthBack, frontProjectUVs, 1f / middleUVScale);

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

		public void GenerateGeometry(Vector3[] pointsInWorldSpace, List<MeshPointInfo> infos, float widthFront, float widthBack, bool frontProjectUVs, float middleUVScale)
		{
			var zPosFront = widthFront;
			var zPosBack = widthBack;

			Mesh mesh;
			if (MeshFilter.sharedMesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh = MeshFilter.sharedMesh;
				mesh.Clear();
			}

			int numOfMeshSegments = pointsInWorldSpace.Length;
			Vector3 pointA, pointB;
			int trisPerSegment = 2;
			int verticesPerSegment = 4;
			Vector3[] meshVertices = new Vector3[numOfMeshSegments * verticesPerSegment];
			Vector2[] meshUVs = new Vector2[numOfMeshSegments * verticesPerSegment];
			int[] meshTriangles = new int[numOfMeshSegments * trisPerSegment * 3];
			int meshVertexIndex = 0;
			int meshUVIndex = 0;
			int meshTrianglesIndex = 0;
			int pointCounter = 0;
			MeshPointInfo infoA;
			MeshPointInfo infoB;
			float offsetTopA, frontMiddleMultiplierA, backMiddleMultiplierA;
			float offsetTopB, frontMiddleMultiplierB, backMiddleMultiplierB;
			Vector3 p0, p1, p2, p3;
			float uvY = 0f;
			for (int i = 0; i < numOfMeshSegments; i++) // all except last
			{
				pointA = transform.InverseTransformPoint(pointsInWorldSpace[i]);
				pointB = transform.InverseTransformPoint(pointsInWorldSpace[(i + 1) % numOfMeshSegments]);
				pointA.z = 0;
				pointB.z = 0;
				
				infoA = MeshPointInfo.FindClosest(infos, pointsInWorldSpace[i]);
				offsetTopA = MeshPointInfo.CalculateTopOffset(infoA);
				frontMiddleMultiplierA = MeshPointInfo.CalculateFrontMiddleMultiplier(infoA);
				backMiddleMultiplierA = MeshPointInfo.CalculateBackMiddleMultiplier(infoA);

				infoB = MeshPointInfo.FindClosest(infos, pointsInWorldSpace[(i + 1) % numOfMeshSegments]);
				offsetTopB = MeshPointInfo.CalculateTopOffset(infoB);
				frontMiddleMultiplierB = MeshPointInfo.CalculateFrontMiddleMultiplier(infoB);
				backMiddleMultiplierB = MeshPointInfo.CalculateBackMiddleMultiplier(infoB);

				//UtilsDebug.DrawCircle(transform.TransformPoint(pointA), 0.2f, Color.red, 10.0f);
				//UtilsDebug.DrawCircle(transform.TransformPoint(pointB), 0.2f, Color.green, 10.0f);

				// vertices
				p0 = meshVertices[meshVertexIndex++] = pointA + new Vector3(0, offsetTopA, +zPosBack * backMiddleMultiplierA);
				p1 = meshVertices[meshVertexIndex++] = pointB + new Vector3(0, offsetTopB, +zPosBack * backMiddleMultiplierB);
				p2 = meshVertices[meshVertexIndex++] = pointB + new Vector3(0, offsetTopB, -zPosFront * frontMiddleMultiplierB);
				p3 = meshVertices[meshVertexIndex++] = pointA + new Vector3(0, offsetTopA, -zPosFront * frontMiddleMultiplierA);

				// UVs
				if (frontProjectUVs)
				{
					meshUVs[meshUVIndex++] = new Vector2(p0.x, p0.y);
					meshUVs[meshUVIndex++] = new Vector2(p1.x, p1.y);
					meshUVs[meshUVIndex++] = new Vector2(p2.x, p2.y);
					meshUVs[meshUVIndex++] = new Vector2(p3.x, p3.y);
				}
				else
				{
					// TODO: If the middle part is stretched in depth by MeshBezierPointInfo then the middle UVs will be distorted.
					// To fix that we would have to either take the MeshBezierPointInfo into account here or recalculate the UVs later.
					Vector2 forward = p1 - p0;
					float scaleDepth = p0.z - p3.z; 
					// uvY -= (int)uvY; // not needed
					meshUVs[meshUVIndex++] = new Vector2(0         , uvY * middleUVScale);
					meshUVs[meshUVIndex++] = new Vector2(0         , (uvY + forward.magnitude) * middleUVScale);
					meshUVs[meshUVIndex++] = new Vector2(scaleDepth * middleUVScale, (uvY + forward.magnitude) * middleUVScale);
					meshUVs[meshUVIndex++] = new Vector2(scaleDepth * middleUVScale, uvY * middleUVScale);
					uvY += forward.magnitude;
				}

				// tri1
				meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 2;
				meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 3;
				meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 4;

				// tri2
				meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 4;
				meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 1;
				meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 2;

				pointCounter++;
			}

			mesh.vertices = meshVertices;
			mesh.uv = meshUVs;
			mesh.triangles = meshTriangles;
			mesh.RecalculateNormals();
			MeshFilter.sharedMesh = mesh;
		}
	}
}