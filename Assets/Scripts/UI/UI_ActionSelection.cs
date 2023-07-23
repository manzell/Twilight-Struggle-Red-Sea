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
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] Image headerBackground;

    public void Setup(ActionSelectionManager selectionManager, UI_ActionSelectionReceiver prefab)
    {
        actionSelectionManager = selectionManager;
        actionSelectionReceiverPrefab = prefab;
        header.text = selectionManager.actingFaction.name;
        headerBackground.color = selectionManager.actingFaction.PrimaryColor;

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
        if (UI_Game.ActiveFaction == actionSelectionManager.actingFaction)
            Open(ui.Card);
    }

    void OnCardDragEnd(IDraggableCard ui) => Close();

    void Style(Card card)
    {
        foreach (Transform t in actionSelectionArea)
            Destroy(t.gameObject);

        foreach (IExecutableAction action in actionSelectionManager.AvailableActions)
        {
            // Check if Card is Valid for the action
            UI_ActionSelectionReceiver receiver = Instantiate(actionSelectionReceiverPrefab, actionSelectionArea);
            receiver.Setup(action, this);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open(Card card)
    {
        Style(card); 
        gameObject.SetActive(true);
    }

    public void Select(IExecutableAction action, Card card = null)
    {
        Close();
        actionSelectionManager.Select(action, card); 
        Destroy(gameObject);
    }

}
