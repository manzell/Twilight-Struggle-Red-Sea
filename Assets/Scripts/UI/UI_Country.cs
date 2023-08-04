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
    [field: SerializeField] public CountryData country { get; private set; }
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
        country.InfluenceChangeEvent = UpdateInfluence;
        Game.StartGameEvent += Setup;
        CountrySelectionManager.CompleteSelectEvent += OnCompleteSelect;
        CountrySelectionManager.StartSelectEvent += OnStartSelect;
    }

    public void Setup()
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

        foreach(Faction faction in influencePanels.Keys)
            if (country.Influence[faction] > 0)
                UpdateInfluence(faction, country.Influence[faction]); 
    }

    void UpdateInfluence(Faction faction, int amount)
    {
        foreach(Faction _faction in influencePanels.Keys)
        {
            UI_InfluenceChip currentChip = influencePanels[_faction].GetComponentInChildren<UI_InfluenceChip>();

            if (faction == _faction)
            {
                if (country.Influence[_faction] > 0)
                {
                    UI_InfluenceChip newChip = Instantiate(influencePrefab, influencePanels[_faction].transform);
                    newChip.Setup(_faction, country);

                    if (amount > 0)
                    {
                        newChip.transform.SetAsLastSibling();
                        newChip.transform.position = new Vector3(Screen.width / 2, -100, 0);
                        newChip.transform.DOLocalMove(Vector3.zero, 1f).OnComplete(() =>
                        {
                            if (currentChip != null)
                                Destroy(currentChip.gameObject);
                        });
                    }
                    else
                    {
                        newChip.transform.SetAsFirstSibling();
                        newChip.transform.position = Vector3.zero;
                    }
                }

                if(country.Influence[_faction] == 0 || amount < 0)
                    currentChip?.transform.DOMove(new Vector3(Screen.width / 2, -100, 0), .5f).OnComplete(() => Destroy(currentChip.gameObject));
            }

            currentChip?.Style(country); 
        }
    }

    void OnStartSelect(CountrySelectionManager selection)
    {
        selectionManager = selection;
        selection.AddSelectableEvent += c => { if (c == country) SetHighlight(selection); };
        selection.SelectCountryEvent += c => { if (c == country) SetHighlight(selection); };
        selection.DeselectCountryEvent += c => { if (c == country) SetHighlight(selection); };
        selection.RemoveSelectableEvent += c => { if (c == country) SetHighlight(selection); };

    }
    void OnCompleteSelect(CountrySelectionManager selection)
    {
        selectionManager = null;
        ClearHighlight();
    }

    public void ClearHighlight() => highlight.GetComponent<Image>().color = Color.clear;
    public void SetHighlight(CountrySelectionManager selection)
    {
        Color color = Color.white;
        Image hl = highlight.GetComponent<Image>();
        RectTransform rect = highlight.GetComponent<RectTransform>();

        if (selection.faction == UI_Game.ActiveFaction)
        {
            if (selection.Selectable.Contains(country))
            {
                if ((selection.maxPerCountry == 0 || selection.Selected.Count(c => c == country) < selection.maxPerCountry) && selection.Selected.Count() < selection.maxSelectable)
                    color = selectableColor;
                else if (selection.Selected.Contains(country))
                    color = selectedColor;
                else
                    color = cantSelectColor;
            }
            else
                color = selection.Selected.Contains(country) ? selectedColor : excludedColor;
        }
        else
            color.a = 0;

        if (rect.rect.Contains(rect.InverseTransformPoint(Input.mousePosition)))
            color.a = color.a + .5f;

        if (hl.color != color)
            hl.DOColor(color, 1 / 3f);
    }

    public void OnPointerEnter(PointerEventData eventData) { if (selectionManager != null) SetHighlight(selectionManager); }         
    public void OnPointerExit(PointerEventData eventData)  { if (selectionManager != null) SetHighlight(selectionManager); }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectionManager != null && selectionManager.faction == UI_Game.ActiveFaction)
            selectionManager.TrySelect(country);
    }
}