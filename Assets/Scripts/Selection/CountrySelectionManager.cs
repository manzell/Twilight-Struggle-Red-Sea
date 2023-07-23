using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

[System.Serializable]
public class CountrySelectionManager
{
    public static System.Action<CountrySelectionManager> StartSelectEvent, CompleteSelectEvent;
    public System.Action<Country> AddSelectableEvent, RemoveSelectableEvent;
    public System.Action<Country> SelectCountryEvent, DeselectCountryEvent;

    public Task<CountrySelectionManager> task => Task.Task;
    public List<Country> Selectable { get; private set; } = new();
    public List<Country> Selected { get; private set; } = new();
    public Faction faction { get; private set; }

    public int minSelectable = 0;
    public int maxSelectable = 1;
    public int maxPerCountry = 99;
    public bool selectMultiple = true;

    System.Func<CountrySelectionManager, Task> completeFunc;
    TaskCompletionSource<CountrySelectionManager> Task = new();

    public CountrySelectionManager(Faction faction, IEnumerable<Country> eligibleCountries, IExecutableAction context,
        System.Action<Country> onSelect, System.Func<CountrySelectionManager, Task> onComplete, int min = 0, int max = 1, int maxPer = 99)
    {
        SelectCountryEvent += onSelect;
        DeselectCountryEvent += onSelect; 
        completeFunc = onComplete;
        minSelectable = min;
        maxSelectable = max;
        maxPerCountry = maxPer; 
        Setup(faction, eligibleCountries, context);
    }

    public CountrySelectionManager(Faction faction, IEnumerable<Country> eligibleCountries, IExecutableAction context) =>
        Setup(faction, eligibleCountries, context); 

    void Setup(Faction faction, IEnumerable<Country> eligibleCountries, IExecutableAction context)
    {
        this.faction = faction;

        StartSelectEvent?.Invoke(this);
        SetSelectable(eligibleCountries);

        // TODO: Convert this to Taskbar Notifaction
        Debug.Log($"Country Selection: {faction.name} Select from {string.Join(", ", eligibleCountries.Select(country => country.name))}");
    }

    public void TrySelect(Country country)
    {
        if (Selected.Contains(country) && (selectMultiple == false || Selected.Count >= maxSelectable || Selected.Count(s => s == country) >= maxPerCountry))
            Deselect(country); 
        else if ((maxPerCountry == 0 || Selected.Count(s => s == country) < maxPerCountry) && Selected.Count() < maxSelectable)
        {
            Selected.Add(country);
            SelectCountryEvent?.Invoke(country);
        }
    }

    public void Deselect(Country country)
    {
        if(Selected.Contains(country))
        {
            Selected.Remove(country);
            DeselectCountryEvent?.Invoke(country);
        }
    }

    public void SetSelectable(IEnumerable<Country> countries)
    {
        //Debug.Log($"SetSelectable ({Selectable.Count}): {string.Join(", ", countries.Select(country => country.name))}");

        IEnumerable<Country> toRemove = Selectable.Where(country => !countries.Contains(country));
        IEnumerable<Country> toAdd = countries.Where(country => !Selectable.Contains(country));

        if(toRemove.Count() > 0)
        {
            //Debug.Log($"Removing Selectable ({Selectable.Count}): {string.Join(", ", toRemove.Select(country => country.name))}");
            RemoveSelectable(toRemove);
        }
        if(toAdd.Count() > 0)
        {
            //Debug.Log($"Adding Selectable ({Selectable.Count}): {string.Join(", ", toAdd.Select(country => country.name))}");
            AddSelectable(toAdd);
        }

        //Debug.Log($"Selectables ({Selectable.Count}): {string.Join(", ", Selectable.Select(country => country.name))}");
    }

    public void ClearSelectable() => RemoveSelectable(Selectable.Union(Selected).ToArray());
    public void RemoveSelectable(IEnumerable<Country> countries) => countries.ToList().ForEach(country => RemoveSelectable(country)); 
    public void AddSelectable(IEnumerable<Country> countries) => countries.ToList().ForEach(country => AddSelectable(country)); 

    public void RemoveSelectable(Country country)
    {
        if(Selectable.Contains(country))
        {
            //Debug.Log($"Removing Selectable: {country.name}");
            Selectable.Remove(country);
            RemoveSelectableEvent?.Invoke(country);
        }
    }

    public void AddSelectable(Country country)
    {
        if(!Selectable.Contains(country))
        {
            Selectable.Add(country); 
            AddSelectableEvent?.Invoke(country);
        }
    }

    public async void Complete()
    {
        if(Selected.Count() >= minSelectable)
        {
            if(completeFunc != null)
                await completeFunc(this);

            ClearSelectable();
            CompleteSelectEvent?.Invoke(this); 
            Task.SetResult(this);
        }
    }
}