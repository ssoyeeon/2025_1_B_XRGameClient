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

        CaracterStats playerStats = FindObjectOfType<CaracterStats>();
        if(playerStats != null || playerStats.currentMana < cardData.manaCost)      //마나 검사
        {
            Debug.Log($"마나가 부족해요 n.n (필요 : {cardData.manaCost}, 현재 : {playerStats?.currentMana ?? 0})");
            transform.position = originalPosition;
            return;
        }

        isDragging = false;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드ㅜ 사용 판정 지역 변수
        bool cardUsed = false;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CaracterStats enemyStats = hit.collider.GetComponent<CaracterStats>();
            if(enemyStats != null)
            {
                if(cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 적에게 {cardData.effectAmount} 대미지를 입혔습니다");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
                }
            }
        }
        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            //플렐이어에게 힐 효과 적용
            //CaracterStats playerStats = hit.collider.GetComponent<CaracterStats>();

            if (playerStats != null)
            {
                if(cardData.cardType == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} 카드로 플레이어의 체력을 {cardData.effectAmount} 회복했습니다");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다.");

                }
            }
        }
        else if(cardManager != null)
        {
            //버린 카드 더미 근처에 드롭했는지 검사
            float disToDiscard = Vector3.Distance(transform.position, cardManager.discardPosition.position);
            if(disToDiscard < 2.0f)
            {
                //카드를 버리기
                cardManager.DiscardCard(cardIndex);
                return;

            }

        }


        //카드를 사용하지 않았다면 원래 위치로 되돌리기
        if (!cardUsed)
        {
            transform.position = originalPosition;
            //손패 재정렬 (필요한 경우)
            cardManager.ArrangeHand();
        }
        else
        {
            //카드를 사용했다면 버린 카드 더미로 이동
            if (cardManager != null)
                cardManager.DiscardCard(cardIndex);

            //카드 사용 시 마나 소모(카드가 성공적으로 사용된 후 추가)
            playerStats.UseMana(cardData.manaCost);
            Debug.Log($"마나를 {cardData.manaCost} 사용 했습니다. (남은 마나 : {playerStats.currentMana})");

        }
    }
}
