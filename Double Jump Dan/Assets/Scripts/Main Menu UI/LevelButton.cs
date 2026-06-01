using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour 
{	
	[SerializeField] Sprite normalSprite;
	public LevelSelectMenu levelSelectMenu { get; set; }
    Button button;
	GameManager gameManager;
	int level;
	bool loadingLevel;

    void Start()
	{
		gameManager = GameManager.Instance;

		if(levelSelectMenu == null)
			Debug.LogError("Level Select Menu is null " + gameObject.name);
		else
			levelSelectMenu.OnLevelButtonsRefresh += Refresh;

		level = int.Parse(gameObject.name);

        button = GetComponent<Button>();

		Refresh();
	}

	void Refresh()
	{
		if(level <= gameManager.currentUser.levelsCompleted)
		{
			if(loadingLevel)
			{
				SpriteState state = button.spriteState;
				state.disabledSprite = normalSprite;	
				button.spriteState = state;
				button.interactable = false;
			}
			else
			{
				button.interactable = true;
			}
		}
		else
		{
			button.interactable = false;
		}
	}
	
    public void LoadLevel()
    {
		loadingLevel = true;
		levelSelectMenu.EnterLevel("Level " + level);
    }
}