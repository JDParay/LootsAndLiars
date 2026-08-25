using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMngr : MonoBehaviour
{
    public static GameMngr Instance;

    public List<CharacterData> teammates; // the 4 NPCs, same list ShopMngr used
    public StatBlock playerStats;         // from StatAllocator
    public List<CharacterData> imposters = new List<CharacterData>(); // exactly 2
    public int trust = 10;
    public int trustCap = 20;
    public int gold = 2000;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitFromShop(List<CharacterData> shopTeammates, StatBlock finalPlayerStats)
    {
        teammates = shopTeammates;
        playerStats = finalPlayerStats;
        AssignImposters();
    }

    void AssignImposters()
    {
        imposters.Clear();
        var shuffled = teammates.OrderBy(x => Random.value).ToList();
        imposters.Add(shuffled[0]);
        imposters.Add(shuffled[1]);
    }

    public bool IsImposter(CharacterData character) => imposters.Contains(character);

    public void AdjustTrust(int amount)
    {
        trust = Mathf.Clamp(trust + amount, 0, trustCap);
        if (trust <= 0)
        {
            // TODO: trigger bad ending
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }
}