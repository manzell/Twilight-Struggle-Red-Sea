using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using UnityEngine.UI; 

public class UI_ActionSelectionReceiver : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    IExecutableAction action;
    UI_ActionSelection uiSelectionWindow;
    [SerializeField] TextMeshProUGUI actionName;
    Image backgroundImage;
    Outline outline; 

    public Faction Faction => uiSelectionWindow.Faction; 

    public void Setup(IExecutableAction action, UI_ActionSelection uiSelection)
    {
        backgroundImage = GetComponent<Image>();
        outline = GetComponent<Outline>();

        this.action = action;
        uiSelectionWindow = uiSelection;
        actionName.text = action.ToString();

        if (action is IActingPlayerAction factionAction)
            factionAction.SetActingFaction(Faction);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"{action} {UI_Game.ActiveFaction} {eventData.selectedObject}");  
        Debug.Log($"{action is IOpsAction} {UI_Game.ActiveFaction == Faction} {eventData.selectedObject.GetComponent<IDraggableCard>() != null}"); 

        if (action is IOpsAction opsAction && UI_Game.ActiveFaction == Faction && eventData.selectedObject.TryGetComponent(out IDraggableCard ui))
        {
            Debug.Log($"{UI_Game.ActiveFaction} plays {ui.Card.name} for {action}");

            new GameState.RemoveCardFromHand(Faction, ui.Card).Execute();
            Destroy(eventData.selectedObject);

            opsAction.SetOps(ui.Card.Ops); 
            uiSelectionWindow.Select(action, ui.Card); 
        }
    }

    Color previousColor = Color.white; 

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(eventData.selectedObject.TryGetComponent(out IDraggableCard ui))
        {
            previousColor = backgroundImage.color; 

            if(action is GameAction gameAction)
                backgroundImage.color = gameAction.Can() ? Color.yellow : Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.color = previousColor;
    }
}