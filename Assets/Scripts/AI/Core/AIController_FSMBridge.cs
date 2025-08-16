using UnityEngine;

namespace MemeArena.AI
{
    public partial class AIController
    {
        // SendMessage targets (loosely coupled to keep demo concise)
        private void FSM_ChangeToAim()       => fsm.ChangeState(AIStateId.Aim);
        private void FSM_ChangeToAttack()    => fsm.ChangeState(AIStateId.Attack);
        private void FSM_ChangeToCooldown()  => fsm.ChangeState(AIStateId.Cooldown);
        private void FSM_ChangeToEvade()     => fsm.ChangeState(AIStateId.Evade);
        private void FSM_ChangeToAcquire()   => fsm.ChangeState(AIStateId.AcquireTarget);
        private void FSM_ChangeToIdle()      => fsm.ChangeState(AIStateId.Idle);
        private void FSM_ChangeToStunned()   => fsm.ChangeState(AIStateId.Stunned);
        private void FSM_ChangeToDead()      => fsm.ChangeState(AIStateId.Dead);
    }
}
