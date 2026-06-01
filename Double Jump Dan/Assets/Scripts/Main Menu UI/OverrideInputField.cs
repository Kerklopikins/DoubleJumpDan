using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class OverrideInputField : InputField
{
    public override void OnUpdateSelected(BaseEventData eventData)
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        base.OnUpdateSelected(eventData);
    }
 
    // [SerializeField] Color[] textColors;
    // [SerializeField] float animationSpeed;

    // int currentColorIndex1 = -1;
    // int currentColorIndex2 = 0;
    // int currentColorIndex3 = 1;
    // int currentColorIndex4 = 2;

    // float animationTimer;
    // Text text;

    // void Start()
    // {
    //     text = GetComponent<Text>();
    // }

    // void Update()
    // {
    //     animationTimer -= Time.unscaledDeltaTime;

    //     if(animationTimer <= 0)
    //     {
    //         currentColorIndex1++;
    //         currentColorIndex2++;
    //         currentColorIndex3++;
    //         currentColorIndex4++;

    //         if(currentColorIndex1 > textColors.Length - 1)
    //             currentColorIndex1 = 0;

    //         if(currentColorIndex2 > textColors.Length - 1)
    //             currentColorIndex2 = 0;
            
    //         if(currentColorIndex3 > textColors.Length - 1)
    //             currentColorIndex3 = 0;
            
    //         if(currentColorIndex4 > textColors.Length - 1)
    //         currentColorIndex4 = 0;

    //         text.text = "<color=#" + ColorUtility.ToHtmlStringRGB(textColors[currentColorIndex1]) + ">FOR</color> " +
    //         "<color=#" + ColorUtility.ToHtmlStringRGB(textColors[currentColorIndex2]) + ">BETA</color> " + "<color=#" +
    //         ColorUtility.ToHtmlStringRGB(textColors[currentColorIndex3]) + ">TESTING</color> " + "<color=#" + 
    //         ColorUtility.ToHtmlStringRGB(textColors[currentColorIndex4]) + ">ONLY!!!</color>";

    //         animationTimer = animationSpeed;
    //     }
    // }
    // public float speed;
    // public Transform wallCheck;
    // public Transform edgeCheck;
    // public Sprite[] snailSprites;
    // public float animationSpeed;
    // public LayerMask collisionMask;
    //     bool notAtEdge;
    // bool hittingWall;
    // int direction;
    // Health health;
    // SpriteRenderer spriteRenderer;
    // int animationIndex = -1;
    // bool hittingEnemy;
    // void Start()
    // {
    //     health = GetComponent<Health>();
    //     spriteRenderer = GetComponent<SpriteRenderer>();

    //     direction = -1;

    //     StartCoroutine(Animation());
    // }

    // void Update()
    // {
    //     if(health.Dead())
    //         return;

    //     hittingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, collisionMask);
    //     notAtEdge = Physics2D.OverlapCircle(edgeCheck.position, 0.2f, 1 << LayerMask.NameToLayer("Collisions"));
    //     transform.Translate(new Vector2(speed * Time.deltaTime * direction, 0), Space.Self);

    //     if(!notAtEdge || hittingWall || hittingEnemy)
    //         direction = -direction;

    //     transform.localScale = new Vector2(-direction, 1);
    // }

    // IEnumerator Animation()
    // {
    //     while(true)
    //     {
    //         animationIndex++;

    //         if(animationIndex > snailSprites.Length - 1)
    //             animationIndex = 0;

    //         spriteRenderer.sprite = snailSprites[animationIndex];
    //         yield return new WaitForSeconds(animationSpeed);
    //     }
    // }
}