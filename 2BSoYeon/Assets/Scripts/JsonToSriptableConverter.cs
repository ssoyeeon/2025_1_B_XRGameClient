


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
    public int? id;                    //int?는 Nullable<int>의 축약 표현입니다. 선언하면 null 값도 가질 수 있는 정수형이 됩니다
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
        string jsonText = File.ReadAllText(jsonFilePath);       //Json 파일을 읽는다.

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
                    Debug.LogWarning($"아이템 ' {itemData.itemName}; 의 유효하지 않은 타입 : {itemData.itemTypeString}");
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
                        Debug.LogWarning($"아이템 '{itemData.nameEng}'의 아이콤을 찾을 수 없습니다. : {itemData.iconPath}");

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
            Debug.LogError($"JSON 변환 오류 : {e}");
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
                //id 있는 행은 대화로 처리 
                if (rowData.id.HasValue)
                {
                    DialogSO dialogSO = ScriptableObject.CreateInstance<DialogSO>();
                    //데이터 복사
                    dialogSO.id = rowData.id.Value;
                    dialogSO.characterName = rowData.characterName;
                    dialogSO.text = rowData.text;
                    dialogSO.nextiId = rowData.nextId.HasValue ? rowData.nextId.Value : -1;
                    dialogSO.portraitPath = rowData.portraitPath;
                    dialogSO.choices = new List<DialogChoiceSO>();
                    //초상화 로드 (경로가 있는 경우)
                    if (!string.IsNullOrEmpty(rowData.portraitPath))
                    {
                        dialogSO.portrait = Resources.Load<Sprite>(rowData.portraitPath);

                        if (dialogSO.portrait == null)
                        {
                            Debug.LogWarning($"대화 {rowData.id}의 초상화를 찾을 수 없습니다.");
                        }
                    }
                    //dialogMap에 추가 
                    dialogMap[dialogSO.id] = dialogSO;
                    createDialogs.Add(dialogSO);
                }
            }

            //2단계 : 선택지 항목 처리 및 연결 
            foreach (var rowData in rowDataList)
            {
                //id가 없고 choiceText 가 있는 행은 선택지로 처리 
                if (!rowData.id.HasValue && !string.IsNullOrEmpty(rowData.choiceText) && rowData.choiceNextId.HasValue)
                {
                    //이전 행의 ID를 부모 ID로 사용 (연속되는 선택지의 경우)
                    int parentId = -1;

                    //이 선택지 바로 위에 있는 대화 (id가 있는 방목)를 찾음
                    int currentIndex = rowDataList.IndexOf(rowData);
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        if (rowDataList[i].id.HasValue)
                        {
                            parentId = rowDataList[i].id.Value;
                            break;
                        }
                    }

                    //부모 ID를 찾지 못했거나 부모 ID 가 -1 인 경우 (첫 번째 항목)
                    if (parentId == -1)
                    {
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'의 부모 대화를 찾을 수 없습니다.");
                    }

                    if (dialogMap.TryGetValue(parentId, out DialogSO parentDialog))
                    {
                        DialogChoiceSO choiceSO = ScriptableObject.CreateInstance<DialogChoiceSO>();
                        choiceSO.text = rowData.choiceText;
                        choiceSO.nextId = rowData.choiceNextId.Value;

                        //선택지 에셋 저장
                        string choiceAssetPath = $"{outputFolder}/Choice_{parentId}_{parentDialog.choices.Count + 1}.asset";
                        AssetDatabase.CreateAsset(choiceSO, choiceAssetPath);
                        EditorUtility.SetDirty(choiceSO);

                        parentDialog.choices.Add(choiceSO);
                    }
                    else
                    {
                        Debug.LogWarning($"선택지 '{rowData.choiceText}'를 연결할 대화 (ID : {parentId})를 찾을 수 없습니다.");
                    }

                }
            }

            //3단계 : 대화 스크립터블 오브젝트 저장
            foreach (var dialog in createDialogs)
            {
                //스크립터블 오브젝트 저장 - ID를 4자리 숫자로 포맷팅 
                string assetPath = $"{outputFolder}/Dialog_{dialog.id.ToString("D4")}.asset";
                AssetDatabase.CreateAsset(dialog, assetPath);

                //에셋 이름 지정
                dialog.name = $"Dialog_{dialog.id.ToString("D4")}";

                EditorUtility.SetDirty(dialog);
            }

            //데이터 베이스 생성
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
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }
}
#endif
