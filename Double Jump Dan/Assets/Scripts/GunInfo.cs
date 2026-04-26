using UnityEngine;
using System;

public class GunInfo: MonoBehaviour
{
    public FireMode fireMode;
    public FireRate _fireRate;
    [Range(0, 100)]
    public int damage;
    [Range(0, 100)]
    public float startingAmmo;
    public int shotsPerBurst;
    public int burstsPerMagazine;
    public float burstCoolDownTime;
    public float aimPointOffset;
    public float lowRumbleAmount;
    public float highRumbleAmount;
    public float rumbleDuration;

    public float fireRate { get; set; }
    public enum FireMode { Single, Automatic, Burst };
    public enum FireRate { ExtremelySlow, VerySlow, Slow, Normal, Fast, VeryFast, ExtremelyFast };
    public float startReloadTimer { get; set; }
    public float reloadTimer { get; set; }
    public float currentAmmo { get; private set; }
    public float maxAmmo { get; private set; }
    public float _burstCoolDownTimer { get; set; }
    public int burstShotCounter { get; set; }
    public bool initiatedBurst { get; set; }
    public event Action<float> OnAmmoChanged;
    public event Action OnShoot;
    public bool canShoot { get; private set; }
    GameInputManager gameInputManager;

    void Awake()
    {
        startReloadTimer = 0.25f;
        reloadTimer = startReloadTimer;

        switch(_fireRate)
        {
            case FireRate.ExtremelySlow:
                fireRate = 2;
                break;
            case FireRate.VerySlow:
                fireRate = 1;
                break;
            case FireRate.Slow:
                fireRate = 0.7f;
                break;
            case FireRate.Normal:
                fireRate = 0.4f;
                break;
            case FireRate.Fast:
                fireRate = 0.3f;
                break;
            case FireRate.VeryFast:
                fireRate = 0.2f;
                break;
            case FireRate.ExtremelyFast:
                fireRate = 0.1f;
                break;
        }
    }

    void Start()
    {
        gameInputManager = GameInputManager.Instance;    
    }
    
    public void Initialize()
    {
        maxAmmo = startingAmmo;
        currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public void Shoot(float amount)
    {
        if(currentAmmo <= 0)
            return;
        
        burstShotCounter++;
        currentAmmo -= amount;
        currentAmmo = Math.Clamp(currentAmmo, 0, maxAmmo);

        OnAmmoChanged?.Invoke(currentAmmo);
        OnShoot?.Invoke();

        gameInputManager.RumbleController(lowRumbleAmount, highRumbleAmount, rumbleDuration);
    }
    
    public void Reload()
    {
        burstShotCounter = 0;
        currentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(maxAmmo);
    }

    public bool CanShoot(bool _canShoot)
    {
        canShoot = _canShoot;
        return _canShoot;
    }
}