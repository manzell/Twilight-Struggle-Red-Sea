using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class UI_Game : MonoBehaviour
{
    public static System.Action<Faction> SetActivePlayerEvent; 
    public static Faction ActiveFaction { get; private set; }

    private void Awake()
    {
        Player player = FindObjectOfType<Player>();
        Game.StartGameEvent += () => SetPlayer(player.Faction);
        Game.PhaseStartEvent += SetPlayer; 
    }

    void SetPlayer(Phase phase)
    {
        if (phase is ActionRound ar)
            SetPlayer(ar.phasingFaction); 
    }

    public static void SetPlayer(Faction faction)
    {
        ActiveFaction = faction;
        SetActivePlayerEvent?.Invoke(faction); 
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
            SetPlayer(Game.current.Players.First(player => player.Faction != ActiveFaction).Faction); 
    }

}

