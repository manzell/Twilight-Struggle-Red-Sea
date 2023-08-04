using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_ActionSelection : MonoBehaviour
{
    public ActionSelectionManager actionSelectionManager { get; private set; }
    [SerializeField] UI_ActionSelectionReceiver actionSelectionReceiverPrefab;
    [SerializeField] Transform actionSelectionArea;
    [SerializeField] GameObject window; 
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] Image headerBackground;

    public Faction Faction => actionSelectionManager.faction; 

    public void Setup(ActionSelectionManager selectionManager)
    {
        actionSelectionManager = selectionManager;
        header.text = selectionManager.faction.name;
        headerBackground.color = selectionManager.faction.PrimaryColor;

        IDraggableCard.CardDragStartEvent += OnCardDragStart;
        IDraggableCard.CardDragEndEvent += OnCardDragEnd;
    }

    private void OnDestroy()
    {
        IDraggableCard.CardDragStartEvent -= OnCardDragStart;
        IDraggableCard.CardDragEndEvent -= OnCardDragEnd;
    }

    void OnCardDragStart(IDraggableCard ui)
    {
        if (UI_Game.ActiveFaction == actionSelectionManager.faction)
            Open(ui.Card);
    }

    void OnCardDragEnd(IDraggableCard ui) => Close();

    void Style(Card card)
    {
        foreach (UI_ActionSelectionReceiver actionReceiver in actionSelectionArea.GetComponentsInChildren<UI_ActionSelectionReceiver>())
            Destroy(actionReceiver.gameObject);

        foreach (IExecutableAction action in actionSelectionManager.AvailableActions)
        {
            // Check if Card is Valid for the action
            UI_ActionSelectionReceiver receiver = Instantiate(actionSelectionReceiverPrefab, actionSelectionArea);
            receiver.Setup(action, this);
        }
    }

    public void Close()
    {
        window.SetActive(false);
    }

    public void Open(Card card)
    {
        Style(card); 
        window.SetActive(true);
    }

    public void Select(IExecutableAction action, Card card = null)
    {
        Close();
        actionSelectionManager.Select(action, card); 
    }
}
