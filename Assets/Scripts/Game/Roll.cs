using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks; 

public class Roll
{
    public static System.Action<Roll> PreRollEvent, RollEvent;
    public Faction faction { get; private set; }
    int Result = 0;

    public Roll(Faction facion)
    {
        this.faction = facion;
        PreRollEvent?.Invoke(this); 

        if (Result == 0)
            Result = Random.Range(0, 6) + 1;

        RollEvent?.Invoke(this); 
    }

    public void SetResult(int i) => Result = Mathf.Clamp(i, 1, 6);

    public static implicit operator int(Roll roll) => roll.Result;
    public static implicit operator string(Roll roll) => roll.Result.ToString(); 
}
