
using UnityEngine;
using System.Collections;

public class FloatingNumber : MonoBehaviour, IPoolable
{
    [SerializeField] float floatHeight;
    [SerializeField] float scaleDuration;
    [SerializeField] float floatDuration;
    [SerializeField] float hangDuration;
    [SerializeField] SpriteRenderer instantKillSprite;

    [SerializeField] SpriteRenderer plusMinusSprite;
    [SerializeField] Sprite[] plusMinusSprites;
    [SerializeField] Transform numbersHolder;
    [SerializeField] SpriteRenderer[] numberSprites;
    [SerializeField] Sprite[] numbers;
    [SerializeField] float[] spacing;
    
    Vector2 numbersHolderStartPosition;

    void OnEnable()
    {
        transform.localScale = Vector2.zero;
    }

    IEnumerator ScaleNumbers(Vector3 from, Vector3 to, bool disable)
    {
        float inTime = 0;
        transform.localScale = from;

        while(inTime < scaleDuration)
        {
            inTime += Time.deltaTime;
            float t = inTime / scaleDuration;
            float smoothT = 1 - Mathf.Pow(1 - t, 4);

            transform.localScale = Vector3.Lerp(from, to, smoothT);

            yield return null;
        }

        transform.localScale = to;

        if(disable)
            gameObject.SetActive(false);
    }

    IEnumerator FloatNumbers(Vector2 startPosition)
    {
        StartCoroutine(ScaleNumbers(Vector3.zero, Vector3.one, false));
        
        float inTime = 0;

        while(inTime < floatDuration)
        {
            inTime += Time.deltaTime;
            float t = inTime / floatDuration;
            float smoothT = 1 - Mathf.Pow(1 - t, 4);

            transform.position = Vector2.Lerp(startPosition, new Vector2(startPosition.x, startPosition.y + floatHeight), smoothT);

            yield return null;
        }

        transform.position = new Vector2(startPosition.x, startPosition.y + floatHeight);
        yield return new WaitForSeconds(hangDuration); 
        
        StartCoroutine(ScaleNumbers(Vector3.one, Vector3.zero, true));
    }

    public void OnObjectReuse(object data)
    {
        FloatingNumberProperties properties = (FloatingNumberProperties)data;
        SetNumber(properties.number, properties.position, properties.plusOrMinus, properties.color, properties.instantKill);
    }

    public void SetNumber(int number, Vector2 position, bool plusOrMinus, Color color, bool instantKill)
    {
        ////Plus is 0
        /// Minus is 1
        transform.position = position;
        plusMinusSprite.sprite = plusOrMinus == true ? plusMinusSprites[0] : plusMinusSprites[1];
        plusMinusSprite.color = color;
        instantKillSprite.color = color;
        
        foreach(SpriteRenderer sprite in numberSprites)
        {
            sprite.color = color;
            
            if(instantKill)
                sprite.gameObject.SetActive(false);
        }

        StartCoroutine(FloatNumbers(position));

        if(instantKill)
        {
            instantKillSprite.gameObject.SetActive(true);
            plusMinusSprite.gameObject.SetActive(false);
            return;
        }
        else
        {
            instantKillSprite.gameObject.SetActive(false);
            plusMinusSprite.gameObject.SetActive(true);
        }

        int value = Mathf.Abs(number);
        
        for(int i = 0; i < numberSprites.Length; i++)
            numberSprites[i].gameObject.SetActive(false);

        if(value == 0)
        {
            numberSprites[0].gameObject.SetActive(true);
            numberSprites[0].sprite = numbers[0];
            return;
        }

        int temp = value;
        int divisor = 1;

        while(temp / divisor >= 10)
            divisor *= 10;

        int index = 0;

        while(divisor > 0 && index < numberSprites.Length)
        {
            int digit = temp / divisor;
            
            numberSprites[index].gameObject.SetActive(true);
            numberSprites[index].sprite = numbers[digit];
            numbersHolder.localPosition = new Vector3(numbersHolderStartPosition.x + spacing[index], numbersHolder.localPosition.y, 0);

            temp %= divisor;
            divisor /= 10;
            index++;
        }
    }
}

public struct FloatingNumberProperties
{
    public int number;
    public Vector3 position;
    public bool plusOrMinus;
    public Color color;
    public bool instantKill;
}