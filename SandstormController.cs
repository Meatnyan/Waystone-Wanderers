using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandstormController : MonoBehaviour {

    ActivatedAbilitySandstorm activatedAbilitySandstorm;

    GameObject rotatorObject;

    GameObject shotPositionObject;

    SpriteRenderer spriteRenderer;

    float startingDuration;

    bool flashing = false;

    PlayerController playerController;

	void Start () {
        playerController = transform.root.GetComponent<PlayerController>();
        activatedAbilitySandstorm = transform.parent.GetComponent<ActivatedAbilitySandstorm>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rotatorObject = transform.GetChild(0).gameObject;
        shotPositionObject = rotatorObject.transform.GetChild(0).gameObject;
        activatedAbilitySandstorm.allowTrigger = false;
        startingDuration = activatedAbilitySandstorm.duration;
        Destroy(gameObject, startingDuration);
	}

    private void OnDestroy()
    {
        activatedAbilitySandstorm.timeOfAbilityEnd = Time.time;
    }

    private void Update()
    {
        if (flashing == false && activatedAbilitySandstorm.timeOfAbilityTrigger + startingDuration - 1f <= Time.time)
            StartCoroutine(Flasher());
    }

    IEnumerator Flasher()
    {
        flashing = true;
        while (true)
        {
            spriteRenderer.color = new Color(0.7f, 0.7f, 0.7f);
            yield return new WaitForSeconds(0.03f);
            spriteRenderer.color = new Color(0.85f, 0.85f, 0.85f);
            yield return new WaitForSeconds(0.03f);
            spriteRenderer.color = new Color(1f, 1f, 1f);
            yield return new WaitForSeconds(0.03f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyShot"))
        {
            GameObject enemyShotObject = collision.gameObject;
            EnemyShotMover enemyShotMover = enemyShotObject.GetComponent<EnemyShotMover>();
            enemyShotObject.transform.parent = rotatorObject.transform;
            enemyShotObject.transform.position = shotPositionObject.transform.position;
            rotatorObject.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            if (playerController.flipped == false)
            {
                enemyShotObject.transform.localRotation = Quaternion.identity;
                enemyShotObject.GetComponent<Rigidbody2D>().velocity = enemyShotObject.transform.right * Random.Range(3f, 4f);
            }
            else
            {
                enemyShotObject.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                enemyShotObject.GetComponent<Rigidbody2D>().velocity = -enemyShotObject.transform.right * Random.Range(3f, 4f);
            }
            enemyShotObject.GetComponent<SpriteRenderer>().color = new Color(0.65f, 1f, 0f);
            enemyShotMover.isFriendly = true;
            Physics2D.IgnoreCollision(enemyShotObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            Physics2D.IgnoreCollision(enemyShotObject.GetComponent<Collider2D>(), transform.root.GetComponent<Collider2D>());
            enemyShotObject.transform.parent = null;
        }
    }
}
