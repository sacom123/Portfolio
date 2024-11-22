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
        Debug.Log("���� ��!");
        PM.PS.AnimEnd = false;
        PM.PS.animsetstate(PM);
    }
    public void SetWeaponEnd()
    {
        Debug.Log("���� Ű�°� ��!");
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
        Debug.Log("���Ⲩ!");
        PM.PS.animsetstate(PM);
        PM.PS.AnimEnd = true;  
        PI.animator.SetBool("Weapon Equip", false);
        PI.animator.SetBool("Weapon Check", false);
    }

    
   
}
