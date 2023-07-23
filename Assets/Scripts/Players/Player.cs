using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; 

public class Player : NetworkBehaviour
{
    public Player Opponent => Faction.Opponent.player; 
    [field: SerializeField] public Faction Faction { get; private set; }
    public static implicit operator Faction(Player player) => player.Faction;

    public List<Card> Hand => Game.current.gameState.Hand(this);

    private void Awake()
    {
        Faction.SetPlayer(this); 
    }
}
