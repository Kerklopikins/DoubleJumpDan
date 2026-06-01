using UnityEngine;
using System.Collections;

public class EmojiMan : MonoBehaviour
{
	[SerializeField] Sprite[] emojis;
	[SerializeField] public float transformRate;

	Player player;
	SpriteRenderer spriteRenderer;
	float timer;
	int emojiIndex;
	float inTime;
	float outTime;
	float duration = 0.25f;
	float t;
	float smoothT;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		timer = transformRate;

		emojiIndex = Random.Range(0, emojis.Length);
		spriteRenderer.sprite = emojis[emojiIndex];

		player = LevelManager.Instance.player;
		player.OnPlayerKilled += OnPlayerKilled;
	}

	void OnPlayerKilled()
	{	
		timer = transformRate;

		transform.localScale = Vector3.one;
		transform.localEulerAngles = Vector3.zero;
	}
	
	void Update()
	{
		if(player.dead)
		{
			timer = transformRate;
			return;
		}

		if(timer > 0)
		{
			timer -= Time.deltaTime;
		}
		else
		{
			StartCoroutine(ChangeEmoji());
			
			emojiIndex++;

			if(emojiIndex > emojis.Length - 1)
				emojiIndex = 0;
			
			timer = transformRate;
		}
	}

	IEnumerator ChangeEmoji()
	{
		inTime = 0;

        while(inTime < duration)
        {
            inTime += Time.deltaTime;
            
            t = inTime / duration;
            smoothT = t * t * (3 - 2 * t);

            transform.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, smoothT);
			transform.localEulerAngles = Vector3.Lerp(Vector2.zero, new Vector3(0, 0, -180), smoothT);
            yield return null;
        }

		spriteRenderer.sprite = emojis[emojiIndex];

		outTime = 0;

        while(outTime < duration)
        {
            outTime += Time.deltaTime;
            
            t = outTime / duration;
            smoothT = t * t * (3 - 2 * t);

            transform.localScale = Vector2.Lerp(Vector2.zero, Vector2.one, smoothT);
			transform.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -180), new Vector3(0, 0, -360), smoothT);
            yield return null;
        }

		transform.localScale = Vector3.one;
		transform.localEulerAngles = Vector3.zero;
	}
}