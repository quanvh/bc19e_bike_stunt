using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacon
{
    [CreateAssetMenu(fileName = "RewardDataAsset", menuName = "DataAsset/RewardDataAsset")]
    public class RewardData : ScriptableObject
    {
        [Space]
        public REWARD_DATA_TYPE type = REWARD_DATA_TYPE.NONE;

        public List<RewardModel> listItems;

    }

    [Serializable]
    public class RewardModel
    {
        [Header("REWARD DATA"), Space]
        [FoldoutGroup("@Name", expanded: false)] public int ID;
        [FoldoutGroup("@Name")] public int TargetID;
        [FoldoutGroup("@Name")] public REWARD_TYPE Type = REWARD_TYPE.GOLD;
        [FoldoutGroup("@Name")] public REWARD_TYPE TargetType = REWARD_TYPE.NONE;
        [FoldoutGroup("@Name")] public int Value = 100;
        [FoldoutGroup("@Name")] public string Name = "Gold";
        [FoldoutGroup("@Name")] public Sprite Thumb;
        [FoldoutGroup("@Name")] public Sprite Icon;
        [FoldoutGroup("@Name")] public Color Color = Color.white;
        [FoldoutGroup("@Name")] public bool IsFree = true;
    }

    public enum REWARD_DATA_TYPE
    {
        NONE = 0,
        DAILY = 1,
        GACHA = 2,
    }

    public enum REWARD_TYPE
    {
        NONE = 0,
        GOLD = 1,
        GEM = 2,
        CAR = 3,
        COLOR = 4,
        DECAL = 5,
        WHEEL = 6,
        ADS = 7,
        MAGNET = 8,
        CARD = 9,
        HELMET = 10,
        CHARACTER = 11,
    }
}