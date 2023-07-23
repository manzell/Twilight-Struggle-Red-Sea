using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UI_Notification : MonoBehaviour
{
    [SerializeField] GameObject notificationBar;
    [SerializeField] TextMeshProUGUI notificationText;

    static UI_Notification instance;

    private void Awake()
    {
        instance = this;
        notificationBar.SetActive(false); 
    }
    public static void SetNotification(string note)
    {
        instance.notificationText.text = note;
        instance.notificationBar.SetActive(true); 
    }

    public static void ClearNotification(string note)
    {
        if(instance.notificationText.text == note)
            instance.notificationBar.SetActive(false); 
    }
}
