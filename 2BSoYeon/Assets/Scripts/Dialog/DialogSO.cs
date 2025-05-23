using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog" , menuName = "Dialog System/Dialog")]

public class DialogSO : ScriptableObject
{
    public int id;
    public string characterName;
    public string text;
    public int nextiId;
    public List<DialogChoiceSO> choices = new List<DialogChoiceSO>();
    public Sprite portrait;

    [Tooltip("초상화 리소스 경로 (Resources 폴더 내의 경로)")]
    public string portraitPath;
}
