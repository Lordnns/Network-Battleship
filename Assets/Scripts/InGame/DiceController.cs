using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DiceController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text enemyResultText;
    [SerializeField] private TMP_Text playerResultText;
    [SerializeField] private Button button;
    private int playerResult;
    private int enemyResult;

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip diceRollSound;
    [SerializeField] private AudioClip finalResultSound;
    
    [Header("Dice Animation")]
    [SerializeField] private float diceAnimationTime = 3f;
    [SerializeField] private float diceDigitTime = 0.2f;

    private PlayerNetwork playerNetwork;
    private PlayerController playerController;
    

    private void Start()
    {

        audioSource = GetComponent<AudioSource>();
        var playerNetworks = FindObjectsOfType<PlayerNetwork>();
        foreach (var p in playerNetworks)
        {
            if (p.IsOwner)
            {
                playerNetwork = p;
                playerController = p.GetComponent<PlayerController>();
            }
        }

    }

    public void UpdateText()
    {
        enemyResult = playerNetwork.GetOpponent().diceValue.Value;
        enemyResultText.text = enemyResult.ToString();
    }

    public void ThrowDice(int upperBound)
    {
        button.interactable = false;
        StartCoroutine(RollingAnimation(upperBound));
        
    }

    public void EnableDiceButton()
    {
        button.interactable = true;
    }

    //simulate dice roll
    private IEnumerator RollingAnimation(int upperBound)
    {
        var animTime = diceAnimationTime;
        while (animTime > 0)
        {
            var currentTime = Time.time;
            playerResultText.text = Random.Range(1, upperBound + 1).ToString();
            audioSource.PlayOneShot(diceRollSound, 0.4f);
            currentTime = Time.time - currentTime;
            animTime -= currentTime + diceDigitTime;
            yield return new WaitForSeconds(diceDigitTime);
        }
        
        audioSource.PlayOneShot(finalResultSound);
        
        playerNetwork.hasThrownDice.Value = true;
        playerNetwork.diceValue.Value = Random.Range(1, upperBound + 1);
        playerResultText.text = playerNetwork.diceValue.Value.ToString();
        
        playerController.rollingDiceState.CheckDices();
        
        yield return null;
    }

    public void ShowDiceResult(int local, int remote)
    {
        enemyResultText.text = "";
        playerResultText.text = local > remote ? "You start" : "Enemy is starting";
        
        StartCoroutine(DiceResultDelay());
    }

    private IEnumerator DiceResultDelay()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

}
