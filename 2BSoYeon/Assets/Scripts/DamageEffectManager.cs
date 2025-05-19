using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas uiCanvas;

    public static DamageEffectManager instance { get; private set; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if(uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if(uiCanvas == null)
            {
                Debug.LogError("UI ĵ���ä��� ��������� �߿䤷���Ӥ��ˤ�");
            }
        }
    }

    public void ShowDamageText(Vector3 position, string text, Color color, bool isCritical= false, bool isStatusEffect = false)
    {
        if (textPrefab == null || uiCanvas == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        if(screenPos.z < 0) return;

        GameObject damageText = Instantiate(textPrefab, uiCanvas.transform);

        RectTransform rectTransform = damageText.GetComponent<RectTransform>();
        if(rectTransform != null)
        {
            rectTransform.position = screenPos;
        }
        
        TextMeshProUGUI tmp = damageText.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
            tmp.outlineColor = new Color(
                Mathf.Clamp01(color.r - 0.3f),
                Mathf.Clamp01(color.g - 0.3f),
                Mathf.Clamp01(color.b - 0.3f),
                color.a
            );

            float scale = 1.0f;

            int numbericValue;                                  //�ؽ�Ʈ�� ���������Ӥ� �椷�� ������ ���� ũ�� ������ 
            if (int.TryParse(text.Replace("+","").Replace("CRITI","").Replace("HEAL CRIT",""),out numbericValue))
            {
                scale = Mathf.Clamp(numbericValue / 15f, 0.8f, 2.5f);
            }
            if (isCritical) scale = 1_4f;
            if (isStatusEffect) scale *= 0.8f;

            damageText.transform.localScale = new Vector3 (scale, scale, scale);
        }

        DamageTextEffect effect = damageText.AddComponent<DamageTextEffect>();
        if(effect != null)
        {
            effect.Initialized(isCritical, isStatusEffect);
            if(isStatusEffect)
            {
                effect.SetVerticalMovement();
            }
        }

    }

    public void ShowDamage(Vector3 position, int amount, bool isCritical = false)
    {
        string text = amount.ToString();
        Color color = isCritical ? new Color(1.0f, 0.8f, 0.0f) : new Color(1.0f, 0.3f, 0.3f);
        if(isCritical)
        {
            text = "CRIT!\n" + text;
        }

        ShowDamageText(position, text, color, isCritical );
    }
    public void ShowHeal(Vector3 position, int amount, bool isCritical = false)
    {
        string text = amount.ToString();
        Color color = isCritical ? new Color(1.0f, 0.8f, 0.0f) : new Color(1.0f, 0.3f, 0.3f);
        if(isCritical)
        {
            text = "HEAL CRIT!\n" + text;
        }

        ShowDamageText(position, text, color, isCritical );
    }

    public void ShowMiss(Vector3 position)                      //��;�� �� ��  
    {
        ShowDamageText(position , "MISS",Color.gray,false);
    }

    public void ShowStatusEffect(Vector3 position, string effectName)
    {
        Color color;

        switch(effectName.ToLower())
        {
            case "position":
                color = new Color(0.5f, 0.1f, 0.5f);
                break;
            case "burn":
                color = new Color(1.0f, 0.4f, 0.0f);
                break;
            case "freeze":
                color = new Color(0.5f, 0.8f, 1.0f);
                break;
            case "stun":
                color = new Color(1.0f, 1.0f, 0.0f);
                break;
            default:
                color = new Color(1.0f, 1.0f, 1.0f);
                break;

        }
        ShowDamageText(position,effectName.ToLower(), color, false,true);
    }

}
