using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Faction : ScriptableObject
{
    [field: SerializeField] public string factionName { get; private set; }
    public System.Action<Card> AddCardEvent, LoseCardEvent;

    [field: SerializeField] public Image Icon { get; private set; }
    [field: SerializeField] public Color PrimaryColor { get; private set; }
    [field: SerializeField] public Color SecondaryColor { get; private set; }
    [field: SerializeField] public Faction Opponent { get; private set; }

    public Player player { get; private set; }
    public void SetPlayer(Player player) => this.player = player;

    public List<Card> Hand => Game.current.gameState.hands[this];
}
