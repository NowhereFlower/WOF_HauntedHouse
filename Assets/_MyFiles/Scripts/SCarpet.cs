using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class SCarpet : MonoBehaviour
{
    [SerializeField] private Transform lampPrefab;

    public HashSet<int> Init(float z)
    {
        transform.position = new Vector3(0, 0, z);
        HashSet<int> locations = new() { -6, 6 };
        int numLamp = Random.Range(1, 5);
        for(int i = 0; i < numLamp; i++)
        {
            Transform lamp = Instantiate(lampPrefab, transform);
            int xPos = Random.Range(-5, 6);
            lamp.position = new Vector3(xPos, 0.2f, z);

            locations.Add(xPos);
        }

        return locations;
    }
}
