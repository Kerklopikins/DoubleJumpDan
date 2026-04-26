using UnityEngine;

public class ShadowDanTracer : MonoBehaviour
{
	[SerializeField] GameObject shadowDanTracer;
	[SerializeField] Transform playerArmOne;
	[SerializeField] Transform playerArmTwo;
	[SerializeField] Sprite idleSprite;
	[SerializeField] Sprite jumpingSprite;
	[SerializeField] float rate;
	[SerializeField] int tracersToSpawn;

	float _rate;
	Player player;

	void Start()
	{
		_rate = rate;
		player = GetComponent<Player>();
		PoolManager.Instance.CreatePool(shadowDanTracer.name, shadowDanTracer.gameObject, tracersToSpawn);
	}
	
	void Update()
	{
		if(player.dead)
			return;
		
		_rate -= Time.deltaTime;

		if(_rate <= 0)
		{
			ShadowDanProperties properties = new ShadowDanProperties();
			properties.position = transform.position;
			properties.scale = transform.localScale;
			properties.armOneRotation = playerArmOne.rotation;
			properties.armTwoRotation = playerArmTwo.rotation;
			
			if(!player.grounded)
				properties.sprite = jumpingSprite;
			else
				properties.sprite = idleSprite;

			PoolManager.Instance.ReuseObject(shadowDanTracer.name, properties);
			
			_rate = rate;
		}
	}
}