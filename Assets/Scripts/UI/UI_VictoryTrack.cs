using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class UI_VictoryTrack : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI vpTrack, label;
    [SerializeField] Image background;
    [SerializeField] Faction USA, USSR;

    private void Awake()
    {
        Game.AdjustVPEvent += (f, a) => UpdateVP();
        Game.StartGameEvent += UpdateVP; 
    }

    void UpdateVP()
    {
        int vp = Game.current.gameState.VictoryPoints;

        vpTrack.text = Mathf.Abs(vp).ToString();

        if (vp == 0)
        {
            background.color = Color.white;
            vpTrack.color = Color.black;
            label.text = "EVEN"; 
        }
        else
        {
            vpTrack.color = Color.white; 

            if (vp > 0)
            {
                background.color = USA.PrimaryColor;
                label.text = "USA Lead"; 
            }
            else
            {
                background.color = USSR.PrimaryColor;
                label.text = "USSR Lead";
            }
        }
    }
}