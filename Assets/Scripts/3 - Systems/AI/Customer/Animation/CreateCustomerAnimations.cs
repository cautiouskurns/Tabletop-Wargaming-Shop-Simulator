using UnityEngine;
using UnityEditor;

public class CreateCustomerAnimations : EditorWindow
{
    [MenuItem("Tools/Create Customer Animations")]
    public static void CreateAnimations()
    {
        // Create Idle Animation
        AnimationClip idleClip = new AnimationClip();
        idleClip.name = "Customer_Idle";
        idleClip.legacy = false;
        
        // Create a simple idle animation with slight breathing effect
        AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 1f, 2f, 1.02f);
        scaleCurve.AddKey(new Keyframe(4f, 1f));
        scaleCurve.preWrapMode = WrapMode.Loop;
        scaleCurve.postWrapMode = WrapMode.Loop;
        
        idleClip.SetCurve("", typeof(Transform), "localScale.y", scaleCurve);
        
        AssetDatabase.CreateAsset(idleClip, "Assets/Animations/Customer/Customer_Idle.anim");
        
        // Create Walk Animation
        AnimationClip walkClip = new AnimationClip();
        walkClip.name = "Customer_Walk";
        walkClip.legacy = false;
        
        // Create a simple walk animation with bobbing motion
        AnimationCurve bobbingCurve = AnimationCurve.Linear(0f, 0f, 0.5f, 0.1f);
        bobbingCurve.AddKey(new Keyframe(1f, 0f));
        bobbingCurve.preWrapMode = WrapMode.Loop;
        bobbingCurve.postWrapMode = WrapMode.Loop;
        
        walkClip.SetCurve("", typeof(Transform), "localPosition.y", bobbingCurve);
        
        AssetDatabase.CreateAsset(walkClip, "Assets/Animations/Customer/Customer_Walk.anim");
        
        // Create Animator Controller
        UnityEditor.Animations.AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/Animations/Customer/CustomerAnimatorController.controller");
        
        // Add parameters
        controller.AddParameter("WalkSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        
        // Add states
        var idleState = controller.layers[0].stateMachine.AddState("Idle");
        idleState.motion = idleClip;
        
        var walkState = controller.layers[0].stateMachine.AddState("Walk");
        walkState.motion = walkClip;
        
        // Set default state
        controller.layers[0].stateMachine.defaultState = idleState;
        
        // Add transitions
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.25f;
        
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsWalking");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.25f;
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Customer animations and animator controller created successfully!");
    }
}