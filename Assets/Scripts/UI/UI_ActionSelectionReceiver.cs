using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using UnityEngine.UI; 

public class UI_ActionSelectionReceiver : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    IExecutableAction action;
    UI_ActionSelection uiSelectionWindow;
    [SerializeField] TextMeshProUGUI actionName;
    Image backgroundImage;
    Color previousColor = Color.white; 

    public Faction Faction => uiSelectionWindow.Faction; 

    public void Setup(IExecutableAction action, UI_ActionSelection uiSelection)
    {
        backgroundImage = GetComponent<Image>();

        this.action = action;
        uiSelectionWindow = uiSelection;
        actionName.text = action.ToString();

        if (action is IActingPlayerAction factionAction)
            factionAction.SetActingFaction(Faction);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (UI_Game.ActiveFaction == Faction && eventData.selectedObject.TryGetComponent(out IDraggableCard ui))
        {
            new GameState.RemoveCardFromHand(Faction, ui.Card).Execute();
            Destroy(eventData.selectedObject);

            uiSelectionWindow.Select(action, ui.Card); 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        previousColor = backgroundImage.color;

        if (action is ICardAction cardAction)
        { 
            if(eventData.selectedObject != null && eventData.selectedObject.TryGetComponent(out UI_DraggableCard uiCard))
                backgroundImage.color = cardAction.Can(uiCard.Card) ? Color.yellow : Color.red;
            else if(uiSelectionWindow.actionSelectionManager.card != null)
                backgroundImage.color = cardAction.Can(uiSelectionWindow.actionSelectionManager.card) ? Color.yellow : Color.red;
        }
        else
            backgroundImage.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.color = previousColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UI_Game.ActiveFaction == Faction)
            uiSelectionWindow.Select(action, uiSelectionWindow.actionSelectionManager.card);
    }
}