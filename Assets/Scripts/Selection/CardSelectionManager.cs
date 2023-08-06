using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class CardSelectionManager
{
    public System.Action<Card> CardSelectEvent, CardDeselectEvent;
    public static System.Action<CardSelectionManager> StartCardSelectionEvent, CompleteCardSelectionEvent;

    public Faction faction { get; private set; }
    public List<Card> Selectable { get; private set; }
    public List<Card> Selected { get; private set; }
    public int minSelect { get; private set; }
    public int maxSelect { get; private set; }

    public bool CanSubmit => Selected.Count >= minSelect && Selected.Count <= maxSelect; 
    public TaskCompletionSource<CardSelectionManager> Task = new();

    public CardSelectionManager(Faction faction, IEnumerable<Card> selectable, System.Action<Card> OnSelect) : this(faction, selectable) =>
        CardSelectEvent += OnSelect; 

    public CardSelectionManager(Faction faction, IEnumerable<Card> selectable)
    {
        this.faction = faction;
        Selectable = new(selectable);
        Selected = new List<Card>();
        minSelect = 0;
        maxSelect = 1;

        StartCardSelectionEvent?.Invoke(this); 
    }

    public void Select(Card card)
    {
        if (Selectable.Contains(card) && !Selected.Contains(card) && Selected.Count < maxSelect)
        {
            Selected.Add(card);
            CardSelectEvent?.Invoke(card); 
        }
        else if(Selectable.Contains(card) && Selected.Contains(card))
        {
            Selected.Remove(card);
            CardDeselectEvent?.Invoke(card); 
        }
    }

    public void Complete()
    {
        if(Selected.Count >= minSelect && Selected.Count <= maxSelect)
            Task.SetResult(this);
    }
}
