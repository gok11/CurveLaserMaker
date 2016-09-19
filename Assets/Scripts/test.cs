using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("call");
    }
}
