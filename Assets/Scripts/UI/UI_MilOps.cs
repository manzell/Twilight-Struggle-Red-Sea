using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector; 

public class UI_MilOps : SerializedMonoBehaviour
{
    [SerializeField] Dictionary<Faction, GameObject> MilOps;

    private void Awake()
    {
        Game.AdjustMilOpsEvent += UpdateMilOps; 
    }

    void UpdateMilOps(Faction faction, int j)
    {
        List<Image> images = new(MilOps[faction].GetComponentsInChildren<Image>());

        Debug.Log($"UI: Setting {faction.name} milOps to {Game.current.gameState.milOps[faction]}");

        for (int i = 0; i < images.Count; i++)
        {
            if (i < Game.current.gameState.milOps[faction])
                images[i].color = faction.PrimaryColor;
            else if (i < Game.current.gameState.defcon)
                images[i].color = Color.green;
            else
                images[i].color = Color.clear; 
        }
    }
}
