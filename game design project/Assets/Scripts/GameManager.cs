using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int Score = 0;


    //public TextMeshPro scoreText;

    [SerializeField]
    private TMP_Text uiText;

    public void add_score(int score) 
    {
        Score += score;
        uiText.text = "Score: " + Score.ToString();
    }

}
