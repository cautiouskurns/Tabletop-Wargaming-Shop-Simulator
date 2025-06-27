using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime;

namespace TabletopShop
{
    public class SimpleTestAction : Action
    {
        public override TaskStatus OnUpdate()
        {
            Debug.Log("Test action executed!");
            return TaskStatus.Success;
        }
    }
}