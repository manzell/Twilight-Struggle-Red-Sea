using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DraggableHeader : MonoBehaviour, IDragHandler
{
    [SerializeField] Transform window;

    public void OnDrag(PointerEventData eventData) => window.position += (Vector3)eventData.delta;
}
