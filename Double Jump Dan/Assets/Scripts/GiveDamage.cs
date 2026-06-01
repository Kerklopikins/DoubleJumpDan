using UnityEngine;

public class GiveDamage : MonoBehaviour
{
    public GiveDamageTo giveDamageTo;
    public bool controlledExternally;
    public int damageToGive = 10;
    public bool instantKill;
	public bool givePlayerKnockBack = true;
    public bool rotationBasedKnockBack;
    public int knockBack = 15;
    public float rotationOffset;
    public int xKnockBack = 15;
    public int yKnockBack = 15;
    public float xKnockBackOffset = 1;
    public bool offsetAdjustmentMode;
    
    public enum GiveDamageTo { Player, Enemy }
    float playerInputDelay = 0.25f;
    public Player player { get; private set; }
    public bool hit { get; set; }

    void Start()
    {
        player = LevelManager.Instance.player;
    }
    void OnCollisionStay2D(Collision2D other)
    {
        if(giveDamageTo == GiveDamageTo.Enemy)
            return;

        if(controlledExternally)
            return;

        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if(!instantKill)
                GiveDamageToPlayer(damageToGive, playerInputDelay, givePlayerKnockBack, xKnockBack, yKnockBack, xKnockBackOffset, transform, rotationBasedKnockBack, knockBack, rotationOffset);
            else
                GiveDamageToPlayer(player.health, 0, false, 0, 0, 0, transform, false, 0, 0);
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if(giveDamageTo == GiveDamageTo.Enemy)
            return;

        if(controlledExternally)
            return;

        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if(!instantKill)
                GiveDamageToPlayer(damageToGive, playerInputDelay, givePlayerKnockBack, xKnockBack, yKnockBack, xKnockBackOffset, transform, rotationBasedKnockBack, knockBack, rotationOffset);
            else
                GiveDamageToPlayer(player.health, 0, false, 0, 0, 0, transform, false, 0, 0);
        }
    }

    public void GiveDamageToPlayer(int damage, float inputDelay, bool giveKnockBack, int knockBackX, int knockBackY, float xOffset, Transform otherTransform, bool _rotationBasedKnockBack, int _knockBack, float _rotationOffset)
    {
        hit = true;
        player.TakeDamage(damage, inputDelay, giveKnockBack, knockBackX, knockBackY, xOffset, otherTransform, _rotationBasedKnockBack, _knockBack, _rotationOffset);
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(offsetAdjustmentMode)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(transform.position.x - xKnockBackOffset, transform.position.y, 0), 0.125f);
            Gizmos.DrawWireSphere(new Vector3(transform.position.x + xKnockBackOffset, transform.position.y, 0), 0.125f);
        }
    }
    #endif
}