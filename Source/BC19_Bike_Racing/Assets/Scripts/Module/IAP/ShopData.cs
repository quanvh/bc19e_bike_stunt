using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if USE_INAPP
using UnityEngine.Purchasing;
#endif

[CreateAssetMenu(fileName = "ShopDataAsset", menuName = "DataAsset/ShopDataAsset")]
public class ShopData : ScriptableObject
{
    public List<InappProduct> listItems;

    public InappProduct GetProduct(int _idx)
    {
        return listItems.Where(t => t.ID == _idx).First();
    }

    public InappProduct GetProduct(string _idx)
    {
        return listItems.Where(t => t.ProductId.Equals(_idx) || t.Name.Equals(_idx)).First();
    }
}

[Serializable]
public class InappProduct
{
    public int ID;
#if USE_INAPP
    public ProductType Type;
#endif
    public string ProductId;
    public string Name;
    public float Price;
    public string Description;
    public Sprite Thumb;
}