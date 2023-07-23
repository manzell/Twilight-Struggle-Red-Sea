using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggableCard : IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static System.Action<IDraggableCard> CardDragStartEvent, CardDragEndEvent, CardDropEvent;
    public Card Card { get; }
}
