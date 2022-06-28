using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/New Gun")]
public class GunInfo : ItemInfo
{
    public float damage;
    public float threshold;
    public int maxAmmo;
    public int maxLoadedAmmo;
    public float reloadThreshold;
    public AudioClip fire;
    public AudioClip reload;
    public Sprite gunIcon;
    public float offset;
    public bool auto;
    public float recoilRotation;
}
