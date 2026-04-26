using UnityEngine;
using System.Collections;

public class UIScreenManager : MonoBehaviour
{
    public Animator initiallyOpen;

    public Animator currentOpenPanel { get; set; }
    Animator currentPanel;
    int m_OpenParameterId;
    Transform cursor;
    const string k_OpenTransitionName = "Open";
    const string k_ClosedStateName = "Closed";
    public float transitionTimer { get; set; }

    void Start()
    {
        StartCoroutine(StartCo());
        cursor = GetComponent<MainMenuManager>().cursor.transform;
    }

    void Update()
    {
        if(transitionTimer > 0)
            transitionTimer -= Time.deltaTime;
    }

    IEnumerator StartCo()
    {
        yield return new WaitForEndOfFrame();

        m_OpenParameterId = Animator.StringToHash(k_OpenTransitionName);
        OpenPanel(initiallyOpen);
    }

    public void OpenPanel(Animator anim)
    {
        if(currentPanel == anim)
            return;

        currentOpenPanel = anim;

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

        animator.transform.SetAsLastSibling();
        cursor.SetAsLastSibling();

        animator.gameObject.SetActive(true);
        animator.SetBool("Open", true);
    }
}