using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening; 

public class UI_Card : MonoBehaviour, IDraggableCard, IPointerEnterHandler, IPointerExitHandler
{
    //public static System.Action<IDraggableCard> CardDragStartEvent, CardDragEndEvent, CardDropEvent;
    [field: SerializeField] public Card Card { get; private set; }

    [SerializeField] TextMeshProUGUI cardName, influenceText, cardText, cardFlowText;
    [SerializeField] Image nameplateBG;
    Transform parent;
    int index;

    private void Awake()
    {
        if (Card != null)
            Setup(Card);
    }

    public void Setup(Card card)
    {
        this.Card = card;

        influenceText.text = card.Ops.Value().ToString();
        cardName.text = card.Name;
        cardText.text = card.CardText;

        if (card.Faction != null)
        {
            cardName.color = Color.white;
            nameplateBG.color = card.Faction.PrimaryColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(2, 0.5f);
        //transform.DOLocalMoveY(-transform.GetComponent<RectTransform>().sizeDelta.y / 2, 0.5f);
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
        //transform.DOLocalMoveY(transform.GetComponent<RectTransform>().sizeDelta.y / 2, 0.5f);
        Color color = cardText.color;
        color.a = 0;
        cardText.DOColor(color, .6f);
        cardFlowText.DOColor(color, .6f);

        if (UI_Game.ActiveFaction.Hand.Contains(Card))
            transform.SetSiblingIndex(index);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Todo - right now either player can drag any other player's card. 
        if(transform.parent.TryGetComponent(out UI_Hand uiHand))
        {
            parent = uiHand.transform;

            uiHand.RemoveCardFromHand(this);
            EventSystem.current.SetSelectedGameObject(gameObject);
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            IDraggableCard.CardDragStartEvent?.Invoke(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        
        parent.GetComponent<UI_Hand>()?.AddCardToHand(this);

        IDraggableCard.CardDragEndEvent?.Invoke(this);
    }
}
