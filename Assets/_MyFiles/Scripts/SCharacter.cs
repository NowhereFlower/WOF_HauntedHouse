using UnityEngine;

public class SCharacter : MonoBehaviour
{
    [SerializeField] private SGameManager game;
    [SerializeField] private GameObject character;
    [SerializeField] private ParticleSystem deathParticle;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Vehicle") && character.activeSelf)
        {
            Kill(collision.GetContact(0).point);
        }
    }

    public void Kill(Vector3 collisionPoint)
    {
        character.SetActive(false);
        deathParticle.transform.position = collisionPoint;
        deathParticle.transform.LookAt(transform.position + Vector3.up);

        deathParticle.Play();
        game.PlayerCollision();
    }

    public void Reset()
    {
        character.SetActive(true);
        deathParticle.Clear();
    }
}
