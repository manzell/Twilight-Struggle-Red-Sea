using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_TurnTrack : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI topLine, subLine;

    private void Awake()
    {
        Game.PhaseStartEvent += UpdateTurnTrack;
    }

    void UpdateTurnTrack(Phase phase)
    {
        if (phase is ActionRound ar)
        {
            topLine.text = ar.name.Substring(0, 3);
            topLine.color = ar.phasingFaction.PrimaryColor;
            subLine.text = Game.current.gameState.CurrentTurn.name;
        }
        if (phase is Turn turn)
        {
            topLine.color = Color.black;
            topLine.text = $"T{turn.name.Substring(5, 1)}";
            subLine.text = string.Empty;
        }
        if (phase is HeadlinePhase headlinePhase)
        {
            topLine.color = Color.black;
            topLine.text = $"T{Game.current.gameState.CurrentTurn.name.Substring(5, 1)}";
            subLine.text = "Headline Phase";
        }
    }
}
