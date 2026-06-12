using UnityEngine;

public class FakeGem: MonoBehaviour, IPoolable
{
    TransformProperties properties;

    public void OnObjectReuse(object data)
    {
        properties = (TransformProperties)data;
        transform.position = properties.position;
        transform.localScale = properties.scale;
        transform.rotation = properties.rotation;
    }
}