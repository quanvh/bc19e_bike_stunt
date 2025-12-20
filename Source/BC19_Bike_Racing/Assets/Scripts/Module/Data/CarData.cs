using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Bacon
{
    [CreateAssetMenu(fileName = "CarDataAsset", menuName = "DataAsset/CarDataAsset")]
    public class CarData : ScriptableObject
    {
        public List<CarModel> listCars;

        public void UpdateFromSaveData(List<CarModelSave> _saveData)
        {
            foreach (var model in listCars)
            {
                model.Unlock = false;
                model.CurrentAds = 0;
                model.CurrentCard = 0;
                model.CurrentColor = 0;
                model.FreeSpinCustom = true;
                if (_saveData != null)
                {
                    var temp = _saveData.FirstOrDefault(x => x.ID == model.ID);
                    if (temp != null)
                    {
                        model.Unlock = temp.Unlock;
                        model.CurrentAds = temp.Ads;
                        model.CurrentCard = temp.Card;
                        model.CurrentColor = temp.Color;
                        model.FreeSpinCustom = temp.FreeLuckySpinCustom;
                    }
                }
            }
        }

        public List<CarModelSave> SaveDataList
        {
            get => listCars?.Select(x => new CarModelSave
            {
                ID = x.ID,
                Ads = x.CurrentAds,
                Card = x.CurrentCard,
                Unlock = x.Unlock,
                Color = x.CurrentColor > 0 ? x.CurrentColor : x.BaseColor,
                FreeLuckySpinCustom = x.FreeSpinCustom
            }).ToList();
        }

        public void Reset()
        {
            foreach (var model in listCars)
            {
                model.CurrentAds = 0;
                model.CurrentCard = 0;
            }
        }

        public int FirstUnlock
        {
            get
            {
                var item = listCars.Where(t => t.Unlock == true).OrderBy(t => t.ID).FirstOrDefault();
                return item != null ? item.ID : 0;
            }
        }
    }


    [Serializable]
    public class CarModel : CarModelBase
    {
        [FoldoutGroup("@Name"), Header("ADDITION INFO")]
        public int Mass;
    }

    [Serializable]
    public class CarModelBase
    {
        [FoldoutGroup("@Name", expanded: false), Space] public int ID;
        [FoldoutGroup("@Name")] public bool Unlock;
        [FoldoutGroup("@Name")] public int Price;
        [FoldoutGroup("@Name")] public PRICE_TYPE UnlockType;
        [FoldoutGroup("@Name")] public string Name;
        [FoldoutGroup("@Name")] public GameObject Source;
        [FoldoutGroup("@Name")] public GameObject CarGhost;
        [FoldoutGroup("@Name")] public int BaseColor;
        [FoldoutGroup("@Name")] public int CurrentColor;
        [FoldoutGroup("@Name")] public Sprite Thumb;
        [FoldoutGroup("@Name")] public string Description;


        [FoldoutGroup("@Name"), Header("Ads Unlock")]
        public bool UnlockByAds;
        [FoldoutGroup("@Name")] public int PriceAds;
        [HideInInspector] public int CurrentAds;

        [FoldoutGroup("@Name"), Header("Card Unlock")]
        public bool UnlockByCard;
        [FoldoutGroup("@Name")] public int CardId;
        [FoldoutGroup("@Name")] public int PriceCard;
        [FoldoutGroup("@Name")] public int CurrentCard;
        [FoldoutGroup("@Name")] public bool FreeSpinCustom;
    }

    [Serializable]
    public class CarModelSave
    {
        public CarModelSave()
        {
            ID = 1;
            Ads = 0;
            Card = 0;
            Color = 0;
            Unlock = true;
            FreeLuckySpinCustom = true;
        }

        public int ID;
        public int Ads;
        public int Card;
        public bool Unlock;
        public int Color;
        public bool FreeLuckySpinCustom;
    }

}