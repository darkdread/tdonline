using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class CatapultTurret : Turret, IAudioClipObject
{
    [Header("Initializations")]
    public AudioClipObject audioClipObject;

    public AudioClipObject GetAudioClipObject()
    {
        return audioClipObject;
    }
}
