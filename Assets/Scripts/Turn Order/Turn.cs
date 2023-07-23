using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks; 

public class Turn : Phase
{
    [field: SerializeField] public Faction initiativeFaction { get; private set; }
    public GameState.WarEra Era { get; private set; }

}
