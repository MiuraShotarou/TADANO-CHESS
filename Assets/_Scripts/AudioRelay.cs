using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioRelay : MonoBehaviour
{
    [SerializeField] InGameManager _inGameManager;
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
}
