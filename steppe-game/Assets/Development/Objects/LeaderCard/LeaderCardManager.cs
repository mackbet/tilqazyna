using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderCardManager : MonoBehaviour
{
    [Header("Text elements")]
    [SerializeField] private TextMeshProUGUI placeText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Avatar images")]
    [SerializeField] private Sprite boySprite;
    [SerializeField] private Sprite girlSprite;
    [SerializeField] private Image image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCardInfo(UserModel userModel, int userPlace)
    {
        placeText.text = userPlace.ToString();
        nameText.text = userModel.Name;
        pointsText.text = userModel.Points.ToString();
        levelText.text = userModel.Level.ToString();

        // 0 = Male, 1 = Female
        image.sprite = userModel.Sex == 0 ? boySprite : girlSprite;
    }
}
