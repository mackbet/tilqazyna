using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StreamOfWordsTarget : CustomImageButton
{
    [SerializeField] private Phrase phrase;
    [SerializeField] private Material material;
    [SerializeField] private float materialDuration = 2f;

    private Coroutine materialCoroutine;

    public Phrase Phrase => phrase;
    public string Word => phrase.LocalizedString.GetLocalizedString();

    public event Action<StreamOfWordsTarget> OnTargetClicked;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif

    public void SetPhrase(Phrase newPhrase)
    {
        phrase = newPhrase;
    }

    protected override void Clicked()
    {
        base.Clicked();
        OnTargetClicked?.Invoke(this);
    }

    public void ApplyTemporaryMaterial()
    {
        if (image == null || material == null) return;

        // Останавливаем предыдущую корутину, если она запущена
        if (materialCoroutine != null)
        {
            StopCoroutine(materialCoroutine);
        }

        // Запускаем новую корутину
        materialCoroutine = StartCoroutine(ApplyMaterialCoroutine());
    }

    private IEnumerator ApplyMaterialCoroutine()
    {
        // Применяем эффект материала
        image.material = material;

        // Ждем указанное время
        yield return new WaitForSeconds(materialDuration);

        // Возвращаем оригинальный материал
        image.material = null;

        materialCoroutine = null;
    }

    public void SetMaterialDuration(float duration)
    {
        materialDuration = duration;
    }

    private void OnDisable()
    {
        // Останавливаем корутину и восстанавливаем материал при деактивации
        if (materialCoroutine != null)
        {
            StopCoroutine(materialCoroutine);
            materialCoroutine = null;
        }

        if (image != null)
        {
            image.material = null;
        }
    }
}
