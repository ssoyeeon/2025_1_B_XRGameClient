using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalEffect
{
    public CardData.AdditionalEffectType effectType;
    public int effectAmount;

    public string GetDescription()
    {
        switch(effectType)
        {
            case CardData.AdditionalEffectType.DrawCard:
                return $"ī�� {effectAmount}�� ��ο� ";

            case CardData.AdditionalEffectType.DiscardCard:
                return $"ī�� {effectAmount}�� ������ ";

            case CardData.AdditionalEffectType.GainMana:
                return $"���� {effectAmount} ȹ�� ";

            case CardData.AdditionalEffectType.ReduceEnemyMana:
                return $"�� ���� {effectAmount}���� ";

            case CardData.AdditionalEffectType.ReduceCardCost:
                return $"���� ī�� ��� {effectAmount}���� ";
            default:
                return " ";
        }
    }
}
