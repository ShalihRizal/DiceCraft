# Enemy System Documentation

## Overview
A modular enemy system with event-based behaviors, trait combinations, and loot drops.

## Core Components

### 1. EnemyData (ScriptableObject)
Defines enemy configuration:
- **Basic Info**: Name, type (Normal/Elite/Boss), prefab, icon
- **Stats**: HP, damage, move speed, attack interval
- **Traits**: List of modular behaviors
- **Loot**: Reference to loot table
- **Visual**: Health bar color

### 2. EnemyTrait (ScriptableObject)
Base class for all enemy behaviors:
- **Trigger Event**: When the trait activates
- **ExecuteEffect()**: Override to define behavior
- **OnApplied()**: Called when trait is first applied

### 3. Enemy (MonoBehaviour)
Runtime enemy instance:
- Manages HP and triggers events
- Executes traits at appropriate times
- Handles loot drops on death
- Supports mouse hover tooltips

## Event Triggers

| Trigger | When It Fires |
|---------|---------------|
| `OnSpawned` | Enemy enters battlefield |
| `OnDamaged` | Every time enemy takes damage |
| `On50Percent` | HP drops below 50% (once) |
| `On25Percent` | HP drops below 25% (once) |
| `On1Percent` | HP near death (last stand) |
| `OnDeath` | Enemy dies |
| `OnBuffed` | Enemy receives a buff |
| `OnShielded` | Enemy gains a shield |
| `OnAttack` | Enemy attacks player |

## Built-in Traits

### RegenerationTrait (OnDamaged)
- Heals 5% of max HP when damaged
- Good for tanky enemies

### EnrageTrait (On50Percent)
- Gains +50% attack speed at half HP
- Makes enemies more dangerous when wounded

### SummonMinionsTrait (On25Percent)
- Spawns 2 weak enemies at 25% HP
- Requires minion prefab assignment

### DeathExplosionTrait (OnDeath)
- Deals AoE damage to nearby dice
- Can damage player's board

### VampiricTrait (OnAttack)
- Heals for 10% of damage dealt
- Sustains enemy during combat

### FortifyTrait (OnSpawned)
- Starts with shield equal to 30% max HP
- Makes enemies tankier from the start

## Loot System

### LootTable (ScriptableObject)
Defines weighted drop chances:
- **Gold**: 100% (5-15 for Normal, 20-50 for Elite, 100-200 for Boss)
- **Dice Pips**: 50% for Boss (10-20)
- **Relics**: 15% for Elite, 80% for Boss
- **Dice**: 40% for Boss
- **Health Orbs**: Instant healing

### LootPickup (MonoBehaviour)
Collectible loot drops:
- Auto-collects on proximity
- Click to collect manually
- Visual representation based on type

## Creating Enemies

### Example: Elite Necromancer
```
EnemyData:
- Type: Elite
- HP: 200
- Damage: 15
- Traits:
  - SummonMinionsTrait (OnDeath)
  - RegenerationTrait (OnDamaged)
```

### Example: Boss Golem
```
EnemyData:
- Type: Boss
- HP: 1000
- Damage: 30
- Traits:
  - FortifyTrait (OnSpawned)
  - EnrageTrait (On50Percent)
  - DeathExplosionTrait (OnDeath)
```

## Tooltip System

### EnemyTooltip
Shows when hovering over enemies:
- Enemy name (colored by type)
- Current/Max HP
- Damage and attack speed
- List of active traits with descriptions

### Integration
- Automatically displays on mouse hover
- Hides on mouse exit
- Uses same tooltip system as dice/relics

## Usage

1. **Create Enemy Data**:
   - Right-click → Create → Game → Enemy Data
   - Configure stats and assign traits

2. **Create Traits**:
   - Right-click → Create → Game → Enemy → Traits → [Trait Type]
   - Configure trait parameters

3. **Create Loot Table**:
   - Right-click → Create → Game → Loot Table
   - Add loot entries with weights

4. **Assign to Spawner**:
   - Update WaveConfig to use EnemyData
   - Spawner will handle trait execution

## Future Enhancements

- [ ] Shield system implementation
- [ ] Buff system implementation
- [ ] More trait types (Berserk, Shield Burst, etc.)
- [ ] Trait visual effects
- [ ] Enemy AI behaviors
- [ ] Boss-specific mechanics
