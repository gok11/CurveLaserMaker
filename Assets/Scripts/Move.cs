using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour, IPointGetter {

    float waitTime = 0.5f;

    float time;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sinSpeed = 10;
    [SerializeField] float vertSpeed = 5;

    Vector3? IPointGetter.GetPoint()
    {
        return transform.position;
    }

    void Update () {
        if (Time.time < waitTime) return;
        time += Time.deltaTime;

        var vec = Vector3.right * moveSpeed;
        vec.y = Mathf.Sin(time * sinSpeed) * vertSpeed;
        transform.position += vec * Time.deltaTime;
	}
}
