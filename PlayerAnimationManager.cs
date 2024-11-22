using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializedDictionary("Weapon Type", "Animation Clip")]
    public SerializedDictionary<WeaponType , SerializedDictionary<string,AnimationClip>> PlayerAni = new SerializedDictionary<WeaponType, SerializedDictionary<string, AnimationClip>>();


}
