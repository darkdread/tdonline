using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class LoseUi : MonoBehaviour
{
    public TextMeshProUGUI shotStone;
    public Button backToMain;

    private void Awake(){
        backToMain.onClick.AddListener(TdGameManager.PlayerLeaveGame);
    }
}
