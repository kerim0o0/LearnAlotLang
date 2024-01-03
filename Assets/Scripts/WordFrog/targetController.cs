using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class targetController : MonoBehaviour
{
    [SerializeField]
    private GameObject wing1, wing2, boomEffect;
    private TextMesh letter;
    private bool canBobb, targetActive;
    private Color targetColor;
    private GameObject shine;
    private char currentLetter;

    void Start()
    {
        canBobb = true;
        targetActive = true;

        letter = (TextMesh)transform.Find("letter").GetChild(0).GetComponent("TextMesh");
        ((MeshRenderer)transform.Find("letter").GetChild(0).GetComponent("MeshRenderer")).sortingOrder = 5;

        shine = transform.Find("shine").gameObject;
        shine.SetActive(false);

        StartCoroutine(moveTarget());
        StartCoroutine(flapWings());

        targetColor = transform.Find("letter").GetComponent<SpriteRenderer>().color;
    }

    void Update()
    {
        if (canBobb)
        {
            float newY = transform.position.y + Mathf.Sin(Time.time * 2f) * 0.0005f;

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private IEnumerator moveTarget() {

        int sibIndex = transform.GetSiblingIndex();
        if (sibIndex == 0) sibIndex++;
        while (targetActive)
        {
            canBobb = false;
            this.transform.DOMove(new Vector3(Random.Range(-6.5f, 6.5f), Random.Range(0.03f, 2f), 90f), 1.5f).OnComplete(() => {
            canBobb = true;
            });
            yield return new WaitForSeconds(5f / sibIndex);
        }   
    }

    private IEnumerator flapWings() {
        while (targetActive) {
            wing1.transform.DORotate(new Vector3(0, -30f, 0), 0.375f).OnComplete(() =>
            {
                wing1.transform.DORotate(new Vector3(0, 0, 0), 0.375f);
            });

            wing2.transform.DORotate(new Vector3(0, 30f, 0), 0.375f).OnComplete(() =>
            {
                wing2.transform.DORotate(new Vector3(0, 0, 0), 0.375f);
            });

            yield return new WaitForSeconds(0.75f);
        }
    }

    public void setLetter(char letter) {
        this.letter.text = (letter + "").ToUpper();
        currentLetter = letter;
    }

    private void OnMouseOver()
    {
        transform.Find("letter").GetComponent<SpriteRenderer>().color = Color.white;
        shine.SetActive(true);

        if (Input.GetMouseButtonDown(0)) {
            GameObject.FindAnyObjectByType<FrogController>().JumpToTarget(this.gameObject);
        }
    }

    private void OnMouseExit()
    {
        transform.Find("letter").GetComponent<SpriteRenderer>().color = targetColor;
        shine.SetActive(false);
    }

    public void selfDistruct() {
        targetActive = false;
        DOTween.Kill(this.gameObject);
        GameObject.Find("UI").GetComponentInChildren<questionController>().recordLetters(currentLetter);
        GameObject boom = Instantiate(boomEffect, this.transform.position, Quaternion.identity, null);
        boom.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0f);
        boom.transform.GetChild(0).DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0f);
        Destroy(boom.gameObject, 1f);
        Destroy(this.gameObject);
    }

}
