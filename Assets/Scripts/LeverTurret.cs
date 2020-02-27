using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class LeverTurret : Turret, IAudioClipObject
{
    [Header("Initializations")]
    public AudioClipObject audioClipObject;

    public AudioClipObject GetAudioClipObject()
    {
        return audioClipObject;
    }
}
