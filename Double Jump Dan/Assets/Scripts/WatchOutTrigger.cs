using UnityEngine;
using System.Collections;

public class WatchOutTrigger : MonoBehaviour
{
	[SerializeField] SpriteRenderer watchOutSquare;
    [SerializeField] AudioClip watchOutSound;

	SpriteRenderer lineSprite;
	Player player;
	bool canActivate = true;

	void Start()
	{
        lineSprite = GetComponent<SpriteRenderer>();
        player = LevelManager.Instance.player;

        lineSprite.size = new Vector2(0, lineSprite.size.y);
        watchOutSquare.transform.position = transform.position;
    }

	void FixedUpdate()
	{
		if(!canActivate)
		{
            Vector3 difference = player.transform.position - transform.position;
            difference.Normalize();

            float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            float correctedRotation = 0;

            if(transform.lossyScale.x > 0)
                correctedRotation = 180;
            else if(transform.lossyScale.x < 0)
                correctedRotation = 0;

            transform.rotation = Quaternion.Euler(0, 0, rotZ + correctedRotation);

            float distance = Vector2.Distance(transform.position, player.transform.position);
            lineSprite.size = new Vector2(distance, lineSprite.size.y);
            watchOutSquare.transform.position = player.transform.position;
            watchOutSquare.transform.rotation = Quaternion.identity;
        }
    }

	IEnumerator AnimateSquare()
	{
		canActivate = false;

        if(watchOutSound != null)
            AudioManager.Instance.PlaySound2D(watchOutSound);

        float inTime = 0;
        watchOutSquare.color = new Color(watchOutSquare.color.r, watchOutSquare.color.g, watchOutSquare.color.b, 0);
		watchOutSquare.transform.localScale = Vector3.one * 2;

        while(inTime < 0.4f)
		{
			watchOutSquare.color = new Color(watchOutSquare.color.r, watchOutSquare.color.g, watchOutSquare.color.b, Mathf.Lerp(0, 1, inTime / 0.3f));
			lineSprite.color = new Color(lineSprite.color.r, lineSprite.color.g, lineSprite.color.b, Mathf.Lerp(0, 1, inTime / 0.3f));
            watchOutSquare.transform.localScale = Vector3.Lerp(Vector3.one * 2, Vector3.one, inTime / 0.3f);
            inTime += Time.deltaTime;
			yield return null;
		}

		yield return new WaitForSeconds(0.3f);

        float outTime = 0;
        watchOutSquare.color = new Color(watchOutSquare.color.r, watchOutSquare.color.g, watchOutSquare.color.b, 0);
        watchOutSquare.transform.localScale = Vector3.one * 2;

        while(outTime < 0.4f)
        {
            watchOutSquare.color = new Color(watchOutSquare.color.r, watchOutSquare.color.g, watchOutSquare.color.b, Mathf.Lerp(1, 0, outTime / 0.3f));
            lineSprite.color = new Color(lineSprite.color.r, lineSprite.color.g, lineSprite.color.b, Mathf.Lerp(1, 0, outTime / 0.3f));
            watchOutSquare.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, outTime / 0.3f);
            outTime += Time.deltaTime;
            yield return null;
        }

		canActivate = true;
    }

	public void Activate()
	{
		if(canActivate)
			StartCoroutine(AnimateSquare());
	}
}