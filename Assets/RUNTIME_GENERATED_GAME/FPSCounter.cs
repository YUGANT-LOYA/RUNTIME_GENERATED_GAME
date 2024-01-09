using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float _deltaTime;
    private float _fps;
    private float _timer;
    [SerializeField] private TextMeshProUGUI fpsCounterText;
    [SerializeField] private float updateInterval;
    
    private void Start()
    {
        Application.targetFrameRate = 120;
        
    }

    private void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        _timer += Time.unscaledDeltaTime;

        // Update FPS every updateInterval seconds
        if (_timer > updateInterval)
        {
            _fps = 1.0f / _deltaTime;
            _timer -= updateInterval;
        }

        fpsCounterText.text = $"FPS:{Mathf.Round(_fps)}";
    }
}