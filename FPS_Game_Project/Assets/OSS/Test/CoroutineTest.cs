using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Coroutine coroutine = StartCoroutine(ScaleUp());
        
        coroutine = StartCoroutine(ScaleDown());
    }

    private IEnumerator ScaleUp()
    {
        while (true)
        {
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(currentScale.x * 1.2f, currentScale.y * 1.2f, currentScale.z * 1.2f);

            yield return null;
        }
    }
    
    private IEnumerator ScaleDown()
    {
        while (true)
        {
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(currentScale.x * 0.7f, currentScale.y * 0.7f, currentScale.z * 0.7f);

            yield return null;
        }
    }
}
