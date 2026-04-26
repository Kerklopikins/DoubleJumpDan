using UnityEngine;

public class MiniPanel : MonoBehaviour 
{
    ExitUIArea exitUIArea;

    void Start()
    {
        exitUIArea = GetComponent<ExitUIArea>();    
    }

    public void Close(Animator animator)
    {
        animator.SetBool("Open", false);
        exitUIArea.SetPreviousPanel();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}