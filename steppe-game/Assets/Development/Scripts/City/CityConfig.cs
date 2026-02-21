using UnityEngine;

[CreateAssetMenu(fileName = "CityConfig", menuName = "City/Config")]
public class CityConfig : ScriptableObject
{
    public GameScene city;
    public GameController[] requiredGames;
    public GameObject trophyPrefab;
}
