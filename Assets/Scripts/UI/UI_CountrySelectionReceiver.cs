using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CountrySelectionReceiver : MonoBehaviour
{
    CountrySelectionManager selectionManager;
    Faction faction;
    public Country country => GetComponent<UI_Country>().country; 

    public void Setup(CountrySelectionManager selectionManager) 
    {
        this.faction = selectionManager.faction;
        this.selectionManager = selectionManager;
        CountrySelectionManager.CompleteSelectEvent += csm => this.selectionManager = null; 
    }
}