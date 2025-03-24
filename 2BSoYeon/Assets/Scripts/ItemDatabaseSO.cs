using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "itemDatabase", menuName = "Inventory/Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    //캐싱을 위한 사전 찾기
    private Dictionary<int, ItemSO> itemsByld;      //Id로 아이템을 찾기 위한 캐싱
    private DIctionary<string, ItemSO> itemsByName;

    public void Initaialize()
    {
        itemsByld = new Dictionary<int, ItemSO>();
        itemsByName = new DIctionary<string, ItemSO>();

        foreach ( var item in items )
        {
            itemsByld[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }

    //ID로 아이템 찾기

    public ItemSO GetItemByld( int id )
    {
        if( itemsByld == null )             //itemsByld가 캐싱이 되어 있지 않다면 초가화 한다.
        {
            Initaialize();
        }
        if (itemsByld.TryGetValue(idm, out ItemSo item))    //id 값을 통해서 itemSO를 찾아서 리턴 한다.
            return item;

        return null;        //없으면 Null
    }

    public ItemSo GetItemByName(string name)
    {
        if (itemsByName == null) { Initaialize(); }
        if (itemsByName.TryGetValue(name, out ItemSO item)
            return item;   

        return null;
    }

    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(items =>  items.Type == type);
    }
}
