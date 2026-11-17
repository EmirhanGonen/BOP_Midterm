using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth = 0;

    [SerializeField] private UnityEvent<OnTakeDamagePackage> onTakeDamageEvent = new UnityEvent<OnTakeDamagePackage>();
    [SerializeField] private UnityEvent onDeadEvent = new UnityEvent();

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        onTakeDamageEvent?.Invoke((currentHealth, maxHealth));
        if (currentHealth > 0)
            return;
        
        onDeadEvent?.Invoke();
    }
}

[System.Serializable]
public struct OnTakeDamagePackage
{
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    public OnTakeDamagePackage(int currentHealth, int maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
    }

    public static implicit operator OnTakeDamagePackage((int, int) tuple)
    {
        return new OnTakeDamagePackage(tuple.Item1, tuple.Item2);
    }
    public static implicit operator OnTakeDamagePackage(System.Tuple<int, int> tuple)
    {
        return new OnTakeDamagePackage(tuple.Item1, tuple.Item2);
    }
}