using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class ActionRound : Phase
{
    public Faction phasingFaction { get; private set; }
    public IExecutableAction playedAction { get; private set; }
    [SerializeField] List<GameAction> defaultActions;
    [SerializeField] UI_ActionSelection actionSelectionPrefab;
    [SerializeField] UI_ActionSelectionReceiver actionReceiverPrefab;

    public void SetPhasingFaction(Faction faction) => phasingFaction = faction;
    public void SetActions(List<GameAction> actions) => defaultActions = actions;
    public void AddAction(GameAction action) => defaultActions.Add(action); 

    protected override async void OnPhase()
    {
        await new GameState.SetPhasingPlayer(phasingFaction).Execute();

        foreach(IActingPlayerAction action in defaultActions.OfType<IActingPlayerAction>())
            action.SetActingFaction(phasingFaction);

        string notification = $"Play {phasingFaction.name} Action Round"; 
        UI_Notification.SetNotification(notification);

        ActionSelectionManager selection = new(phasingFaction, defaultActions, actionSelectionPrefab, actionReceiverPrefab); 
        
        UI_Notification.ClearNotification(notification);

        playedAction = await selection.selectionTask.Task; 

        selection.AvailableActions.Remove(playedAction);
        selection.AvailableActions.Remove(defaultActions.OfType<SpaceRace>().FirstOrDefault());

        // Trigger Opponent's Event if it hasn't been already
        if (playedAction is not SpaceRace && selection.card.Faction == phasingFaction.Opponent)
        {
            PlayCard playCardEvent = selection.AvailableActions.OfType<PlayCard>().FirstOrDefault(); 

            // We HAVE NOT triggered the action
            if (playCardEvent != null)
            {
                Debug.Log("Triggering Opponent's Event");
                playCardEvent.SetCard(selection.card);
                await playCardEvent.Execute(); 
            }
            else // If we triggered the action already, the PlayCard action will be gone, meaning 
            {
                selection = new(phasingFaction, selection.AvailableActions, actionSelectionPrefab, actionReceiverPrefab);
                playedAction = await selection.selectionTask.Task;
            }
        }
    }

    public override void EndPhase()
    {
        if (nextPhase != null)
            new GameState.StartPhase(nextPhase).Execute();
        else
            GetComponentInParent<Phase>()?.EndPhase(); 
    }

    public class Pass : GameAction
    {
        protected override Task Do() => Task.CompletedTask; 
    }
}
