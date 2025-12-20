using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kamgam.Terrain25DLib
{
    public static class MeshUtils
    {
		public static void SmoothMesh(Mesh mesh)
		{
			SmoothMeshes(new List<Mesh>() { mesh });
		}

		public static void SmoothMeshes(List<Mesh> meshes)
		{
			// merge all meshes to one list of vertices and normals
			int vertexCount = meshes.Sum(m => m.vertexCount);
			Vector3[] vertices = new Vector3[vertexCount];
			Vector3[] normals = new Vector3[vertexCount];

			int startVertex = 0;
			for (int m = 0; m < meshes.Count; m++)
			{
				meshes[m].vertices.CopyTo(vertices, startVertex);
				meshes[m].normals.CopyTo(normals, startVertex);
				startVertex += meshes[m].vertexCount;
			}

			Vector3 avgNormal = Vector3.zero;
			List<int> identicalVertices = new List<int>();

			for (int a = 0; a < vertexCount; a++)
			{
				// find identical vertices (slow)
				identicalVertices.Clear();
				identicalVertices.Add(a);
				for (int b = a; b < vertexCount; b++)
				{
					if (
						   Mathf.Abs(vertices[a].x - vertices[b].x) < 0.0001f
						&& Mathf.Abs(vertices[a].y - vertices[b].y) < 0.0001f
						&& Mathf.Abs(vertices[a].z - vertices[b].z) < 0.0001f)
					{
						identicalVertices.Add(b);
					}
				}

				// calc average normal
				// N2H: ignore duplicate normals for average generation or weigh them based on the triangle areas
				avgNormal.x = 0;
				avgNormal.y = 0;
				avgNormal.z = 0;
				for (int i = 0; i < identicalVertices.Count; i++)
				{
					avgNormal += normals[identicalVertices[i]];
				}
				avgNormal /= identicalVertices.Count;
				// set new normal
				for (int i = 0; i < identicalVertices.Count; i++)
				{
					normals[identicalVertices[i]] = avgNormal.normalized;
				}
				//UtilsDebug.DrawVector(transform.TransformPoint(mesh.vertices[a]), transform.TransformVector(avgNormal), Color.green, 10.0f);
			}

			// split merged vertex list and assign to meshes
			startVertex = 0;
			for (int m = 0; m < meshes.Count; m++)
			{
				// apply new normals
				meshes[m].vertices = new ArraySegment<Vector3>(vertices, startVertex, meshes[m].vertexCount).ToArray();
				meshes[m].normals = new ArraySegment<Vector3>(normals, startVertex, meshes[m].vertexCount).ToArray();
				startVertex += meshes[m].vertexCount;
			}
		}

		public static void CombineMesh(MeshFilter[] meshFilters, MeshFilter targetMeshFilter, bool recalculateNormals = true, bool optimize = true, bool mergeVertices = false, float maxSqrDistance = 0.001f)
		{
			CombineInstance[] combine = new CombineInstance[meshFilters.Length];
			int i = 0;
			while (i < meshFilters.Length)
			{
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = Matrix4x4.identity;
				meshFilters[i].gameObject.SetActive(false);
				i++;
			}
			targetMeshFilter.sharedMesh = new Mesh();
			targetMeshFilter.sharedMesh.CombineMeshes(combine);
			if (recalculateNormals)
			{
				targetMeshFilter.sharedMesh.RecalculateNormals();
			}
			if (optimize)
			{
				targetMeshFilter.sharedMesh.Optimize();
			}

			if (mergeVertices)
				MergeVertices(targetMeshFilter.sharedMesh, maxSqrDistance);
		}

		public static void MergeVertices(Mesh mesh, float maxSqrDistance = 0.001f)
        {
			// TODO: Nice to have: only merge vertices whose normals are below a certain angle apart.

			// clean up duplicate vertices
			var vertices = mesh.vertices;
			List<Vector3> newVertices = new List<Vector3>();
			// Tells us which new vertex index the old vertex should be replaced with. N = the old index, mergeIndices[N] = the new index
			int[] mergeIndices = new int[vertices.Length];
			for (int v = 0; v < mergeIndices.Length; v++)
			{
				mergeIndices[v] = -1;
			}
			for (int v = 0; v < vertices.Length; v++)
			{
				if (mergeIndices[v] == -1)
				{
					newVertices.Add(vertices[v]);
					mergeIndices[v] = newVertices.Count - 1;

					for (int v1 = v + 1; v1 < vertices.Length; v1++)
					{
						if (Vector3.SqrMagnitude(vertices[v] - vertices[v1]) < maxSqrDistance)
						{
							mergeIndices[v1] = newVertices.Count - 1;
						}
					}
				}
			}

			// tris
			var triangles = mesh.triangles;
			for (int v = 0; v < triangles.Length; v++)
			{
				triangles[v] = mergeIndices[triangles[v]];
			}
			// TODO: remove triangles which now consist of duplicate point indices
			mesh.triangles = triangles;

			// verts
			// assign new vertices after triangles, otherwise unity will complain about out of bounds tri vertices.
			mesh.vertices = newVertices.ToArray();

			// uvs
			var uvs = mesh.uv;
			var newUVs = new Vector2[newVertices.Count];
			for (int u = 0; u < uvs.Length; u++)
            {
				newUVs[mergeIndices[u]] = uvs[u];
            }
			mesh.uv = newUVs;

			// normals
			var normals = mesh.normals;
			var newNormals = new Vector3[newVertices.Count];
			for (int v = 0; v < normals.Length; v++)
			{
				newNormals[mergeIndices[v]] = normals[v];
			}
			mesh.normals = newNormals;

			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
		}

#if UNITY_EDITOR

		[MenuItem("Tools/2.5D Terrain/Mesh Smooth Normals", priority = 201)]
		public static void SmoothMesh()
		{
			var meshes = Selection.gameObjects.Where(g => g.GetComponent<MeshFilter>() != null).Select(g => g.GetComponent<MeshFilter>().sharedMesh).ToList();
			SmoothMeshes(meshes);
		}

		[MenuItem("Tools/2.5D Terrain/Mesh Recalculate Normals", priority = 200)]
		public static void RecalculateNormals()
		{
			var meshes = Selection.gameObjects.Where(g => g.GetComponent<MeshFilter>() != null).Select(g => g.GetComponent<MeshFilter>().sharedMesh).ToList();
            foreach (var mesh in meshes)
            {
				mesh.RecalculateNormals();
				mesh.RecalculateTangents();
			}
		}

		[MenuItem("Tools/2.5D Terrain/Mesh Invert Normals", priority = 203)]
		public static void InvertNormals()
		{
			var meshFilters = Selection.gameObjects.Where(g => g.GetComponent<MeshFilter>() != null).Select(g => g.GetComponent<MeshFilter>()).ToList();
			foreach (var meshFilter in meshFilters)
			{
				if (meshFilter != null)
				{
					Mesh mesh = meshFilter.sharedMesh;

					Vector3[] normals = mesh.normals;
					for (int i = 0; i < normals.Length; i++)
					{
						normals[i] = -normals[i];
					}
					mesh.normals = normals;

					for (int m = 0; m < mesh.subMeshCount; m++)
					{
						int[] triangles = mesh.GetTriangles(m);
						for (int i = 0; i < triangles.Length; i += 3)
						{
							int temp = triangles[i + 0];
							triangles[i + 0] = triangles[i + 1];
							triangles[i + 1] = temp;
						}
						mesh.SetTriangles(triangles, m);
					}
				}
			}
		}

		[MenuItem("Tools/2.5D Terrain/Mesh Debug Normals", priority = 204)]
		public static void DebugNormals()
		{
			var meshFilters = Selection.gameObjects.Where(g => g.GetComponent<MeshFilter>() != null).Select(g => g.GetComponent<MeshFilter>()).ToList();
			foreach (var meshFilter in meshFilters)
			{
				if (meshFilter != null)
				{
					Mesh mesh = meshFilter.sharedMesh;

					Vector3[] normals = mesh.normals;
					Vector3[] vertices = mesh.vertices;
					for (int i = 0; i < vertices.Length; i++)
					{
						var pos = meshFilter.transform.TransformPoint(vertices[i]);
						var normal = meshFilter.transform.TransformVector(normals[i]);
						Debug.DrawLine(pos, pos + normal, Color.blue, 10f);
					}
				}
			}
		}

		/*
		 * These save methods are all buggy. TODO: investigate and fix.
		 * 
		/// <summary>
		/// Converts a mesh into an OBJ formated string.
		/// Thanks to https://wiki.unity3d.com/index.php?title=ObjExporter
		/// </summary>
		/// <param name="mf"></param>
		/// <returns></returns>
		public static string MeshToStringInObjFormat(MeshFilter mf)
		{
			Mesh m = mf.sharedMesh;
			Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append("g ").Append(mf.name).Append("\n");
			foreach (Vector3 v in m.vertices)
			{
				sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in m.normals)
			{
				sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in m.uv)
			{
				sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
			}
			for (int material = 0; material < m.subMeshCount; material++)
			{
				sb.Append("\n");
				sb.Append("usemtl ").Append(mats[material].name).Append("\n");
				sb.Append("usemap ").Append(mats[material].name).Append("\n");

				int[] triangles = m.GetTriangles(material);
				for (int i = 0; i < triangles.Length; i += 3)
				{
					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
						triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
				}
			}
			return sb.ToString();
		}

		[MenuItem("Tools/2.5D Terrain/Mesh/Save Mesh As OBJ (experimental)")]
		public static void SaveMeshAsObj()
		{
			var obj = Selection.activeGameObject;
			if (obj != null)
			{
				// save the parent GO-s pos+rot
				Vector3 position = obj.transform.position;
				Quaternion rotation = obj.transform.rotation;
				Vector3 scale = obj.transform.localScale;

				// move to the origin for combining
				obj.transform.position = Vector3.zero;
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;

				try
				{
					var meshFilter = obj.GetComponentInChildren<MeshFilter>();
					if (meshFilter != null)
					{
						using (System.IO.StreamWriter sw = new System.IO.StreamWriter(obj.name + "-mesh.obj"))
						{
							System.IO.File.WriteAllText(Application.dataPath + "/" + obj.name + "-mesh.obj", MeshToStringInObjFormat(meshFilter));
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
						}
						Debug.Log("Mesh generated in Assets/" + obj.name + "-mesh.obj");
					}
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					// restore
					obj.transform.position = position;
					obj.transform.rotation = rotation;
					obj.transform.localScale = scale;
				}
			}
		}

		// [MenuItem("Tools/2.5D Terrain/Mesh/Save Mesh As Asset")]
		public static void SaveMeshAsAsset()
		{
			var obj = Selection.activeGameObject;
			if (obj != null)
			{
				// save the parent GO-s pos+rot
				Vector3 position = obj.transform.position;
				Quaternion rotation = obj.transform.rotation;
				Vector3 scale = obj.transform.localScale;

				// move to the origin for combining
				obj.transform.position = Vector3.zero;
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;

				try
				{
					var meshFilter = obj.GetComponentInChildren<MeshFilter>();
					if (meshFilter != null)
					{
						AssetDatabase.CreateAsset(meshFilter.sharedMesh, "Assets/" + obj.name + "-mesh.asset");
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						Debug.Log("Mesh generated in Assets/" + obj.name + "-mesh.asset");
					}
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					// restore
					obj.transform.position = position;
					obj.transform.rotation = rotation;
					obj.transform.localScale = scale;
				}
			}
		}

		// [MenuItem("Tools/2.5D Terrain/Mesh/Save Mesh As Combined Asset")]
		public static void SaveMeshAsCombinedAsset()
		{
			var obj = Selection.activeGameObject;
			if (obj != null)
			{
				// save the parent GO-s pos+rot
				Vector3 position = obj.transform.position;
				Quaternion rotation = obj.transform.rotation;
				Vector3 scale = obj.transform.localScale;

				// move to the origin for combining
				obj.transform.position = Vector3.zero;
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;

				var meshFilter = obj.GetComponentInChildren<MeshFilter>();
				if (meshFilter != null)
				{
					List<CombineInstance> combine = new List<CombineInstance>();


					// combine submeshes
					for (int j = 0; j < meshFilter.sharedMesh.subMeshCount; j++)
					{
						CombineInstance ci = new CombineInstance();
						ci.mesh = meshFilter.sharedMesh;
						ci.subMeshIndex = j;
						ci.transform = meshFilter.transform.localToWorldMatrix;
						combine.Add(ci);
					}

					var mesh = new Mesh();
					mesh.CombineMeshes(combine.ToArray(), true, true);

					AssetDatabase.CreateAsset(mesh, "Assets/" + obj.name + "-combined-mesh.asset");
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					Debug.Log("Mesh generated in Assets/" + obj.name + "-combined-mesh.asset");
				}

				// restore
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				obj.transform.localScale = scale;
			}
		}
		*/
#endif

	}
}

