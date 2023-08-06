using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; 

public class UI_Coup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI header, coupTarget, defconImpact, coupOps, coupBonus, coupDefense, forControl, forPresence, toBreak, toClear;
    [SerializeField] TextMeshProUGUI coupRollText, coupStrengthText, influenceRemovedText, influenceAddedText, influenceLossTitleText, influenceGainTitleText;
    [SerializeField] Button coupButton, abortButton, okButton;
    [SerializeField] GameObject window, prepWindow, resultWindow; 
    Coup coup;

    private void Awake()
    {
        Coup.PrepareCoupEvent += Setup;
    }

    void Setup(Coup coup)
    {
        CountryData country = coup.Target;
        this.coup = coup;

        window.SetActive(true);
        prepWindow.SetActive(true);
        resultWindow.SetActive(false);

        int influenceNeededForControl = country.Influence[coup.ActingFaction.Opponent] + country.Stability - country.Influence[coup.ActingFaction];
        int opsToBreak = country.Influence[coup.ActingFaction.Opponent] - country.Influence[coup.ActingFaction] - country.Stability + 1;

        coupTarget.text = country.name;

        coupOps.text = coup.Ops.Value(coup).ToString();
        coupBonus.text = coup.totalModifier.ToString();
        coupDefense.text = (coup.Target.Stability * 2).ToString(); 
        coupRollText.text = coup.Roll;
        coupStrengthText.text = (coup.Ops.Value(coup) + coup.totalModifier + coup.Roll).ToString(); 

        influenceLossTitleText.text = $"{coup.ActingFaction.Opponent.name} influence removed:";
        influenceGainTitleText.text = $"{coup.ActingFaction.name} influence added:";
        influenceRemovedText.text = $"{Mathf.Min(coup.Target.Influence[coup.ActingFaction.Opponent], coup.coupStrength - coup.Target.Stability * 2)}";
        influenceAddedText.text = $"{Mathf.Max(0, coup.coupStrength - coup.Target.Stability * 2 - coup.Target.Influence[coup.ActingFaction.Opponent])}";

        forControl.text = (influenceNeededForControl + country.Stability * 2 - (coup.Ops.Value(coup) + coup.totalModifier)).ToString();
        forPresence.text = $"{country.Stability * 2 + country.Influence[coup.ActingFaction.Opponent] - (coup.Ops.Value(coup) + coup.totalModifier) + 1}";
        toBreak.text = (opsToBreak + country.Stability * 2 - coup.Ops.Value(coup) + coup.totalModifier).ToString();
        toClear.text = (country.Influence[coup.ActingFaction.Opponent] + country.Stability * 2 - coup.Ops.Value(coup) - coup.totalModifier).ToString();

        if (country.Battleground)
        {
            coupTarget.GetComponentInParent<Image>().color = new Color(.6f, 0f, .05f);

            if (Game.currentState.defcon > 2)
                defconImpact.text = $"Battleground - DEFCON Will Degrade to {Game.currentState.defcon - 1}";
            else
                defconImpact.text = $"Battleground - DEFCON Will Degrade and the game will end. {Game.currentState.phasingFaction.Opponent} will win the game";
        }
        else if (country.Flashpoint)
        {
            coupTarget.GetComponentInParent<Image>().color = new Color(1f, .75f, 0f);

            if (Game.currentState.defcon > 2)
                defconImpact.text = $"Flashpoint - DEFCON may degrade.";
            else
                defconImpact.text = $"Flashpoint - DEFCON may degrade and trigger nuclear war. {Game.currentState.phasingFaction.Opponent} will win the game";
        }
        else
        {
            coupTarget.GetComponentInParent<Image>().color = new Color(.75f, .75f, .75f);
            coupTarget.color = Color.black;

            defconImpact.text = string.Empty;
        }
    }

    public void TriggerCoup()
    {
        prepWindow.SetActive(false);
        coup.CompleteCoup();
        resultWindow.SetActive(true);
    }

    public void AbortCoup()
    {
        coup.Selection.Deselect(coup.Selection.Selected.First()); 
        window.SetActive(false); 
    }

    public void CompleteCoup()
    {
        coup.CompleteCoup();
        window.SetActive(false); 
    }
}
