using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValkyrieSword : MonoBehaviour
{
    public float raycastDist;

    public float distFromWaveTip;

    public LayerMask layerMask;

    MeleeAttacker meleeAttacker;

    private void Awake()
    {
        meleeAttacker = GetComponent<MeleeAttacker>();

        meleeAttacker.additionalOnSwingEffects += RelocatePlayer;
    }

    void RelocatePlayer()
    {
        Vector2 raycastDirection = (meleeAttacker.attackWaveSpawns[0].position - meleeAttacker.playerController.transform.position).normalized;

        RaycastHit2D raycastHit = Physics2D.Raycast(meleeAttacker.playerController.transform.position, raycastDirection, raycastDist, layerMask);

        Vector2 hitPos = raycastHit.collider != null ? raycastHit.point : (Vector2)meleeAttacker.playerController.transform.position + raycastDirection * raycastDist;
        meleeAttacker.playerController.transform.position = hitPos - raycastDirection * distFromWaveTip;
    }
}
