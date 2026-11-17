using UnityEngine;
using UnityEngine.UI;

public class CombatButton : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat = null;
    [SerializeField] private CombatData combatData = null;
    private Button combatButton = null;

    private void Awake()
    {
        combatButton = GetComponent<Button>();
        combatButton.onClick.AddListener(CombatAction);
    }

    private void CombatAction()
    {
        playerCombat.Attack(combatData);
    }
}