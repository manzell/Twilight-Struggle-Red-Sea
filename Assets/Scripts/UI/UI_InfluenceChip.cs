using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 

public class UI_InfluenceChip : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI influenceText;
    [SerializeField] public Image backgroundImage;
    public Faction faction { get; private set; }

    public void Setup(Faction faction)
    {
        this.faction = faction; 
    }

    public void Setup(Faction faction, CountryData country)
    {
        Setup(faction);
        Style(country); 
    }

    public void Style(CountryData country)
    {
        influenceText.text = country.Influence[faction].ToString();
        influenceText.DOColor(country.controllingFaction == faction ? Color.white : faction.PrimaryColor, .5f);
        backgroundImage.DOColor(country.controllingFaction == faction ? faction.PrimaryColor : Color.white, .5f);
    }
}
