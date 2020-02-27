using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Photon.Realtime;
using Photon.Pun;

public interface ITdProgressBarUi {

    TdProgressBarUi progressBarUi {get; set;}

    [PunRPC]
    void StartProgressBarRpc(float duration);

    [PunRPC]
    void StopProgressBarRpc();
}

public static class ITdProgressBarUiHelper {
    
    public static void StartProgressBar(this ITdProgressBarUi iTdProgressBarUi, float duration){
        iTdProgressBarUi.progressBarUi.progressCurrent = 0f;
        iTdProgressBarUi.progressBarUi.progressMax = duration;
        iTdProgressBarUi.progressBarUi.SetProgressBar(0f);
        iTdProgressBarUi.progressBarUi.ShowProgressBar(true);
    }

    public static void StopProgressBar(this ITdProgressBarUi iTdProgressBarUi){
        iTdProgressBarUi.progressBarUi.progressMax = -1f;
        iTdProgressBarUi.progressBarUi.ShowProgressBar(false);
    }
}

public class TdProgressBarUi : MonoBehaviour
{
    public PhotonView _referencedPhotonView;
    public Slider playerProgressBar;

    public float progressMax = -1f;
    public float progressCurrent;
    public System.Action progressCallback;

    private RectTransform rectTransform;

    public static TdProgressBarUi Spawn(PhotonView owner){
        TdProgressBarUi progressBar = Instantiate<TdProgressBarUi>(TdGameManager.gameSettings.progressBarUiPrefab, TdGameManager.instance.gameUiCanvas);
        progressBar._referencedPhotonView = owner;
        return progressBar;
    }

    private void Awake(){
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetProgressBar(float value){
        playerProgressBar.value = value / progressMax;
    }

    public void ShowProgressBar(bool show){
        playerProgressBar.gameObject.SetActive(show);
    }

    public void StartProgressBar(float duration, System.Action callback){
        progressCallback = callback;
        _referencedPhotonView.RPC("StartProgressBarRpc", RpcTarget.All, duration);
    }

    public void StopProgressBar(){
        progressCallback = null;
        _referencedPhotonView.RPC("StopProgressBarRpc", RpcTarget.All);
    }

    public void CompleteProgressBar(){
        if (progressCallback != null){
            // print(progressCallback.Method.Name);
            progressCallback.Invoke();
            progressCallback = null;
        }

        StopProgressBar();
    }

    public bool IsRunning(){
        return progressMax != -1f;
    }

    private void Update(){
        // If Photon destroys the photonview, we'll remove the ui too.
        if (_referencedPhotonView == null){
            Destroy(gameObject);
            return;
        }

        if (TdGameManager.isPaused){
            return;
        }

        if (IsRunning()){
            progressCurrent += Time.deltaTime;
            SetProgressBar(progressCurrent);

            if (_referencedPhotonView.IsMine){
                if (progressCurrent > progressMax){
                    CompleteProgressBar();
                }
            }
        }
        
        transform.position = Camera.main.WorldToScreenPoint(_referencedPhotonView.transform.position);
        // rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x - (rectTransform.rect.width / 2),
        //     rectTransform.anchoredPosition.y);
    }
}
