using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq; 

public class UI_ActionSelectionReceiver : MonoBehaviour, IDropHandler
{
    IExecutableAction action;
    UI_ActionSelection uiSelectionWindow;
    [SerializeField] TextMeshProUGUI actionName; 

    public void Setup(IExecutableAction action, UI_ActionSelection uiSelection)
    {
        this.action = action;
        uiSelectionWindow = uiSelection;
        actionName.text = action.ToString();

        if (action is IActingPlayerAction factionAction)
            factionAction.SetActingFaction(uiSelection.actionSelectionManager.actingFaction);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (action is ICardAction opsAction && UI_Game.ActiveFaction == uiSelectionWindow.actionSelectionManager.actingFaction && 
            eventData.selectedObject.TryGetComponent(out IDraggableCard ui))
        {
            Debug.Log($"{UI_Game.ActiveFaction} plays {ui.Card.name} for {action}");
            Destroy(eventData.selectedObject);
            opsAction.SetCard(ui.Card);
            uiSelectionWindow.Select(action, ui.Card); 
        }
    }
}