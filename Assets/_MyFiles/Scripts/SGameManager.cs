using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SGameManager : MonoBehaviour
{
    [SerializeField] private Transform character;
    [SerializeField] private Transform characterModel;
    [SerializeField] private Transform terrainHolder;

    [SerializeField] private SCarpet carpetPrefab;
    [SerializeField] private float moveDuration = 0.2f;
    [SerializeField] private int spawnDistance = 20;

    enum GameState
    {
        Ready,
        Moving,
        Dead
    }

    private GameState gamestate;
    private Vector2Int characterPos;
    private int spawnLocation;
    private List<(float terrainHeight, HashSet<int> locations)> obstacles = new();

    private void Awake()
    {
        NewLevel();
    }

    private void NewLevel()
    {
        gamestate = GameState.Ready;

        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.2f, -1);

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
        SCarpet carpet = Instantiate(carpetPrefab, terrainHolder);
        obstacles.Add((0.2f, carpet.Init(spawnLocation)));

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
                }
                while(obstacles.Count < (characterPos.y + spawnDistance))
                {
                    SpawnObstacle();
                }
            }
        }

        Vector3 cameraPos = new(character.position.x + 2, 4, character.position.z - 3);
        cameraPos.x = Mathf.Clamp(cameraPos.x, -1, 5);
        Camera.main.transform.position = cameraPos;
    }

    private IEnumerator IMoveCharacter()
    {
        gamestate = GameState.Moving;
        float elapsedTime = 0f;

        float yHeight = 0.2f;
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
}
