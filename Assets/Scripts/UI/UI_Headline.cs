using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq; 

public class UI_Headline : MonoBehaviour
{
    [SerializeField] GameObject window; 

    private void Awake()
    {
        Game.PhaseStartEvent += OnStartHeadlinePhase; 
    }

    public void OnStartHeadlinePhase(Phase phase)
    {
        if (phase is HeadlinePhase headlinePhase)
        {
            headlinePhase.HeadlinesReadyEvent += d => Close(); 
            Open(headlinePhase);
        }
    }

    void Open(HeadlinePhase phase)
    {
        window.SetActive(true);
        foreach (HeadlineDropReceiver receiver in GetComponentsInChildren<HeadlineDropReceiver>())
            receiver.Setup(phase);
    }

    void Close()
    {
        window.SetActive(false); 
    }
}
