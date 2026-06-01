using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectOpitimizer : MonoBehaviour 
{
	[SerializeField] List<Component> componentsToCheck = new List<Component>();
	[SerializeField] List<GameObject> gameObjectsToCheck = new List<GameObject>();
    [SerializeField] bool onlyCheckChild;

    Dictionary<Type, Action<Component, bool>> toggleActions;
	float checkFrequency = 0.5f;
    float checkDistance = 60;
	Player player;
	GameObject child;
	bool haveDelayed;
    float _checkFrequency;
    float safetyTimer = 1;
    Vector2 difference;

    void Awake()
    {
        toggleActions = new Dictionary<Type, Action<Component, bool>>()
        {
            { typeof(Behaviour), (component, state) => ((Behaviour)component).enabled = state },
            { typeof(Renderer), (component, state) => ((Renderer)component).enabled = state },
            { typeof(Collider2D), (component, state) => ((Collider2D)component).enabled = state },
            { typeof(ParticleSystem), (component, state) =>
                {
                    var particleSystem = (ParticleSystem)component;
                    
                    if(state)
                        particleSystem.Play();
                    else
                        particleSystem.Stop();
                }
            }
        };    
    }

    void Start()
	{
        player = LevelManager.Instance.player;
        player.OnPlayerRespawn += Refresh;
        player.OnPlayerTeleported += Refresh;

        if(onlyCheckChild)
		    child = transform.GetChild(0).gameObject;
        
        StartCoroutine(DelayStartCo());
    }

	IEnumerator DelayStartCo()
	{
        yield return new WaitForEndOfFrame();
        haveDelayed = true;
    }

    void ToggleComponent(Component component, bool state)
    {
        if(component == null)
            return;

        foreach(var kvp in toggleActions)
            if(kvp.Key.IsAssignableFrom(component.GetType()))
                kvp.Value.Invoke(component, state);
    }

    void Update()
	{
        if(safetyTimer > 0)
            safetyTimer -= Time.deltaTime;

        if(safetyTimer <= 0)
            if(!haveDelayed)
                haveDelayed = true;

		if(!haveDelayed)
			return;

		_checkFrequency -= Time.deltaTime;

		if(_checkFrequency <= 0)
		{
            Refresh();
            _checkFrequency = checkFrequency;
        }
	}
    public void UnsubscribeFromPlayer()
    {
        player.OnPlayerRespawn -= Refresh;
        player.OnPlayerTeleported -= Refresh;
    }

    void OnDestroy()
    {
        UnsubscribeFromPlayer();
    }

    void OnDisable()
    {
        UnsubscribeFromPlayer();
    }

    #if UNITY_EDITOR
    bool debugMode = false;

    void OnDrawGizmos()
    {
        if(debugMode)
            Gizmos.DrawWireSphere(transform.position, checkDistance);
    }
    #endif

    void Refresh()
    {
        if(onlyCheckChild && child == null)
        {
            UnsubscribeFromPlayer();
            Destroy(gameObject);
            return;
        }

        if(onlyCheckChild && child != null)
            difference = (Vector2)player.transform.position - (Vector2)child.transform.position;
        else
            difference = (Vector2)player.transform.position - (Vector2)transform.position;
        
        float sqrDistance = difference.sqrMagnitude;

        if(sqrDistance <= checkDistance * checkDistance)
        {
            if(onlyCheckChild)
                child.SetActive(true);

            if(componentsToCheck.Count > 0)
            {
                foreach(var component in componentsToCheck)
                    ToggleComponent(component, true);
            }

            if(gameObjectsToCheck.Count > 0)
            {
                foreach(var gmObject in gameObjectsToCheck)
                    gmObject.SetActive(true);
            }
        }
        else
        {
            if(onlyCheckChild)
                child.SetActive(false);

            if(componentsToCheck.Count > 0)
            {
                foreach(var component in componentsToCheck)
                    ToggleComponent(component, false);
            }

            if(gameObjectsToCheck.Count > 0)
            {
                foreach(var gmObject in gameObjectsToCheck)
                    gmObject.SetActive(false);
            }
        }
    }
}