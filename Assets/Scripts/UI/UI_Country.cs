using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using DG.Tweening; 

public class UI_Country : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum HighlightState { none, selectable, selected, hover, excluded }
    [field: SerializeField] public Country country { get; private set; }
    [SerializeField] TextMeshProUGUI countryName;
    [SerializeField] Image namePlate, mainBackground, altContinent1, altContinent2;
    [SerializeField] GameObject highlight;
    [SerializeField] TextMeshProUGUI stability;
    [SerializeField] Color selectableColor, selectedColor, cantSelectColor, excludedColor;
    [SerializeField] UI_InfluenceChip influencePrefab; 
    [SerializeField] Dictionary<Faction, GameObject> influencePanels; 

    CountrySelectionManager selectionManager; 

    private void Awake()
    {
        country.InfluenceChangeEvent += UpdateInfluence;
        Game.StartGameEvent += Style;
        CountrySelectionManager.CompleteSelectEvent += OnCompleteSelect; 
        CountrySelectionManager.StartSelectEvent += OnStartSelect;
    }

    [ContextMenu("Style")]
    public void Style()
    { 
        countryName.text = country.name;
        stability.text = country.Stability.ToString();

        if (country.Battleground)
        {
            countryName.color = Color.white; 
            namePlate.color = Color.red;
        }
        else if (country.Flashpoint)
        {
            countryName.color = Color.black; 
            namePlate.color = Color.yellow;
        }
        else
        {
            countryName.color = Color.black;
            namePlate.color = Color.white;
        }

        if (country.Continents.Count > 0)
        {
            mainBackground.color = country.Continents.First().Color;
            altContinent1.color = country.Continents.Last().Color;
            altContinent2.color = country.Continents.Last().Color;
        }

        foreach (Faction faction in Game.current.Players.Select(player => player.Faction)) 
            UpdateInfluence(faction, country.Influence[faction]); 
    }

    void OnStartSelect(CountrySelectionManager selectionManager)
    {
        this.selectionManager = selectionManager; 
        selectionManager.SelectCountryEvent += c => { if (c == country) SetHighlight(selectionManager); };
        selectionManager.DeselectCountryEvent += c => { if (c == country) SetHighlight(selectionManager); };
        selectionManager.AddSelectableEvent += c => { if (c == country) SetHighlight(selectionManager); };
        selectionManager.RemoveSelectableEvent += c => { if (c == country) SetHighlight(selectionManager); };
    }

    void OnCompleteSelect(CountrySelectionManager selectionManager) 
    {
        this.selectionManager = null;
        SetHighlight(null);
    }

    void UpdateInfluence(Faction faction, int amount)
    {
        UI_InfluenceChip previous = influencePanels[faction].transform.GetComponentInChildren<UI_InfluenceChip>(); 
        UI_InfluenceChip chip = Instantiate(influencePrefab, influencePanels[faction].transform);

        chip.influenceText.color = country.controllingFaction == faction ? Color.white : faction.PrimaryColor;
        chip.backgroundImage.color = country.controllingFaction == faction ? faction.PrimaryColor : Color.white;

        chip.influenceText.text = Mathf.Abs(amount).ToString();

        if (amount >= 0)
        {
            chip.transform.position = new Vector3(Screen.width / 2, -100, 0); 
            chip.transform.DOLocalMove(Vector3.zero, 1f).OnComplete(() => { 
                chip.influenceText.text = country.Influence[faction].ToString();

                if (previous != null)
                    Destroy(previous.gameObject); 
            });
        }
        else
        {
            // TODO Vector3.zero needs to be replaced with some other place on the board
            chip.transform.position = influencePanels[faction].transform.position;

            chip.transform.DOMove(new Vector3(Screen.width / 2, -100, 0), 1f).OnComplete(() => {
                Destroy(chip.gameObject);
            });

            if (previous != null)
            {
                previous.influenceText.text = country.Influence[faction].ToString();
                previous.backgroundImage.color = country.controllingFaction == faction ? faction.PrimaryColor : Color.white;
            }

        }
    }

    public void ClearHighlight() => highlight.GetComponent<Image>().color = Color.clear; 

    public void SetHighlight(CountrySelectionManager selectionManager)
    {
        Color color = Color.white; 
        Image hl = highlight.GetComponent<Image>();
        RectTransform rect = hl.GetComponent<RectTransform>();
        bool mouseOver = rect.rect.Contains(rect.InverseTransformPoint(Input.mousePosition));

        if(selectionManager != null && selectionManager.faction == UI_Game.ActiveFaction)
        {
            if (selectionManager.Selectable.Contains(country))
            {
                if ((selectionManager.maxPerCountry == 0 || selectionManager.Selected.Count(c => c == country) < selectionManager.maxPerCountry) && 
                    selectionManager.Selected.Count() < selectionManager.maxSelectable)
                    color = selectableColor;
                else if (selectionManager.Selected.Contains(country))
                    color = selectedColor;
                else
                    color = cantSelectColor; 
            }
            else
                color = selectionManager.Selected.Contains(country) ? selectedColor : excludedColor; 
        }
        else
            color.a = 0; 

        if (mouseOver)
            color.a = color.a + .5f;

        if (hl.color != color)
            hl.DOColor(color, 1/3f); 
    }

    public void OnPointerEnter(PointerEventData eventData) => SetHighlight(selectionManager);         
    public void OnPointerExit(PointerEventData eventData) => SetHighlight(selectionManager);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectionManager != null && selectionManager.faction == UI_Game.ActiveFaction)
            selectionManager.TrySelect(country);
    }
}