using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UserMenu : MonoBehaviour 
{
    [SerializeField] Button userButton;
    [SerializeField] Transform usersParent;
    [SerializeField] InputField userNameInputField;
    [SerializeField] Button confirmButton;
    [SerializeField] Button cancelButton;
    [SerializeField] GameObject oneToTwentyCharsText;
    [SerializeField] Button newButton;
    [SerializeField] Button deleteButton;
    [SerializeField] Image changeColorButton;
    [SerializeField] Button[] buttons;
    [SerializeField] Image changeUserButton;
    [SerializeField] Text changeUserText;
    [SerializeField] AudioClip buttonClick;
    [SerializeField] Text deleteUserPanelTitleText;
    [SerializeField] Text deleteUserText;
    [SerializeField] Text deleteUserButtonText;
    [SerializeField] Animator createNewUser;
    [SerializeField] InputField createNewUserInputField;
    [SerializeField] Button createNewUserButton;
    [SerializeField] Animator mainMenu;
    public Color[] userColors;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] ExitUIArea exitUIArea;

    public UserButton currentUserButton { get; set; }
    public UIScreenManager uiScreenManager { get; set; }
    public int previousUserColorIndex { get; set; }
    public int colorSelectionIndex { get; set; }
    GameManager gameManager;
	string previousUserName;
    bool createdNewUser;
    public event Action OnUserByteSizeRefresh;
    public event Action OnUserButtonsRefresh;
    public event Action<bool> OnButtonsDisabled;
    EditState editState;
    public enum EditState { Normal, New, Rename, Color };
    Text changeColorText;
    ButtonEffects changeColorButtonEffects;
    Shadow changeColorTextShadow;
    ButtonEffects changeUserButtonEffects;
    Shadow changeUserTextShadow;
    List<string> currentUserNames = new List<string>();
    GameInputManager gameInputManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        uiScreenManager = GetComponent<UIScreenManager>();
        gameInputManager = GameInputManager.Instance;

        if(gameManager.users.Count == 6)
            newButton.interactable = false;
        else
            newButton.interactable = true;

        userNameInputField.interactable = false;
        
        for(int i = 0; i < gameManager.users.Count; i++)
        {
            var _userToggle = (Button)Instantiate(userButton, Vector3.zero, Quaternion.identity);
            _userToggle.transform.SetParent(usersParent.transform);
            _userToggle.transform.localScale = Vector3.one;
            _userToggle.transform.localPosition = Vector3.zero;
        }

        if(gameManager.users.Count == 0)
            GetComponent<UIScreenManager>().initiallyOpen = createNewUser;

        changeColorText = changeColorButton.GetComponentInChildren<Text>();
        changeColorTextShadow = changeColorText.GetComponent<Shadow>();
        changeColorButtonEffects = changeColorButton.GetComponent<ButtonEffects>();
        changeUserTextShadow = changeUserText.GetComponent<Shadow>();
        changeUserButtonEffects = changeUserButton.GetComponent<ButtonEffects>();
        
        UpdateChangeUserButton();

        editState = EditState.Normal;
    }
    
    void Update()
    {
        changeUserText.text = gameManager.currentUser.userName;
                                
        if(editState == EditState.New)
        {
            cancelButton.interactable = true;
            oneToTwentyCharsText.SetActive(true);

            if(userNameInputField.text.Length > 0 && !currentUserNames.Contains(userNameInputField.text))
            {
                confirmButton.interactable = true;

                if(gameInputManager.ReturnButtonDown() && uiScreenManager.transitionTimer <= 0)
                {
                    if(!createdNewUser)
                    {
                        createdNewUser = true;
                        confirmButton.interactable = false;
                        AudioManager.Instance.PlaySound2D(buttonClick);
                        CreateNewUser();
                    }
                }
            }
            else
            {
                confirmButton.interactable = false;
            }

            SetTextColorAndShadow(currentUserButton.colorIndex);
        }
		
        if(editState == EditState.Rename)
        {
            cancelButton.interactable = true;
            oneToTwentyCharsText.SetActive(true);

            currentUserButton.userName = userNameInputField.text;
            currentUserButton.usernameText.text = currentUserButton.userName;

            if(userNameInputField.text.Length > 0 && !currentUserNames.Contains(userNameInputField.text) || userNameInputField.text == previousUserName)
            {
                confirmButton.interactable = true;

                if(gameInputManager.ReturnButtonDown() && uiScreenManager.transitionTimer <= 0)
                {
                    AudioManager.Instance.PlaySound2D(buttonClick);
                    RenameUser();
                }
            }
            else
            {
                confirmButton.interactable = false;
            }

            SetTextColorAndShadow(currentUserButton.colorIndex);
        }

        if(editState == EditState.Color)
        {
            oneToTwentyCharsText.SetActive(false);
            cancelButton.interactable = true;
            confirmButton.interactable = true;

            if(gameInputManager.ReturnButtonDown() && uiScreenManager.transitionTimer <= 0)
            {
                AudioManager.Instance.PlaySound2D(buttonClick);
                ConfirmChangeColor();
            }
        }

        if(editState == EditState.Normal)
        {
            oneToTwentyCharsText.SetActive(false);
            cancelButton.interactable = false;
            confirmButton.interactable = false;

            if(currentUserButton != null)
            {
                changeUserButton.color = userColors[currentUserButton.colorIndex];
                changeColorButton.color = userColors[currentUserButton.colorIndex];
                SetTextColorAndShadow(currentUserButton.colorIndex);
            }
        }

        if(editState != EditState.Normal)
        {
            exitUIArea.enabled = false;

            if(gameInputManager.EscapeButtonDown() && uiScreenManager.transitionTimer <= 0)
                Cancel();
        }
        else
        {
            exitUIArea.enabled = true;
        }
            
        if(gameManager.users.Count <= 1)
            deleteButton.interactable = false;

        /////At the very start when there are no users
        if(gameManager.users.Count == 0)
        {
            if(createNewUserInputField.text.Length > 0)
            {
                createNewUserButton.interactable = true;

                if(gameInputManager.ReturnButtonDown())
                {
                    if(!createdNewUser)
                    {
                        createdNewUser = true;
                        createNewUserButton.interactable = false;
                        CreateNewUser();
                        AudioManager.Instance.PlaySound2D(buttonClick);
                        uiScreenManager.OpenPanel(mainMenu);
                    }
                    
                }
            }
            else
                createNewUserButton.interactable = false;
        }
    }

    public void RefreshUserByteSizes()
    {
        OnUserByteSizeRefresh?.Invoke();
    }

    public void New()
    {
        AudioManager.Instance.PlaySound2D(buttonClick);
        uiScreenManager.transitionTimer = 0.2f;

        userNameInputField.interactable = true;
        OnButtonsDisabled?.Invoke(false);

        for(int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = false;

        eventSystem.SetSelectedGameObject(userNameInputField.gameObject);
        editState = EditState.New;

        currentUserNames.Clear();

        foreach(var user in gameManager.users)
            currentUserNames.Add(user.userName);
    }

    public void CreateNewUser()
	{
        string userName = "";

        if(gameManager.users.Count == 0)
            userName = createNewUserInputField.text;
        else
            userName = userNameInputField.text;

        userNameInputField.interactable = false;
        
        for(int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = true;
        
        List<int> userIDs = new List<int>();
        List<string> userNames = new List<string>();

        foreach(var user in gameManager.users)
        {
            userIDs.Add(user.userID);
            userNames.Add(user.userName);
        }

        if(!userNames.Contains(userName))
        {
            User user = new User();
            
            int randomUserID;

            do
            {
                randomUserID = UnityEngine.Random.Range(0, int.MaxValue);
            }
            while(userIDs.Contains(randomUserID));
            
            gameManager.users.Add(user);
            user.userName = userName;
            user.userID = randomUserID;
            user.userColorIndex = UnityEngine.Random.Range(1, userColors.Length);

            gameManager.currentUser = user;

            gameManager.SaveUserData();
            gameManager.SaveData();

            var _userToggle = (Button)Instantiate(userButton, Vector3.zero, Quaternion.identity);
            _userToggle.GetComponentInChildren<Text>().text = userName;
            _userToggle.transform.SetParent(usersParent.transform);
            _userToggle.transform.localScale = Vector3.one;
            _userToggle.transform.localPosition = Vector3.zero;
        }
        
        UpdateChangeUserButton();

        if(gameManager.users.Count == 6)
            newButton.interactable = false;
        else
            newButton.interactable = true;

        userNameInputField.text = "";
        OnUserButtonsRefresh?.Invoke();

        StartCoroutine(DelayAllowingRenaming());
        editState = EditState.Normal;
    }

    IEnumerator DelayAllowingRenaming()
    {
        yield return new WaitForSeconds(0.25f);
        createdNewUser = false;    
    }

    public void Rename()
    {
        AudioManager.Instance.PlaySound2D(buttonClick);
        uiScreenManager.transitionTimer = 0.2f;

		previousUserName = currentUserButton.userName;

        userNameInputField.interactable = true;
        
        for(int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = false;

        eventSystem.SetSelectedGameObject(userNameInputField.gameObject);
        userNameInputField.text = currentUserButton.userName;

        OnButtonsDisabled?.Invoke(false);
        editState = EditState.Rename;

        currentUserNames.Clear();

        foreach(var user in gameManager.users)
            currentUserNames.Add(user.userName);
    }

    public void RenameUser()
    {
        userNameInputField.interactable = false;
        OnButtonsDisabled?.Invoke(true);
        gameManager.currentUser.userName = userNameInputField.text;
        gameManager.SaveUserData();
        gameManager.SaveData();

        for(int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = true;

        userNameInputField.text = "";
        editState = EditState.Normal;
    }
    
    public void ChangeColor()
    {
        AudioManager.Instance.PlaySound2D(buttonClick);
        uiScreenManager.transitionTimer = 0.2f;

        colorSelectionIndex ++;

        if(colorSelectionIndex > userColors.Length - 1)
            colorSelectionIndex = 0;

        currentUserButton.ChangeColor(colorSelectionIndex);
        changeColorButton.color = userColors[colorSelectionIndex];
        SetTextColorAndShadow(colorSelectionIndex);

        for(int i = 0; i < buttons.Length - 1; i++)
            buttons[i].interactable = false;

        OnButtonsDisabled?.Invoke(false);
        editState = EditState.Color;
    }

    public void ConfirmChangeColor()
    {
        gameManager.currentUser.userColorIndex = currentUserButton.colorIndex;
        previousUserColorIndex = currentUserButton.colorIndex;
        colorSelectionIndex = previousUserColorIndex;
        
        for(int i = 0; i < buttons.Length - 1; i++)
            buttons[i].interactable = true;

        OnButtonsDisabled?.Invoke(true);
        gameManager.SaveUserData();
        gameManager.SaveData();
        editState = EditState.Normal;
    }

    void SetTextColorAndShadow(int index)
    {
        if(index == 0)
        {
            changeColorButtonEffects.textStartingColor = Color.black;
            changeColorButtonEffects.disabledColor = Color.black;
            changeColorTextShadow.enabled = false;
            changeUserButtonEffects.textStartingColor = Color.black;
            changeUserButtonEffects.disabledColor = Color.black;
            changeUserTextShadow.enabled = false;
        }
        else
        {
            changeColorButtonEffects.textStartingColor = Color.white;
            changeColorButtonEffects.disabledColor = Color.white;
            changeColorText.color = Color.white;
            changeColorTextShadow.enabled = true;

            changeUserText.color = Color.white;
            changeUserButtonEffects.textStartingColor = Color.white;
            changeUserButtonEffects.disabledColor = Color.white;
            changeUserTextShadow.enabled = true;
        }
    }

    public void Confirm()
    {
        uiScreenManager.transitionTimer = 0.2f;

        if(editState == EditState.New)
        {
            AudioManager.Instance.PlaySound2D(buttonClick);
            CreateNewUser();
        }

        if(editState == EditState.Rename)
        {
            AudioManager.Instance.PlaySound2D(buttonClick);
            RenameUser();
        }

        if(editState == EditState.Color)
        {
            AudioManager.Instance.PlaySound2D(buttonClick);
            ConfirmChangeColor();
        }
    }

    public void Cancel()
    {
        AudioManager.Instance.PlaySound2D(buttonClick);
        uiScreenManager.transitionTimer = 0.2f;

        if(editState == EditState.Rename)
        {
            userNameInputField.interactable = false;
            userNameInputField.text = "";
            
            currentUserButton.userName = previousUserName;
            currentUserButton.usernameText.text = previousUserName;
        }

        if(editState == EditState.New)
        {
            userNameInputField.interactable = false;
            userNameInputField.text = "";
        }

        if(editState == EditState.Color)
        {
            currentUserButton.ChangeColor(previousUserColorIndex);
            colorSelectionIndex = previousUserColorIndex;
        }

        editState = EditState.Normal;
        OnButtonsDisabled?.Invoke(true);

        for(int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = true;
    }

    void UpdateChangeUserButton()
    {
        if(gameManager.users.Count <= 0)
            return;
        
        int colorIndex = gameManager.currentUser.userColorIndex;

        changeUserButton.color = userColors[colorIndex];
        SetTextColorAndShadow(colorIndex);
    }

    public void SetDeletePanelTexts()
    {
        deleteUserPanelTitleText.text = "Delete " + gameManager.currentUser.userName;
        deleteUserText.text = "Are you sure you want to delete " + gameManager.currentUser.userName + "?" + "\nAll data for that user will be lost.";
        deleteUserButtonText.text = "Delete " + gameManager.currentUser.userName;
    }

    public void Delete()
    {
        if(gameManager.users.Count > 0)
        {
            gameManager.DeleteUserData(gameManager.currentUser.userID);
            gameManager.users.RemoveAt(currentUserButton.transform.GetSiblingIndex());
            gameManager.currentUser = gameManager.users[0];
            currentUserButton.Remove();
            gameManager.SaveData();

            RefreshUsers();
        }
        else if(gameManager.users.Count == 0)
        {
            userNameInputField.interactable = true;

            for(int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = false;
            }

            eventSystem.SetSelectedGameObject(userNameInputField.gameObject);
        }

        if(gameManager.users.Count == 6)
            newButton.interactable = false;
        else
            newButton.interactable = true;
    }

    public void RefreshUsers()
    {
        OnUserButtonsRefresh?.Invoke();
    }
}