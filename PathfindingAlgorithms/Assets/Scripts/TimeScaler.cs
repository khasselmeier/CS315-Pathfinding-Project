using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    [Header("Available Time Speeds")]
    public float normalSpeed = 1f;
    public float speed1_5 = 1.5f;
    public float speed2 = 2f;
    public float speed3 = 3f;

    void Start()
    {
        // Start at normal time speed
        SetTimeScale(normalSpeed);
    }

    public void SetNormalSpeed()
    {
        SetTimeScale(normalSpeed);
    }

    public void Set1_5Speed()
    {
        SetTimeScale(speed1_5);
    }

    public void Set2xSpeed()
    {
        SetTimeScale(speed2);
    }

    public void Set3xSpeed()
    {
        SetTimeScale(speed3);
    }

    private void SetTimeScale(float value)
    {
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log($"Time Scale set to {value}x");
    }
}