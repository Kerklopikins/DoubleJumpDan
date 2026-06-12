using UnityEngine;

public class Rainbow : MonoBehaviour, IPoolable
{
    [Header("Main Stuff")]
    [SerializeField] float maxDistance;
    [SerializeField] GiveDamage giveDamage;
    [SerializeField] SpriteRenderer rainbowSprite;
    [SerializeField] BoxCollider2D _collider;
    
    Vector3 targetPoint;
    float speed;
    ProjectileProperties projectileProperties;
    
    void FixedUpdate()
    {
        float correctedSpeed = Mathf.Abs(speed);

        transform.position = Vector2.MoveTowards(transform.position, targetPoint, correctedSpeed * Time.deltaTime);

        if(transform.position != targetPoint)
        {
            _collider.enabled = true;
            rainbowSprite.size += new Vector2(correctedSpeed * Time.deltaTime, 0);
        }
        else
        {
            _collider.enabled = false;

            rainbowSprite.size -= new Vector2(correctedSpeed * Time.deltaTime, 0);
            rainbowSprite.size = new Vector2(Mathf.Clamp(rainbowSprite.size.x, 0, 100), rainbowSprite.size.y);

            if(rainbowSprite.size.x <= 0)
                gameObject.SetActive(false);
        }
    }

    public void OnObjectReuse(object data)
    {
        giveDamage.hit = false;
        
        projectileProperties = (ProjectileProperties)data;

        speed = projectileProperties.speed;
        giveDamage.damageToGive = projectileProperties.damage;
        transform.position = projectileProperties.position;
        transform.rotation = projectileProperties.rotation;
        transform.localScale = projectileProperties.scale;
        targetPoint = transform.position + transform.right * transform.localScale.x * maxDistance;

        speed = Mathf.Abs(projectileProperties.speed);

        _collider.enabled = true;
        rainbowSprite.size = new Vector2(0, rainbowSprite.size.y);
    }
}