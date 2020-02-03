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

    public void SetTarget(TdPlayerController playerController){
        _playerController = playerController;
        playerNameText.text = playerController.photonView.Owner.NickName;
    }

    private void Update(){
        // If Photon destroys the player controller, we'll remove the ui too.
        if (_playerController == null){
            Destroy(gameObject);
            return;
        }

        playerNameText.transform.position = Camera.main.WorldToScreenPoint(_playerController.transform.position);
    }
}
