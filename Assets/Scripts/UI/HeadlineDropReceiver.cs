using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq; 

public class HeadlineDropReceiver : MonoBehaviour, IDropHandler
{
    [SerializeField] Faction faction;
    [SerializeField] TextMeshProUGUI headlineTitle;
    HeadlinePhase headlinePhase; 

    private void Awake()
    {
        UI_Game.SetActivePlayerEvent += Style;
    }

    public void Setup(HeadlinePhase headlinePhase)
    {
        this.headlinePhase = headlinePhase;
        Style(UI_Game.ActiveFaction);
    }

    public void Style(Faction activeFaction)
    {
        if(headlinePhase.HasSubmittedHeadline(activeFaction) && headlinePhase.HasSubmittedHeadline(faction))
            headlineTitle.text = $"{headlinePhase.headlines[faction].Name} ({headlinePhase.headlines[faction].Ops.Value()})";
        else if(headlinePhase.HasSubmittedHeadline(faction))
            headlineTitle.text = $"**{faction.name} Headline**";
        else
            headlineTitle.text = $"Awaiting {faction.name} Headline";
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.selectedObject.TryGetComponent(out IDraggableCard drag) && !drag.Card.Effects.Any(effect => effect is CannotHeadline) &&
            !headlinePhase.headlines.ContainsKey(faction) && UI_Game.ActiveFaction == faction)
        {
            headlinePhase.SubmitHeadline(faction, drag.Card);
            Style(UI_Game.ActiveFaction);

            Destroy(eventData.selectedObject);
        }
    }
}
