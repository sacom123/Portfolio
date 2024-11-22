using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackReset : StateMachineBehaviour
{
    [SerializeField] string triggerName;
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(triggerName);
        animator.SetBool(triggerName, false);
    }
}
