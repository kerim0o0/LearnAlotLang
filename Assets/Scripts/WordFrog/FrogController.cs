using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class FrogController : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float timeElapsed;

    [SerializeField]
    private List<GameObject> frogModes;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        StartCoroutine(blinkCo());
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        float scaleVariation = Mathf.Sin(timeElapsed * 2) * 0.025f; 

        transform.localScale = new Vector3(
            originalScale.x + scaleVariation,
            originalScale.y + (scaleVariation / 2f),
            originalScale.z
        );
    }

    public void setTargetMode(int index) {
        for (int i = 0; i < this.transform.childCount; i++) {
            if (this.transform.GetChild(i).gameObject.activeInHierarchy) {
                this.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        this.transform.GetChild(index).gameObject.SetActive(true);
        
    }

    private IEnumerator blinkCo() {
        while (true) {
            frogModes[frogModes.Count - 1].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            frogModes[frogModes.Count - 1].gameObject.SetActive(false);
            yield return new WaitForSeconds(Random.Range(1.5f, 3.5f));
        }
    }

    public void JumpToTarget(GameObject target) {

        StartCoroutine(IJumpToTarget(target));
        spin();
    }

    private IEnumerator IJumpToTarget(GameObject target) {
        setTargetMode(2);
        this.transform.DOMove(target.transform.position, 0.5f).OnComplete(() => {
            this.transform.DOMove(originalPosition, 0.5f);
            target.GetComponent<targetController>().selfDistruct();
        });

        yield return new WaitForSeconds(0.5f);
        setTargetMode(1);
    }

    public void spin() {
        transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
               .SetEase(Ease.Linear); // Set the type of easing. Linear for a constant speed
    }
}
