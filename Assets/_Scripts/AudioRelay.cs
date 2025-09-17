using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioRelay : MonoBehaviour
{
    [SerializeField] InGameManager _inGameManager;

    void InGame000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["I"].FirstOrDefault(c => c.name.Contains("0")); //鐘の音
        _inGameManager._SEAudioSource.Play();
    }
    void InGame001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["I"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void InGame002()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["I"].FirstOrDefault(c => c.name.Contains("2"));
        _inGameManager._SEAudioSource.Play();
    }
    void PAttack000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["P"].FirstOrDefault(c => c.name.Contains("0"));
        _inGameManager._SEAudioSource.Play();
    }
    void PAttack001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["P"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void PAttack002()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["P"].FirstOrDefault(c => c.name.Contains("2"));
        _inGameManager._SEAudioSource.Play();
    }
    void RAttack001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["R"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void RAttackEffect000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["R"].FirstOrDefault(c => c.name.Contains("2"));
        _inGameManager._SEAudioSource.Play();
    }
    void NAttack000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["N"].FirstOrDefault(c => c.name.Contains("0"));
        _inGameManager._SEAudioSource.Play();
    }
    void NAttack001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["N"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void NAttack002()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["N"].FirstOrDefault(c => c.name.Contains("2"));
        _inGameManager._SEAudioSource.Play();
    }
    void NAttack003()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["N"].FirstOrDefault(c => c.name.Contains("3"));
        _inGameManager._SEAudioSource.Play();
    }
    void BAttack000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["B"].FirstOrDefault(c => c.name.Contains("0"));
        _inGameManager._SEAudioSource.Play();
    }
    void BAttack001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["B"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void QAttack000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["Q"].FirstOrDefault(c => c.name.Contains("0"));
        _inGameManager._SEAudioSource.Play();
    }
    void QAttack001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["Q"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void QAttack002()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["Q"].FirstOrDefault(c => c.name.Contains("2"));
        _inGameManager._SEAudioSource.Play();
    }
    void KAttack000()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["K"].FirstOrDefault(c => c.name.Contains("0"));
        _inGameManager._SEAudioSource.Play();
    }
    void KAttack001()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["K"].FirstOrDefault(c => c.name.Contains("1"));
        _inGameManager._SEAudioSource.Play();
    }
    void KAttack002()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["K"].FirstOrDefault(c => c.name.Contains("2"));
        _inGameManager._SEAudioSource.Play();
    }
    void KAttack003()
    {
        _inGameManager._SEAudioSource.clip = _inGameManager._SEAudioClipDict["K"].FirstOrDefault(c => c.name.Contains("3"));
        _inGameManager._SEAudioSource.Play();
    }
}
