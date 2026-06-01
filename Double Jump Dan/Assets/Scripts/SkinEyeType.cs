using UnityEngine;

public class SkinEyeType : MonoBehaviour
{
    public EyeType eyeType;
    public EyeBrowType eyeBrowType;
    
    public enum EyeType { Single, Double, Tripple, White, Custom, None }
    public enum EyeBrowType { Single, None }
}