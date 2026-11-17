using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private CombatData[] combatData = System.Array.Empty<CombatData>();
    [SerializeField] private PlayerAnimationController playerAnimationController = null;

    public void Attack(in CombatData combatData)
    {
        playerAnimationController.PlayCombatAnimation(in combatData);
    }
    
    public void Attack(in int index)
    { 
        Attack(combatData[index]);
    }
}