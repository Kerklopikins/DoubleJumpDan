using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    public enum BoundsBehaviour { Nothing, Constrain, Kill }

    public BoundsBehaviour above;
    public BoundsBehaviour below;
    public BoundsBehaviour left;
    public BoundsBehaviour right;

    Player player;
    BoxCollider2D bounds;
    BoxCollider2D _boxCollider2D;

    void Start()
    {
        player = GetComponent<Player>();
        bounds = LevelManager.Instance.levelBounds;
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if(player.dead)
            return;

        var colliderSize = new Vector2(_boxCollider2D.size.x * Mathf.Abs(transform.localScale.x), _boxCollider2D.size.y * Mathf.Abs(transform.localScale.y)) / 2;

        if(above != BoundsBehaviour.Nothing && transform.position.y + colliderSize.y > bounds.bounds.max.y)
            ApplyBoundsBehaviour(above, new Vector2(transform.position.x, bounds.bounds.max.y - colliderSize.y));

        if(below != BoundsBehaviour.Nothing && transform.position.y - colliderSize.y < bounds.bounds.min.y)
            ApplyBoundsBehaviour(below, new Vector2(transform.position.x, bounds.bounds.min.y + colliderSize.y));

        if(right != BoundsBehaviour.Nothing && transform.position.x + colliderSize.x > bounds.bounds.max.x)
            ApplyBoundsBehaviour(right, new Vector2(bounds.bounds.max.x - colliderSize.x, transform.position.y));

        if(left != BoundsBehaviour.Nothing && transform.position.x - colliderSize.x < bounds.bounds.min.x)
            ApplyBoundsBehaviour(left, new Vector2(bounds.bounds.min.x + colliderSize.x, transform.position.y));
    }

    void ApplyBoundsBehaviour(BoundsBehaviour behaviour, Vector2 constrainedPosition)
    {
        if(behaviour == BoundsBehaviour.Kill)
        {
            player.TakeDamage(player.health, 0, false, 0, 0, 0, transform, false, 0, 0);
            return;
        }

        transform.position = constrainedPosition;
    }
}