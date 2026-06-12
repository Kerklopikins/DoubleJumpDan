using UnityEngine;

public class LightningGlow : MonoBehaviour, IPoolable
{
    [SerializeField] float glowFadeTime;
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float lifeTime;

    float glowFadeSpeed;
    float percent;
    float _lifeTime;
    TransformProperties properties;
    
    void Start()
    {
        glowFadeSpeed = 1 / glowFadeTime;
    }

    public void OnObjectReuse(object data)
    {
        properties = (TransformProperties)data;
        transform.position = properties.position;
        transform.localScale = properties.scale;
        transform.rotation = properties.rotation;

        _lifeTime = lifeTime;
        percent = 0;
        _particleSystem.Play();
    }

	void Update() 
	{
        if(_lifeTime > 0)
            _lifeTime -= Time.deltaTime;
        else
            gameObject.SetActive(false);

        if(percent < 1)
        {
            percent += Time.deltaTime * glowFadeSpeed;

            float alpha = Mathf.Lerp(1, 0, percent);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        }
	}
}