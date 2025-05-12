using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public int cardIndex;

    //3D ī�� ��� 
    public MeshRenderer cardRenderer;       //ī�� �����ä��� icon or �Ϥ��ä��Ѥ��� 
    public TextMeshPro nameText;            //�̸� �ؽ�Ʈ
    public TextMeshPro costText;            //��� �ؽ�Ʈ
    public TextMeshPro attackText;          //���ݷ�/ȿ�� �ؽ�Ʈ
    public TextMeshPro descriptionText;     //���� �ؽ�Ʈ

    public bool isDragging = false;
    private Vector3 originalPosition;       //�巡�� �� ������ ��ġ

    //���̾� ����ũ 
    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    private CardManager cardManager;

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        cardManager = FindObjectOfType<CardManager>();

        SetupCard(cardData);

    }

    //ī�� ������ ����
    public void SetupCard(CardData data)
    {
        cardData = data;

        if(nameText != null ) nameText.text = data.cardName;
        if(costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if(descriptionText != null) descriptionText.text = data.description.ToString();

        if(cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }

        if(descriptionText != null)
        {
            descriptionText.text = data.description + data.GetAdditionalEffectsDescription();
        }
    }

    private void OnMouseDown()
    {
        //�巡�� ���� �� ���� ��ġ ����
        originalPosition = transform.position;
        isDragging = true;
    }
    private void OnMouseDrag()
    {
        if(isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);

        }    
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if(cardManager != null)
        {
            float distToDiscard = Vector3.Distance(transform.position, cardManager.discardPosition.position);
            if (distToDiscard < 2.0f)
            {
                cardManager.DiscardCard(cardIndex);
                return;
            }
        }

        CharacterStats playerStats = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            playerStats = playerObj.GetComponent<CharacterStats>();
        }
        if (playerStats == null || playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"������ �����մϴ�! (�ʿ� : {cardData.manaCost}, ���� : {playerStats?.currentMana ?? 0}");
            transform.position = originalPosition;
            return;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        bool cardUsed = false;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if(enemyStats != null)
            {
                if(cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} ī��� ������ {cardData.effectAmount} �������� �������ϴ�.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("�� ī��� ������ ����� �� �����ϴ�.");
            }
        }
        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            if(playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} ī��� �÷��̾��� ü���� {cardData.effectAmount} ȸ���߽��ϴ�.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("�� ī��� �÷��̾�� ��� �� �� �����ϴ�.");
            }
        }

        if(!cardUsed)
        {
            transform.position = originalPosition;
            if (cardManager != null)
                cardManager.ArrangeHand();
            return;
        }

        playerStats.UseMana(cardData.manaCost);
        Debug.Log($"������ {cardData.manaCost} ��� �߽��ϴ�. (���� ���� : {playerStats.currentMana}");

        if(cardData.additionalEffects != null && cardData.additionalEffects.Count > 0)
        {
            ProcessAdditionalEffectsAndDiscard();
        }
        else
        {
            if (cardManager != null)
                cardManager.DiscardCard(cardIndex);
        }
    }
    
    private void ProcessAdditionalEffectsAndDiscard()
    {
        //ī�� ������ �� �ε��� ����
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        //�߰� ȿ�� ����
        foreach(var effect in cardDataCopy.additionalEffects)
        {
            switch(effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:
                    for(int i = 0; i < effect.effectAmount; i++)
                    {
                        if(cardManager != null)
                        {
                            cardManager.DrawCard();
                        }

                    }
                    Debug.Log($"{effect.effectAmount} ���� ī�带 ��ο� �߽��ϴ�.");
                    break;

                case CardData.AdditionalEffectType.DiscardCard:     //ī�� ������ ���� (���� ������)
                    for(int i = 0; i < effect.effectAmount; i++)
                    {
                        if(cardManager != null && cardManager.handCards.Count > 0)
                        {
                            int randomindex = Random.Range(0,cardManager.handCards.Count);      //���� ũ�� �������� ���� �ε��� ����

                            Debug.Log($"���� ī�� ������ : ���õ� �ε��� {randomindex}, ���� ���� ũ�� : {cardManager.handCards.Count}");

                            if(cardIndexCopy < cardManager.handCards.Count)
                            {
                                if(randomindex != cardIndexCopy)
                                {
                                    cardManager.DiscardCard(randomindex);

                                    //���� ���� ī���� �ε����� ���� ī���� �ε������� �۴ٸ� ���� ī���� �ε����� 1 ���� ���Ѿ� ��
                                    if(randomindex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                                else if(cardManager.handCards.Count > 1)
                                {
                                    //�ٸ� ī�� ����
                                    int newIndex = (randomindex + 1)% cardManager.handCards.Count;
                                    cardManager.DiscardCard(newIndex);

                                    if (randomindex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                            }
                            else
                            {
                                //cardIndexCpoy�� �� �̻� ��ȿ���� ���� ���, �ƹ� ī�峪 ����
                                cardManager.DiscardCard(randomindex);
                            }
                        }
                    }
                    Debug.Log($"�������� {effect.effectAmount}���� ī�带 ���Ƚ��ϴ�.");
                    break;

                case CardData.AdditionalEffectType.GainMana:
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if(playerObj != null)
                    {
                        CharacterStats playerStats = playerObj.GetComponent<CharacterStats>();
                        if (playerStats != null)
                        {
                            playerStats.GainMana(effect.effectAmount);
                            Debug.Log($"������ {effect.effectAmount} ȹ�� �߽��ϴ�! (���� ���� : {playerStats.currentMana})");
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Ebentg");
                    foreach(var enemy in enemies)
                    {
                        CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
                        if (enemyStats != null)
                        {
                            enemyStats.GainMana(effect.effectAmount);
                            Debug.Log($"������ {effect.effectAmount} �� ������ (���� ���� : {effect.effectAmount} ���� ���׽��ϴ�.)");
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceCardCost:
                    for (int i = 0; i < cardManager.cardObjects.Count; i++)
                    {
                        CardDisplay display = cardManager.cardObjects[i].GetComponent<CardDisplay>();
                        if(display != null && display != this)
                        {
                            TextMeshPro costText = display.costText;
                            if(costText != null )
                            {
                                int originalCost = display.cardData.manaCost;
                                int newCost = Mathf.Max(0, originalCost - effect.effectAmount);
                                costText.text = newCost.ToString();
                                costText.color = Color.green; 
                            }
                        }
                    }   //���� ī�� ��� ���� ȿ�� �ð������θ�.
                    break;

            }
        }

        if(cardManager != null)
            cardManager.DiscardCard(cardIndexCopy);
    }
}
