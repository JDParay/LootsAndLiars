using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine;

public class GameMngr : MonoBehaviour
{
    public static GameMngr Instance;

    public List<CharacterData> teammates;
    public StatBlock playerStats;         
    public List<CharacterData> imposters = new List<CharacterData>(); // exactly 2
    public List<CharacterData> markedTonight = new List<CharacterData>();
    public bool hasCompletedFirstPhase2 = false;
    public bool hasKickedToday = false;
    public List<CharacterData> kickedMembers = new List<CharacterData>();
    public UnityEvent<int, int> OnTrustChanged;
    public UnityEvent<int> OnGoldChanged;   
    public UnityEvent OnTrustDepleted;            
    private bool trustDepletedFired = false;
    public int trust = 10;
    public int trustCap = 20;
    public int gold = 2000;
    public int currentDay = 0;
    
    [Header("Endings")]
    public string loseSceneName = "GameOver";
    public string winSceneName = "GameWon";
    

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
        currentDay = 0;              
        trust = 10;
        markedTonight.Clear();
        kickedMembers.Clear();       
        hasKickedToday = false;      
        hasCompletedFirstPhase2 = false;
        AssignImposters();
        OnTrustChanged?.Invoke(trust, trustCap);
        trustDepletedFired = false;
    }

    public void SetStartingGold(int amount)
    {
        gold = amount;
        OnGoldChanged?.Invoke(gold);
    }

    void AssignImposters()
    {
        imposters.Clear();
        var shuffled = teammates.OrderBy(x => Random.value).ToList();
        imposters.Add(shuffled[0]);
        imposters.Add(shuffled[1]);
    }

    public int ApplyImposterTheft()
    {
        var activeImposters = imposters.Where(imp => !IsKicked(imp)).ToList();
        if (activeImposters.Count == 0) return 0;

        int totalStat = activeImposters.Sum(imp => imp.runtimeStats.haste + imp.runtimeStats.scavenge);
        int deduction = totalStat * 60;

        AddGold(-deduction);   
        AdjustTrust(-1);

        return deduction;
    }

    public bool IsImposter(CharacterData character) => imposters.Contains(character);

    public void AdjustTrust(int amount)
    {
        trust = Mathf.Clamp(trust + amount, 0, trustCap);
        OnTrustChanged?.Invoke(trust, trustCap);

        if (trust <= 0 && !trustDepletedFired)
        {
            trustDepletedFired = true;
            OnTrustDepleted?.Invoke();
            TriggerLoseEnding();
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public void MarkSuspect(CharacterData character)
    {
        if (!markedTonight.Contains(character))
            markedTonight.Add(character);
    }

    public bool IsMarked(CharacterData character) => markedTonight.Contains(character);

    public void ClearMarksForNewDay()
    {
        markedTonight.Clear();
    }

    public void KickMember(CharacterData character)
    {
        if (!kickedMembers.Contains(character))
            kickedMembers.Add(character);
    }

    public bool IsKicked(CharacterData character) => kickedMembers.Contains(character);

    public void TriggerLoseEnding()
    {
        SceneFader.Instance.FadeToScene(loseSceneName);
    }

    public void TriggerWinEnding()
    {
        SceneFader.Instance.FadeToScene(winSceneName);
    }
}