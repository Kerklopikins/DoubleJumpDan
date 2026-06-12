using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] Transform laserSpawnPoint;
    [SerializeField] SpriteRenderer laserSprite;
    
    int collisionMask;
    RaycastHit2D hit;
    Ray2D ray;
    Player player;

    void Start()
    {
        collisionMask = (1 << LayerMask.NameToLayer("Collisions")) | (1 << LayerMask.NameToLayer("Enemies"));

        player = LevelManager.Instance.player;
        player.OnPlayerKilled += PlayerKilledOrLevelFinished;
        LevelManager.Instance.OnLevelFinished += PlayerKilledOrLevelFinished;
    }

    void Update()
    {
        if(!player.dead && !LevelManager.Instance.FinishedLevel())
        {
            ray = new Ray2D(laserSpawnPoint.position, GunDirection());
            hit = Physics2D.Raycast(ray.origin, ray.direction, 50, collisionMask);

            if(hit)
                laserSprite.size = new Vector2(hit.distance + 0.125f, laserSprite.size.y);
            else
                laserSprite.size = new Vector2(50 + 0.125f, laserSprite.size.y);
        }
        else
        {
            laserSprite.size = new Vector2(0, laserSprite.size.y);
        }
    }

    void PlayerKilledOrLevelFinished()
    {
        laserSprite.size = new Vector2(0, laserSprite.size.y);
    }
    
    Vector3 GunDirection()
    {
        if(transform.lossyScale.x < 0)
            return -laserSpawnPoint.right;
        else if(transform.lossyScale.x > 0)
            return laserSpawnPoint.right;
        else
            return Vector3.zero;
    }
}