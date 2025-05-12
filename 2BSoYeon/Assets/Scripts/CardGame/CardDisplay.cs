using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public int cardIndex;

    //3D 카드 요소 
    public MeshRenderer cardRenderer;       //카드 렌ㄷㅓㄹㅓ icon or 일ㄹㅓㅅㅡㅌㅡ 
    public TextMeshPro nameText;            //이름 텍스트
    public TextMeshPro costText;            //비용 텍스트
    public TextMeshPro attackText;          //공격력/효과 텍스트
    public TextMeshPro descriptionText;     //설명 텍스트

    public bool isDragging = false;
    private Vector3 originalPosition;       //드래그 전 원ㄹㅐ 위치

    //레이어 마스크 
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

    //카드 데이터 생성
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
        //드래그 시작 시 원래 위치 저장
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
            Debug.Log($"마나가 부족합니다! (필요 : {cardData.manaCost}, 현재 : {playerStats?.currentMana ?? 0}");
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
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 데미지를 입혔습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
            }
        }
        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            if(playerStats != null)
            {
                if (cardData.cardType == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다.");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("이 카드는 플레이어에게 사용 할 수 없습니다.");
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
        Debug.Log($"마나를 {cardData.manaCost} 사용 했습니다. (남은 마나 : {playerStats.currentMana}");

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
        //카드 데이터 및 인덱스 보존
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        //추가 효과 적용
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
                    Debug.Log($"{effect.effectAmount} 장의 카드를 드로우 했습니다.");
                    break;

                case CardData.AdditionalEffectType.DiscardCard:     //카드 버리기 구현 (랜덤 버리기)
                    for(int i = 0; i < effect.effectAmount; i++)
                    {
                        if(cardManager != null && cardManager.handCards.Count > 0)
                        {
                            int randomindex = Random.Range(0,cardManager.handCards.Count);      //손패 크기 기준으로 랜덤 인덱스 생성

                            Debug.Log($"랜덤 카드 버리기 : 선택된 인덱스 {randomindex}, 현재 손패 크기 : {cardManager.handCards.Count}");

                            if(cardIndexCopy < cardManager.handCards.Count)
                            {
                                if(randomindex != cardIndexCopy)
                                {
                                    cardManager.DiscardCard(randomindex);

                                    //만약 버린 카드의 인덱스가 현재 카드의 인덱스보다 작다면 현재 카드의 인덱스를 1 감소 시켜야 함
                                    if(randomindex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                                else if(cardManager.handCards.Count > 1)
                                {
                                    //다른 카드 선택
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
                                //cardIndexCpoy가 더 이상 유효하지 않은 경우, 아무 카드나 버림
                                cardManager.DiscardCard(randomindex);
                            }
                        }
                    }
                    Debug.Log($"랜덤으로 {effect.effectAmount}장의 카드를 버렸습니다.");
                    break;

                case CardData.AdditionalEffectType.GainMana:
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if(playerObj != null)
                    {
                        CharacterStats playerStats = playerObj.GetComponent<CharacterStats>();
                        if (playerStats != null)
                        {
                            playerStats.GainMana(effect.effectAmount);
                            Debug.Log($"마나를 {effect.effectAmount} 획득 했습니다! (현재 마나 : {playerStats.currentMana})");
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
                            Debug.Log($"마나를 {effect.effectAmount} 의 마나를 (현재 마나 : {effect.effectAmount} 감소 시켰습니다.)");
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
                    }   //다음 카드 비용 감소 효과 시각적으로만.
                    break;

            }
        }

        if(cardManager != null)
            cardManager.DiscardCard(cardIndexCopy);
    }
}
