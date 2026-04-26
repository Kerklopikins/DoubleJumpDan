using UnityEngine;

public class RattlesnakeAI : MonoBehaviour
{
	float rattleRange = 15;
	float strikeRange = 7;
	float strikeTimer;
	float playerDistance;
	Player player;
	Animator animator;
	
	void Start()
	{
		player = LevelManager.Instance.player;
        animator = GetComponent<Animator>();
	}
	
	void Update()
	{
		if(player.dead)
		{
			strikeTimer = 0;
			return;
		}
		
		playerDistance = Vector3.Distance(transform.position, player.transform.position);

		if(player.transform.position.x > transform.position.x)
			transform.localScale = new Vector3(-1, 1, 1);
		else
			transform.localScale = new Vector3(1, 1, 1);

		if(playerDistance <= rattleRange)
		{
			//rattle.Play();
			animator.SetBool("Rattle", true);
		}
		else
		{
			//rattle.Stop();
			animator.SetBool("Rattle", false);
		}

		if(playerDistance <= strikeRange)
		{
			strikeTimer -= Time.deltaTime;

			if(strikeTimer <= 0)
			{
				animator.SetTrigger("Strike");
				strikeTimer = 1;
			}
		}
		else
		{
			strikeTimer = 0;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, strikeRange);
	}
}