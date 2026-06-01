using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] AudioClip checkpointSound;
    [SerializeField] float beaconGrowSpeed;

    [SerializeField] SpriteRenderer beaconSprite;
    [SerializeField] Color inactiveColor;
    [SerializeField] Color activeColor;   

    [SerializeField] float flashSpeed;
    [SerializeField] float maxDetectionHeight;
    
    float beaconStartSize;
    Player player;
    bool playerEntered;
    bool raycasted;
    float maxBeaconHeight;
    float xDistance = 1.5f;
    
    void Start()
    {
        player = LevelManager.Instance.player;
        beaconStartSize = beaconSprite.size.y;
        maxBeaconHeight = beaconStartSize + 1;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * flashSpeed, 1);
        float alpha = Mathf.Lerp(0.5f, 1f, t);
        Color c = beaconSprite.color;
        c.a = alpha;
        beaconSprite.color = c;
        
        if(playerEntered)
        {
            if(beaconSprite.size.y >= maxBeaconHeight + 1)
                return;

            if(!raycasted)
            {
                RaycastHit2D hit = Physics2D.Raycast(beaconSprite.transform.position, Vector2.up, 100, 1 << LayerMask.NameToLayer("Collisions"));

                if(hit)
                    maxBeaconHeight = hit.distance;
                else
                    maxBeaconHeight = 100;

                raycasted = true;
            }
            
            beaconSprite.color = new Color(activeColor.r, activeColor.g, activeColor.b, beaconSprite.color.a);            
            beaconSprite.size += new Vector2(0, beaconGrowSpeed * Time.deltaTime);
            beaconSprite.size = new Vector2(beaconSprite.size.x, Mathf.Clamp(beaconSprite.size.y, beaconStartSize, maxBeaconHeight + 1));
        }
        else
        {
            beaconSprite.color = new Color(inactiveColor.r, inactiveColor.g, inactiveColor.b, beaconSprite.color.a);

            float playerXPositionAbs = Mathf.Abs(transform.position.x - player.transform.position.x);

            if(playerXPositionAbs < xDistance && player.transform.position.y > transform.position.y - 1)
            {
                if(player.transform.position.y > transform.position.y - 1 + maxDetectionHeight)
                    return;

                LevelManager.Instance.UpdateSpawnPoint(transform.position);
                AudioManager.Instance.PlaySound2D(checkpointSound);
                playerEntered = true;
            }
        }        
    }   

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y - 1, 0), Vector3.up + new Vector3(0, maxDetectionHeight - 1, 0));

        //Gizmos.DrawSphere(new Vector3(transform.position.x - xDistance, transform.position.y, 0), 0.1f);
        //Gizmos.DrawSphere(new Vector3(transform.position.x + xDistance, transform.position.y, 0), 0.1f);
    }
    #endif
}