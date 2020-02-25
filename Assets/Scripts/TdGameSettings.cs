using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerEmoteSprite {
    public Sprite sprite;
    public PlayerEmote emote;
}

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

    public List<PlayerEmoteSprite> playerEmoteSprites;
    public float playerEmoteDuration = 2f;
    
    public string collectableResourceDirectory = "Collectable";

    public EnemySpawner[] enemySpawners;
    public string enemyResourceDirectory = "Enemy";

    public Sprite GetSpriteFromEmote(PlayerEmote emote){
        foreach(PlayerEmoteSprite playerEmoteSprite in playerEmoteSprites){
            if (playerEmoteSprite.emote == emote){
                return playerEmoteSprite.sprite;
            }
        }

        return null;
    }
}
