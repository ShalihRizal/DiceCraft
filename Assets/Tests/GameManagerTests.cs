using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameManagerTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(gameManagerObject);
    }

    [Test]
    public void GameManager_StartsWithCombatInactive()
    {
        Assert.IsFalse(gameManager.IsCombatActive);
    }

    [Test]
    public void GameManager_StartCombat_SetsCombatActive()
    {
        gameManager.StartCombat();
        Assert.IsTrue(gameManager.IsCombatActive);
    }

    [Test]
    public void GameManager_EndCombat_SetsCombatInactive()
    {
        gameManager.StartCombat();
        gameManager.EndCombat();
        Assert.IsFalse(gameManager.IsCombatActive);
    }
}
