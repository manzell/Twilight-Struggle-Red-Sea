using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq; 

public class SpaceRace : GameAction, IOpsAction, ICardAction
{
    public Stat Ops { get; private set; }
    public Card Card { get; private set; }
    public SpaceStage spaceStage { get; private set; }
    public Roll roll { get; private set; }
    public int OpsUsed { get; private set; }

    public List<Modifier> Modifiers { get; }

    protected override Task Do()
    {
        Debug.Log($"Attempting Space Race with {Ops.Value(this)} Ops ({spaceStage.OpsRequired} needed)"); 

        if (Ops.Value(this) >= spaceStage.OpsRequired) // TODO: This check should not be needed because of a bool Can() function that limits Action Choice?
        {
            roll = new(ActingFaction); 

            if (roll <= spaceStage.MaxRoll)
                spaceStage.Complete(ActingFaction);

            Debug.Log($"{ActingFaction.name} attempts <b>{spaceStage.name}</b> with {Card.name}. {ActingFaction} rolls {roll} " +
                $"{(roll <= spaceStage.MaxRoll ? "and succeeds!" : "and the mission fails")}");
        }

        return Task.CompletedTask; 
    }

    public void SetCard(Card card)
    {
        Card = card; 
    }

    public SpaceRace(Faction faction)
    {
        SetActingFaction(faction);
        spaceStage = Game.current.gameState.SpaceRaceItems.FirstOrDefault(stage => !stage.factions.Contains(faction));
    }

    // TODO: Check for Space Race attempts in the current turn
    public bool Can(Faction faction) => spaceStage != null && Card.Ops.Value(this) >= spaceStage.OpsRequired;

    public void SetOps(Stat stat) => Ops = stat;
}