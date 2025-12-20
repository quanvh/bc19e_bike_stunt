using UnityEngine;
using Kamgam.Terrain25DLib.Helpers;
using System.Linq;

namespace Kamgam.Terrain25DLib
{
	[CreateAssetMenu(fileName = "FoliageSet New", menuName = "2.5D Terrain/FoliageSet Settings", order = 1001)]
	public class FoliageGeneratorSetSettings : ScriptableObject
	{
		[System.Serializable]
		public class PrefabRatio
		{
			public float Ratio = 1f;
			public GameObject Prefab;

			[System.Serializable]
			public class MaterialRatio
			{
				[Tooltip("Some meshes may use multiple materials. Use this index to define which index this material should be used for. Keep as 0 if there is just one material.")]
				public int MaterialIndex = 0;

				[Tooltip("The ratio describes how often a material is used in ratio to other materials in the list.\n\nRemember: the material index defines for which material index it should be used. Therefore the ratios are grouped by material index.")]
				public float Ratio = 1f;

				public Material Material;
			}

			[Tooltip("A list of materials to be used for this prefab. Ratios need to be >= 1.\n\nThe ratio describes how often a material is used in ratio to other materials in the list.\n\nThe material index defines for which material index the material should be used. Therefore the ratios are grouped by material index.")]
			public MaterialRatio[] MaterialRatios;

			public bool HasMaterialForIndex(int materialIndex = 0)
			{
				if (MaterialRatios.Length == 0)
				{
					return false;
				}
				for (int i = 0; i < MaterialRatios.Length; i++)
				{
					if(MaterialRatios[i].MaterialIndex == materialIndex && MaterialRatios[i].Ratio > 0.0001f )
					{
						return true;
					}
				}
				return false;
			}

			public int GetRandomMaterialRatioIndex(int materialIndex = 0)
			{
				if (MaterialRatios.Length == 0)
				{
					return -1;
				}
				return Utils.RandomResultForRatios(MaterialRatios.Select(m => { if (m.MaterialIndex == materialIndex) return m.Ratio; else return 0; }).ToArray());
			}

			public Material GetRandomMaterial(int materialIndex = 0)
			{
				if (MaterialRatios.Length == 0)
				{
					return null;
				}
				int index = GetRandomMaterialRatioIndex(materialIndex);
				return MaterialRatios[index].Material;
			}
		}

		[Header("Prefabs")]
		[Tooltip("A list of prefabs to place. Ratios need to be >= 1.\n\nThe ratio describes how often a prefab is placed in ratio to other prefabs in the list.")]
		public PrefabRatio[] PrefabRatios;


		[Header("Density")]

		[Range(0, 1f), Tooltip("Notice: The density is calculated only along the local x axis. If you have a deepth terrain (along z-axis) you may have to increase it.")]
		public float DensityFront = 0.9f;

		[Range(0, 1f)]
		public float DensityBack = 0.9f;

		[Min(0.01f), Tooltip("The density settings from above define where between MinDistance and MaxDistance the average distance between objects will be.")]
		public float DensityMinDistance = 0.1f;

		[Min(0.01f)]
		public float DensityMaxDistance = 5f;


		[Header("Spread")]

		[Tooltip("Should an object be place at every point the ray hits on the way down or just at the first?")]
		public bool PlaceInCaves = true;

		[Tooltip("Should an object be place even if the terrain mesh normal at that point is facing the other direction (i.e. it would be upside down)?")]
		public bool AllowUpsideDown = false;

		[Tooltip("Distance left and right at the end of the terrain.")]
		public float Margin = 4f;

		[Tooltip("Should the objects be placed in the middle area?")]
		public bool PlaceInMiddle = false;

		[Range(-10f, 10f), Tooltip("If objects are not allowed to be placed in the middle then how much are they allowed to overlap the middle coming in from the sides?")]
		public float MiddleWidthOverlap = 0f;

		[Range(-0.1f, 100), Tooltip("Spread limit in z direction from 0 (center of middle) to this value (local space). Below zero means no limit.")]
		public float DepthLimitFront = -0.1f;

		[Range(-0.1f, 100), Tooltip("Spread limit in z direction from 0 (center of middle) to this value (local space). Below zero means no limit.")]
		public float DepthLimitBack = -0.1f;

		[Tooltip("Controls how many objects should be placed along the z-axis (depth).")]
		public AnimationCurve DepthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });

		[Header("Size, Rotation, ...")]
		[Tooltip("Should the placed obects be scaled randomly? Set to 1 if not.")]
		public float MinScale = 0.5f;

		[Tooltip("Should the placed obects be scaled randomly? Set to 1 if not.")]
		public float MaxScale = 1f;

		[Tooltip("Should the placed objects be aligned to normal of the point where the terrain was hit instead of pointing straight up?")]
		public bool AlignRotationWithTerrain = false;

		[Tooltip("Should the placed objects alignment vector (normal of hit point) be taken from the mesh or should it be calculated based on the hit triangle. Useful to negate the effect of smoothed mesh normals (see MeshGenerator > 'Smooth Meshes').")]
		public bool UseMeshNormalsForAlignment = false;

		[Tooltip("Should the placed objects be rotated randomly around the objects y axis?\n\nFor example: a value of 360 means it will be randomly rotated between -180 and +180 degrees (range of 360).")]
		[Range(0, 360)]
		public float RotYVarianceInDeg = 360f;

		[Tooltip("Should the placed objects be rotated randomly around the z axis?")]
		[Range(0, 180)]
		public float RotZVarianceInDeg = 10f;

		[Range(0, 2f)]
		[Tooltip("How much should the placed object be sunk into the ground?")]
		public float GroundIntersection = 0.1f;
		
		[Range(0, 1)]
		[Tooltip("The higher the value the steeper the terrain can be under the placed objects. 0 = only completely flat terrain is allowed, 1 = even vertical cliffs are allowed")]
		public float SlopeLimit = 0.34f;


		public PrefabRatio[] GetRatios()
		{
			return PrefabRatios;
		}

		public int GetRandomPrefabIndex()
		{
			return Utils.RandomResultForRatios(GetRatios().Select(m => m.Ratio).ToArray());
		}

		public PrefabRatio GetPrefab(int index)
		{
			return GetRatios()[index];
		}

		public PrefabRatio GetRandomPrefab()
		{
			return GetRatios()[GetRandomPrefabIndex()];
		}

		public bool HasValidPrefabRatios()
        {
			if (PrefabRatios == null || PrefabRatios.Length == 0)
				return false;

            foreach (var ratio in PrefabRatios)
            {
				if (ratio.Prefab == null)
					return false;
            }

			return true;
        }

#if UNITY_EDITOR
		public void OnValidate()
        {
			DensityMaxDistance = Mathf.Max(DensityMinDistance, DensityMaxDistance);

			// Fix the Ratio being 0 by default.
			if (PrefabRatios.Length > 0)
            {
                foreach (var ratio in PrefabRatios)
                {
					if (ratio.Prefab == null)
						ratio.Ratio = 1;
				}
			}
        }
#endif
	}
}