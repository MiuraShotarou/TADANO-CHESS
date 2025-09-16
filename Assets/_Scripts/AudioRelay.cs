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
}
