using UnityEngine;

public class PoolParticle : MonoBehaviour, IPoolable
{
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] float lifeTime;

    float _lifeTime;
    TransformProperties properties;
    
    void Update()
    {
        if(_lifeTime > 0)
            _lifeTime -= Time.deltaTime;
        else
            gameObject.SetActive(false);
    }

    public void OnObjectReuse(object data)
    {
        properties = (TransformProperties)data;
        transform.position = properties.position;
        transform.localScale = properties.scale;
        transform.rotation = properties.rotation;

        _lifeTime = lifeTime;
        _particleSystem.Play();
    }
}