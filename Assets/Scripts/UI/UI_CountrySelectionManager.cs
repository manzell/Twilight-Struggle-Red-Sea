using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; 

public class UI_CountrySelectionManager : MonoBehaviour
{
    [SerializeField] GameObject buttonWindow;
    [SerializeField] Button submitButton; 

    private void Start()
    {
        CountrySelectionManager.StartSelectEvent += Setup;
        CountrySelectionManager.CompleteSelectEvent += sel => buttonWindow.SetActive(false);
    }

    public void Setup(CountrySelectionManager selection)
    {
        buttonWindow.SetActive(true);

        selection.SelectCountryEvent += c => SetButtonSelectStatus(selection); 
        selection.DeselectCountryEvent += c => SetButtonSelectStatus(selection);
        
        SetButtonSelectStatus(selection);
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(() => selection.Complete()); 
    }

    void SetButtonSelectStatus(CountrySelectionManager selectionManager)
    {
        submitButton.enabled = selectionManager != null && selectionManager.Selected.Count >= selectionManager.minSelectable &&
            selectionManager.Selected.Count <= selectionManager.maxSelectable;
    }
}
