using Gamekit3D;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerAttack : NetworkBehaviour
{
    PlayerManager PM;
    [SerializeField] PlayerAnim playerAnim;
    [SerializeField] PlayerInputCheck playerInput;
    [SerializeField] PlayerState PS;
    [SerializeField] private float ReInputTime = 2f;

    [SerializeField] private int ComboCount = 0;

    public int MoveForce = 20;
    public bool canmove = true;

    public bool delay = false;


    [SerializeField] Transform EffectPoint;

    private Coroutine AttackCoroutine;

    public TrailRenderer AttackEffect;

    public BoxCollider AttackCollider;

    void Start()
    {
        PM = this.GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        
    }

    public void OnAttack(InputAction.CallbackContext cont)
    {
        if (cont.started && !PM.isDialog && PM.PS.WeaponEquip && !PM.PS.PlayerIsDead && !PM.PIC.InventoryOpen)
        {
            playerInput.moveInput = Vector2.zero;
            playerInput.isAttack = true;
            Debug.Log("is Attack");
            if (!delay)
            {
                switch (cont.control.name)
                {
                    case "rightTrigger":                    
                        //OnSkill(1);
                        break;
                    case "leftTrigger":                    
                        if (PM.CurrentWeapon == null)
                            return;
                        else if (PM.CurrentWeapon != null)
                        {
                            switch (PM.CurrentWeapon.Type)
                            {
                                case WeaponType.Sword:
                                   Debug.Log("isSKill1");                                    
                                    break;
                                case WeaponType.Axe:
                                    MeleeAttack();
                                    break;
                                case WeaponType.Dagger:
                                    MeleeAttack();
                                    break;
                                case WeaponType.bow:
                                    BowAttack();
                                    break;
                            }
                        }
                        break;
                    case "buttonSouth":
                    case "leftButton":
                        if (PM.CurrentWeapon == null)
                            return;
                        else if(PM.CurrentWeapon != null)
                        {
                            switch (PM.CurrentWeapon.Type)
                            {
                                case WeaponType.Sword:
                                    MeleeAttack();
                                    break;
                                case WeaponType.Axe:
                                    MeleeAttack();
                                    break;
                                case WeaponType.Dagger:
                                    MeleeAttack();
                                    break;
                                case WeaponType.bow:
                                    BowAttack();
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }

    public void OnSkill(InputAction.CallbackContext cont)
    {
        if (!PM.isDialog && PM.PS.WeaponEquip && !PM.PS.PlayerIsDead && cont.started)
        {
            switch (cont.control.name)
            {
                case "z":
                    // 스킬1 사용: 예를 들어, 스킬1 애니메이션 재생
                    Debug.LogError("스킬1 사용");                    
                    playerAnim.TakeSkillAnimation(1);
                    break;

                case "x":
                    // 스킬2 사용: 예를 들어, 스킬2 애니메이션 재생
                    Debug.LogError("스킬2 사용");                    
                    playerAnim.TakeSkillAnimation(2);
                    break;
            }
        }
    }

    public void CheckAttackReInput(float reInputTime)
    {
        if (AttackCoroutine != null)
        {
            Debug.Log("코루틴 중지");
            StopCoroutine(AttackCoroutine);
        }
        AttackCoroutine = StartCoroutine(AttackReInput(reInputTime));
    }

    private IEnumerator AttackReInput(float reInputTime)
    {
        float currentTime = 0f;
        while(true)
        {
            currentTime += Time.deltaTime;
            if(currentTime >= reInputTime)
            {
                break;
            }
            yield return null;
        }
        ComboCount = 0;
        playerInput.animator.SetInteger("ComboCount", 0);
        Debug.Log("콤보 초기화");
    }

    private IEnumerator AttackDelay(float Delay)
    {
        float currentTime = 0f;
        while (true)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= Delay)
            {
                break;
            }
            yield return null;
        }
        delay = false;
    }

    private void MeleeAttack()
    {
        if (ComboCount <= 3)
        {
            ComboCount++;
            PM.PS.ComboCount = ComboCount;
            playerInput.animator.SetInteger("ComboCount", ComboCount);
            playerAnim.TakeAttack();
            delay = true;
            StartCoroutine(AttackDelay(0.2f));
            CheckAttackReInput(ReInputTime);
        }
    }
    private void Skill()
    {
        PM.EM.EffectOneShot(transform);
    }
    private void BowAttack()
    {

    }


}
