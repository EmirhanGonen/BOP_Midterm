using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor.Animations;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New " + nameof(CombatData), menuName = "ScriptableObjects/Combat/" + nameof(CombatData))]
public class CombatData : ScriptableObject
{
    private const string SkillGroup = "Skill";
    private const string SkillInfoGroup = SkillGroup + "/Info";
    private const string CombatGroup = "Combat";
    private const string TimingGroup = "Movement & Timing";

    [HorizontalGroup(SkillGroup, Width = 90)]
    [PreviewField(85, ObjectFieldAlignment.Left), HideLabel]
    [SerializeField] private Sprite skillIcon = null;

    [VerticalGroup(SkillInfoGroup)]
    [LabelText("Name"), LabelWidth(80)]
    [ValidateInput(nameof(IsNotEmpty), "Skill name is empty.", InfoMessageType.Warning)]
    [SerializeField] private string skillName = string.Empty;

    [VerticalGroup(SkillInfoGroup)]
    [LabelText("Animation"), LabelWidth(80)]
    [ValueDropdown(nameof(GetAnimatorStateNames), AppendNextDrawer = true, DisableGUIInAppendedDrawer = false)]
    [SerializeField] private string attackAnimationKey = string.Empty;

    [VerticalGroup(SkillInfoGroup)]
    [LabelText("Cooldown"), LabelWidth(80), MinValue(0)]
    [SerializeField] private float cooldown = 3.00f;

    [BoxGroup(CombatGroup)]
    [SerializeField] private int damage = 0;

    [BoxGroup(CombatGroup)]
    [LabelText("Attack Distance"), MinValue(0)]
    [SerializeField] private float attackDistance = 2.00f;

    [FoldoutGroup(TimingGroup, Expanded = true)]
    [MinValue(0)]
    [SerializeField] private float chargeDuration = 0.60f;

    [FoldoutGroup(TimingGroup)]
    [MinValue(0.01f)]
    [SerializeField] private float dashDuration = 0.12f;

    [FoldoutGroup(TimingGroup)]
    [MinValue(0.01f)]
    [SerializeField] private float returnDuration = 0.25f;

    [FoldoutGroup(TimingGroup)]
    [ShowInInspector, ReadOnly, LabelText("Total Movement")]
    private float TotalMovementDuration => chargeDuration + dashDuration + returnDuration;

    public string GetSkillName() => skillName;
    public Sprite GetSkillIcon() => skillIcon;
    public int GetDamageAmount() => damage;
    public int GetAttackAnimationClipHash() => Animator.StringToHash(attackAnimationKey);

    public float GetCooldown() => cooldown;
    public float GetAttackDistance() => attackDistance;
    public float GetChargeDuration() => chargeDuration;
    public float GetDashDuration() => dashDuration;
    public float GetReturnDuration() => returnDuration;

    private static bool IsNotEmpty(string value) => !string.IsNullOrWhiteSpace(value);


    private static IEnumerable<ValueDropdownItem<string>> GetAnimatorStateNames()
    {
#if UNITY_EDITOR
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/Animations" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (!controller)
                continue;

            foreach (AnimatorControllerLayer layer in controller.layers)
                foreach (ChildAnimatorState state in layer.stateMachine.states)
                    yield return new ValueDropdownItem<string>($"{controller.name}/{state.state.name}", state.state.name);
        }
#else
        yield break;
#endif
    }
}
