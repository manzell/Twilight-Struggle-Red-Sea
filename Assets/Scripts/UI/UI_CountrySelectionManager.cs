using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; 

public class UI_CountrySelectionManager : MonoBehaviour
{
    [SerializeField] GameObject buttonWindow;
    [SerializeField] Button submitButton;
    CountrySelectionManager selectionManager; 

    private void Start()
    {
        CountrySelectionManager.StartSelectEvent += Setup;
        CountrySelectionManager.CompleteSelectEvent += sel => buttonWindow.SetActive(false);
    }

    public void Setup(CountrySelectionManager selection)
    {
        selectionManager = selection;
        buttonWindow.SetActive(true);

        selectionManager.SelectCountryEvent += OnSelectionEvent;
        selectionManager.DeselectCountryEvent += OnSelectionEvent;
        
        SetButtonEnabledStatus();
    }

    void OnSelectionEvent(CountryData country) => SetButtonEnabledStatus(); 

    void SetButtonEnabledStatus()
    {
        submitButton.enabled = selectionManager != null && selectionManager.Selected.Count >= selectionManager.minSelectable &&
            selectionManager.Selected.Count <= selectionManager.maxSelectable;
    }

    public void Submit()
    {
        if(selectionManager != null)
        {
            selectionManager.SelectCountryEvent -= OnSelectionEvent;
            selectionManager.DeselectCountryEvent -= OnSelectionEvent;
            selectionManager.Complete();
            selectionManager = null;
        }
    }
}
