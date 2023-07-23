using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class UI_SelectableCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [field: SerializeField] public Card Card { get; private set; }

    [SerializeField] TextMeshProUGUI cardName, influenceText, cardText, cardFlowText;
    [SerializeField] Image nameplateBG, highlight;
    CardSelectionManager cardSelectionManager;

    public void Setup(CardSelectionManager manager, Card card)
    {
        Card = card;
        cardSelectionManager = manager; 
        influenceText.text = card.Ops.Value().ToString();
        cardName.text = card.Name;
        cardText.text = card.CardText;

        if (card.Faction != null)
        {
            cardName.color = Color.white;
            nameplateBG.color = card.Faction.PrimaryColor;
        }

        manager.CardSelectEvent += c => UpdateHighlight(); 
        manager.CardDeselectEvent += c => UpdateHighlight();
    }

    void UpdateHighlight()
    {
        highlight.color = cardSelectionManager.Selected.Contains(Card) ? Color.yellow : Color.green; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight.color = cardSelectionManager.Selected.Contains(Card) ? Color.red : Color.green; 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.color = cardSelectionManager.Selected.Contains(Card) ? Color.yellow : Color.clear;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        cardSelectionManager.Select(Card); 
    }
}
