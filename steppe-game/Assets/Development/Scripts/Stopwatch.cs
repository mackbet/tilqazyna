using TMPro;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private float time;
    [SerializeField] private bool countDown = false;

    private bool isRunning = false;
    private float startTime;

    private void Update()
    {
        if (!isRunning) return;

        if (countDown)
        {
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                time = 0f;
                StopStopwatch();
            }
        }
        else
        {
            time += Time.deltaTime;
        }

        UpdateDisplay();
    }

    public void StartStopwatch()
    {
        isRunning = true;
        startTime = time;
    }

    public void StopStopwatch()
    {
        isRunning = false;
    }

    public void ResetStopwatch()
    {
        time = countDown ? startTime : 0f;
        UpdateDisplay();
    }

    public void RestartStopwatch()
    {
        ResetStopwatch();
        StartStopwatch();
    }

    public void SetTime(float newTime)
    {
        time = newTime;
        startTime = newTime;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (textField == null) return;

        textField.text = Mathf.RoundToInt(time).ToString();
    }

    public float GetTime() => time;
    public bool IsRunning() => isRunning;
}