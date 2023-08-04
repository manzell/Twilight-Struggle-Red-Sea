using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 

public class UI_Notification : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI notificationText;

    static UI_Notification instance;

    private void Awake()
    {
        instance = this;
        UI_Game.SetActivePlayerEvent += SetNotificationColor;
    }

    void SetNotificationColor(Faction faction)
    {
        GetComponent<Image>().color = faction.PrimaryColor; 
    }

    public static void SetNotification(string note)
    {
        instance.notificationText.text = note;
        instance.transform.DOMoveY(instance.GetComponent<RectTransform>().sizeDelta.y, .5f); 
    }

    public static void ClearNotification() => instance.transform.DOMoveY(0, .5f);

    public static void ClearNotification(string note)
    {
        if(instance.notificationText.text == note)
            instance.transform.DOMoveY(0, .5f);
    }
}
