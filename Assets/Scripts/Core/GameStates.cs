using UnityEngine;

public class PreparationState : IState
{
    private GameManager _gameManager;

    public PreparationState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Enter()
    {
        Debug.Log("Entering Preparation State");
        // Logic for preparation phase (e.g., enable shop, disable combat UI)
    }

    public void Execute()
    {
        // Check for player input to start combat
        // This might be handled by UI events calling GameManager methods
    }

    public void Exit()
    {
        Debug.Log("Exiting Preparation State");
    }
}

public class CombatState : IState
{
    private GameManager _gameManager;

    public CombatState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Enter()
    {
        Debug.Log("Entering Combat State");
        _gameManager.IsCombatActive = true;
        GameEvents.RaiseCombatStarted();
        Object.FindFirstObjectByType<EnemySpawner>()?.StartCombat();
    }

    public void Execute()
    {
        // Combat logic updates if needed
    }

    public void Exit()
    {
        Debug.Log("Exiting Combat State");
        _gameManager.IsCombatActive = false;
        GameEvents.RaiseCombatEnded();
    }
}

public class RewardState : IState
{
    private GameManager _gameManager;

    public RewardState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Enter()
    {
        Debug.Log("Entering Reward State");
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.GenerateRewards();
        }
        else
        {
            _gameManager.FinishCombatPhase();
        }
    }

    public void Execute()
    {
        // Wait for player to pick reward
    }

    public void Exit()
    {
        Debug.Log("Exiting Reward State");
    }
}

public class GameOverState : IState
{
    private GameManager _gameManager;

    public GameOverState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Enter()
    {
        Debug.Log("Entering Game Over State");
        GameEvents.RaiseGameOver();
        Time.timeScale = 0f;
    }

    public void Execute()
    {
        // Wait for restart
    }

    public void Exit()
    {
        Debug.Log("Exiting Game Over State");
        Time.timeScale = 1f;
    }
}

public class MapState : IState
{
    private GameManager _gameManager;

    public MapState(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Enter()
    {
        Debug.Log("Entering Map State");
        _gameManager.IsCombatActive = false;
        
        // Show Map UI
        Object.FindFirstObjectByType<MapUI>()?.Show();
    }

    public void Execute()
    {
        // Wait for player to select a node
    }

    public void Exit()
    {
        Debug.Log("Exiting Map State");
        // Hide Map UI
        Object.FindFirstObjectByType<MapUI>()?.Hide();
    }
}
