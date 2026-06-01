using UnityEngine;
using System.Collections;
using System;
using UnityEngine.InputSystem.UI;

public class UIScreenManager : MonoBehaviour
{
    public Animator initiallyOpen;
    [SerializeField] InputSystemUIInputModule uIModule;

    public Animator currentOpenPanel { get; set; }
    public bool CanExit { get; private set; }
    Animator currentPanel;
    int m_OpenParameterId;
    Transform cursor;
    const string k_OpenTransitionName = "Open";
    const string k_ClosedStateName = "Closed";
    public float transitionTimer { get; set; }
    bool panelOpen;

    void Start()
    {
        StartCoroutine(StartCo());
        cursor = GetComponent<MainMenuManager>().cursor.transform;
    }

    void Update()
    {
        if(transitionTimer > 0)
        {
            transitionTimer -= Time.deltaTime;
        }
        else
        {
            if(!panelOpen)
            {
                ToggleMouseInput(true);
                CanExit = false;    
                panelOpen = true;
            }
        }
    }

    IEnumerator StartCo()
    {
        yield return new WaitForEndOfFrame();

        m_OpenParameterId = Animator.StringToHash(k_OpenTransitionName);
        OpenPanel(initiallyOpen);
    }

    public void ToggleMouseInput(bool enabled)
    {        
        if(enabled)
        {
            uIModule.leftClick.action.Enable();
            uIModule.scrollWheel.action.Enable();
            CanExit = false;
        }
        else
        {
            uIModule.leftClick.action.Disable();
            uIModule.scrollWheel.action.Disable();
            CanExit = true;
        }
    }

    public bool MouseInputDisabled()
    {
        if(!uIModule.leftClick.action.enabled
        && !uIModule.scrollWheel.action.enabled)
            return true;
        else
            return false;
    }

    public void OpenPanel(Animator anim)
    {
        if(currentPanel == anim)
            return;

        currentOpenPanel = anim;
        
        panelOpen = false;
        ToggleMouseInput(false);

        anim.gameObject.SetActive(true);
        anim.transform.SetAsLastSibling();
        cursor.SetAsLastSibling();

        CloseCurrent();

        currentPanel = anim;
        currentPanel.SetBool(m_OpenParameterId, true);
    }

    void CloseCurrent()
    {
        if(currentPanel == null)
            return;

        currentPanel.SetBool(m_OpenParameterId, false);

        StartCoroutine(DisablePanelDeleyed(currentPanel));
        currentPanel = null;
    }

    IEnumerator DisablePanelDeleyed(Animator anim)
    {
        bool closedStateReached = false;
        bool wantToClose = true;

        while(!closedStateReached && wantToClose)
        {
            if(!anim.IsInTransition(0))
                closedStateReached = anim.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

            wantToClose = !anim.GetBool(m_OpenParameterId);

            yield return new WaitForEndOfFrame();
        }

        if(wantToClose)
            anim.gameObject.SetActive(false);
    }

    public void OpenMiniPanel(Animator animator)
    {
        currentOpenPanel = animator;

        panelOpen = false;
        ToggleMouseInput(false);

        animator.transform.SetAsLastSibling();
        cursor.SetAsLastSibling();

        animator.gameObject.SetActive(true);
        animator.SetBool("Open", true);
    }
}