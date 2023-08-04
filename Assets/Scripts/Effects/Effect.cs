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