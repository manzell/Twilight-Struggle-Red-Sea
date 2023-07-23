using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UI_Defcon : MonoBehaviour
{
    [SerializeField] List<GameObject> defConLevels;

    private void Awake()
    {
        Game.AdjustDefconEvent += i => UpdateDefcon();
        Game.StartGameEvent += UpdateDefcon; 
    }

    public void UpdateDefcon()
    {
        for(int i = 0; i < 5; i++)
            defConLevels[i].SetActive(i + 1 == Game.current.gameState.defcon);
    }
}
