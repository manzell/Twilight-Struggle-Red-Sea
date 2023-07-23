using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq; 

[System.Serializable]
public abstract class Effect
{
    public string Name;

    public virtual void OnApply() { }
    public List<Modifier> Modifiers { get; protected set; } = new(); 
}

public class AfghanistanInvasion : Effect 
{
    public override void OnApply()
    {
        Effect directive = Game.currentState.effects.FirstOrDefault(effect => 
        effect is TwilightStruggle.Cards.RedSea.Directive30.PresidentialDirective30Effect directive);

        if (directive != null)
            new GameState.CancelEffect(directive).Execute();
    }
}

