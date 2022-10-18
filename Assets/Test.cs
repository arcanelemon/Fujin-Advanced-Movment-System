using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    float maxTime = 0.5f;
    float resetTime = 0.2f;
    float holdTime;

    bool decrement;
    IEnumerator yieldFrameCoroutine;

    private void Update()
    {
        if (decrement && holdTime > 0)
            holdTime -= Time.deltaTime;
    }

    void StartYieldFrameCoroutine() 
    {
        StopYieldFrameCoroutine();
        yieldFrameCoroutine = YieldFrame();
        StartCoroutine(yieldFrameCoroutine);
    }

    void StopYieldFrameCoroutine() 
    {
        if (yieldFrameCoroutine != null)
            StopCoroutine(yieldFrameCoroutine);
    }

    IEnumerator YieldFrame() 
    {
        decrement = false;
        float resetDuration = 0;
        while (resetDuration < resetTime)
        {
            yield return new WaitForFixedUpdate();
            resetDuration += Time.deltaTime;
        }
        decrement = true;
    }

    public void PrintName() 
    {
        Debug.Log(gameObject.name);
    }

    public void Interact() 
    {
        Debug.Log("Interact");
    }

    public float QueryHold() 
    {
        holdTime += Time.deltaTime;
        StartYieldFrameCoroutine();

        if (holdTime >= maxTime) 
        {
            holdTime = 0;
            return 1; 
        }

        return holdTime / maxTime;
    }
}
