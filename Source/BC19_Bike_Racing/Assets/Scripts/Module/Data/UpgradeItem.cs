using System;
using UnityEngine;


namespace Bacon
{
    [Serializable]
    public class UpgradeItem
    {
        public string name = "";

        public int maxLevel = 10;

        [Header("Price Detail")]
        public PRICE_TYPE updateType = PRICE_TYPE.FREE;

        public int priceBase = 1000;

        public int priceStep = 100;

        public float priceScale = 1f;

        protected int _price;

        [Header("Level Detail")]
        public float valueBase = 250f;

        public float valueMax = 270f;

        [SerializeField]
        protected int rewardPointBase = 50;

        public float ValueStep => (valueMax - valueBase) / maxLevel;

        public int Price(int _level) => Mathf.FloorToInt(priceBase + _level * priceStep * priceScale);

        public int RewardPoint(int _level) => _level * rewardPointBase;

        public float CurrentValue(int _level) => valueBase + _level * ValueStep;

        public float NextValue(int _level) => valueBase + (_level + 1) * ValueStep;

        public float MaxValue() => valueBase + maxLevel * ValueStep;

        public float ChangedValue(int _level) => NextValue(_level) - CurrentValue(_level);

        public bool IsMax(int _level) => _level >= maxLevel;

        public UpgradeItem(string _name)
        {
            name = _name;
        }
    }
}