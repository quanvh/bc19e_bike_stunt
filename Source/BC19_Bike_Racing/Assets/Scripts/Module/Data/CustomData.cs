using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bacon
{
    [CreateAssetMenu(fileName = "CustomDataAsset", menuName = "DataAsset/CustomDataAsset")]
    public class CustomData : ScriptableObject
    {
        [Space]
        public CUSTOM_TYPE type;

        [Space]
        public List<CustomModel> listItems;

        public void UpdateFromSaveData(List<CustomModelSave> _saveData)
        {
            foreach (var model in listItems)
            {
                model.Unlock = false;
                model.FreeSpinCustom = true;
                if (_saveData != null)
                {
                    var temp = _saveData.FirstOrDefault(x => x.ID == model.ID);
                    if (temp != null)
                    {
                        model.Unlock = temp.Unlock;
                        model.CurrentAds = temp.Ads;
                        model.CurrentCard = temp.Card;
                        model.FreeSpinCustom = temp.FreeSpinCustom;
                    }
                }
            }
        }

        public List<CustomModelSave> SaveDataList
        {
            get => listItems?.Select(x => new CustomModelSave
            {
                ID = x.ID,
                Unlock = x.Unlock,
                Ads = x.CurrentAds,
                Card = x.CurrentCard,
                FreeSpinCustom = x.FreeSpinCustom,
            }).ToList();
        }


        public CustomModel GetCustom(int _id)
        {
            return listItems.Where(t => t.ID == _id).FirstOrDefault();
        }

        public void Reset()
        {
            foreach (var model in listItems)
            {
                model.CurrentAds = 0;
                model.CurrentCard = 0;
            }
        }

        public int FirstUnlock
        {
            get
            {
                var item = listItems.Where(t => t.Unlock == true).OrderBy(t => t.ID).FirstOrDefault();
                return item != null ? item.ID : 0;
            }
        }
    }

    [Serializable]
    public class CustomModel
    {
        public CustomModel(int _id)
        {
            ID = _id;
            Price = 0;
            Name = "";
        }

        [Header("CUSTOM DATA")]
        [FoldoutGroup("@Name", expanded: false)] public int ID;
        [FoldoutGroup("@Name")] public bool Unlock;
        [FoldoutGroup("@Name")] public int Price;
        [FoldoutGroup("@Name")] public PRICE_TYPE UnlockType;
        [FoldoutGroup("@Name")] public string Name;
        [FoldoutGroup("@Name")] public string Description;
        [FoldoutGroup("@Name")] public Material Mat;

        [FoldoutGroup("@Name")] public CUSTOM_TYPE Type;
        [FoldoutGroup("@Name")] public GameObject Source;
        [FoldoutGroup("@Name")] public Sprite Thumb;
        [FoldoutGroup("@Name")] public Color ColorCode;


        [FoldoutGroup("@Name"), Header("Ads Unlock")]
        public bool UnlockByAds;
        [FoldoutGroup("@Name")] public int PriceAds;
        [HideInInspector] public int CurrentAds;

        [FoldoutGroup("@Name"), Header("Card Unlock")]
        public bool UnlockByCard;
        [FoldoutGroup("@Name")] public int CardId;
        [FoldoutGroup("@Name")] public int PriceCard;
        [FoldoutGroup("@Name")] public bool FreeSpinCustom;
        [HideInInspector] public int CurrentCard;

        [Header("Target")]
        [FoldoutGroup("@Name")] public int TargetID;
        [FoldoutGroup("@Name")] public REWARD_TYPE TargetType = REWARD_TYPE.NONE;
    }

    [Serializable]
    public class CustomModelSave
    {
        public int ID;
        public bool Unlock;
        public int Ads;
        public int Card;
        public bool FreeSpinCustom = true;
    }


    public enum CUSTOM_TYPE
    {
        NONE = 0,
        COLOR = 1,
        DECAL = 2,
        WHEEL = 3,
        VOLANT = 4,
        LANGUAGE = 5,
        CHARACTER = 6,
        HELMET = 7,
        CARD = 8,
        MORE_GAME = 9,
    }
}