using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{   
    [SerializeField] Text rgbValuesText;
    [SerializeField] RawImage hueImage;
    [SerializeField] RawImage satValImage;
    [SerializeField] Slider hueSlider;
    [SerializeField] InputField hexInputField;
    [SerializeField] Image[] customSkinImages;
    [SerializeField] SVImageControl svImageControl;
    [SerializeField] RectTransform satValImageRectTransform;
    [SerializeField] Text invalidText;

    [HideInInspector] public float currentHue;
    [HideInInspector] public float currentSaturation;
    [HideInInspector] public float currentValue;
    Color currentColor;
    Texture2D hueTexture;
    Texture2D svTexture;
    string previousHexCode;

    char HexOnlyCharacters(char c)
    {
        c = char.ToUpper(c);

        if(c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F' || char.IsNumber(c))
            return c;
        else
            return '\0';
    }
    
    public void Initialize()
    {
        CreateHueImage();
        CreateSVImage();
        UpdateOutput();
        
        hexInputField.onValidateInput += delegate(string s, int i, char c)
        {
            return HexOnlyCharacters(c);
        };
    }

    void Update()
    {
        if(hexInputField.text != previousHexCode)
        {
            UpdateHexData(); 
            previousHexCode = hexInputField.text;
        }
    }

    void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "Hue Texture";

        for(int i = 0; i < hueTexture.height; i++)
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));

        hueTexture.Apply();
        currentHue = 0;
        hueImage.texture = hueTexture;
    }
    
    void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "Sat Val Texture";

        for(int y = 0; y < svTexture.height; y++)
            for(int x = 0; x < svTexture.width; x++)
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));

        svTexture.Apply();
        currentSaturation = 0;
        currentValue = 0;

        satValImage.texture = svTexture;
    }

    void UpdateOutput()
    {
        currentColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);

        if(!hexInputField.isFocused)
        {
            hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);
            previousHexCode = hexInputField.text;    
            invalidText.text = "";
        }

        rgbValuesText.text = "<color=red>Red " + Mathf.RoundToInt(255 * currentColor.r).ToString() + "</color>  " + "<color=green>Green " + Mathf.RoundToInt(255 * currentColor.g).ToString() + "</color>  " + "<color=blue>Blue " + Mathf.RoundToInt(255 * currentColor.b).ToString() + "</color>";
        
        for(int i = 0; i < customSkinImages.Length; i++)
            customSkinImages[i].color = currentColor;       
    }

    public void SetSV(float s, float v)
    {
        currentSaturation = s;
        currentValue = v;

        UpdateOutput();
    }
    
    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;

        for(int y = 0; y < svTexture.height; y++)
            for(int x = 0; x < svTexture.width; x++)
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));

        svTexture.Apply();
        UpdateOutput();
    }

    ///Called at the very start, plus when the shop refreshes, and when an item is equipped
    public void UpdateCurrentColor(float hue, float saturation, float value)
    {
        currentHue = hue; 
        currentSaturation = saturation; 
        currentValue = value; 

        hueSlider.value = currentHue;
        svImageControl.Initialize();
        svImageControl.SetColorPickerPosition(new Vector2(currentSaturation, currentValue), new Vector2(satValImageRectTransform.sizeDelta.x, satValImageRectTransform.sizeDelta.y));
        
        SetSV(currentSaturation, currentValue);
    }

    public void RefreshHexData()
    {
        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);
    }

    void UpdateHexData()
    {
        if(hexInputField.text.Length < 6)
        {
            invalidText.text = "Invalid Hex";
            return;
        }

        Color newColor;

        if(ColorUtility.TryParseHtmlString("#" + hexInputField.text, out newColor))
        {
            invalidText.text = "";
            Color.RGBToHSV(newColor, out currentHue, out currentSaturation, out currentValue);
        }
        else
        {
            invalidText.text = "Invalid Hex";
            return;
        }
        
        hueSlider.value = currentHue;

        UpdateOutput();
        svImageControl.SetColorPickerPosition(new Vector2(currentSaturation, currentValue), new Vector2(satValImageRectTransform.sizeDelta.x, satValImageRectTransform.sizeDelta.y));
    }
}