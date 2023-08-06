using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class ActionRound : Phase
{
    [SerializeField] List<GameAction> defaultActions;

    public Card playedCard { get; private set; }
    public IExecutableAction playedAction { get; private set; }
    public Faction phasingFaction { get; private set; }

    public void SetPhasingFaction(Faction faction) => phasingFaction = faction;
    public void SetActions(List<GameAction> actions) => defaultActions = actions;
    public void AddAction(GameAction action) => defaultActions.Add(action);

    ActionSelectionManager selection;

    protected override async void OnPhase()
    {
        List<IExecutableAction> triggeredActions = new(); 

        await new GameState.SetPhasingPlayer(phasingFaction).Execute();

        UI_Notification.SetNotification($"Play {phasingFaction.name} Action Round");

        selection = new(phasingFaction, defaultActions, null, AfterSelection);

        await selection.selectionTask.Task;

        UI_Notification.ClearNotification();

        async void AfterSelection(IExecutableAction action)
        {
            playedCard = selection.card; 

            triggeredActions.Add(action);
            selection.AvailableActions.Remove(action);
            selection.AvailableActions.Remove(defaultActions.OfType<SpaceRace>().FirstOrDefault());

            bool cardIsUnfriendly = selection.card.Faction == selection.faction.Opponent;
            bool cardEvented = !selection.AvailableActions.OfType<Event>().Any();
            bool isCardEvent = action is Event;

            if(!cardIsUnfriendly || action is SpaceRace)
            {
                EndPhase(); 
            }
            else if(!isCardEvent && !cardEvented)
            {
                Debug.Log($"Card was not Evented, so automatically eventing {selection.card.name}");                    
                await new Event(selection.faction, selection.card).Execute();

                EndPhase(); 
            }
            else
            {
                Card card = selection.card;

                UI_Game.SetPlayer(selection.faction);
                UI_Notification.SetNotification($"{selection.faction} choose use for Ops");

                selection = new(phasingFaction, selection.AvailableActions);
                selection.SetCard(card);
                selection.Open();
 
                await selection.selectionTask.Task;

                EndPhase(); 
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
