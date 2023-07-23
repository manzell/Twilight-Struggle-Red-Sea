using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UI_Hand : MonoBehaviour
{
    [SerializeField] UI_Card cardPrefab;
    [SerializeField] GameObject unknownCardPrefab;
    [SerializeField] Faction faction;
    [SerializeField] float overlap;
    [SerializeField] float speed;

    List<Card> hand = new(); 
    Dictionary<Card, UI_Card> UIs = new();

    int sign => transform.position.x > (Screen.width / 2) ? 1 : -1;

    private void Awake()
    {
        UI_Game.SetActivePlayerEvent += OnSetActivePlayer;
        faction.AddCardEvent += AddCardToHand;
        faction.LoseCardEvent += RemoveCardFromHand;
    }

    private void Update()
    {
        PositionCards(); 
    }

    public void OnSetActivePlayer(Faction faction)
    {
        hand = new();
        UIs = new();

        foreach (Transform t in transform)
            Destroy(t.gameObject); 

        foreach (Card card in this.faction.Hand)
            AddCardToHand(card); 
    }

    public void AddCardToHand(Card card)
    {
        if(UI_Game.ActiveFaction == faction)
        {
            UI_Card uiCard = Instantiate(cardPrefab, transform);
            uiCard.Setup(card);
            uiCard.name = card.Name;

            AddCardToHand(uiCard); 
        }
        else
        {
            GameObject instancedCard = Instantiate(unknownCardPrefab, transform);
            instancedCard.name = $"Unknown {faction.name} Card";
        }
    }

    public void AddCardToHand(UI_Card uiCard)
    {
        int index = sign == 1 ?
            UIs.Values.Count(ui => ui.transform.position.x < uiCard.transform.position.x) :
            UIs.Values.Count(ui => ui.transform.position.x > uiCard.transform.position.x);

        uiCard.transform.SetParent(transform);
        uiCard.transform.SetSiblingIndex(index); 
        UIs.Add(uiCard.Card, uiCard);
        hand.Insert(index, uiCard.Card);
    }

    public void RemoveCardFromHand(Card card)
    {
        if (UIs.TryGetValue(card, out UI_Card uiCard))
        {
            RemoveCardFromHand(uiCard); 
            Destroy(uiCard.gameObject);
        }
    }

    public void RemoveCardFromHand(UI_Card uiCard)
    {
        if(uiCard.transform.IsChildOf(this.transform))
        {
            // instead of destroying the card, we pop it out of our Hand. Then whatever happens, happens. 
            uiCard.transform.SetParent(transform.parent);

            UIs.Remove(uiCard.Card);
            hand.Remove(uiCard.Card);
        }
    }

    void PositionCards()
    {
        foreach (Transform t in transform)
        {
            int index = 0;
            float previousWidth = 0f;
            float distance = Time.deltaTime * speed; 

            if(UI_Game.ActiveFaction == faction && t.TryGetComponent(out IDraggableCard uiCard))
            {
                index = Mathf.Max(index, hand.IndexOf(uiCard.Card));
                previousWidth = hand.GetRange(0, index).Sum(card => UIs[card].GetComponent<RectTransform>().sizeDelta.x);
            }
            else
            {
                index = Mathf.Max(index, t.GetSiblingIndex());
                previousWidth = transform.children().Where(_t => _t.GetSiblingIndex() < index).Sum(_t => _t.GetComponent<RectTransform>().sizeDelta.x);
            }

            Vector3 destination = new((previousWidth + (t.GetComponent<RectTransform>().sizeDelta.x / 2) - (overlap * index)) * sign, 0, 0);
            Vector3 waypoint = Vector3.Lerp(t.localPosition, destination, Mathf.Min(distance, 1));

            //t.SetSiblingIndex(sign == 1 ? index : transform.childCount - index);
            t.SetLocalPositionAndRotation(waypoint, Quaternion.identity);
        }
    }
}