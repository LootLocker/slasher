using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WorldText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lerpSpeed;

    // Set text and color of the text
    public void SetText(string text, Color color, float duration = 1f)
    {
        this.text.text = text;
        this.text.color = color;
        
        // Start the moveText coroutine
        StartCoroutine(MoveText(duration));
    }

    // Coroutine to move the text upwards and fade it out
    public IEnumerator MoveText(float duration = 1f)
    {
        float t = 0;
        Color color = text.color;
        Vector3 position = transform.position;
        Vector3 targetPosition = position + Vector3.up * 2f;
        while (t < duration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, t / duration);
            position.y = Mathf.Lerp(position.y, targetPosition.y, t / duration);
            text.color = color;
            transform.position = position;
            yield return null;
        }
        Destroy(gameObject);
    }
    
}
