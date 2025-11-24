using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuletGame3D : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int playerHP = 3;
    public int enemyHP = 3;

    [Header("Body Transformlarý")]
    public Transform playerBody;           //Player düþsün istersek bunu kullanalým.
    public Transform enemyBody;

    [Header("Yere Düþme Ayarlarý")]
    public float knockDownAngle = 80f;
    public float knockDuration = 0.2f;
    public float standUpDelay = 0.6f;

    private bool canRoll = true;
    private bool playerTurnToShoot;
    private bool enemyTurnToShoot;

    void Update()
    {
        if (canRoll && Input.GetMouseButtonDown(0))
        {
            RollDice();
        }

        if (playerTurnToShoot && Input.GetMouseButtonDown(0))
        {
            PlayerShoot();
        }
    }

    void RollDice()                       //Zar atma mantýðý dostum
    {
        canRoll = false;
        playerTurnToShoot = false;
        enemyTurnToShoot = false;

        int playerRoll = Random.Range(1, 13);
        int enemyRoll = Random.Range(1, 13);

        Debug.Log($"Player: {playerRoll}  Enemy: {enemyRoll}");

        if (playerRoll > enemyRoll)
        {
            Debug.Log("Player kazandý, bombastik atýþ geliyor...");
            playerTurnToShoot = true;
        }
        else if(playerRoll < enemyRoll)
        {
            Debug.Log("Enemy kazandý, enayi vurmayý deneyecek...");
            enemyTurnToShoot = true;
        }
        else
        {
            Debug.Log("Berabere, moto moto bidaha atýyor...");
            canRoll = true;
        }
    }

    void PlayerShoot()                  //Babaððð ateþ etme olayý
    {
        playerTurnToShoot = false;
        enemyHP --;
        Debug.Log("Babaððð pompiþledi! Enemy HP: " + enemyHP);

        StartCoroutine(KnockDownAndUp(enemyBody));
        CheckEndOrNextRound();
    }

    IEnumerator EnemyShootRoutine()
    {
        enemyTurnToShoot = false ;
        yield return new WaitForSeconds(1f);

        playerHP --;
        Debug.Log($"Ucube Ateþ etti. Player :{playerHP}");

        CheckEndOrNextRound();
    }

    IEnumerator KnockDownAndUp(Transform target)
    {
        if (target == null) yield break;

        Quaternion startRot = target.rotation;
        Quaternion knockedRot = Quaternion.Euler(
            target.eulerAngles.x + knockDownAngle,
            target.eulerAngles.y,
            target.eulerAngles.z
        );

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / knockDuration;
            target.rotation =   Quaternion.Slerp(startRot, knockedRot, t);
            yield return null;
        }
        yield return new WaitForSeconds (standUpDelay);

        t = 0f;
        while(t < 1f)
        {
            t += Time.deltaTime / knockDownAngle;
            target.rotation = Quaternion.Slerp(knockedRot, startRot, t);

        }
    }

    void CheckEndOrNextRound()
    {
        if(playerHP <= 0)
        {
            Debug.Log("Babaðððððð öldü. Kaybettik goddammet");
            return;
        }
        if (enemyHP <= 0)
        {
            Debug.Log("Babaðððððð pompiþlediiii. Kazandýk ihtiyar");
            return;
        }

        canRoll = true;
        Debug.Log("Tekrar zar at bakalým.");
    }
}
