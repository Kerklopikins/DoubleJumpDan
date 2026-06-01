using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class UserButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] Text playtimeCounterText;
    public Text usernameText;
    [SerializeField] Shadow[] dropShadows;
    [SerializeField] AudioClip buttonClick;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite highlightedSprite;
    [SerializeField] Sprite disabledSprite;
    public User user { get; set; }
    public string userName { get; set; }
    public int colorIndex { get; set; }
    GameManager gameManager;
    UserMenu userMenu;
    Button button;
    bool isPointerOver;
    bool isPointerDown;
    Image buttonImage;
    MainMenuManager mainMenuManager;
    bool isCurrentUser;

	void Start() 
	{
        gameManager = GameManager.Instance;
        userMenu = GameObject.FindWithTag("Main Menu").GetComponent<UserMenu>();
        mainMenuManager = userMenu.GetComponent<MainMenuManager>();

        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        int transformIndex = transform.GetSiblingIndex();

        user = gameManager.users[transformIndex];
        userName = user.userName;
        colorIndex = user.userColorIndex;

        buttonImage.color = userMenu.userColors[colorIndex];

        SetTextColorAndShadow();

        usernameText.text = userName;
        playtimeCounterText.text = GetTotalPlaytimeString(user.totalPlaytime);

        gameObject.name = userName;
        
        Refresh();

        userMenu.OnUserButtonsRefresh += Refresh;
        userMenu.OnButtonsDisabled += ButtonDisable;

        mainMenuManager.initialUserButtons.Add(gameObject);
	}
    
    public void Remove()
    {
        userMenu.OnUserButtonsRefresh -= Refresh;
        userMenu.OnButtonsDisabled -= ButtonDisable;
        
        Destroy(gameObject);
    }
    
    public void Refresh()
    {        
        if(gameManager.currentUser.userID == user.userID)
        {
            userMenu.currentUserButton = this;
            userMenu.previousUserColorIndex = colorIndex;
            userMenu.colorSelectionIndex = colorIndex;
            
            playtimeCounterText.fontSize = 30;
            usernameText.fontSize = 30;

            button.interactable = false;
            isCurrentUser = true;
        }
        else
        {
            playtimeCounterText.fontSize = 25;
            usernameText.fontSize = 25;

            button.interactable = true;
            isCurrentUser = false;
        }
    }

    public void ButtonDisable(bool interactable)
    {
        if(gameManager.currentUser.userID != user.userID)
            button.interactable = interactable;
    }
    
	public void SelectUser() 
	{
        gameManager.SaveUserData();
        gameManager.currentUser = user;
        gameManager.LoadUserData();
        gameManager.SaveData();
        userMenu.RefreshUsers();
	}

    public void ChangeColor(int _colorIndex)
    {
        colorIndex = _colorIndex;
        buttonImage.color = userMenu.userColors[colorIndex];

        SetTextColorAndShadow();
    }

    void SetTextColorAndShadow()
    {
        if(colorIndex == 0)
            for(int i = 0; i < dropShadows.Length; i++)
                dropShadows[i].enabled = false;
        else
            for(int i = 0; i < dropShadows.Length; i++)
                dropShadows[i].enabled = true;

        playtimeCounterText.color = TextColor();
        usernameText.color = TextColor();
    }
    void Update()
    {
        if(isCurrentUser)
            playtimeCounterText.text = GetTotalPlaytimeString(gameManager.currentUser.totalPlaytime);

        if(!button.interactable)
        {
            SetDisabled();
            return;
        }

        if(isPointerDown && isPointerOver)
            SetPressed();
        else if(isPointerOver)
            SetHighlighted();
        else if(!isPointerOver && isPointerDown)
            SetPressed();
        else
            SetNormal();
    }

    string GetTotalPlaytimeString(double value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        string formatted = "";

        if(t.Days > 0)
            formatted += t.Days + "d ";
        
        if(t.Hours > 0)
            formatted += t.Hours + "h ";

        if(t.Minutes > 0)
            formatted += t.Minutes + "m ";
        
        formatted += t.Seconds + "s ";

        return formatted;
    }

    Color TextColor()
    {
        if(colorIndex == 0)
            return Color.black;
        else
            return Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(buttonClick != null && button.interactable)
            AudioManager.Instance.PlaySound2D(buttonClick);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {       
        isPointerDown = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    void SetNormal()
    {
        buttonImage.sprite = normalSprite;
        usernameText.color = TextColor();
        playtimeCounterText.color = TextColor();
    }

    void SetHighlighted()
    {
        buttonImage.sprite = highlightedSprite;
        usernameText.color = Color.white;
        playtimeCounterText.color = Color.white;
    }

    void SetPressed()
    {
        buttonImage.sprite = highlightedSprite;
        usernameText.color = Color.white;
        playtimeCounterText.color = Color.white;
    }

    void SetDisabled()
    {
        if(gameManager.currentUser.userID == user.userID)
        {
            buttonImage.sprite = highlightedSprite;
            usernameText.color = Color.white;
            playtimeCounterText.color = Color.white;
        }
        else
        {
            buttonImage.sprite = disabledSprite;
            usernameText.color = TextColor();
            playtimeCounterText.color = TextColor();
        }
    }
}