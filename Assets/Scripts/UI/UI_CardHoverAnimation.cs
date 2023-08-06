using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CardHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    UI_Card ui;

    private void Awake()
    {
        ui = GetComponent<UI_Card>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(2, 0.5f);
        Color color = ui.cardText.color;
        color.a = 1;
        ui.cardText.DOColor(color, .4f);
        ui.cardFlowText.DOColor(color, .4f);

        if (UI_Game.ActiveFaction.Hand.Contains(ui.Card))
        {
            ui.index = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, .5f);
        Color color = ui.cardText.color;
        color.a = 0;
        ui.cardText.DOColor(color, .6f);
        ui.cardFlowText.DOColor(color, .6f);

        if (UI_Game.ActiveFaction.Hand.Contains(ui.Card))
            transform.SetSiblingIndex(ui.index);
    }
}
