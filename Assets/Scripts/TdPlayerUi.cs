using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class TdPlayerUi : MonoBehaviour
{
    private TdPlayerController _playerController;
    public TextMeshProUGUI playerNameText;
    public GameObject playerUseButton;

    public void SetTarget(TdPlayerController playerController){
        _playerController = playerController;

        playerNameText.text = playerController.photonView.Owner.NickName;
    }

    public void ShowUseButton(bool show){
        playerUseButton.SetActive(show);
    }

    private void Update(){
        // If Photon destroys the player controller, we'll remove the ui too.
        if (_playerController == null){
            Destroy(gameObject);
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_playerController.transform.position);
        // playerNameText.transform.position = Camera.main.WorldToScreenPoint(_playerController.transform.position);
    }
}
