using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TdGameSettings : MonoBehaviour
{
    [Header("Initialization")]
    public int castleMaxHealth = 5000;
    public Transform[] playerStartPositions;
    public Color[] playerColors = new Color[]{
        Color.red,
        Color.blue,
        Color.cyan,
        Color.green
    };
    public TdPlayerUi playerUiPrefab;
    public bool progressInstant = false;
    public float progressReloadTime = 1f;
    public float progressCollectTime = 2f;

    public EmoteButton playerEmoteButtonPrefab;
    public List<PlayerEmoteSprite> playerEmoteSprites;
    public float playerEmoteDuration = 2f;
    
    public string collectableResourceDirectory = "Collectable";
    public string turretExtensionResourceDirectory = "TurretExtension";

    public GameObject enemyStunPrefab;
    public EnemySpawner[] enemySpawners;
    public string enemyResourceDirectory = "Enemy";

    public Sprite GetSpriteFromString(string buttonString){
        foreach(PlayerEmoteSprite playerEmoteSprite in playerEmoteSprites){
            if (playerEmoteSprite.buttonString == buttonString){
                return playerEmoteSprite.sprite;
            }
        }

        return null;
    }

    public PlayerEmoteSprite GetPlayerEmoteSpriteFromString(string buttonString){
        foreach(PlayerEmoteSprite playerEmoteSprite in playerEmoteSprites){
            if (playerEmoteSprite.buttonString == buttonString){
                return playerEmoteSprite;
            }
        }

        throw new System.Exception("PlayerEmoteSprite not found.");
    }
}
