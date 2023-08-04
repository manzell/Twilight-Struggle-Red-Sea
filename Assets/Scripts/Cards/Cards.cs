using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace TwilightStruggle.Cards.RedSea
{ 
    public class ScoreContinent : CardAction 
    {
        [SerializeField] ContinentScore continentScoreCalc;
        [SerializeField] CountryData strategicSeaLanes; 

        protected override Task Do()
        {
            (Faction faction, int vps) score = continentScoreCalc.Value();

            if (strategicSeaLanes.controllingFaction == null)
            {
                if (strategicSeaLanes.Influence[score.faction] > strategicSeaLanes.Influence[score.faction.Opponent])
                    score.vps += 1;
                if (strategicSeaLanes.Influence[score.faction] < strategicSeaLanes.Influence[score.faction.Opponent])
                    score.vps -= 1;
            }

            if(score.vps < 0)
            {
                score.vps = Mathf.Abs(score.vps);
                score.faction = score.faction.Opponent; 
            }
            
            if (strategicSeaLanes.controllingFaction == score.faction)
                score.vps *= 2;

            return Task.CompletedTask; 
        }
    }

    public class AfghanistanInvasion
    {
        public class AfghanistanInvasionEffect : Effect
        {
            public override void OnApply()
            {
                new GameState.CancelEffect(Game.currentState.effects.FirstOrDefault(effect => effect is Directive30.PresidentialDirective30Effect)).Execute();
            }
        }
    }

    public class ApolloSoyuz : CardAction
    {
        protected override Task Do()
        {            
            SpaceStage spaceStage = new NextSpaceStage(ActingFaction).Value(); 

            if (spaceStage != null)
                new GameState.AchieveSpaceStage(ActingFaction, spaceStage).Execute();

            return Task.CompletedTask;
        }
    }

    public class ArabLeague : CardAction
    {
        [SerializeField] List<CountryData> requiredCountries;
        [SerializeField] int VPaward = 3, countriesControlRequirement = 3; 

        protected override Task Do()
        {
            if (requiredCountries.Count(country => country.controllingFaction == ActingFaction) >= countriesControlRequirement)
                new GameState.AdjustVP(ActingFaction, VPaward).Execute();

            return Task.CompletedTask;
        }
    }

    public class CarterDoctrine : CardAction
    {
        System.Action<Roll> doctrine;

        protected override Task Do()
        {
            doctrine = QueryCarterDoctrine; 
            Roll.PreRollEvent += doctrine;
            Game.TurnEndEvent += t => Roll.PreRollEvent -= doctrine;

            return Task.CompletedTask;
        }

        async void QueryCarterDoctrine(Roll roll)
        {
            if(roll.faction == Card.Faction)
            {
                // Prompt the player to use the Carter Doctrine
                ActionSelectionManager selection = new(roll.faction, 
                    new List<IExecutableAction>() { new SetRoll(roll, 1), new SetRoll(roll, 6), new Pass() });

                IExecutableAction action = await selection.selectionTask.Task; 

                if(action is SetRoll setRoll)
                {
                    await action.Execute();
                    Roll.PreRollEvent -= doctrine; 
                }
            }
        }

        public class SetRoll : IExecutableAction
        {
            Roll roll;
            int value;
            public SetRoll(Roll roll, int value)
            {
                this.roll = roll;
                this.value = value;
            }

            public Task Execute()
            {
                roll.SetResult(value);
                return Task.CompletedTask;
            }
        }

        public class Pass : GameAction
        {
            protected override Task Do() => Task.CompletedTask; 
        }
    }

    public class CENTCOM : CardAction
    {
        //Place the CENTCOM marker next to one country. The USSR may not make Coup attempts or Realignment rolls against that country.

        CountryData country; 
        protected override async Task Do()
        {
            // US Places the CENTCOM Marker
            UI_Notification.SetNotification("Select a Country to place the CENTOM Marker on");

            CountrySelectionManager selection = new(Card.Faction, Game.currentState.Countries, this,
                null, sel => new GameState.AddEffect(new CentcomToken(sel.Selected.First())).Execute());

            await selection.task; 
        }

        void FilterCentcomCountry(IExecutableAction action, IEnumerable<CountryData> targetList)
        {
            if(action is Coup || action is Realign && action is IActingPlayerAction acting && acting.ActingFaction == Card.Faction.Opponent)
            {
                targetList = targetList.Where(c => c != country); 
                Debug.Log($"Removing {country} from List? Try to target it!"); 
            }
        }

        public class CentcomToken : Effect
        {
            public CountryData country { get; private set; }
            public CentcomToken(CountryData country) => this.country = country;
        }
    }

    public class CyrusVance : CardAction
    {
        [SerializeField] int CardToDraw, VPaward;

        protected override async Task Do()
        {
            List<Card> cards = new(); 

            for(int i = 0; i < Mathf.Min(CardToDraw, Game.currentState.hands[ActingFaction.Opponent].Count); i++)
            {
                GameState.RemoveCardFromHand remove = new(ActingFaction.Opponent);
                await remove.Execute();
                cards.Add(remove.card);
            }

            if (cards.Count > 1)
            {
                ActionSelectionManager selection = new(ActingFaction, cards.Select(card => new GameState.Discard(card)));

                GameState.Discard selectedAction = (GameState.Discard)await selection.selectionTask.Task;
                await selectedAction.Execute();

                if(selectedAction.card.Faction == Card.Faction)
                    await new GameState.AdjustVP(Card.Faction, VPaward).Execute();

                cards.Remove(selectedAction.card);

                await new GameState.AddCardToHand(cards[0], ActingFaction.Opponent).Execute(); 
            }
            if(cards.Count == 1)
            {
                await new GameState.Discard(cards[0]).Execute();

                if (cards[0].Faction == Card.Faction)
                    await new GameState.AdjustVP(Card.Faction, VPaward).Execute(); 
            }
        }
    }

    public class DanielArapMoi : CardAction
    {
        [SerializeField] CountryData kenya;
        [SerializeField] int controlVPAward = 1, altAward = 2; 
        [SerializeField] UI_ActionSelectionReceiver selectionReceiver; 

        protected override async Task Do()
        {
            if(kenya.controllingFaction == Card.Faction)
            {
                await new GameState.AdjustVP(Card.Faction, controlVPAward).Execute(); 

                ActionSelectionManager selection = new ActionSelectionManager(Card.Faction, 
                    new List<IExecutableAction>() { new GameState.AdjustDEFCON(1), new GameState.AdjustDEFCON(-1) });

                IExecutableAction selectedAction = await selection.selectionTask.Task;
                await selectedAction.Execute(); 
            }
            else
                await new GameState.AdjustInfluence(ActingFaction, kenya, altAward).Execute();
        }
    }

    public class EPLF : CardAction
    {
        //If Ethiopia is USSR Controlled, gain 1 VP and add 1 Influence in Ethiopia and 1 adjacent country. 

        [SerializeField] CountryData ethiopia; 

        protected override async Task Do()
        {
            await new GameState.AdjustInfluence(ActingFaction, ethiopia, 1).Execute();

            CountrySelectionManager selection = new(ActingFaction, ethiopia.Neighbors, this, 
                null, async sel => await new GameState.AdjustInfluence(ActingFaction, sel.Selected.FirstOrDefault(), 1).Execute());
            
            await selection.task;
        }
    }

    public class F35Es : CardAction
    {
        [SerializeField] Continent africa;
        CountrySelectionManager selection;
        // The USSR may make 1 Realignment roll against each country in Africa. This Event prevents the "#RS3 Ogaden War" card from being played as an Event. 
        protected override async Task Do()
        {
            List<CountryData> africanCountries = new(Game.currentState.Countries.Where(country => country.Continents.Contains(africa) && country.Influence[ActingFaction.Opponent] > 0)); 

            await new GameState.AddEffect(new F35EsEffect()).Execute();

            selection = new(ActingFaction, africanCountries, this,
                country =>
                {
                    new Realign.Attempt(ActingFaction, country).Do();
                    selection.RemoveSelectable(country);
                }, null, 0, africanCountries.Count);

            selection.selectMultiple = false;
            await selection.task; 
        }

        public class F35EsEffect : Effect { }
    }

    public class Famine : CardAction
    {
        //Place Famine markers in two adjacent countries.Immediately make a Coup attempt in one of the countries with a Famine marker
        //using the Operations value of this card.Famine markers add a +1 modifier to Coup attempts in that country.
        //Remove marker after any successful Coup attempt.

        CountrySelectionManager selection;

        protected override async Task Do()
        {
            UI_Notification.SetNotification("Place Famine Markers in two adjacent countries");

            selection = new(ActingFaction, Game.currentState.Countries, this,
                SelectFamineCountry, FamineCoup, 2, 2, 1);

            await selection.task;
            UI_Notification.ClearNotification("Place Famine Markers in two adjacent countries"); 
        }

        void SelectFamineCountry(CountryData country)
        {
            if (selection.Selected.Count() == 0)
                selection.SetSelectable(Game.currentState.Countries);
            else if (selection.Selected.Count() == 1)
                selection.SetSelectable(country.Neighbors);
            else
                selection.ClearSelectable();
        }

        async Task FamineCoup(CountrySelectionManager sel)
        {
            foreach (CountryData country in sel.Selected)
                await new GameState.AddEffect(new FamineToken(country)).Execute();

            IEnumerable<CountryData> coupTargets = sel.Selected.Where(country => country.Influence[ActingFaction.Opponent] > 0);

            if (coupTargets.Count() > 0)
            {
                // TODO - Make the eligible targets obey basic rules AND require a famine marker. For now, just use the targets from the list
                Coup coup = new(ActingFaction);
                coup.SetTargets(coupTargets);
                coup.SetOps(Card.Ops);

                await coup.Execute();
            }
        }

        public class FamineToken : Effect
        {
            public static System.Action<FamineToken> OnFamineTokenAdd, OnFamineTokenApply, OnFamineTokenRemove; 
            public CountryData country { get; private set; }

            public FamineToken(CountryData country)
            {
                Coup.AfterCoupEvent += RemoveFamineToken;

                this.country = country;
                Name = $"Famine in {country.name}";

                Modifier famineMod = new(+1, "Famine Token");
                famineMod.Conditions.Add(new FamineCondition(country));
                Modifiers.Add(famineMod); 

                OnFamineTokenAdd?.Invoke(this); 

                Debug.Log($"Famine Token added to {country.name} (Modifiers: {Modifiers.Count})");
            }

            public class FamineCondition : Condition
            {
                CountryData country;
                public FamineCondition(CountryData country) => this.country = country;
                public override bool Can(IContext context) => context is Coup coup && coup.target == country;
            }

            async void RemoveFamineToken(Coup coup)
            {
                if (coup.target == country)
                {
                    Coup.AfterCoupEvent -= RemoveFamineToken;
                    await new GameState.CancelEffect(this).Execute(); 
                }
            }
        }
    }

    public class FrenchConnection : CardAction
    {
        //Remove all USSR Influence in Djibouti or Madagascar and add 2 US Influence in 1 of those countries. 
        [SerializeField] List<CountryData> targetCountries = new();
        [SerializeField] int influenceAward; 

        protected override async Task Do()
        {
            if(targetCountries.Any(country => country.Influence[Card.Faction.Opponent] > 0))
            {
                CountrySelectionManager selection = new(ActingFaction, targetCountries.Where(country => country.Influence[Card.Faction.Opponent] > 0), this,
                    async country => await new GameState.AdjustInfluence(ActingFaction.Opponent, country, -country.Influence[ActingFaction.Opponent]).Execute(), null);

                await selection.task;

                selection = new(ActingFaction, targetCountries.Where(country => country.Influence[Card.Faction.Opponent] > 0), this,
                    async country => await new GameState.AdjustInfluence(ActingFaction, country, influenceAward).Execute(), null);

                await selection.task; 
            }
        }
    }

    public class HeroOfTheCrossing : CardAction
    {
        //Discard 1 USSR associated card from the US hand. Retrieve 1 US associated or neutral card from the discard pile and add it to the US hand. 

        protected override async Task Do()
        {
            IEnumerable<Card> cards = ActingFaction.Hand.Where(card => card.Faction == ActingFaction.Opponent);
            CardSelectionManager selection = new(ActingFaction, cards);

            cards = (await selection.Task.Task).Selected;
            foreach (Card card in cards)
                await new GameState.Discard(card).Execute(); 

            selection = new(ActingFaction, Game.currentState.discards.Where(c => c.Faction != ActingFaction.Opponent));

            cards = (await selection.Task.Task).Selected;
            foreach (Card card in cards)
            {
                await new GameState.AddCardToHand(card, ActingFaction).Execute();
                await new GameState.RemoveCardFromDeck(card).Execute();
            }
        }
    }

    public class IndianOcean : CardAction
    {
        // Neutral: Score 2 VPs. All countries are treated as Battleground countries for the purposes of Coup attempts for the remainder of the turn. 
        [SerializeField] int VPaward;

        protected override Task Do()
        {
            new GameState.AdjustVP(ActingFaction, VPaward).Execute();

            GameAction.CompleteAction += DegradeNonBattleground;
            Game.TurnEndEvent += RemoveDegradeEffect;
            
            return Task.CompletedTask;
        }

        public void DegradeNonBattleground(IExecutableAction action)
        {
            if (action is Coup coup && coup.target.Battleground == false)
                new GameState.AdjustDEFCON(-1).Execute(); 
        }

        public void RemoveDegradeEffect(Turn t)
        {
            GameAction.CompleteAction -= DegradeNonBattleground;
            Game.TurnEndEvent -= RemoveDegradeEffect;
        }
    }

    public class IsraelPeriphery : CardAction
    {
        //The US Player may make 2 Realignment rolls in Africa with an additional +1 modifier. 
        [SerializeField] Continent africa;
        [SerializeField] Modifier modifier;
        [SerializeField] int realignAttempts; 

        protected override async Task Do()
        {
            Realign realign = new(ActingFaction);
            realign.Modifiers.Add(modifier);
            realign.SetOps(new(realignAttempts));
            realign.targetRules.Add(new CountriesInContinent(africa));
            await realign.Execute();
        }
    }

    public class KenyaJoinsRDF : CardAction
    {
        [SerializeField] CountryData kenya; 
        //Add 1 US Influence in Kenya and remove 1 USSR Influence in a non-USSR Controlled country. 

        protected override async Task Do()
        {
            await new GameState.AdjustInfluence(ActingFaction, kenya, 1).Execute(); 

            IEnumerable<CountryData> targets = Game.currentState.Countries.Where(country => country.Influence[ActingFaction.Opponent] > 0 && country.controllingFaction != ActingFaction.Opponent);
            CountrySelectionManager selection = new(ActingFaction, targets, this,
                async country => await new GameState.AdjustInfluence(ActingFaction.Opponent, country, -1).Execute(), null);

            await selection.task; 
        }
    }

    public class MarxistLeninistVanguards : CardAction
    {
        //The USSR may reallocate up to 3 Influence from 1 country to other countries, no more than 2 Influence per country.

        [SerializeField] int maxPerCountry = 2;
        [SerializeField] int maxRelocate = 3;
        CountrySelectionManager selection;
        
        protected override async Task Do()
        {
            int relocated = 0;

            // TODO - I could call RelocateTo from the OnComplete event on the Selection - but because I create a new selection and I'm getting confused, I'm doing in linearly.
            // Someday come back and figure out if there's a cleaner/better way. 
            selection = new(ActingFaction, Game.currentState.Countries.Where(country => country.Influence[ActingFaction] > 0), this,
                RelocateFrom, null, 0, maxRelocate);

            await selection.task;

            selection = new(ActingFaction, Game.currentState.Countries, this,
                RelocateTo, null, relocated, relocated);

            await selection.task; 

            async void RelocateFrom(CountryData country)
            {
                if (country.Influence[ActingFaction] > 0)
                {
                    relocated++;

                    if (country.Influence[ActingFaction] == 0)
                        selection.RemoveSelectable(country);
                    // TODO: Add these to the Selection Queue so when we reset the selection, we can reset any state changes that we executed as well. 
                    await new GameState.AdjustInfluence(ActingFaction, country, -1).Execute(); 
                }
            }

            async void RelocateTo(CountryData country)
            {
                if(relocated > 0)
                {
                    relocated--;

                    if (selection.Selected.Count(c => c == country) >= maxPerCountry)
                        selection.RemoveSelectable(country);

                    await new GameState.AdjustInfluence(ActingFaction, country, 1).Execute(); 
                }
            }
        }
    }

    public class MengistuHaileMariam : CardAction
    {
        //The USSR may immediately play any US associated card in their hand as if the event were associated with the USSR.
        //Reverse US and USSR on any pertinent event text. 
        protected override async Task Do()
        {
            CardSelectionManager selection = new(ActingFaction, ActingFaction.Hand.Where(card => card.Faction == ActingFaction.Opponent));
            Card card = (await selection.Task.Task).Selected.FirstOrDefault();

            if(card != null)
            {
                PlayCard play = new(ActingFaction, card);
                Faction previousFaction = card.Faction;

                card.SetFaction(ActingFaction);

                await play.Execute();

                card.SetFaction(previousFaction);
            }
        }
    }

    public class MrNyet : CardAction
    {
        CountrySelectionManager selection;
        //Degrade the DEFCON level by 1. Remove all US Influence in 2 countries where the USSR has Influence, no more than 1 country per Region. 
        protected override async Task Do()
        {
            new GameState.AdjustDEFCON(-1); 

            IEnumerable<CountryData> targets = Game.currentState.Countries.Where(country => country.Influence.Values.All(val => val > 0));
            selection = new(ActingFaction, targets, this, RemoveUSInfluence, null,
                Mathf.Min(2, Game.currentState.Countries.Where(country => country.Influence.Values.All(val => val > 0)).Count()), 2); 

            async void RemoveUSInfluence(CountryData country)
            {
                await new GameState.AdjustInfluence(ActingFaction.Opponent, country, -country.Influence[ActingFaction.Opponent]).Execute();
                selection.RemoveSelectable(targets.Where(c => c.Continents.Except(country.Continents).Count() > 0));

                if (selection.Selectable.Count() == 0)
                    selection.Complete(); 
            }

            await selection.task; 
        }
    }

    public class OAU : CardAction
    {
        //Discard an Event card associated with your opponent and roll a die.Roll 4-6, use the discarded card's Operations value this Action Round.
        //Roll 1-3, conduct no Operations this Action Round.

        [SerializeField] UI_ActionSelectionReceiver actionReceiverPrefab;

        protected override async Task Do()
        {
            Card discard = null;

            Roll roll = new(ActingFaction); 

            if(roll >= 4)
            {
                // Use the ops value to place, coup or realign
                Coup coup = new(ActingFaction);
                Realign realign = new(ActingFaction);
                Place place = new(ActingFaction); 
                ActionSelectionManager selection = new(ActingFaction, new List<GameAction>() { coup, realign, place, });

                coup.SetOps(discard.Ops);
                realign.SetOps(discard.Ops);
                place.SetOps(discard.Ops); 

                IExecutableAction selectedAction = await selection.selectionTask.Task;
                await selectedAction.Execute(); 
            }
        }
    }

    public class OgadenWar : CardAction
    {
        // Somalia invades Ethiopia.Roll a die and subtract(-1) for every USSR Controlled country adjacent to Ethiopia.
        // On a modified roll of 5-6, the US receives 2 VPs and replaces all USSR Influence in Ethiopia with US Influence.
        // The US adds 2 to its Military Operations. This Event canot be used after the "#RS29 F-5Es Delivered" Event has been played. 

        [SerializeField] CountryData ethiopia;
        [SerializeField] int vpAward, rollRequirement, milOpsAward; 

        protected override Task Do()
        {
            Roll roll = new(ActingFaction); 

            if(roll - ethiopia.Neighbors.Count(country => country.controllingFaction == ActingFaction.Opponent) >= rollRequirement)
            {
                new GameState.AdjustVP(ActingFaction, vpAward).Execute();
                new GameState.AdjustInfluence(ActingFaction, ethiopia, ethiopia.Influence[ActingFaction.Opponent]).Execute();
                new GameState.AdjustInfluence(ActingFaction.Opponent, ethiopia, -ethiopia.Influence[ActingFaction.Opponent]).Execute();
            }

            new GameState.AdjustMilOps(ActingFaction, milOpsAward).Execute();
            return Task.CompletedTask; 
        }
    }

    public class PeaceCorps : CardAction
    {
        //Remove 1 USSR Influence from a non-USSR Controlled country.Add 1 US Influence to 2 US Controlled countries.
        [SerializeField] int influenceToRemove, influenceToAdd; 

        protected override async Task Do()
        {
            IEnumerable<CountryData> targets = Game.currentState.Countries.Where(country => country.controllingFaction != ActingFaction.Opponent &&
                country.Influence[ActingFaction.Opponent] > 0);

            CountrySelectionManager removeSelection = new(ActingFaction, targets, this, 
                null, async selection => await new GameState.AdjustInfluence(ActingFaction.Opponent, selection.Selected.First(), -1).Execute(),
                influenceToRemove, influenceToRemove);

            await removeSelection.task;

            IEnumerable<CountryData> addInfluence = Game.currentState.Countries.Where(country => country.controllingFaction == ActingFaction); 

            CountrySelectionManager addSelection = new(ActingFaction, addInfluence, this,
                null, async selection => {
                    foreach(CountryData country in selection.Selected)
                        await new GameState.AdjustInfluence(ActingFaction, country, 1).Execute();
                }, 
                Mathf.Min(addInfluence.Count(), influenceToAdd), influenceToAdd);

            addSelection.maxPerCountry = 1; 
            addSelection.selectMultiple = false;

            await addSelection.task; 
        }
    }

    public class Directive30 : CardAction
    {
        // The USSR may add 1 Influence anywhere, and may also take an 8th Action Round this turn. 
        [SerializeField] ActionRound actionRoundPrefab; 

        protected override async Task Do()
        {
            CountrySelectionManager selection = new(ActingFaction, Game.currentState.Countries, this,
                async country => await new GameState.AdjustInfluence(ActingFaction, country, 1).Execute(), 
                AddExtraActionRound);

            await selection.task; 

            Task AddExtraActionRound(CountrySelectionManager csm)
            {
                ActionRound actionRound = GameObject.Instantiate(actionRoundPrefab, Game.currentState.CurrentTurn.transform);
                actionRound.SetPhasingFaction(ActingFaction);
                actionRound.AddAction(new ActionRound.Pass());

                return Task.CompletedTask; 
            }
        }

        public class PresidentialDirective30Effect : Effect
        {

        }
    }

    public class Separatists : CardAction
    {
        // Add sufficient Influence in 1 non-Battleground country to equal your opponent's Influence. 
        protected override async Task Do()
        {
            CountrySelectionManager Selection = new(ActingFaction, Game.currentState.Countries.Where(country => !country.Battleground && country.Influence[ActingFaction.Opponent] > 0), this,
                async country => await new GameState.AdjustInfluence(ActingFaction, country, country.Influence[ActingFaction.Opponent] - country.Influence[ActingFaction]).Execute(), null);

            await Selection.task; 
        }
    }

    public class Seychelles : CardAction
    {
        //Add 1 USSR Influence to Strategic Sea Lanes for every Middle Eastern country without US Influence.
        [SerializeField] Faction US;
        [SerializeField] Continent continent;
        [SerializeField] CountryData strategicSeaLanes; 

        protected override async Task Do()
        {
            await new GameState.AdjustInfluence(ActingFaction, strategicSeaLanes, 
                Game.currentState.Countries.Count(country => country.Continents.Contains(continent) && country.Influence[US] == 0)).Execute();
        }
    }

    public class SovietsAirliftCubans : CardAction
    {
        //Remove all US Influence from 1 country in Africa. 
        [SerializeField] Continent africa;

        protected override async Task Do()
        {
            IEnumerable<CountryData> targets = Game.currentState.Countries.Where(country => country.Continents.Contains(africa) && country.Influence[ActingFaction.Opponent] != 0);
            int minSelection = Mathf.Clamp(targets.Count(), 0, 1);

            CountrySelectionManager selection = new(ActingFaction, targets, this, null, RemoveUSInfluence, minSelection, 1); 
                
            await selection.task;

            async Task RemoveUSInfluence(CountrySelectionManager selection)
            {
                CountryData country = selection.Selected.First(); 
                await new GameState.AdjustInfluence(ActingFaction.Opponent, country, -country.Influence[ActingFaction.Opponent]).Execute();
            }
        }
    }

    public class Stagflation : CardAction
    {
        // The US must reveal any Scoring cards in the US hand. The USSR may then conduct Operations using this card's Operations value.
        // If the US revealed a Scoring card, 1 must be played on the next US Action Round. 
        [SerializeField] List<Card> scoringCards; 

        protected override async Task Do()
        {
            IEnumerable<Card> usHeldScoringCards = Game.currentState.hands[ActingFaction.Opponent].Intersect(scoringCards);

            if (usHeldScoringCards.Count() > 0)
            {
                ActionSelectionManager selection = new(ActingFaction,
                    new List<IExecutableAction> { new Coup(ActingFaction, Card), new Realign(ActingFaction), new Place(ActingFaction) });

                IExecutableAction action = await selection.selectionTask.Task;
                await action.Execute();
            }

            // There is an edge case where under USAID the Soviets can event this in T2 AR7 and US won't be forced to play their card... but if they ho
            foreach (Card card in Game.currentState.hands[ActingFaction.Opponent])
                if(scoringCards.Contains(card))
                    card.revealed = true; 
        }
    }
}