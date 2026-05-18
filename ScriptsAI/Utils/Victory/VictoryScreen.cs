using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VictoryScreen : MonoBehaviour
{
    public static VictoryScreen Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject panel;
    public TextMeshProUGUI victoryText;
    public Button restartButton;

    public Animator animator;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        restartButton.onClick.AddListener(GameManager.Instance.RestartBattle);
    }

    public void Show(BANDO winner)
    {
        panel.SetActive(true);

        victoryText.text = winner == BANDO.Red
            ? "¡Victoria del Bando Rojo!"
            : "¡Victoria del Bando Azul!";

        victoryText.color = winner == BANDO.Red ? Color.red : Color.cyan;

        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        yield return null;
        animator.SetTrigger("Show");
    }
}