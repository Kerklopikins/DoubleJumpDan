using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour 
{	
	[SerializeField] LevelSelectMenu levelSelectMenu;
	[SerializeField] EventSystem eventSystem;

	ButtonEffects buttonEffects;
    Button button;
	GameManager gameManager;
	int level;

    void Start()
	{
		gameManager = GameManager.Instance;
		buttonEffects = GetComponent<ButtonEffects>();

		if(levelSelectMenu == null)
			Debug.LogError("Level Select Menu is null " + gameObject.name);
		else
			levelSelectMenu.OnLevelButtonsRefresh += Refresh;

		level = int.Parse(gameObject.name);

        button = GetComponent<Button>();

		Refresh();
	}

	void Update()
	{
		if(eventSystem.gameObject.activeSelf == false)
		{
			if(level <= gameManager.currentUser.levelsCompleted)
			{
				buttonEffects.DiscontinueInput(true);
				buttonEffects.SetNormal();
				button.enabled = false;
			}
		}
	}
	
	void Refresh()
	{
		if(level <= gameManager.currentUser.levelsCompleted)
            button.interactable = true;
		else
			button.interactable = false;
	}
	
    public void LoadLevel()
    {
		GameInputManager.Instance.RumbleController(0.5f, 0.5f, 0.75f);
		LevelLoadingManager.Instance.LoadScene("Level " + level);
    }
}