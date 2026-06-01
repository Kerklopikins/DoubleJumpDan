using UnityEngine;

public class Spikes : MonoBehaviour
{
    float killThreshold = 0.5f;
    //private float debugLength = 2;
    void Awake()
    {
        
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        Vector2 hitDirection = (other.transform.position - transform.position).normalized;
        float dot = Vector2.Dot(hitDirection, transform.up);
    
        if(dot > killThreshold)
            KillPlayer(other.gameObject.GetComponent<Player>());
    }

    public void KillPlayer(Player player)
    {
        player.TakeDamage(player.health, 0, false, 0, 0, 0, transform, false, 0, 0);
    }

    //#if UNITY_EDITOR
    //void OnDrawGizmos()
    //{
        //float angle = Mathf.Acos(killThreshold);

        //Vector3 left = Quaternion.Euler(0, 0, angle) * transform.up;
        //Vector3 right = Quaternion.Euler(0, 0, -angle) * transform.up;

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, transform.position + left * debugLength);
        //Gizmos.DrawLine(transform.position, transform.position + right * debugLength);

        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(transform.position, transform.position + transform.up * debugLength);
    //}
    //#endif
}