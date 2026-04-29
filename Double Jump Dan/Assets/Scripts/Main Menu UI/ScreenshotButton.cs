using UnityEngine;
using UnityEngine.UI;

public class ScreenshotButton : MonoBehaviour
{
    [SerializeField] AudioClip clickSound;
    [SerializeField] Image image;
    public ScreenshotsMenu screenshotsMenu { get; set; }
    public UIScreenManager uiScreenManager { get; set; }
	
	public void ViewScreenshot() 
	{
        if(screenshotsMenu.screenshotViewerAnimator.transform.GetChild(0).transform.localScale.x > 0 && screenshotsMenu.screenshotViewerAnimator.gameObject.activeInHierarchy)
            return;

        screenshotsMenu.screenshotViewerText.text = image.name.Replace(".jpg", "");
        screenshotsMenu.screenshotViewerImage.sprite = image.sprite;
        uiScreenManager.OpenPanel(screenshotsMenu.screenshotViewerAnimator);

        AudioManager.Instance.PlaySound2D(clickSound);
	}
}