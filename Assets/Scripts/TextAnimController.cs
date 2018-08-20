using UnityEngine;
using UnityEngine.UI;

public class TextAnimController : MonoBehaviour
{
    public Color[] colors;
    public float delay;

    private TextMesh _textMesh;
    private Text _textUI;
    private float _nextTime;
    private int _colorIndex;

    void Awake()
    {
        _textMesh = GetComponent<TextMesh>();
        _textUI = GetComponent<Text>();
    }

    void Update()
    {
        if (Time.time >= _nextTime)
        {
            if (_textMesh != null)
                _textMesh.color = colors[_colorIndex];
            else if(_textUI != null)
                _textUI.color = colors[_colorIndex];

            _colorIndex = (_colorIndex + 1) % colors.Length;

            _nextTime = Time.time + delay;
        }
    }
}
