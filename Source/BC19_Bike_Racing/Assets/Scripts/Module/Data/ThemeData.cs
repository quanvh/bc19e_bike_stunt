using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Bacon
{
    [CreateAssetMenu(fileName = "ThemeDataAsset", menuName = "DataAsset/ThemeDataAsset")]
    public class ThemeData : ScriptableObject
    {
        public List<ThemeModel> listthemes;
        public List<Sprite> skies;
    }

    [Serializable]
    public class ThemeModel : CustomModel
    {
        public ThemeModel(int _id) : base(_id)
        {
        }

        [Header("Theme"), Space]
        [FoldoutGroup("@Name")] public THEME_NAME _name;

        [FoldoutGroup("@Name")] public Sprite Sky;

        [FoldoutGroup("@Name")] public Kamgam.Terrain25DLib.FoliageGeneratorSetSettings[] foliageSettings;

        [FoldoutGroup("@Name")] public AnimationCurve[] Curves;

        [FoldoutGroup("@Name")] public GameObject BridgePartPrefab;

        [FoldoutGroup("@Name")] public GameObject BridgeEdgePartPrefab;

    }

    public enum THEME_NAME
    {
        AUTUMN = 1,
        BEACH = 2,
        FOREST = 3,
        GEM = 4,
        WINTER = 5,
        CANDY = 6,
        SHEEP = 7,
    }
}