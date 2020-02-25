using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class TdPlayerUi : MonoBehaviour
{
    private TdPlayerController _playerController;
    
    public TextMeshProUGUI playerNameText;
    public Slider playerProgressBar;
    public GameObject playerUseButton;
    public Image playerEmote;

    private float emoteTimeout;

    public void SetTarget(TdPlayerController playerController){
        _playerController = playerController;
        UpdateEmoteList();

        playerNameText.text = playerController.photonView.Owner.NickName;
    }

    public void ShowUseButton(bool show){
        playerUseButton.SetActive(show);
    }

    public void UpdateEmoteList(){
        Transform emoteCanvas = TdGameManager.instance.emoteCanvas;
        foreach(Transform t in emoteCanvas){
            Destroy(t.gameObject);
        }

        foreach(PlayerEmoteSprite playerEmoteSprite in TdGameManager.gameSettings.playerEmoteSprites){
            EmoteButton emoteButton = Instantiate<EmoteButton>(
                TdGameManager.gameSettings.playerEmoteButtonPrefab, emoteCanvas);
            
            emoteButton.SetEmoteButton(playerEmoteSprite.sprite, playerEmoteSprite.emoteButtonDisplayString);
        }
        
    }

    public void ShowEmoteList(bool show){
        TdGameManager.instance.emoteCanvas.gameObject.SetActive(show);
    }

    public void SetEmote(Sprite sprite, float duration){
        emoteTimeout = duration;
        playerEmote.sprite = sprite;
        playerEmote.gameObject.SetActive(sprite != null);
    }

    public void SetProgressBar(float value){
        playerProgressBar.value = value;
    }

    public void ShowProgressBar(bool show){
        playerProgressBar.gameObject.SetActive(show);
    }

    private void Update(){
        // If Photon destroys the player controller, we'll remove the ui too.
        if (_playerController == null){
            Destroy(gameObject);
            return;
        }

        if (TdGameManager.isPaused){
            return;
        }
        
        emoteTimeout -= Time.deltaTime;
        if (emoteTimeout <= 0 && playerEmote.gameObject.activeSelf){
            SetEmote(null, 0f);
        }
        transform.position = Camera.main.WorldToScreenPoint(_playerController.transform.position);
        // playerNameText.transform.position = Camera.main.WorldToScreenPoint(_playerController.transform.position);
    }
}
