using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector; 

public class UI_PlayCard : SerializedMonoBehaviour
{
    [SerializeField] UI_Card cardPrefab;
    [SerializeField] Dictionary<Faction, Transform> cardPlayAreas;

    UI_Card activeCard; 

    private void Awake()
    {
        PlayCard.StartPlayCardEvent += PrepCard;
        PlayCard.FinishPlayCardEvent += RemoveCard;
    }

    void PrepCard(PlayCard play)
    {
        activeCard = Instantiate(cardPrefab, cardPlayAreas[play.ActingFaction]);
        activeCard.Setup(play.card);
        activeCard.Highlight(play.ActingFaction); 

        if (cardPlayAreas[play.ActingFaction].position.x < Screen.width / 2)
            activeCard.transform.SetLocalPositionAndRotation(new Vector3(-200, 0, 0), Quaternion.identity); 
        else
            activeCard.transform.SetLocalPositionAndRotation(new Vector3(200, 0, 0), Quaternion.identity);

        activeCard.transform.DOLocalMoveX(0, 1f); 
    }

    void RemoveCard(PlayCard play)
    {
        activeCard.transform.DOLocalMoveX(
            cardPlayAreas[play.ActingFaction].position.x < Screen.width / 2 ? -200 : 200, 1f).OnComplete(() => {
                Destroy(activeCard.gameObject);
                play.completion.SetResult(play); 
                }); 
    }
}
