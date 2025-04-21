using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<CardData> deckCards = new List<CardData>();         //���� �ִ� ī�� 
    public List<CardData> handCards = new List<CardData>();         //�տ� �ִ� ī��
    public List<CardData> discardCards = new List<CardData>();      //���� ī�� ����

    public GameObject cardPrefab;           //ī�� ������
    public Transform deckPosition;              //�� ��ġ
    public Transform handPosition;            //�� �߾� ��ġ
    public Transform discardPosition;         //���� ī�� ���� ��ġ

    public List<GameObject> cardObjects = new List<GameObject>();           //���� ī�� ���� ������Ʈ


    void Start()
    {
        ShuffleDeck();              //���� �� ī�� ����

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))         //DŰ�� ������ ī�� ��ο�
        {
            DrawCard();
        }
        if(Input.GetKeyDown(KeyCode.F))         //FŰ�� ������ ���� ī�带 ������ �ǵ����� ���� 
        {
            ReturnDiscardsToDeck();
        }
        ArrangeHand();                          //���� ��ġ ������Ʈ
    }

    //�� ����
    public void ShuffleDeck()
    {
        List<CardData> tempDeck = new List<CardData>(deckCards);        //�ӽ� ����Ʈ�� ī�� ����
        deckCards.Clear();

        while (tempDeck.Count > 0)
        {
            int randIndex = Random.Range(0, tempDeck.Count);
            deckCards.Add(tempDeck[randIndex]);
            tempDeck.RemoveAt(randIndex);

            Debug.Log("���� ������! : " + deckCards.Count + "��");
        }
    }

    public void DrawCard()
    {
        if (handCards.Count >= 6)
        {
            Debug.Log("���а� ���� á���ϴ�. ! (�ִ� 6��)");
            return;
        }
        if (deckCards.Count == 0)
        {
            Debug.Log("���� ī�尡 �����ϴ�.");
            return;
        }

        //������ �� �� ī�� ��������
        CardData cardData = deckCards[0];
        deckCards.RemoveAt(0);

        //���п� �߰�
        handCards.Add(cardData);

        //ī�� ���� ������Ʈ ����
        GameObject cardObj = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);

        //ī�� ���� ����
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();

        if (cardDisplay != null)
        {
            cardDisplay.SetupCard(cardData);
            cardDisplay.cardIndex = handCards.Count - 1;
            cardObjects.Add(cardObj);
        }

        //���� ��ġ ������Ʈ
        ArrangeHand();

        Debug.Log("ī�带 ��ο� �߽��ϴ�. : " + cardData.cardName + "(���� : " + handCards.Count + " /6");
    }

    public void ArrangeHand()       //�տ� �ִ� ī�� ������
    {
        if (handCards.Count == 0) return;

        //���� ��ġ�� ���� ����
        float cardWidth = 1.2f;
        float spacing = cardWidth + 1.8f;
        float totalWidth = (handCards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        //�� ī�� ��ġ ����
        for(int i = 0; i < cardObjects.Count; i++)
        {
            if(cardObjects[i] != null)
            {
                //�巡�� ���� ī��� �ǳʶٱ�
                CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
                if (display != null && display.isDragging)
                    continue;       //�׳� �ѱ���.

                //��ǥ ��ġ ���
                Vector3 tarPosition = handPosition.position + new Vector3(startX + (i * spacing), 0, 0);

                //�ε巯�� �̵�
                cardObjects[i].transform.position = Vector3.Lerp(cardObjects[i].transform.position, tarPosition, Time.deltaTime * 10f);

            }
        }

    }
    public void DiscardCard(int handIndex)
    {
        if(handIndex < 0 || handIndex >= handCards.Count)
        {
            Debug.Log("��ȿ���� ���� ī�� �ε��� �Դϴ�!");
            return;
        }

        //���п��� ī�� ��������
        CardData cardData = handCards[handIndex];
        handCards.RemoveAt(handIndex );

        //���� ī�� ���̿� �߰�
        discardCards.Add(cardData);

        //�ش� ī�� ���� ������Ʈ ����
        if(handIndex < cardObjects.Count)
        {
            Destroy(cardObjects[handIndex]);
            cardObjects.RemoveAt(handIndex);
        }

        for(int i = 0;i < cardObjects.Count;i++)
        {
            CardDisplay display = cardObjects[i].GetComponent<CardDisplay>();
            if(display != null) display.cardIndex = i;


        }

        ArrangeHand();
        Debug.Log("ī�带 ���Ƚ��ϴ�." + cardData.cardName);

    }

    //���� ī�带 ������ �ǵ����� ����
    public void ReturnDiscardsToDeck()
    {
        if(discardCards.Count == 0)
        {
            Debug.Log("���� ī�� ���̰� ��� �ֽ��ϴ�.");
            return;
        }

        deckCards.AddRange(discardCards);                   //���� ī�带 ��� ���� �߰�
        discardCards.Clear();                               //���� ī�� ���� ����
        ShuffleDeck();                                      //�� ����

        Debug.Log("���� ī��" + deckCards.Count + "���� ������ �ǵ����� �������ϴ�.");

    }
}
