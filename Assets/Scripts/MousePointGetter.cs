using UnityEngine;
using System.Collections;

public class MousePointGetter : MonoBehaviour, IPointGetter
{
    Vector3? IPointGetter.GetPoint()
    {
        if (!Input.GetMouseButton(0)) return null;
        
        var screenMousePos = Input.mousePosition;
        screenMousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(screenMousePos);
    }
}
