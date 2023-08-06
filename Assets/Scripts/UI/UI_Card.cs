using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class UI_Card : MonoBehaviour
{
    public static System.Action<UI_Card> OnCardDragStart, OnCardDragEnd;
    [field: SerializeField] public GameObject highlight { get; private set; }
    [field: SerializeField] public GameObject showCard; 
    [field: SerializeField] public TextMeshProUGUI cardName { get; private set; }
    [field: SerializeField] public TextMeshProUGUI influenceText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI cardText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI cardFlowText { get; private set; }

    [SerializeField] Image nameplateBG;

    public Card Card { get; private set; }
    public int index;

    private void Awake()
    {
        if (Card != null)
            Setup(Card);
    }

    public void Setup(Card card)
    {
        Card = card;
        cardName.text = card.Name;
        cardText.text = card.CardText;

        if (influenceText != null)
            influenceText.text = card.Ops.Value().ToString();

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

}
