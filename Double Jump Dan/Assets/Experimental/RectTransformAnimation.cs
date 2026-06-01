using UnityEngine;

public class RectTransformAnimation : MonoBehaviour
{
    [SerializeField] Vector3 startScale;
    [SerializeField] Vector3 endScale;
    [SerializeField] float animationSpeed;
    [SerializeField] float phaseOffset;

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();    
    }

    void Update()
    {
        rectTransform.localScale = Vector3.Lerp(startScale, endScale, Mathf.Sin(Time.time * animationSpeed + phaseOffset) * 0.5f + 0.5f);
    }
}