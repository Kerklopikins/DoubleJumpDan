using UnityEngine;
using System.Collections;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] BoxCollider2D platformCollider;
    [SerializeField] float positionOffsetY;
    [SerializeField] float positionOffsetX;
    [SerializeField] float collisionDelay;
    [SerializeField] bool offsetAdjustmentMode;
    public float centerPointOffset;

    Player player;
    Collider2D playerCollider;
    bool falling;
    GameInputManager gameInputManager;

    void Start()
    {
        player = LevelManager.Instance.player;
        playerCollider = player.GetComponent<Collider2D>();
        gameInputManager = GameInputManager.Instance;
    }
    
    void Update()
    {
        float playerXPositionAbs = Mathf.Abs(transform.position.x + centerPointOffset - player.transform.position.x);

        if(playerXPositionAbs >= platformCollider.size.x * 0.5f + positionOffsetX && player.transform.position.y > transform.position.y + positionOffsetY)
        {
            Physics2D.IgnoreCollision(platformCollider, playerCollider, true);

            if(offsetAdjustmentMode)
                if(TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
                    renderer.color = Color.red;

            return;
        }

        if(player.transform.position.y < transform.position.y + positionOffsetY)
        {
            Physics2D.IgnoreCollision(platformCollider, playerCollider, true);
            
            if(offsetAdjustmentMode)
                if(TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
                    renderer.color = Color.green;
        }
        else
        {            
            if(player.fallButtonTimer <= 0 && gameInputManager.GetVerticalInput() < -gameInputManager.VerticalInputSensitivity && !falling)
            {
                StartCoroutine(FallCo());
                return;
            }

            if(!falling)
                Physics2D.IgnoreCollision(platformCollider, playerCollider, false);

            if(offsetAdjustmentMode)
                if(TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
                    renderer.color = Color.red;
        }
    }

    IEnumerator FallCo()
    {
        falling = true;
        Physics2D.IgnoreCollision(platformCollider, playerCollider, true);
        yield return new WaitForSeconds(collisionDelay);
        Physics2D.IgnoreCollision(platformCollider, playerCollider, false);
        falling = false;
    }

    void OnDrawGizmos()
    {
        if(!offsetAdjustmentMode)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + centerPointOffset, transform.position.y + positionOffsetY, 0), 0.2f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + platformCollider.size.x * 0.5f + positionOffsetX + centerPointOffset, transform.position.y, 0), 0.2f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - platformCollider.size.x * 0.5f - positionOffsetX + centerPointOffset, transform.position.y, 0), 0.2f);
    }
}