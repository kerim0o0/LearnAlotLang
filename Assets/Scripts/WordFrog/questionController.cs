using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class questionController : MonoBehaviour
{

    [SerializeField]
    private List<questions> questionList = new List<questions>();
    [SerializeField]
    private Image questionImage;
    [SerializeField]
    private GameObject lettersParent;
    [SerializeField]
    private GameObject letterPrefab;
    [SerializeField]
    private GameObject targetPrefab;
    [SerializeField]
    private GameObject targetParent;
    private List<char> recordedLetters = new List<char>();
    private List<GameObject> currentLetters = new List<GameObject>();

    private int questionIndex;

    [System.Serializable]
    public class questions {
        public string question;
        public string correctAnswer;
        public Sprite questionImage;
    }
    void Start()
    {
        questionIndex = -1;
        StartCoroutine(loadQuestion());
    }

    void Update()
    {
        
    }

    private IEnumerator loadQuestion() {
        questionIndex++;
        this.questionImage.sprite = questionList[questionIndex].questionImage;
        char[] tempLetters = questionList[questionIndex].correctAnswer.ToCharArray();
        this.questionImage.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < tempLetters.Length; i++) {
            GameObject _letter = Instantiate(letterPrefab, Vector3.zero, Quaternion.identity, lettersParent.transform);
            _letter.transform.DOLocalMoveY(0, 0f);
            _letter.transform.DOLocalMoveX((130 * i), 0.25f);
            currentLetters.Add(_letter);
            _letter.transform.SetAsFirstSibling();
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < tempLetters.Length; i++)
        {
            GameObject _target = Instantiate(targetPrefab, Vector3.zero, Quaternion.identity, targetParent.transform);
            yield return new WaitForSeconds(0.25f);
            _target.GetComponent<targetController>().setLetter(tempLetters[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void recordLetters(char letter) {
        string _letter = (letter + "").ToLower();
        currentLetters[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _letter;
        currentLetters.RemoveAt(0);
        recordedLetters.Add(letter);
        if (currentLetters.Count <= 0) {
            StartCoroutine(GiveResult());
        }
    }

    private IEnumerator GiveResult() {
        yield return new WaitForSeconds(0f);
        string answer = "";
        for (int i = 0; i < recordedLetters.Count; i++) {
            answer += recordedLetters[i] + "";
        }
        recordedLetters.Clear();
        if (answer == questionList[questionIndex].correctAnswer)
        {
            yield return new WaitForSeconds(1f);
            GameObject.Find("Frog").GetComponent<FrogController>().setTargetMode(3);
            yield return new WaitForSeconds(1f);
            GameObject.Find("UI").transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.25f);
            GameObject.Find("Frog").GetComponent<FrogController>().spin();
            yield return new WaitForSeconds(1f);
            GameObject.Find("UI").transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.25f);
        }
        else {
            yield return new WaitForSeconds(1f);
            GameObject.Find("Frog").GetComponent<FrogController>().setTargetMode(4);
            GameObject.Find("UI").transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(1f, 0.25f);
            yield return new WaitForSeconds(1f);
            GameObject.Find("UI").transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().DOFade(0f, 0.25f);
        }

        yield return new WaitForSeconds(1.5f);

        GameObject.Find("Frog").GetComponent<FrogController>().setTargetMode(1);

        for (int i = 0; i < lettersParent.transform.childCount; i++) {
            lettersParent.transform.GetChild(i).transform.DOLocalMoveX(-130f, 0.25f);
            yield return new WaitForSeconds(0.25f);
        }

        for (int i = 0; i < lettersParent.transform.childCount; i++){
            Destroy(lettersParent.transform.GetChild(i).gameObject);
        }

        this.questionImage.DOFade(0f, 0.5f);
        yield return new WaitForSeconds(0.25f);
        if(!((questionIndex+1) > questionList.Count - 1))StartCoroutine(loadQuestion());
    }

    public void sayQuestion() {
        StartCoroutine(GameObject.Find("SpeechManager").GetComponent<SpeechManager>().Say(questionList[questionIndex].question));
    }
}
