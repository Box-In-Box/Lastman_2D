using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    int direction;
    public float timer; //패널 이동 간 딜레이 코루틴 스탑으로 설명 버그 수정

    [SerializeField] float attackDelay0; //default = 0.5
    [SerializeField] float attackDelay1; //default = 5
    [SerializeField] float defenceDelay; //default = 2
    [SerializeField] bool attackable0 = true;
    [SerializeField] bool attackable1 = true;
    [SerializeField] bool defensible = true;

    public Transform attackPosition;
    public Transform defencePosition;
    public GameObject bulletObject0;
    public GameObject bulletObject1;

    [Header("-----Audio Clip-----")]
    public AudioClip attck_0_clip;
    public AudioClip attck_1_clip;
    public AudioClip defence_clip;

    private void Update()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            dir.x = -1;
            direction = 3;
            animator.SetInteger("Direction", 3);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            dir.x = 1;
            direction = 2;
            animator.SetInteger("Direction", 2);
        }

        if (Input.GetKey(KeyCode.W))
        {
            dir.y = 1;
            direction = 1;
            animator.SetInteger("Direction", 1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dir.y = -1;
            direction = 0;
            animator.SetInteger("Direction", 0);
        }

        dir.Normalize();
        animator.SetBool("IsMoving", dir.magnitude > 0);

        Shot();
        Defence();
        TimeReset();
    }

    void Shot()
    {
        if (attackable0 && Input.GetKeyDown(KeyCode.Mouse0)) {
            attackable0 = false;
            StartCoroutine(AttackDelayCoroutine0(attackDelay0));
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - attackPosition.transform.position;
            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            attackPosition.rotation = Quaternion.AngleAxis(angle , Vector3.forward);

            GameObject go = Instantiate(bulletObject0, attackPosition.transform.position, attackPosition.rotation);
            SoundManager.instance.PlayerSFXPlay(audioSource, "Attack_0", attackPosition, attck_0_clip);
            timer = 0;
        }

        if (attackable1 && Input.GetKeyDown(KeyCode.Mouse1)) {
            attackable1 = false;
            StartCoroutine(AttackDelayCoroutine1(attackDelay1));
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - attackPosition.transform.position;
            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            attackPosition.rotation = Quaternion.AngleAxis(angle , Vector3.forward);

            GameObject go = Instantiate(bulletObject1, attackPosition.transform.position, attackPosition.rotation);
            SoundManager.instance.PlayerSFXPlay(audioSource, "Attack_1", attackPosition, attck_1_clip);
        }
    }

    void Defence()
    {
        if (defensible && Input.GetKeyDown(KeyCode.Space)) {
            defensible = false;

            defencePosition.GetChild(direction).gameObject.SetActive(true);
            defencePosition.GetChild(direction).gameObject.GetComponent<LobbyDefenceScript>().DefenceActiveFalse();

            SoundManager.instance.PlayerSFXPlay(audioSource, "Defence", attackPosition, defence_clip);
            StartCoroutine(DefenceDelayCoroutine(defenceDelay));
        }
    }

    IEnumerator AttackDelayCoroutine0(float attackDelay)
    {
        yield return new WaitForSeconds(attackDelay);
        attackable0 = true;
    }

    IEnumerator AttackDelayCoroutine1(float attackDelay)
    {
        yield return new WaitForSeconds(attackDelay);
        attackable1 = true;
    }

    IEnumerator DefenceDelayCoroutine(float defenceDelay)
    {
        yield return new WaitForSeconds(defenceDelay);
        defensible = true;
    }

    void TimeReset()
    {
        if (attackable0 == false) {
            if (timer == 0) {
                timer = Time.deltaTime;
            }
            if (timer >= attackDelay0 * 2) {
                attackable0 = true;
                attackable1 = true;
                defensible = true;
                timer = 0;
            }
            timer += Time.deltaTime;
        }
    }
}
