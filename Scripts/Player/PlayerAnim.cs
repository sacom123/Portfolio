using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Player�� ��� �ִϸ��̼��� ����
/// Attack, Move, Jump, Skill, Die
/// Attack�� int AttackType�� � Ÿ���� ������ �ؾ��ϴ��� Ȯ�� 
/// Move�� PlayerInput�� Move �Լ��� OnMove�Լ����� MoveInput�� �޾ƿͼ� setFloat���� �ִϸ��̼� ó��
/// Skill�� PlayerAttack���� Skill�� ����� �� ȣ�� Ÿ���� ��� �ش� �Լ����� �´� Ÿ���� ��ȯ ���� -> Int ������
/// Die, TakeDamage�� PlayerStates���� ȣ�� �޾Ƽ� ó��
/// </summary>

public class PlayerAnim : NetworkBehaviour
{
    [SerializeField] public Animator PlayerAnimator;
    [SerializeField] PlayerInputCheck playerInput;
    [SerializeField] public Collider AttackCollider;
    PlayerManager PM;

    int hashAttackType = Animator.StringToHash("AttackType"); // player attack type�� �����ϴ� ����
    int hashSkillType = Animator.StringToHash("SkillType"); // player skill type�� �����ϴ� ����     

    private void Start()
    {
        PM = playerInput.PM;    
    }

    private void Update()
    {
        AnimatorStateInfo stateInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(2);

        if (PlayerAnimator.IsInTransition(2))
        {
            playerInput.isAttack = true;
        }
        else
        {
            if(stateInfo.normalizedTime >=0.5f)
            {
                playerInput.isAttack = false;
            }
        }
    }


    // ���⸦ �ٲٰų� �� ó���� ������ ���⿡ ���� Ÿ���� int ������ ����
    public int AttackType
    {
        get => PlayerAnimator.GetInteger(hashAttackType);
        set => PlayerAnimator.SetInteger(hashAttackType, value);
    }

    public int SkillType
    {
        get => PlayerAnimator.GetInteger(hashSkillType);
        set => PlayerAnimator.SetInteger(hashSkillType, value);
    }

    public void TakeAttack()
    {
        PlayerAnimator.SetBool("IsAttack", true);
    }

    public void TakeSkill(int skillnum,int weaponID)
    {
        PlayerAnimator.SetInteger("WeaponID", weaponID);
        PlayerAnimator.SetInteger("SkillType", skillnum);
        PlayerAnimator.SetTrigger("Skill");
    }

    public void TakeDamageAnim()
    {
        playerInput.CanMove = false;
        PlayerAnimator.SetTrigger("TakeDamage");
    }

    public void DieAnim()
    {
        PlayerAnimator.SetBool("Die",true);
    }

    public void AttackStart()
    {
        //StartCoroutine(AttackMove());
    }

    public void AttackEnable()
    {
        Debug.Log("attacollOn");
        PM.Playerattack.AttackCollider.enabled = true;
        PM.Playerattack.AttackEffect.enabled = true;
    }

    public void AttackDIsable()
    {
        Debug.Log("attacollOff");
        PM.Playerattack.AttackCollider.enabled = false;
        PM.Playerattack.AttackEffect.enabled = false;
    }

    [SerializeField] private float moveForce = 3;

    private float attackDuration = 0.3f; // Duration of the attack
    private float elapsedTime = 0f;

    IEnumerator AttackMove()
    {
        Vector3 startPosition = playerInput.transform.position;
        Vector3 targetPosition = startPosition + playerInput.transform.forward * moveForce;

        startPosition.y = playerInput.transform.position.y;
        targetPosition.y = playerInput.transform.position.y;

        elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp the position smoothly
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / attackDuration);
            newPosition.y = playerInput.transform.position.y;
            // Move the character
            playerInput.characterController.Move(newPosition - playerInput.transform.position);

            // Wait until the next frame
            yield return null;
        }
    }

    public void AttackEffect()
    {
        PM.CallCmdAttackEffect();
    }

    [TargetRpc]
    public void RpcAttackEffect(NetworkConnection conn)
    {
        StopCoroutine(Swing());
        StartCoroutine(Swing());
    }

    IEnumerator Swing()
    {
        yield return YieldCache.WaitForSeconds(0.1f);
        PM.Playerattack.AttackCollider.enabled = true;
        PM.Playerattack.AttackEffect.enabled = true;

        yield return YieldCache.WaitForSeconds(1f);
        PM.Playerattack.AttackCollider.enabled = false;

        yield return YieldCache.WaitForSeconds(2f);
        PM.Playerattack.AttackEffect.enabled = false;
    }


    public void TakeSkillAnimation(int skillNum)
    {
        switch (skillNum)
        {
            case 1:
                Debug.Log("��ų1 �ִϸ��̼� ���");
                PlayerAnimator.SetBool("Skill1", true);
                break;

            case 2:
                Debug.Log("��ų2 �ִϸ��̼� ���");
                PlayerAnimator.SetBool("Skill2", true);                
                break;                            
        }
    }

    public void Skill1AnimEnd(string type)
    {
        PlayerAnimator.SetBool(type, false);
    }

}

