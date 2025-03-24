


#if Unity_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class JsonToSriptableConverter : EditorWindow
{
    private string JsonDilePath = "";
    private string outputFolder = "Assets/ScriptableObjects/items";
    private bool creatDatabase = true;

    [MenuIten("Tools/Json to Scriptable Objects")]
    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableConverter>("JSON to Scriptable Objects")      
    }

    private void ConverJsonToScriptableObjects()
    {
        if(!Directiory.Exits(outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
        }
    }
    string jsonText = File.ReadAllText(jsonDilePath);       //Json 파일을 읽는다.
    try
    {
        List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

        List<ItemSO> createdItems = new List<ItemSo>();
        foreach (var itemData in itemdataList)
        {
            ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

            itemSO.id = itemData.id;
            itemSO.itemName = itemData.itemName;
            itemSO.nameEng = itemData.nameEng;
            itemSO.description = itemData.description;

            if(System.Enum.TryParse(itemData.itemTypeString, out Itemtype parsedType))
            {
            itemSO.itemType = parsedType;
            }
            else
            {
            Debug.LogWarning($"아이템 ' {itemData.itemName}; 의 유효하지 않은 타입 : {itemData.itemTypeString}");
            }

            itemSO.price = itemData.price;
            itemSO.power = itemData.power;
            itemSO.level = itemData.level;
            itemSO.isStackable = itemData.isStackable;
        }
    }
    catch (System.Exception e)
    {
        EditorUtility.DisplayDialog("Error , $"Failed to Covert JSON : {e.Messge}","OK");
        Debug.LogError($"JSON 변환 오류 : {e}");
}
#endif
