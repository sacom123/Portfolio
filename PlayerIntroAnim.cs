using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerIntroAnim : NetworkBehaviour
{
    [SerializeField] public GameObject AnimWeapon;
    [SerializeField] public GameObject BackWeapon;
    [SerializeField] private PlayerInputCheck PI;
    [SerializeField] private PlayerAttack PA;
    [SerializeField] private PlayerManager PM;


    public void SetWeapon()
    {
        Debug.Log("무기 켜!");
        PM.PS.AnimEnd = false;
        PM.PS.animsetstate(PM);
    }
    public void SetWeaponEnd()
    {
        Debug.Log("무기 키는거 끝!");
        PM.PS.AnimEnd = true;
        PI.animator.SetBool("Weapon Equip", true);
        PI.animator.SetBool("Weapon Check", false);
    }

    public void unequipWeapon()
    {
        PM.PS.AnimEnd = false;
    }

    public void unequipWeaponEnd()
    {
        Debug.Log("무기꺼!");
        PM.PS.animsetstate(PM);
        PM.PS.AnimEnd = true;  
        PI.animator.SetBool("Weapon Equip", false);
        PI.animator.SetBool("Weapon Check", false);
    }

    
   
}
