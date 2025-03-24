using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "itemDatabase", menuName = "Inventory/Database")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    //ĳ���� ���� ���� ã��
    private Dictionary<int, ItemSO> itemsByld;      //Id�� �������� ã�� ���� ĳ��
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

    //ID�� ������ ã��

    public ItemSO GetItemByld( int id )
    {
        if( itemsByld == null )             //itemsByld�� ĳ���� �Ǿ� ���� �ʴٸ� �ʰ�ȭ �Ѵ�.
        {
            Initaialize();
        }
        if (itemsByld.TryGetValue(idm, out ItemSo item))    //id ���� ���ؼ� itemSO�� ã�Ƽ� ���� �Ѵ�.
            return item;

        return null;        //������ Null
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
