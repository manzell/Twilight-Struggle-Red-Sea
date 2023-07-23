using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

public class ActionSelectionManager
{
    UI_ActionSelection selectionWindow;

    public List<IExecutableAction> AvailableActions { get; private set; }
    public Faction actingFaction { get; private set; }
    public TaskCompletionSource<IExecutableAction> selectionTask { get; private set; }
    public Card card { get; private set; }

    public ActionSelectionManager(Faction faction, IEnumerable<IExecutableAction> actions, UI_ActionSelection selectionWindow, UI_ActionSelectionReceiver selectionPrefab)
    {
        selectionTask = new();
        AvailableActions = new(actions);
        actingFaction = faction;
        this.selectionWindow = GameObject.Instantiate(selectionWindow, GameObject.FindObjectOfType<UI_Game>().transform);
        this.selectionWindow.Setup(this, selectionPrefab);
        this.selectionWindow.gameObject.SetActive(false);
    }

    public void Select(IExecutableAction action, Card card = null)
    {
        this.card = card; 
        action.Execute();
        selectionTask.SetResult(action);
    }
}