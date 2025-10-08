using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class FadeInOutManager : SingleTonGeneric<FadeInOutManager>
{
    [SerializeField] Image FadeImage;
    [SerializeField] float Fadeduration = 2f;
    [SerializeField] float FadeSceneDelay;
    public bool isFadingOut = false;
    private bool isFadingIn = false;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isFadingIn)
            StartCoroutine(WaitFadeOutThenFadeIn());
    }

    private IEnumerator WaitFadeOutThenFadeIn()
    {
        while (isFadingOut)
            yield return null;

        isFadingIn = true;
        yield return StartCoroutine(FadeIn());
        isFadingIn = false;
    }

    public void FadeOutAndLoad(System.Action onComplete = null)
    {
        StartCoroutine(FadeOutAndLoadRoutine(onComplete));
    }

    private IEnumerator FadeOutAndLoadRoutine(System.Action onComplete)
    {
        isFadingOut = true;
        yield return StartCoroutine(FadeOut());

        if (FadeSceneDelay > 0)
            yield return new WaitForSeconds(FadeSceneDelay);

        isFadingOut = false;
        onComplete?.Invoke(); // FadeIn은 SceneLoaded에서 실행
    }

    public void FadeInLoad()
    {
        StartCoroutine(WaitFadeOutThenFadeIn());
    }
    private IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = FadeImage.color;
        c.a = 1f;
        FadeImage.color = c;

        while (t < Fadeduration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / Fadeduration);
            FadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        FadeImage.color = c;
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color c = FadeImage.color;
        c.a = 0f;
        FadeImage.color = c;

        while (t < Fadeduration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / Fadeduration);
            FadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        FadeImage.color = c;
    }
}
