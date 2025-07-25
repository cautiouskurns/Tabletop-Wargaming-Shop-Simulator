#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals.Physics
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Returns success when the agent enters a 2D trigger. This task will only receive the physics callback if it is being reevaluated (with a conditional abort or under a parallel task).
    /// </summary>
    [NodeDescription("Returns success when an object enters the 2D trigger. This task will only receive the physics callback if it is being reevaluated (with a conditional abort or under a parallel task).")]
    public class HasEnteredTrigger2D : Conditional
    {
        [Tooltip("The tag of the GameObject that the trigger should be checked against.")]
        [SerializeField] protected SharedVariable<string> m_Tag;
        [Tooltip("The entered trigger.")]
        [SerializeField] protected SharedVariable<Collider2D> m_StoredOtherCollider;

        protected override bool ReceiveTriggerEnter2DCallback => true;

        private bool m_EnteredTrigger;

        /// <summary>
        /// Returns true when the agent has entered a trigger.
        /// </summary>
        /// <returns>True when the agent has entered a trigger.</returns>
        public override TaskStatus OnUpdate()
        {
            return m_EnteredTrigger ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// The agent has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger that the agent entered.</param>
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!string.IsNullOrEmpty(m_Tag.Value) && !other.gameObject.CompareTag(m_Tag.Value)) {
                return;
            }

            if (m_StoredOtherCollider != null && m_StoredOtherCollider.IsShared) { m_StoredOtherCollider.Value = other; }

            m_EnteredTrigger = true;
        }
    }
}
#endif