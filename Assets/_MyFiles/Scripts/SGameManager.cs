using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SGameManager : MonoBehaviour
{
    [SerializeField] private Transform character;

    [SerializeField] private float moveDur = 0.2f;

    enum GameState
    {
        Ready,
        Moving,
        Dead
    }

    private GameState gamestate;
    private Vector2Int characterPos;

    private void Awake()
    {
        NewLevel();
    }

    private void NewLevel()
    {
        gamestate = GameState.Ready;

        characterPos = new Vector2Int(0, -1);
        character.position = new Vector3(0, 0.2f, -1);
    }


    private bool InStartArea(Vector2Int location)
    {
        if((location.y > -5) && (location.y < 0) && (location.x < -6) && (location.x < 6))
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

                if (InStartArea(destination))
                {
                    characterPos = destination;

                    StartCoroutine(IMoveCharacter());
                }
            }
        }
    }

    private IEnumerator IMoveCharacter()
    {
        gamestate = GameState.Moving;
        float elapsedTime = 0f;

        float yHeight = 0.2f;
        Vector3 startPos = character.position;
        Vector3 endPos = new(characterPos.x, yHeight, characterPos.y);

        while(elapsedTime < moveDur)
        {
            float percent = elapsedTime / moveDur;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, percent);
            character.position = newPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        character.position = endPos;

        if(gamestate == GameState.Moving)
        {
            gamestate = GameState.Ready;
        }
    }
}
