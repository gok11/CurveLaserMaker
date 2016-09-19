using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LRLaser : MonoBehaviour {

    [SerializeField] Vector2 start, end;

    public LineRenderer lr
    {
        get
        {
            return _lr ? _lr : _lr = GetComponent<LineRenderer>();
        }
    }
    private LineRenderer _lr;

    void Start()
    {
        lr.enabled = true;
    }


}
