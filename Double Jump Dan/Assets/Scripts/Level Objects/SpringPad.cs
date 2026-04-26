using UnityEngine;
using System.Collections;

public class SpringPad : MonoBehaviour
{
    [SerializeField] AudioClip springSound;
    [SerializeField] Sprite[] springSprites;
    [SerializeField] int springForce;
    [SerializeField] float upAnimationSpeed;
    [SerializeField] float downAnimationSpeed;
    [SerializeField] float maxDetectionHeight;
    
    Player player;
    bool playerEntered;
    public float xDistance = 1.5f;
    SpriteRenderer spriteRenderer;
    bool canAnimate = true;

    void Start()
    {
        player = LevelManager.Instance.player;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float playerXPositionAbs = Mathf.Abs(transform.position.x - player.transform.position.x);

        if(playerXPositionAbs < xDistance && player.transform.position.y > transform.position.y)
        {
            if(player.transform.position.y > transform.position.y + maxDetectionHeight)
            {
                playerEntered = false;
                return;
            }
            
            if(!playerEntered)
            {
                AudioManager.Instance.PlaySound2D(springSound);

                if(canAnimate)
                    StartCoroutine(AnimateSpring());

                playerEntered = true;
            }     
        }
        else
        {
            playerEntered = false;
        }
    }   

    IEnumerator AnimateSpring()
    {
        canAnimate = false;
        player.CancelYVelocity();
        
        for(int i = springSprites.Length - 1; i > -1; i--)
        {
            spriteRenderer.sprite = springSprites[i];
            yield return new WaitForSeconds(downAnimationSpeed);
        }
        
        player.AddVelocity(Vector2.up, springForce);

        for(int i = 0; i < springSprites.Length; i++)
        {
            spriteRenderer.sprite = springSprites[i];
            yield return new WaitForSeconds(upAnimationSpeed);
        }


        canAnimate = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, 1, 0), Vector3.one * 2);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y, 0), Vector3.up + new Vector3(0, maxDetectionHeight, 0));

        Gizmos.DrawSphere(new Vector3(transform.position.x - xDistance, transform.position.y + 1, 0), 0.1f);
        Gizmos.DrawSphere(new Vector3(transform.position.x + xDistance, transform.position.y + 1, 0), 0.1f);
    }
}
