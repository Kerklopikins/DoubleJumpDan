
using UnityEngine;
using UnityEngine.UI;

public class PerformanceTracker : MonoBehaviour
{
    Text text;
    float timer;
    int frames;
    float fps;
    float ms;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {   
        timer += Time.unscaledDeltaTime;
        frames++;

        if(timer >= 0.25f)
        {
            ms = Time.unscaledDeltaTime * 1000;
            fps = frames / timer;
            frames = 0;
            timer = 0;
        }
        
        text.text = Mathf.RoundToInt(fps) + " fps\n" + ms.ToString("0.00") + " ms";
    }
}