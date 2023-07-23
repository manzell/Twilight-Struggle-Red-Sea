using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; 

public class UI_Coup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI header, coupTarget, defconImpact, coupOps, coupBonus, coupDefense, forControl, forPresence, toBreak, toClear;
    [SerializeField] TextMeshProUGUI coupStrengthText, influenceRemovedText, influenceAddedText, influenceLossTitleText, influenceGainTitleText;
    [SerializeField] Button coupButton, abortButton, okButton;
    [SerializeField] GameObject window, prepWindow, resultWindow; 
    Coup coup;

    private void Awake()
    {
        Coup.PrepareCoupEvent += Setup;
        Coup.AfterCoupEvent += Report; 
    }

    void Setup(Coup coup)
    {
        this.coup = coup;
        Country country = coup.target; 
        resultWindow.SetActive(false);
        prepWindow.SetActive(true);
        window.SetActive(true); 

        coupTarget.text = country.name;

        if(country.Battleground)
        {
            coupTarget.GetComponentInParent<Image>().color = new Color(.6f, 0f, .05f);
            coupTarget.color = Color.gray;

            if (Game.currentState.defcon > 2)
                defconImpact.text = $"Battleground - DEFCON Will Degrade to {Game.currentState.defcon - 1}";
            else
                defconImpact.text = $"Battleground - DEFCON Will Degrade and the game will end. {Game.currentState.phasingFaction.Opponent} will win the game";
        }
        else if (country.Flashpoint)
        {
            coupTarget.GetComponentInParent<Image>().color = new Color(1f, .75f, 0f);
            coupTarget.color = Color.gray; 

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

        coupOps.text = coup.Ops.Value(coup).ToString();
        coupBonus.text = coup.totalModifier.ToString();
        coupDefense.text = (coup.target.Stability * 2).ToString(); 

        // EG: Opponent has 2-0 lead on Country X, a 2 Stability country. A player needs 4 influence to control (

        int influenceNeededForControl = country.Influence[coup.ActingFaction.Opponent] + country.Stability - country.Influence[coup.ActingFaction];
        forControl.text = (influenceNeededForControl + country.Stability * 2 - (coup.Ops.Value(coup) + coup.totalModifier)).ToString();

        int opsForPresence = country.Stability * 2 + country.Influence[coup.ActingFaction.Opponent] - (coup.Ops.Value(coup) + coup.totalModifier) + 1;
        forPresence.text = opsForPresence.ToString();

        // Thailand (2 stab) with 5 Opponent Influence and 2 of your Influence. We need to remove 2 opponent influence, thus we need a total of 6 ops ; 

        int opsToBreak = country.Influence[coup.ActingFaction.Opponent] - country.Influence[coup.ActingFaction] - country.Stability + 1;

        toBreak.text = (opsToBreak + country.Stability * 2 - coup.Ops.Value(coup) + coup.totalModifier).ToString();

        toClear.text = (country.Influence[coup.ActingFaction.Opponent] + country.Stability * 2 - coup.Ops.Value(coup) - coup.totalModifier).ToString();   
    }

    void Report(Coup coup)
    {
        prepWindow.SetActive(false);
        resultWindow.SetActive(true);
        coupButton.gameObject.SetActive(false); 
        abortButton.gameObject.SetActive(false);
        okButton.gameObject.SetActive(true);
    }

    public void TriggerCoup()
    {
        coup.Selection.Complete();
    }

    public void AbortCoup()
    {
        coup.Selection.Deselect(coup.Selection.Selected.First()); 
        window.SetActive(false); 
    }
}
