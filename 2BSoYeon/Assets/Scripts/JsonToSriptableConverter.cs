


#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using Unity.VisualScripting;
using System;

public enum ConversionType
{
    Item,
    Dialogs
}
[Serializable]
public class DialogRowData
{
    public int? id;                    //int?�� Nullable<int>�� ��� ǥ���Դϴ�. �����ϸ� null ���� ���� �� �ִ� �������� �˴ϴ�
    public string characterName;
    public string text;
    public int? nextId;
    public string portraitPath;
    public string choiceText;
    public int? choiceNextId;

}
public class JsonToSriptableConverter : EditorWindow
{
    private string jsonFilePath = "";
    private string outputFolder = "Assets/ScriptableObjects";
    private bool creatDatabase = true;
    private ConversionType conversionType = ConversionType.Item;

    [MenuItem("Tools/Json to Scriptable Objects")]
    public static void ShowWindow()
    {
        GetWindow<JsonToSriptableConverter>("JSON to Scriptable Objects");
    }

    void OnGUI()
    {
        GUILayout.Label("JSON to Scriptable Object Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (GUILayout.Button("Select JSON File"))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }
        EditorGUILayout.LabelField("Selected File : ", jsonFilePath);
        EditorGUILayout.Space();
        conversionType = (ConversionType)EditorGUILayout.EnumPopup("Conversion Type:", conversionType);

        if (conversionType == ConversionType.Item && outputFolder == "Asset/ScriptableObjects")
        {
            outputFolder = "Assets/SciptableObjects/Items";
        }
        else if (conversionType == ConversionType.Dialogs && outputFolder == "Assets/ScriptableObject")
        {
            outputFolder = "Assets/ScriptableObjects/Dialogs";
        }
        outputFolder = EditorGUILayout.TextField("Output Folder : ", outputFolder);
        creatDatabase = EditorGUILayout.Toggle("Create Database Asset", creatDatabase);
        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Scriptable Objects"))
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file firest", "OK");
                return;
            }
            switch (conversionType)
            {
                case ConversionType.Item:
                    ConverJsonToScriptableObjects();
                    break;
                case ConversionType.Dialogs:
                    ConverJsonToScriptableObjects();
                    break;
            }
            ConverJsonToScriptableObjects();
        }
    }
    private void ConverJsonToScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        string jsonText = File.ReadAllText(jsonFilePath);       //Json ������ �д´�.

        try
        {
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();
            foreach (var itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();

                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                if (System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"������ ' {itemData.itemName}; �� ��ȿ���� ���� Ÿ�� : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                if (!string.IsNullOrEmpty(itemData.iconPath))
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}.png");
                    if (itemSO.icon == null)
                    {
                        Debug.LogWarning($"������ '{itemData.nameEng}'�� �������� ã�� �� �����ϴ�. : {itemData.iconPath}");

                    }
                }
                string assetPath = $"{outputFolder}/item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";
                AssetDatabase.CreateAsset(itemSO, assetPath);

                itemSO.name = $"item{itemData.id.ToString("D4")}+{itemData.nameEng}";
                createdItems.Add(itemSO);

                EditorUtility.SetDirty(itemSO);
            }
            if (creatDatabase && createdItems.Count > 0)
            {
                ItemDatabaseSO database = ScriptableObject.CreateInstance<ItemDatabaseSO>();
                database.items = createdItems;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/itemDatabase.asset");
                EditorUtility.SetDirty(database);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Sucess", $"Created {createdItems.Count} scriptable objects!", "OK");

        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to Covert JSON : {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
    private void ConvertJsonToItemScriptableObjects()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        string jsonText = File.ReadAllText(jsonFilePath);
        try
        {
            List<DialogRowData> rowDataList = JsonConvert.DeserializeObject<List<DialogRowData>>(jsonText);
            Dictionary<int, DialogSO> dialogMap = new Dictionary<int, DialogSO>();
            List<DialogSO> createDialogs = new List<DialogSO>();
            foreach (var rowData in rowDataList)
            {
                //id �ִ� ���� ��ȭ�� ó�� 
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();
                    //������ ����
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextiId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.portraitPath = rowData.portraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();
                    //�ʻ�ȭ �ε� (��ΰ� �ִ� ���)
                    if (!string.IsNullOrEmpty(rowData.portraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);

                        if (dialogSO.portrait == null)
                        {
                            Debug.LogWarning($"��ȭ {rowData.id}�� �ʻ�ȭ�� ã�� �� �����ϴ�.");
                        }
                    }
                    //dialogMap�� �߰� 
                    dialogMap[dialogSO.id] = dialogSO;
                    createDialogs.Add(dialogSO);
                }
            }

            //2�ܰ� : ������ �׸� ó�� �� ���� 
            foreach (var rowData in rowDataList)
            {
                //id�� ���� choiceText �� �ִ� ���� �������� ó�� 
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    //���� ���� ID�� �θ� ID�� ��� (���ӵǴ� �������� ���)
                    int parentId = -1;

                    //�� ������ �ٷ� ���� �ִ� ��ȭ (id�� �ִ� ���)�� ã��
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    //�θ� ID�� ã�� ���߰ų� �θ� ID �� -1 �� ��� (ù ��° �׸�)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� �θ� ��ȭ�� ã�� �� �����ϴ�.");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        //������ ���� ����
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
                        EditorUtility.SetDirty(choiceSO);

                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"������ '{rowData.choiceText}'�� ������ ��ȭ (ID : {parentId})�� ã�� �� �����ϴ�.");
                    }

                }
            }

            //3�ܰ� : ��ȭ ��ũ���ͺ� ������Ʈ ����
            foreach (var dialog in createDialogs)
            {
                //��ũ���ͺ� ������Ʈ ���� - ID�� 4�ڸ� ���ڷ� ������ 
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                //���� �̸� ����
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            //������ ���̽� ����
            if (creatDatabase && createDialogs.Count > 0)
            {
                DialogDatabaseSO database = ScriptableObject.CreateInstance<DialogDatabaseSO>();
                database.dialogs = createDialogs;

                AssetDatabase.CreateAsset(database, $"{outputFolder}/DialogDatabase.asset");
                EditorUtility.SetDirty(database);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Creatd {createDialogs.Count} dialog scriptable objects!", "OK");

        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON ��ȯ ���� : {e}");
        }
    }
}
#endif
