using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Stat
{
    [SerializeField] int baseValue;

    public string name;
    public List<Modifier> modifiers = new();
    public Stat(int i) => baseValue = i;

    public int Value() => baseValue;
    public int Value(IExecutableAction context) => baseValue + ModAmount(context);

    public int ModAmount(IExecutableAction context)
    {
            int flatMod = 0;
            float pctMod = 0f;

            foreach (Modifier modifier in modifiers)
            {
                switch (modifier.ModifierType)
                {
                    case Modifier.Type.Flat:
                        flatMod += (int)modifier.Value(context);
                        break;
                    case Modifier.Type.Percent:
                        pctMod += modifier.Value(context);
                        break;
                }
            }
            return (int)(flatMod + (baseValue * pctMod));
    }

    public Stat(Stat oldStat)
    {
        baseValue = oldStat.baseValue;
        name = oldStat.name;
        modifiers = oldStat.modifiers; 
    }
}
