using UnityEngine;

[CreateAssetMenu(fileName = "ThreeInRowLevel", menuName = "ThreeInRow/Level Data")]
public class ThreeInRowLevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    
    [Header("Items")]
    public ThreeInRowItemData[] availableItems;
    
    [Header("Goals")]
    public int scoreGoal;
    public int movesLimit;
    
    [Header("Rewards")]
    public int coinsReward = 10;
    public int experienceReward = 60;
}