using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

[System.Serializable]
public class Modifier
{
    public enum Type { Flat, Percent }
    public Type ModifierType;
    public string source; 
    [SerializeReference] public List<Condition> Conditions = new(); 
    [SerializeField] float value;

    public string Name;

    public Modifier(int value, string source = null)
    {
        this.value = value;
        this.source = source; 
    }

    public virtual float Value(IContext context) => Conditions.All(cond => cond.Can(context)) ? value : 0;
}