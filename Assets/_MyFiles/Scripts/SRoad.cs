using System.Collections.Generic;
using UnityEngine;

public class SRoad : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> vechicals;

    private int direction = 1;
    private float speed = 1f;
    private List<Rigidbody> spawnedVechicals = new();

    public HashSet<int> Init(float z)
    {
        transform.position = new Vector3(0, 0, z);
        direction = 2 * Random.Range(0, 2) - 1;

        float minSpeed = Mathf.Lerp(1f, 5f, z / 500f);
        float maxSpeed = Mathf.Lerp(5f, 10f, z / 500f);
        speed = Random.Range(minSpeed, maxSpeed);

        int idx = Random.Range(0, vechicals.Count);
        int vechicalCount = Random.Range(1, 5);
        float gap = Random.Range(2f, 6f);

        for(int i = 0; i < vechicalCount; i++)
        {
            Rigidbody vechicle = Instantiate(vechicals[idx], new Vector3((i * gap) * -direction, 0.1f, z), Quaternion.Euler(0f, 90 * direction, 0f), transform);
            spawnedVechicals.Add(vechicle);
        }
        return new() { -6, 6 };
    }

    private void FixedUpdate()
    {
        foreach(Rigidbody vehicle in spawnedVechicals)
        {
            Vector3 moveAmount = new(speed * direction * Time.fixedDeltaTime, 0f, 0f);
            vehicle.MovePosition(vehicle.position + moveAmount);

            Vector3 pos = vehicle.position;
            if((direction > 0) && (pos.x > 12))
            {
                pos.x = -12;
                vehicle.position = pos;
            }
            else if((direction < 0) && (pos.x < -12))
            {
                pos.x = 12;
                vehicle.position= pos;
            }
        }
    }
}
