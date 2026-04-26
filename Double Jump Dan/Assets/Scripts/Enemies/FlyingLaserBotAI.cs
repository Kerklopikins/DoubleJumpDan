using UnityEngine;

public class FlyingLaserBotAI : MonoBehaviour
{
	[SerializeField] float speed;
	[SerializeField] Transform firePoint;
	[SerializeField] float fireRate;
	[SerializeField] int laserDamage;
	[SerializeField] float laserSpeed;
	[SerializeField] float laserLifeTime;
	[SerializeField] bool useRaycastEndPoint;
	[SerializeField] float firePointRotationOffset;
	[SerializeField] Projectile laser;
	[SerializeField] float targetHeight;
	[SerializeField] LayerMask layerMask;
	[SerializeField] AudioClip shootSound;

	Player player;
	float _fireRate;
	bool on;
	float playerDistance = 40; 
	float _playerDistance; 
	WatchOutTrigger watchOutTrigger;
	bool triggeredWatchOut;

	void Start()
	{
		player = LevelManager.Instance.player;
        watchOutTrigger = GetComponentInChildren<WatchOutTrigger>();
	}

	void Update()
	{
		_fireRate -= Time.deltaTime;
		_playerDistance = Vector3.Distance(transform.position, player.transform.position);

		if(_playerDistance <= playerDistance)
			on = true;
		else
			on = false;

		if(!on)
		{
            triggeredWatchOut = false;
			return;
		}

		if(on && !triggeredWatchOut)
		{
            triggeredWatchOut = true;
			watchOutTrigger.Activate();
		}
	}

	void FixedUpdate()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50, layerMask);

		if(!on)
			return;

		transform.position = Vector3.Slerp(transform.position, new Vector3(player.transform.position.x, hit.point.y + targetHeight, 0), speed * Time.deltaTime);
		//transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, hit.point.y + targetHeight, 0), speed * Time.deltaTime);

		if(_fireRate <= 0)
		{
			AudioManager.Instance.PlaySound2D(shootSound);

			var _laser = (Projectile)Instantiate(laser, firePoint.position, firePoint.rotation);

			if(useRaycastEndPoint)
			{
				float shotDistance = 200;

				RaycastHit2D laserHit;
				Ray2D ray = new Ray2D(firePoint.position, firePoint.right);
				laserHit = Physics2D.Raycast(ray.origin, ray.direction, shotDistance, 1 << LayerMask.NameToLayer("Collisions"));
				Vector2 targetPoint;

				if(laserHit)
					targetPoint = laserHit.point;
				else
					targetPoint = ray.direction * shotDistance;

				_laser.SetEndPoint(targetPoint);
			}

			//_laser.giveDamage.damageToGive = laserDamage;
			//_laser.lifeTime = laserLifeTime;
			//_laser.useRayCast = useRaycastEndPoint;
			//_laser.speed = laserSpeed;

			_fireRate = fireRate;
		}

		Vector3 difference = player.transform.position - transform.position;
		difference.Normalize();

		float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
		firePoint.rotation = Quaternion.Euler(0, 0, rotZ + firePointRotationOffset);
	}
}