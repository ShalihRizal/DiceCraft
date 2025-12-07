using UnityEngine;

[CreateAssetMenu(fileName = "Trait_Fortify", menuName = "Game/Enemy/Traits/Fortify")]
public class FortifyTrait : EnemyTrait
{
    [Header("Fortify Settings")]
    public float shieldPercent = 30f; // Shield equal to 30% of max HP

    public override void ExecuteEffect(Enemy owner)
    {
        if (owner.enemyData == null) return;

        float shieldAmount = owner.MaxHealth * (shieldPercent / 100f);
        owner.AddShield(shieldAmount);
        
        Debug.Log($"{owner.name} gained {shieldAmount} shield!");
    }
}
