using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Linq; 

[CreateAssetMenu] 
public class Card : SerializedScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Faction Faction { get; private set; }
    [field: SerializeField] public GameState.WarEra Era { get; private set; }
    [field: SerializeField] public Stat Ops { get; private set; }
    [field: SerializeField] public Image CardImage { get; private set; }
    [field: SerializeReference] public List<Effect> Effects { get; private set; }
    [field: SerializeField] public bool RemovedEvent { get; private set; }
    [field: SerializeField, TextArea] public string CardText { get; private set; }
    [field: SerializeField] public string CardFlavor { get; private set; }

    [field: SerializeField] List<IExecutableAction> OnPlayActions;

    public bool revealed;     

    public virtual bool Can(Faction faction) => true;

    public async Task Execute(Faction faction)
    {
        if (Can(faction))
        {
            foreach (IExecutableAction action in OnPlayActions)
            {
                if (action is ICardAction cardAction)
                    cardAction.SetCard(this);
                if(action is IActingPlayerAction actingPlayerAction)
                    actingPlayerAction.SetActingFaction(Faction == null ? faction : Faction);

                await action.Execute();
            }
        }
        else
            Debug.Log($"Can't play {name}"); 
    }

    public void SetFaction(Faction faction) => Faction = faction; 
}