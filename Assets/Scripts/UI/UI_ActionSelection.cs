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
    [SerializeField] UI_Card selectedCard; 
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] Image headerBackground;

    public Faction Faction => actionSelectionManager.faction;
    bool openable; 

    public void Setup(ActionSelectionManager selectionManager)
    {
        actionSelectionManager = selectionManager;
        header.text = selectionManager.faction.name;
        headerBackground.color = selectionManager.faction.PrimaryColor;
        openable = true; 

        IDraggableCard.CardDragStartEvent += OnCardDragStart;
        IDraggableCard.CardDragEndEvent += OnCardDragEnd;
        actionSelectionManager.SetCardEvent += OnSetCard;

        foreach (UI_ActionSelectionReceiver actionReceiver in actionSelectionArea.GetComponentsInChildren<UI_ActionSelectionReceiver>())
            Destroy(actionReceiver.gameObject);

        AddActions();
        selectedCard.transform.parent.gameObject.SetActive(selectionManager.card != null); 
    }

    private void OnDestroy()
    {
        IDraggableCard.CardDragStartEvent -= OnCardDragStart;
        IDraggableCard.CardDragEndEvent -= OnCardDragEnd;
    }

    void OnCardDragStart(IDraggableCard ui)
    {
        if (UI_Game.ActiveFaction == actionSelectionManager.faction)
            Open();
    }

    void OnCardDragEnd(IDraggableCard ui) => Close();

    void AddActions()
    {
        RectTransform rect = actionSelectionArea.GetComponent<RectTransform>();
        float padding = actionSelectionArea.GetComponent<VerticalLayoutGroup>().spacing;

        foreach (IExecutableAction action in actionSelectionManager.AvailableActions)
        {
            // Check if Card is Valid for the action
            UI_ActionSelectionReceiver receiver = Instantiate(actionSelectionReceiverPrefab, actionSelectionArea);
            receiver.Setup(action, this);
        }

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 
            actionSelectionManager.AvailableActions.Count * (actionSelectionReceiverPrefab.GetComponent<RectTransform>().sizeDelta.y + padding) + padding); 
    }

    public void Close() => window.SetActive(false);

    public void Open()
    {
        if (openable)
            window.SetActive(true);
    }

    public void Select(IExecutableAction action, Card card = null)
    {
        UI_DraggableCard.DraggingDisabled = false;
        openable = false; 
        Close();
        actionSelectionManager.Select(action, card ?? actionSelectionManager.card);
    }

    void OnSetCard(Card card)
    {
        selectedCard.Setup(card); 
        selectedCard.transform.parent.gameObject.SetActive(card != null); 
    }
}
