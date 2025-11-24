using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class CraftStuffButton : CustomImageButton
{
    [SerializeField] private LocalizableText localizableText;
    [SerializeField] private SoundButton soundButton;
    private CraftStuffVariant currentStuff;
    private CraftToolsProfession currentProfession;
    private bool isSelected = false;
    private Material outlineMaterial;
    private CraftToolsProfession assignedProfession;
    private CraftToolsProfession correctProfession;
    public bool IsCorrect => assignedProfession == correctProfession;

    public event Action<CraftStuffButton, CraftStuffVariant, CraftToolsProfession> OnStuffClicked;

    public CraftStuffVariant CurrentStuff => currentStuff;
    public CraftToolsProfession CurrentProfession => currentProfession;
    public bool IsSelected => isSelected;

    public void Initialize(CraftStuffVariant stuff, CraftToolsProfession profession)
    {
        currentStuff = stuff;
        currentProfession = profession;
        isSelected = false;

        image.sprite = stuff.Sprite;
        localizableText.SetString(stuff.Phrase.LocalizedString);
        soundButton.SetAudio(stuff.Phrase.LocalizedAudio);

        assignedProfession = profession;

        image.material = Instantiate(image.material);
        outlineMaterial = image.material;
    }

    protected override void Clicked()
    {
        base.Clicked();

        if (isSelected) return;

        OnStuffClicked?.Invoke(this, currentStuff, currentProfession);
    }

    public void SetSelected(bool isCorrect)
    {
        isSelected = true;

        outlineMaterial.SetColor("_OutlineColor", isCorrect ? Color.cyan : Color.red);

    }

    public void Reset()
    {
        currentStuff = null;
        currentProfession = null;
        isSelected = false;
    }

    public void SetCorrectProfession(CraftToolsProfession profession)
    {
        correctProfession = profession;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        soundButton.gameObject.SetActive(false);
    }

    public void HideSoundButton()
    {
        soundButton.gameObject.SetActive(false);
    }
}
