using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

[System.Serializable]
public class CountrySelectionManager
{
    // TODO make this non-static. 
    public static System.Action<CountrySelectionManager> StartSelectEvent, CompleteSelectEvent;
    public System.Action<CountryData> AddSelectableEvent, RemoveSelectableEvent;
    public System.Action<CountryData> SelectCountryEvent, DeselectCountryEvent;
    public System.Action<CountrySelectionManager> startSelectEvent, completeSelectEvent;

    public Task<CountrySelectionManager> task => Task.Task;
    public List<CountryData> Selectable { get; private set; } = new();
    public List<CountryData> Selected { get; private set; } = new();
    public Faction faction { get; private set; }

    public int minSelectable = 0;
    public int maxSelectable = 1;
    public int maxPerCountry = 99;
    public bool selectMultiple = true;

    TaskCompletionSource<CountrySelectionManager> Task = new();

    public CountrySelectionManager(Faction faction, IEnumerable<CountryData> eligibleCountries, IExecutableAction context,
        System.Action<CountryData> onSelect, System.Action<CountrySelectionManager> onComplete, int min = 0, int max = 1, int maxPer = 99)
    {
        SelectCountryEvent += onSelect;
        DeselectCountryEvent += onSelect;
        completeSelectEvent += onComplete; 
        minSelectable = min;
        maxSelectable = max;
        maxPerCountry = maxPer; 
        Setup(faction, eligibleCountries, context);
    }

    public CountrySelectionManager(Faction faction, IEnumerable<CountryData> eligibleCountries, IExecutableAction context) =>
        Setup(faction, eligibleCountries, context); 

    void Setup(Faction faction, IEnumerable<CountryData> eligibleCountries, IExecutableAction context)
    {
        this.faction = faction;

        StartSelectEvent?.Invoke(this);
        SetSelectable(eligibleCountries);
    }

    public void TrySelect(CountryData country)
    {
        if (Selected.Contains(country) && (selectMultiple == false || Selected.Count >= maxSelectable || Selected.Count(s => s == country) >= maxPerCountry))
            Deselect(country); 
        else if ((maxPerCountry == 0 || Selected.Count(s => s == country) < maxPerCountry) && Selected.Count() < maxSelectable)
        {
            Selected.Add(country);
            SelectCountryEvent?.Invoke(country);
        }
    }

    public void Deselect(CountryData country)
    {
        if(Selected.Contains(country))
        {
            Selected.Remove(country);
            DeselectCountryEvent?.Invoke(country);
        }
    }

    public void SetSelectable(IEnumerable<CountryData> countries)
    {
        IEnumerable<CountryData> toRemove = Selectable.Where(country => !countries.Contains(country));
        IEnumerable<CountryData> toAdd = countries.Where(country => !Selectable.Contains(country));

        if (toRemove.Count() > 0)
            RemoveSelectable(toRemove);
        if(toAdd.Count() > 0)
            AddSelectable(toAdd);
    }

    public void ClearSelectable() => RemoveSelectable(Selectable.Union(Selected).ToArray());
    public void RemoveSelectable(IEnumerable<CountryData> countries) => countries.ToList().ForEach(country => RemoveSelectable(country)); 
    public void AddSelectable(IEnumerable<CountryData> countries) => countries.ToList().ForEach(country => AddSelectable(country)); 

    public void RemoveSelectable(CountryData country)
    {
        if(Selectable.Remove(country))
            RemoveSelectableEvent?.Invoke(country);
    }

    public void AddSelectable(CountryData country)
    {
        if(!Selectable.Contains(country))
        {
            Selectable.Add(country); 
            AddSelectableEvent?.Invoke(country);
        }
    }

    public void Complete()
    {
        if(Selected.Count() >= minSelectable)
        {
            ClearSelectable();
            UI_Notification.ClearNotification();

            completeSelectEvent?.Invoke(this);
            CompleteSelectEvent?.Invoke(this);

            Task.SetResult(this);
        }
    }
}