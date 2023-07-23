using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector; 

public abstract class Phase : SerializedMonoBehaviour
{
    [SerializeField] public List<IExecutableAction> PhaseStartActions = new(), 
        PhaseEndActions = new();
    public Phase nextPhase; 

    public System.Action PhaseStartEvent, PhaseEndEvent;

    private void Awake()
    {
        PhaseStartEvent += () => Game.PhaseStartEvent?.Invoke(this);
        PhaseEndEvent += () => Game.PhaseEndEvent?.Invoke(this);
    }

    public void StartPhase()
    { 
        PhaseStartEvent?.Invoke();
        
        foreach (IExecutableAction action in PhaseStartActions)
            action.Execute();

        OnPhase();
    }

    protected virtual void OnPhase() { } 

    public virtual void EndPhase()
    {
        foreach (IExecutableAction action in PhaseEndActions)
            action.Execute();

        PhaseEndEvent?.Invoke(); 
    }
}
