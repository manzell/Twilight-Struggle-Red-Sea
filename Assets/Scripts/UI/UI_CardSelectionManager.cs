using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UI_CardSelectionManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI headline; 
    [SerializeField] GameObject cardSelectionWindow, cardDisplayArea;
    [SerializeField] UI_SelectableCard selectableCardPrefab; 
    [SerializeField] Button SubmitButton, ResetButton; 

    private void Awake()
    {
        CardSelectionManager.StartCardSelectionEvent += Open;
        CardSelectionManager.CompleteCardSelectionEvent += Close; 
    }

    public void Open(CardSelectionManager selection)
    {
        int countMultiplier = Mathf.Min(3, selection.Selectable.Count);
        float cardWidth = selectableCardPrefab.GetComponent<RectTransform>().sizeDelta.x;

        cardSelectionWindow.SetActive(true);
        SubmitButton.onClick.AddListener(selection.Complete); 
        selection.CardSelectEvent += card => SubmitButton.enabled = selection.CanSubmit;
        selection.CardDeselectEvent += card => SubmitButton.enabled = selection.CanSubmit;

        headline.text = $"Select {(selection.minSelect == selection.maxSelect ? selection.minSelect.ToString() : selection.minSelect + " to " + selection.maxSelect)} " +
            $"{(selection.maxSelect == 1 ? "Card" : "Cards")}";

        cardSelectionWindow.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
            cardWidth * countMultiplier + 25 * (countMultiplier + 1)); 

        foreach (Card card in selection.Selectable)
            Instantiate(selectableCardPrefab, cardDisplayArea.transform).Setup(selection, card); 
    }

    public void Close(CardSelectionManager selection)
    {
        foreach (Transform t in cardDisplayArea.transform)
            Destroy(t.gameObject); 

        cardSelectionWindow.SetActive(false);
        SubmitButton.onClick.RemoveAllListeners();
    }    
}
