using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

public class SGameManager : MonoBehaviour
{
    [SerializeField] private Transform character;
    [SerializeField] private Transform characterModel;
    [SerializeField] private Transform terrainHolder;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private GameObject panel;

    [SerializeField] private SCarpet carpetPrefab;
    [SerializeField] private SRoad roadPrefab;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private int spawnDistance = 20;
    private int score = 0;

    enum GameState
    {
        Ready,
        Moving,
        Dead
    }

    private GameState gamestate;
    private Vector2Int characterPos;
    private int spawnLocation;
    private List<(float terrainHeight, HashSet<int> locations, GameObject obj)> obstacles = new();



    private void Awake()
    {
        
        NewLevel();
    }

    private void NewLevel()
    {
        panel.SetActive(true);
        gamestate = GameState.Ready;

        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.2f, -1);
        character.GetComponent<SCharacter>().Reset();
        score = 0;
        scoreText.text = "0";

        obstacles.Clear();
        foreach(Transform child in terrainHolder)
        {
            Destroy(child.gameObject);
        }

        spawnLocation = 0;
        for(int i = 0; i < spawnDistance; i++)
        {
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        float roadProbaility = Mathf.Lerp(0.5f, 0.9f, spawnDistance / 250f);

        if (UnityEngine.Random.value < roadProbaility)
        {
            SRoad road = Instantiate(roadPrefab, terrainHolder);
            obstacles.Add((0.1f, road.Init(spawnLocation), road.gameObject));
            road.gameObject.name = $"{spawnLocation} - Road";
        }
        else
        {
            SCarpet carpet = Instantiate(carpetPrefab, terrainHolder);
            obstacles.Add((0.2f, carpet.Init(spawnLocation), carpet.gameObject));
            carpet.gameObject.name = $"{spawnLocation} - Carpet";

        }

            spawnLocation++;
    }


    private bool InStartArea(Vector2Int location)
    {
        if((location.y > -5) && (location.y < 0) && (location.x > -6) && (location.x < 6))
        {
            return true;
        }
        return false;
    }

    private void Update()
    {
        if(gamestate == GameState.Ready)
        {
            Vector2Int moveDirection = Vector2Int.zero;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.identity;
                moveDirection.y = 1;
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, 180, 0);
                moveDirection.y = -1;
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, -90, 0);
                moveDirection.x = -1;
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                character.localRotation = Quaternion.Euler(0, 90, 0);
                moveDirection.x = 1;
            }

            if(moveDirection != Vector2Int.zero)
            {
                Vector2Int destination = characterPos + moveDirection;

                if (InStartArea(destination) || ((destination.y >= 0) && !obstacles[destination.y].locations.Contains(destination.x)))
                {
                    characterPos = destination;

                    StartCoroutine(IMoveCharacter());
                    if((destination.y + 1) > score)
                    {
                        score = destination.y + 1;
                        scoreText.text = $"{score}";
                    }
                }
                while(obstacles.Count < (characterPos.y + spawnDistance))
                {
                    SpawnObstacle();

                    int oldIndex = characterPos.y - spawnDistance;
                    if((oldIndex >= 0) && (obstacles[oldIndex].obj != null))
                    {
                        Destroy(obstacles[oldIndex].obj);
                    }
                }

                if(characterPos.y < (score - 10)){
                    character.GetComponent<SCharacter>().Kill(character.transform.position + new Vector3(0,0.2f, 0.5f));
                }
            }
        }

        if(gamestate == GameState.Dead && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NewLevel();
        }

        Vector3 cameraPos = new(character.position.x + 2, 4, character.position.z - 3);
        cameraPos.x = Mathf.Clamp(cameraPos.x, -1, 5);
        Camera.main.transform.position = cameraPos;
    }

    private IEnumerator IMoveCharacter()
    {
        gamestate = GameState.Moving;
        panel.SetActive(false);
        float elapsedTime = 0f;

        float yHeight = 0.2f;
        if(characterPos.y >= 0)
        {
            yHeight = obstacles[characterPos.y].terrainHeight;
        }
        Vector3 startPos = character.position;
        Vector3 endPos = new(characterPos.x, yHeight, characterPos.y);
        Quaternion startRotation = characterModel.localRotation;
        while(elapsedTime < moveDuration)
        {
            float percent = elapsedTime / moveDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, percent);
            newPos.y = yHeight + (0.5f * Mathf.Sin(Mathf.PI * percent));

            character.position = newPos;

            Vector3 rotation = characterModel.localRotation.eulerAngles;
            characterModel.localRotation = Quaternion.Euler(-5f * Mathf.PI * Mathf.Cos(Mathf.PI * percent), rotation.y, rotation.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        character.position = endPos;
        characterModel.localRotation = startRotation;

        if(gamestate == GameState.Moving)
        {
            gamestate = GameState.Ready;
        }

        
    }
    public void PlayerCollision()
    {
        gamestate = GameState.Dead;
    }
}
