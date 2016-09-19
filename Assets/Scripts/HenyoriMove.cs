using UnityEngine;
using System.Collections;

public class HenyoriMove : MonoBehaviour, IPointGetter {

    private const float waitTime = 0.5f;
    private WaitForSeconds waitOnStart = new WaitForSeconds(waitTime);

    [SerializeField] float moveSpeed = 3;

    Vector3? IPointGetter.GetPoint()
    {
        return transform.position;
    }

    IEnumerator Start () {
        yield return waitOnStart;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(changeSpeedCor(-0.5f, 0.8f, true));
        yield return StartCoroutine(changeDirectionCor(-180, 1.5f, true));
        yield return StartCoroutine(changeDirectionCor(-120, 1, true));
        StartCoroutine(changeSpeedCor(2f, 1f, true));
        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(changeDirectionCor(80, 0.8f, true));
        yield return StartCoroutine(changeDirectionCor(-80, 0.8f, true));
    }

    void Update()
    {
        if (Time.time < waitTime) return;
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    IEnumerator changeSpeedCor(float dest, float term, bool relative)
    {
        if(relative)
        {
            dest += moveSpeed;
        }

        if (term <= 0)
        {
            moveSpeed = dest;
            yield break;
        }

        float startSpeed = moveSpeed;
        float startTime = Time.time;
        var waitForEndOfFrame = new WaitForEndOfFrame();

        while (startTime + term > Time.time)
        {
            float rate = (Time.time - startTime) / term;
            float curSpeed = startSpeed * (1 - rate) + dest * rate;
            moveSpeed = curSpeed;
            yield return waitForEndOfFrame;
        }

        moveSpeed = dest;
    }

    IEnumerator changeDirectionCor(float dest, float term, bool relative)
    {
        if(relative)
        {
            dest += transform.eulerAngles.z;
        }

        if(term <= 0)
        {
            setAngleZ(dest);
            yield break;
        }

        float startDir   = transform.eulerAngles.z;
        float startTime  = Time.time;
        var waitForEndOfFrame = new WaitForEndOfFrame();

        while (startTime + term > Time.time)
        {
            float rate = (Time.time - startTime) / term;
            float curDir = startDir * (1 - rate) + dest * rate;
            setAngleZ(curDir);
            yield return waitForEndOfFrame;
        }

        setAngleZ(dest);
    }

    void setAngleZ(float newAngle)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, newAngle);
    }
}
