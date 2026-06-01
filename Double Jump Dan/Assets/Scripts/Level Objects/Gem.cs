using UnityEngine;

public class Gem : MonoBehaviour
{
    [SerializeField] int gemsToGivePlayer;
    [SerializeField] GameObject collectEffect;
    [SerializeField] AudioClip collectSound;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer gemShine;

    float magnetDuration = 0.75f;
    SpriteRenderer centralizedGem;
    bool collected;
    float inTime = 0;
    Vector3 startPosition;
    
	void Start()
	{
        centralizedGem = LevelManager.Instance.centralizedGem;
        spriteRenderer.sprite = LevelManager.Instance.doubleGems ? LevelManager.Instance.gemSprites[1] : LevelManager.Instance.gemSprites[0];
        
        startPosition = transform.position;
    }

	void Update()
	{
        gemShine.sprite = centralizedGem.sprite;

        if(collected)
        {
            gemShine.enabled = false;
            
            if(inTime < magnetDuration)
            {
                inTime += Time.deltaTime;
                float t = Mathf.Clamp01(inTime / magnetDuration);

                float easedT = t * t;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(1, 0, easedT));
                transform.localEulerAngles += Vector3.forward * 1000 * Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, StatsHUD.Instance.gemIcon.position, easedT);
            }
            else
            {
                LevelManager.Instance.AddGems(gemsToGivePlayer);
                Destroy(transform.parent.gameObject);
            }
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player" && !collected)
        {   
            AudioManager.Instance.PlayRandomSound2D(collectSound, 0.985f, 1, 0);
            collectEffect.SetActive(true);
            collected = true;
        }
    }
}