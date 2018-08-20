using System;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DebugEx : MonoBehaviour
{
    public bool DebugToScreen = true;
    public bool DebugToConsole = true;
    public bool DisplayFps = true;
    public int MaxLines = 20;

    private readonly List<string> _lines = new List<string>();
    private bool _dirty;
    private Text _textComponent;
    private float _lastTime;
    private float _fps;
    private float _lastFrameCount;

    private static DebugEx _debugEx;

    private void Awake()
    {
        _textComponent = GetComponent<Text>();

        _lastTime = Time.time;
        _lastFrameCount = Time.frameCount;
    }

    private void Update()
    {
        if (DisplayFps)
        {
            if (Time.time - _lastTime > 1)
            {
                _lastTime = Time.time;
                _fps = Time.frameCount - _lastFrameCount;
                _lastFrameCount = Time.frameCount;
            }

            _dirty = true;
        }

        if (_textComponent != null && DebugToScreen && _dirty)
        {
            //_textComponent.pixelOffset = new Vector2(10, Screen.height - 10);

            _textComponent.text = DisplayFps 
                ? "Screen Debug Area:\n" + "FPS: " + _fps + "\n" + Text 
                : "Screen Debug Area:\n" + Text;

            _dirty = false;
        }
    }

    private static DebugEx GetDebugExObject()
    {
        return _debugEx ?? (_debugEx = FindObjectOfType<DebugEx>());
    }

    public static void LogStatic(object o)
    {
        var debugOnScreen = GetDebugExObject();
        if (debugOnScreen != null)
        {
            debugOnScreen.Log(o);
        }
    }

    [StringFormatMethod("format")]
    public static void LogFormatStatic(string format, params object[] args)
    {
        var debugOnScreen = GetDebugExObject();
        if (debugOnScreen != null)
        {
            debugOnScreen.LogFormat(format, args);
        }
    }

    public static void ClearStatic()
    {
        var debugOnScreen = GetDebugExObject();
        if (debugOnScreen != null)
        {
            debugOnScreen.Clear();
        }
    }

    public void Log(object o)
    {
        if (_lines.Count >= MaxLines)
            _lines.RemoveAt(0);

        _lines.Add(o.ToString());

        if (DebugToConsole)
            Debug.Log(o.ToString());

        _dirty = true;
    }

    public void LogException(Exception e)
    {
        Log(e.Message);
    }

    [StringFormatMethod("format")]
    public void LogFormat(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    public void Clear()
    {
        _lines.Clear();
        _dirty = true;
    }

    public string Text
    {
        get { return string.Join("\n", _lines.ToArray()); }
    }
}
