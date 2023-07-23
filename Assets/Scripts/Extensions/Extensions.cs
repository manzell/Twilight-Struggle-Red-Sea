using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public static class Extensions
{
    public static IEnumerable<Transform> children(this Transform t) => t.Cast<Transform>();
}
