using UnityEngine;
using UnityEngine.UI;
using System;

public class AnimatedImage : MonoBehaviour
{
    public enum PlaybackMode { ONESHOT, REPEAT, PINGPONG }

    public PlaybackMode mode = PlaybackMode.REPEAT;
    public float frameTime = 0.1f;
    public bool autoplay = false;
    public Sprite[] sprites;

    public event Action OnAnimationFinished;

    private float t;
    private int idx;
    private bool forward;
    private Image img;
    private SpriteRenderer sr;
    private bool started;

    public void Init()
    {
        forward = true;
        sr = GetComponent<SpriteRenderer>();
        img = GetComponent<Image>();
        t = 0;
        idx = 0;
        started = autoplay;
    }

    public void Tick(float dT)
    {
        if (isFinished())
            return;

        t += dT;
        if (t > frameTime)
        {
            t = 0;
            NextFrame();
        }
    }

    public bool isFinished()
    {
        return !started || (mode == PlaybackMode.ONESHOT && idx >= sprites.Length);
    }

    public void Restart()
    {
        idx = 0;
        t = 0;
        started = true;
    }

    public void Stopp()
    {
        started = false;
    }

    private void NextFrame()
    {
        if (mode == PlaybackMode.PINGPONG && !forward)
            idx--;
        else
            idx++;

        if (idx < 0)
        {
            idx = 0;
            forward = true;
        }
        else if (idx >= sprites.Length)
        {
            OnAnimationFinished?.Invoke();
            if (mode == PlaybackMode.ONESHOT)
                return;
            else if (mode == PlaybackMode.REPEAT)
                idx = 0;
            else if (mode == PlaybackMode.PINGPONG)
            {
                idx = sprites.Length - 1;
                forward = false;
            }
        }
        if (img != null)
            img.sprite = sprites[idx];
        else if (sr != null)
            sr.sprite = sprites[idx];
    }
}
