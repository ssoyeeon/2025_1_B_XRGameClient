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

    private bool isDragging = false;
    private Vector3 originalPosition;       //�巡�� �� ������ ��ġ

    //���̾� ����ũ 
    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    // Start is called before the first frame update
    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");
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

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //ī��� ��� ���� ���� ����
        bool cardUsed = false;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CaracterStats enemyStats = hit.collider.GetComponent<CaracterStats>();
            if(enemyStats != null)
            {
                if(cardData.cardType == CardData.CardType.Attack)
                {
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} ī��� ������ {cardData.effectAmount} ������� �������ϴ�");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("�� ī��� ������ ����� �� �����ϴ�.");
                }
            }
        }
        else if(Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            CaracterStats playerStats = hit.collider.GetComponent<CaracterStats>();

            if (playerStats != null)
            {
                if(cardData.cardType == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} ī��� �÷��̾��� ü���� {cardData.effectAmount} ȸ���߽��ϴ�");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("�� ī��� ������ ����� �� �����ϴ�.");

                }
            }
        }

        //ī�带 ������� �ʾҴٸ� ���� ��ġ�� �ǵ�����
        if(!cardUsed)
        {
            transform.position = originalPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
