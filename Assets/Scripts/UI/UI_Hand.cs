using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector; 

public class UI_Hand : SerializedMonoBehaviour
{    
    [SerializeField] UI_Card cardPrefab;
    [SerializeField] Faction faction;
    [SerializeField] float overlap;
    [SerializeField] float speed;

    public int sign => transform.position.x > (Screen.width / 2) ? 1 : -1;
    bool handChanged; 

    public Dictionary<Card, CardHandData> hand = new(); 

    public class CardHandData
    {
        public enum CardHandRole { None, Space, Hold, Event, Ops, Coup, Realign }
        public CardHandRole role;
        public int index;
        public bool revealed;
        public bool drag;
        public Tween tween;
        public UI_Card UI; 
    }

    private void Update()
    {
        if (handChanged)
            PositionCards(); 
    }

    private void Awake()
    {
        UI_Game.SetActivePlayerEvent += OnSetActivePlayer;
        UI_Card.OnCardDragStart += ui => PositionCards();
        UI_Card.OnCardDragEnd += ui => PositionCards(); 

        faction.AddCardEvent += AddCardToHand;
        faction.RemoveCardEvent += RemoveCardFromHand;

        hand = new();
    }

    public void OnSetActivePlayer(Faction faction)
    {
        Debug.Log($"OnSetActivePlayer: Creating 2 Tweens for each of {this.faction.name}'s {hand.Keys.Count(card => !hand[card].drag)} cards");

        foreach(Card card in hand.Keys.Where(card => !hand[card].drag))
        {
            hand[card].UI.transform.DORotate(new Vector3(0, faction == this.faction ? 0 : 180f, 0), 1f);

            DOTween.Sequence().SetDelay(.35f).OnComplete(() => {
                hand[card].UI.SetDisplay(faction == this.faction);
                hand[card].UI.GetComponent<RectTransform>().localScale = new(faction == this.faction ? 1 : -1 * hand[card].UI.GetComponent<RectTransform>().localScale.x, 1, 1);
            });
        }
    }

    public void AddCardToHand(Card card)
    {
        UI_Card uiCard = Instantiate(cardPrefab, transform);
        CardHandData handData = new();

        uiCard.name = card.Name;
        uiCard.gameObject.AddComponent<UI_DraggableCard>();
        uiCard.Setup(card); 

        handData.index = hand.Count;
        handData.UI = uiCard; 
        handChanged = true;

        hand.Add(card, handData);
        handData.UI.transform.SetParent(transform);
        handData.UI.SetDisplay(UI_Game.ActiveFaction == faction);
    }

    public void RemoveCardFromHand(Card card)
    {
        hand[card].tween.Kill();

        if (hand.Remove(card))
            handChanged = true;

        foreach (Card _card in hand.Keys)
            hand[_card].index = hand.Values.Count(chd => chd.index < hand[_card].index);
    }

    public void PositionCards()
    {
        List<Card> cards = hand.Keys.Where(card => hand[card].drag == false).ToList();

        foreach(Card card in cards)
        {
            float previousWidth = cards.Where(_card => hand[_card].index < hand[card].index)
                .Sum(_card => hand[_card].UI.GetComponent<RectTransform>().sizeDelta.x - overlap);

            if (hand[card].tween != null)
                hand[card].tween.Kill();

            hand[card].UI.transform.SetSiblingIndex(hand.Keys.OrderBy(_card => hand[_card].index).ToList().IndexOf(card)); 

            hand[card].tween = hand[card].UI.transform.DOLocalMove(new Vector3(sign * (previousWidth + hand[card].UI.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0), speed)
                .SetEase(Ease.InOutSine)
                .SetSpeedBased(true);
        }

        handChanged = false; 
    }
}