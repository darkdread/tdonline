using Unity;
using UnityEngine;

interface IAudioClipObject {
    AudioClipObject GetAudioClipObject();
}

[System.Serializable]
public struct AudioClipName {
    public AudioClip[] clips;
    public string name;
}

[CreateAssetMenu(fileName = "AudioClipObject", menuName = "AudioClipObject")]
public class AudioClipObject: ScriptableObject {
    public AudioClipName[] audioClipNames = new AudioClipName[]{
        new AudioClipName(){
            name = "Attack"
        },
        new AudioClipName(){
            name = "Death"
        }
    };

    public AudioClip GetAudioClipFromString(string clipName, int indexOfClip = -1){
        foreach(AudioClipName audioClipName in audioClipNames){
            if (audioClipName.name == clipName){

                if (audioClipName.clips.Length <= 0){
                    return null;
                }
                
                // Get random clip.
                if (indexOfClip == -1){
                    return audioClipName.clips[Random.Range(0, audioClipName.clips.Length)];
                }

                return audioClipName.clips[indexOfClip];
            }
        }

        return null;
    }
}