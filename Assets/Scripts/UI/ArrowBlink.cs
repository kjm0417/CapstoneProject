using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArrowBlink : MonoBehaviour
{
    [SerializeField]
    private float fadeTime;
    private Image fadeImage; //페이드 효과에 사용되는 이미지 -> arrow

    private void Awake()
    {
        fadeImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        StartCoroutine(FadeInOut());
    }

    private void OnDisable()
    {
        StopCoroutine(FadeInOut()); //코루틴 종료
    }

    IEnumerator FadeInOut()
    {
        while (true)
        {
            yield return StartCoroutine(Fade(1, 0)); //페이드 인

            yield return StartCoroutine(Fade(0, 1)); //페이드 아웃
        }
    }

    IEnumerator Fade(float start,float end)
    {
        float current = 0;
        float percent = 0;

        while(percent < 1)
        {
            current += Time.deltaTime;
            percent = current / fadeTime;

            Color color = fadeImage.color;
            color.a = Mathf.Lerp(start, end, percent); //퍼센트로 보간
            fadeImage.color = color;

            yield return null;
        }

    }
}
