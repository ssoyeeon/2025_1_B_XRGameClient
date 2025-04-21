using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public int id;
    public string itemName;
    public string description;
    public string nameEng;
    public string itemTypeString;
    [NonSerialized]
    public ItemType itemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public string iconPath;

    public void InitalizeEnums()
    {
        if(Enum.TryParse(itemTypeString, out ItemType parsedType))
        {
            itemType = parsedType;
        }
        else
        {
            Debug.LogError($"아이템 '{itemName}' 에 유효하지 않은 아이템 타임 : {itemTypeString}");
            //기본 값 설정
            itemType = ItemType.Consumable;
        }
    }
}
