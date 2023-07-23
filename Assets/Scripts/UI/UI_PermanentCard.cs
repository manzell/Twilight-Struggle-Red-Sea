using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro; 

public class UI_PermanentCard : SerializedMonoBehaviour, IDraggableCard, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [field: SerializeField] public Card Card { get; private set; }
    [SerializeField] Faction faction; 
    [SerializeField] GameObject tile;
    [SerializeField] TextMeshProUGUI opsText; 

    private void Awake()
    {
        GameState.SetPermanentCardStatus.OnSetPermanentCardStatus += UpdateCard;
        Game.StartGameEvent += () => UpdateCard(Card, Game.currentState.PermanentCards[Card].faction, Game.currentState.PermanentCards[Card].faceup); 
    }

    void UpdateCard(Card card, Faction faction, bool faceup)
    {
        if(card == Card)
        {
            if(this.faction == Game.currentState.PermanentCards[Card].faction) 
                tile.GetComponent<CanvasGroup>().DOFade(faceup ? 1 : 0.4f, .3f);
            else
                tile.GetComponent<CanvasGroup>().DOFade(0f, .3f);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"OnBeginDrag({tile.name})");
        if (Game.currentState.PermanentCards[Card].faction == UI_Game.ActiveFaction)
        {
            GameObject dragClone = Instantiate(tile, transform.parent);
            EventSystem.current.SetSelectedGameObject(dragClone);
            dragClone.GetComponent<CanvasGroup>().blocksRaycasts = false;
            dragClone.GetComponent<CanvasGroup>().alpha = 0.5f;
            dragClone.transform.SetPositionAndRotation(tile.transform.position, transform.rotation);
            IDraggableCard.CardDragStartEvent?.Invoke(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        eventData.selectedObject.transform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {      
        IDraggableCard.CardDragEndEvent?.Invoke(this);
        Destroy(eventData.selectedObject);
        EventSystem.current.SetSelectedGameObject(null);  
    }
}
