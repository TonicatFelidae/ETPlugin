using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class UIAvatar : MonoBehaviour
{
    public Transform avatar;
    public Image Image => avatar.GetComponent<Image>();
    public void Play(AvatarAnimation avatarAnimation, float speed = 1, TweenCallback callBack = null)
    {
        Sequence sequence = DOTween.Sequence();
        switch (avatarAnimation)
        {
            case AvatarAnimation.ZoomOut:
                avatar.DOScale(0, speed);
                break;
            case AvatarAnimation.ZoomIn:
                avatar.DOScale(1, speed);
                break;
            case AvatarAnimation.ZoomFadeOut:
                sequence.Append(avatar.DOScale(0, speed));
                sequence.Play();
                Image.DOFade(0, speed/2);
                break;
            case AvatarAnimation.ZoomFadeIn:
                sequence.Append(avatar.DOScale(1.1f, speed));
                sequence.Append(avatar.DOScale(1f, speed/2));
                sequence.Play();
                Image.DOFade(1, speed/2);
                break;
            case AvatarAnimation.FadeOut:
                Image.DOFade(0, speed);
                break;
            default:
                break;
        }
    }
    public void Play(AvatarAnimation avatarAnimation, float parameter, float speed, TweenCallback callBack = null)
    {
        Sequence sequence = DOTween.Sequence();
        switch (avatarAnimation)
        {
            case AvatarAnimation.FadeIn:
                Image.DOFade(parameter, speed).OnComplete(callBack);
                break;
            default:
                break;
        }
    }
    public enum AvatarAnimation
    {
        ZoomOut,
        ZoomIn,
        ZoomFadeOut,
        ZoomFadeIn,
        FadeIn,
        FadeOut,

    }
}
