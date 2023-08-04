using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static System.Action<UI_Card> OnCardDragStart, OnCardDragEnd;
    [SerializeField] GameObject highlight, showCard; 
    [SerializeField] TextMeshProUGUI cardName, influenceText, cardText, cardFlowText;
    [SerializeField] Image nameplateBG;

    public Card Card { get; private set; }
    int index;

    private void Awake()
    {
        if (Card != null)
            Setup(Card);
    }

    public void Setup(Card card)
    {
        Card = card;

        influenceText.text = card.Ops.Value().ToString();
        cardName.text = card.Name;
        cardText.text = card.CardText;

        if (card.Faction != null)
        {
            cardName.color = Color.white;
            nameplateBG.color = card.Faction.PrimaryColor;
        }
    }

    public void SetDisplay(bool show) => showCard.SetActive(show);

    public void Highlight(Faction faction)
    {
        Color color = faction.PrimaryColor;
        color.a = 0.75f;

        highlight.GetComponent<Image>().color = color;
        highlight.SetActive(true); 
    }

    public void RemoveHighlight() => highlight.SetActive(false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(2, 0.5f);
        Color color = cardText.color;
        color.a = 1;
        cardText.DOColor(color, .4f);
        cardFlowText.DOColor(color, .4f);

        if (UI_Game.ActiveFaction.Hand.Contains(Card))
        {
            index = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, .5f);
        Color color = cardText.color;
        color.a = 0;
        cardText.DOColor(color, .6f);
        cardFlowText.DOColor(color, .6f);

        if (UI_Game.ActiveFaction.Hand.Contains(Card))
            transform.SetSiblingIndex(index);
    }
}
