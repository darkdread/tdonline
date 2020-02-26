using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class LeverTurret : Turret, IAudioClipObject
{
    public AudioClipObject audioClipObject;

    public AudioClipObject GetAudioClipObject()
    {
        return audioClipObject;
    }
}
