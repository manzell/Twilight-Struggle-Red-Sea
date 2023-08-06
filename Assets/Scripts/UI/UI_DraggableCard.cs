using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq; 

public class UI_DraggableCard : MonoBehaviour, IDraggableCard
{
    public static System.Action<IDraggableCard> CardDragStartEvent, CardDragEndEvent, CardDropEvent;
    public static bool DraggingDisabled;

    public Card Card => GetComponent<UI_Card>().Card;

    Transform parent;
    Transform dragUISpace;

    void Awake()
    {
        dragUISpace = GameObject.Find("Active UI").transform; // TODO Yeah Yeah it's a magic string but this is a prefab so
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parent = transform.parent; 

        // TODO - Find a way to disable Card Dragging 
        if (parent.TryGetComponent(out UI_Hand uiHand) && !DraggingDisabled)
        {
            transform.SetParent(dragUISpace);
            eventData.selectedObject = gameObject; 
            uiHand.hand[Card].drag = true;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            
            IDraggableCard.CardDragStartEvent?.Invoke(this);
            uiHand.PositionCards(); 
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parent.TryGetComponent(out UI_Hand uiHand))
            transform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (parent.TryGetComponent(out UI_Hand uiHand))
        {
            transform.SetParent(parent); 
            uiHand.hand[Card].drag = false;
            
            List<Card> cards = uiHand.hand.Keys.OrderBy(card => uiHand.hand[card].UI.transform.position.x * uiHand.sign).ToList();

            foreach(Card card in uiHand.hand.Keys)
                uiHand.hand[card].index = cards.IndexOf(card); 

            uiHand.PositionCards();
        }

        IDraggableCard.CardDragEndEvent?.Invoke(this);
        eventData.selectedObject = null;
    }
}
