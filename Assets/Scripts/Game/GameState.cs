using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Threading.Tasks; 

[System.Serializable]
public class GameState : SerializedScriptableObject
{
    public enum WarEra { EarlyWar, MidWar, LateWar, FinalScoring }
    [field: SerializeField] public List<Turn> Turns { get; private set; }

    [field: SerializeField] public Dictionary<AdjustInfluence.FactionCountry, int> influence;
    [field: SerializeField] public Dictionary<Faction, List<Card>> hands { get; private set; }
    [field: SerializeField] public List<Card> deck { get; private set; }
    [field: SerializeField] public Dictionary<Card, (Faction faction, bool faceup)> PermanentCards { get; private set; }
    [field: SerializeField] public HashSet<Card> discards { get; private set; }
    [field: SerializeField] public HashSet<Card> removed { get; private set; }

    [field: SerializeField] public HashSet<Effect> effects { get; private set; }

    public int VictoryPoints => vps[Game.current.Players.First().Faction] - vps[Game.current.Players.Last().Faction];
    [field: SerializeField] Dictionary<Faction, int> vps;
    
    [field: SerializeField] public Dictionary<Faction, int> milOps { get; private set; }
    [field: SerializeField] public int defcon { get; private set; } = 4;
    
    Stack<Phase> phases = new();
    [property: SerializeField] public Phase CurrentPhase => phases.FirstOrDefault();
    [property: SerializeField] public Turn CurrentTurn => phases.OfType<Turn>().First();
    [property: SerializeField] public ActionRound CurrentActionRound => phases.OfType<ActionRound>().First(); 
    [field: SerializeField] public Faction phasingFaction { get; private set; }    
    
    [field: SerializeField] public List<IExecutableAction> ExecutedActions { get; private set; }

    [field: SerializeField] public List<SpaceStage> SpaceRaceItems { get; private set; }

    public List<Card> Hand(Faction faction) => hands[faction];
    public HashSet<Country> Countries => new(influence.Keys.Select(fc => fc.country));

    public Dictionary<Faction, int> Influence(Country country) => influence.Where(fc => fc.Key.country == country).ToDictionary(fc => fc.Key.faction, fc => fc.Value);
    public Dictionary<Country, int> Influence(Faction faction) => influence.Where(fc => fc.Key.faction == faction).ToDictionary(fc => fc.Key.country, fc => fc.Value);

    public void RefreshInfluence()
    {
        Dictionary<AdjustInfluence.FactionCountry, int> _influence = new(); 

        foreach(KeyValuePair<AdjustInfluence.FactionCountry, int> kvp in influence)
            _influence.Add(new AdjustInfluence.FactionCountry(kvp.Key.faction, Instantiate(kvp.Key.country)), kvp.Value);

        influence = _influence; 
    }
    public void SetPlayerHand(Faction faction, IEnumerable<Card> hand) => hands[faction] = new(hand);

    [System.Serializable]
    public abstract class StateChange : IExecutableAction
    {
        public abstract void Do();
        public virtual void Undo() { }
        public List<Condition> Conditions = new();

        public Task Execute()
        {
            if (Conditions == null || Conditions.All(condition => condition.Can(this)))
                Do();

            return Task.CompletedTask; 
        }
    }

    public class SetPhasingPlayer : StateChange
    {
        Faction faction, previousFaction; 

        public SetPhasingPlayer(Faction faction)
        {
            this.faction = faction;
            UI_Game.SetPlayer(faction);
            previousFaction = Game.current.gameState.phasingFaction;
        }

        public override void Undo()
        {
            Game.current.gameState.phasingFaction = previousFaction;
        }

        public override void Do()
        {
            Debug.Log($"Phasing Player: {faction}");
            Game.current.gameState.phasingFaction = faction;
        }
    }

    public class ApplyOpsModifier : StateChange
    {
        [SerializeField] OpsModifier modifier;
        [SerializeField] Faction faction;

        public override void Do()
        {
            GameAction.PrepareAction += ApplyModifier;
            Game.TurnEndEvent += t => GameAction.PrepareAction -= ApplyModifier;
        }

        void ApplyModifier(IExecutableAction action)
        {
            if (action is IOpsAction opsAction && opsAction.ActingFaction == faction)
                opsAction.Ops.modifiers.Add(modifier);
        }

        public class OpsModifier : Modifier
        {
            public int minOps { get; private set; }

            public OpsModifier(int value, string source = null) : base(value, source)
            {
                minOps = 1; 
            }
        }
    }

    public class AddEffect : StateChange
    {
        [SerializeField] Effect effect; 
        public AddEffect(Effect effect) => this.effect = effect;

        public override void Do()
        {
            if(!Game.currentState.effects.Contains(effect))
            {
                Game.current.gameState.effects.Add(effect);
                Game.AddEffectEvent?.Invoke(effect);
                effect.OnApply();
            }
        }
    }

    public class CancelEffect : StateChange
    {
        Effect effect;
        public CancelEffect(Effect effect) => this.effect = effect;

        public override void Do()
        {
            if (Game.currentState.effects.Contains(effect))
            {
                Game.current.gameState.effects.Remove(effect);
                Game.CancelEffectEvent?.Invoke(effect);
            }
        }
    }
    
    public class AdjustMilOps : StateChange
    {
        [SerializeField] Faction faction;
        [SerializeField] int amount; 
        int previousMilOps;

        public AdjustMilOps(Faction faction, int amount)
        {
            this.faction = faction;
            this.amount = amount;
        }

        public override void Undo()
        {
            Game.current.gameState.milOps[faction] = previousMilOps;
        }

        public override void Do()
        {
            Debug.Log($"{faction} {(amount >= 0 ? "+" : string.Empty)}{amount} MilOps");

            previousMilOps = Game.current.gameState.milOps[faction];
            int destinationMilOps = Mathf.Clamp(previousMilOps + amount, 0, 5);


            if (destinationMilOps != previousMilOps)
            {
                Debug.Log($"Triggering Adjust MilOps Event for {destinationMilOps} {faction.name}");
                Game.current.gameState.milOps[faction] = destinationMilOps;
                Game.AdjustMilOpsEvent?.Invoke(faction, destinationMilOps - previousMilOps); 
            }
        }
    }

    public class AdjustVP : StateChange
    {
        [SerializeField] int amount;
        [SerializeField] Faction faction;

        public AdjustVP(Faction faction, int amount)
        {
            this.faction = faction;
            this.amount = amount; 
        }

        public override void Do()
        {
            Debug.Log(faction);
            Debug.Log(amount);
            Debug.Log(Game.currentState.vps[faction]); 
            Game.currentState.vps[faction] += amount; 

            if(amount != 0)
                Game.AdjustVPEvent?.Invoke(faction, amount);
        }
    }

    public class AdjustDEFCON : StateChange
    {
        int previousDEFCON;
        [SerializeField] int amount;

        public AdjustDEFCON(int amount)
        {
            this.amount = amount;
        }

        public override void Undo()
        {
            Game.current.gameState.defcon = previousDEFCON;
        }

        public override void Do()
        {
            previousDEFCON = Game.current.gameState.defcon;
            int defcon = Mathf.Clamp(Game.current.gameState.defcon + amount, 1, Game.current.MaxDEFCON);

            if (defcon - previousDEFCON != 0)
            {
                Debug.Log($"DEFCON {(defcon - previousDEFCON > 0 ? "Improves" : "Degrades")} to {defcon}"); 
                Game.current.gameState.defcon = defcon;
                Game.AdjustDefconEvent?.Invoke(defcon - previousDEFCON);
            }
        }
    }

    public class AdjustInfluence : StateChange
    {
        [SerializeField] Dictionary<FactionCountry, int> infChange; 
        Dictionary<FactionCountry, int> previousInfluence;

        public AdjustInfluence(FactionCountry factionCountry, int influenceAmount) =>
            infChange = new Dictionary<FactionCountry, int>() { {factionCountry, influenceAmount }}; 

        public AdjustInfluence(Faction faction, Country country, int amount) => 
            infChange = new Dictionary<FactionCountry, int>() { { new FactionCountry(faction, country), amount } };

        public override void Undo()
        {
            Game.current.gameState.influence = previousInfluence;
        }

        public override void Do()
        {
            previousInfluence = Game.current.gameState.influence;

            foreach (FactionCountry kvp in infChange.Keys)
            {
                int currentInfluence = Game.current.gameState.Influence(kvp.country)[kvp.faction];
                int newInfluence = Mathf.Max(currentInfluence + infChange[kvp], 0);
                
                if(currentInfluence != newInfluence)
                {
                    Debug.Log($"{(infChange[kvp] < 0 ? string.Empty : "+")}{infChange[kvp]} {kvp.faction.name} Influence in {kvp.country}");
                    Game.current.gameState.influence[kvp] = newInfluence;
                    kvp.country.InfluenceChangeEvent?.Invoke(kvp.faction, newInfluence - currentInfluence);
                }
            }
        }

        [System.Serializable]
        public struct FactionCountry : IComparer<Faction>
        {
            public Faction faction;
            public Country country;

            public FactionCountry(Faction faction, Country country)
            {
                this.faction = faction;
                this.country = country; 
            }

            public int Compare(Faction x, Faction y)
            {
                return 0; 
            }
        } 
    }

    public class ShuffleDrawDeck : StateChange
    {
        List<Card> previousDeck;

        public override void Do()
        {
            previousDeck = Game.current.gameState.deck;
            Game.current.gameState.deck = new(Game.current.gameState.deck.OrderBy(card => Random.value));

            Debug.Log($"Reshuffled Draw Deck ({Game.current.gameState.deck.Count()} Cards)");
        }

        public override void Undo()
        {
            Game.current.gameState.deck = previousDeck;
        }
    }

    public class StartPhase : StateChange
    {
        [SerializeField] Phase phase;
        Phase previousPhase; 

        public StartPhase(Phase phase)
        {
            this.phase = phase; 
        }

        public override void Do()
        {
            Debug.Log($"Starting Phase: {phase.name}");

            previousPhase = Game.current.gameState.CurrentPhase;
            Game.current.gameState.phases.Push(phase);
            phase.StartPhase();
        }
    }

    public class AddCardsToDeck : StateChange
    {
        [SerializeField] List<Card> addedCards = new(); 
        List<Card> previousDeck;

        public AddCardsToDeck(IEnumerable<Card> cards) => addedCards = new(cards); 

        public override void Do()
        {
            Debug.Log($"{addedCards.Count} {(addedCards.Count == 1 ? "card" : "cards")} added to Draw Deck");
            previousDeck = new(Game.currentState.deck); 

            foreach (Card card in addedCards)
            {
                if (!Game.currentState.PermanentCards.ContainsKey(card))
                {
                    Card _card = Instantiate(card);
                    Game.current.gameState.deck.Add(_card);

                    _card.name = card.name;
                }
            }

            new ShuffleDrawDeck().Execute();
        }

        public override void Undo()
        {
            Game.current.gameState.deck = previousDeck;
        }
    }

    public class DealUp : StateChange
    {
        public override void Do()
        {
            for(int i = 0; i < Game.current.HandSize; i++)
            {
                foreach (Player player in Game.current.Players)
                    if (player.Hand.Count < Game.current.HandSize)
                        new DealCard(player).Execute(); 
            }
        }
    }

    public class DrawCard : StateChange
    {
        public Card card { get; private set; }

        public override void Undo()
        {
            Game.current.gameState.deck.Insert(0, card);
        }

        public override void Do()
        {
            card = Game.currentState.deck.First();
            Game.currentState.deck.Remove(card); 
        }
    }

    public class DealCard : StateChange
    {
        [SerializeField] Faction faction;
        [SerializeField] Card card; 

        public DealCard(Faction faction) => this.faction = faction;
        public DealCard(Faction faction, Card card) : this(faction) => this.card = card; 

        public override void Do()
        {
            if (faction != null)
            {
                if (card == null)
                    card = Game.current.gameState.deck.First();
                else
                    card = Game.currentState.deck.FirstOrDefault(c => c.Name == card.Name) ?? Instantiate(card); 

                Debug.Log($"{card.Name} dealt to {faction.name}");

                Game.currentState.deck.Remove(card);
                Game.currentState.hands[faction].Add(card);

                faction.AddCardEvent?.Invoke(card);
                Game.DrawCardEvent?.Invoke(faction, card);
            }
        }

        public override void Undo()
        {
            if (card != null)
            {
                Game.current.gameState.deck.Insert(0, card);
                Game.current.gameState.hands[faction].Remove(card);
            }
        }
    }

    public class AddCardToHand : StateChange
    {
        [SerializeField] Faction faction;
        [field: SerializeField] public Card card { get; private set; }

        public AddCardToHand(Card card, Faction faction)
        {
            this.card = card;
            this.faction = faction; 
        }

        public override void Do()
        {
            if (!Game.currentState.hands[faction].Contains(card))
            {
                Game.current.gameState.hands[faction].Add(card);
                faction.AddCardEvent?.Invoke(card);
            }
        }
    }

    public class RemoveCardFromDeck : StateChange
    {
        public static System.Action<Card> RemoveCardFromDeckEvent; 

        Card card; 

        public RemoveCardFromDeck(Card card)
        {
            this.card = card; 
        }

        public override void Do()
        {
            Game.currentState.deck.Remove(card);
            RemoveCardFromDeckEvent?.Invoke(card); 
        }
    }

    public class RemoveCardFromHand : StateChange
    {
        [SerializeField] Faction faction;
        [field: SerializeField] public Card card { get; private set; }

        public RemoveCardFromHand(Faction faction, Card card)
        {
            this.faction = faction;
            this.card = card;
        }

        public RemoveCardFromHand(Faction faction)
        {
            this.faction = faction;
            this.card = Game.currentState.hands[faction].OrderBy(x => Random.value).FirstOrDefault(); 
        }

        public override void Do()
        {
            if (Game.current.gameState.hands[faction].Remove(card))
            {
                Debug.Log($"{card} removed from {faction} hand");
                faction.LoseCardEvent?.Invoke(card);
            }
        }

        public override void Undo() => Game.current.gameState.hands[faction].Add(card);
    }

    public class RemoveCardFromGame : StateChange
    {
        Card card;

        public RemoveCardFromGame(Card card)
        {
            this.card = card;
        }

        public override void Do()
        {
            if (card != null)
                Game.current.gameState.removed.Add(card);
        }

        public override void Undo()
        {
            if (card != null)
                Game.current.gameState.removed.Remove(card);
        }
    }

    public class RevealCard : StateChange
    {
        [SerializeField] Card card;
        [SerializeField] Faction faction;
        public RevealCard(Faction faction, Card card)
        {
            this.card = card;
            this.faction = faction;
        }

        public override void Do()
        {
            card.revealed = true;
            Game.RevealCardEvent?.Invoke(faction, card);
        }
    }

    public class Discard : StateChange
    {
        public Card card { get; private set; }

        public Discard(Card card) => this.card = card;

        public override void Do()
        {
            if (card != null)
                Game.current.gameState.discards.Add(card);
        }

        public override void Undo()
        {
            if (card != null)
                Game.current.gameState.discards.Remove(card);
        }
    }

    public class AchieveSpaceStage : StateChange
    {
        Faction faction; 
        SpaceStage stage;

        public AchieveSpaceStage(Faction faction, SpaceStage stage)
        {
            this.faction = faction;
            this.stage = stage; 
        }

        public override void Do()
        {
            if (!stage.factions.Contains(faction))
            {
                stage.factions.Add(faction);
                stage.Complete(faction);
            }
        }
    }

    public class SetPermanentCardStatus : StateChange
    {
        public static System.Action<Card, Faction, bool> OnSetPermanentCardStatus; 
        [SerializeField] Card card;
        [SerializeField] Faction faction; 
        [SerializeField] bool faceup;

        public SetPermanentCardStatus(Card card, Faction faction, bool faceup)
        {
            this.card = card;
            this.faction = faction;
            this.faceup = faceup; 
        }

        public override void Do()
        {
            if(Game.currentState.PermanentCards.ContainsKey(card))
            {
                Game.currentState.PermanentCards[card] = (faction, faceup); 
                OnSetPermanentCardStatus?.Invoke(card, faction, faceup);
            }
        }
    }

    public class InfluenceForControl : StateChange
    {
        [SerializeField] Faction faction;
        [SerializeField] Country country; 

        public InfluenceForControl(Faction faction, Country country)
        {
            this.faction = faction;
            this.country = country;
        }

        public override void Do()
        {
            int influenceToAdd = country.Influence[faction.Opponent] + country.Stability - country.Influence[faction];

            if (influenceToAdd > 0)
                new AdjustInfluence(faction, country, influenceToAdd);
        }
    }

    public class SetStability : StateChange
    {
        [SerializeField] Country country;
        [SerializeField] int stability; 

        public SetStability(Country country, int stability)
        {
            this.country = country;
            this.stability = stability;
        }

        public override void Do()
        {
            country.Stability = stability;
            country.UpdateCountryEvent?.Invoke();
        }
    }
}