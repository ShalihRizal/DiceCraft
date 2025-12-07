using UnityEngine;

[CreateAssetMenu(fileName = "Trait_Enrage", menuName = "Game/Enemy/Traits/Enrage")]
public class EnrageTrait : EnemyTrait
{
    [Header("Enrage Settings")]
    public float attackSpeedBoost = 0.5f; // +50% attack speed (reduce interval)

    public override void ExecuteEffect(Enemy owner)
    {
        owner.fireInterval /= (1f + attackSpeedBoost);
        Debug.Log($"{owner.name} is ENRAGED! Attack speed increased!");
    }
}
