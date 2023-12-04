using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentUI : MonoBehaviour
{
    public GameObject constantUIParent;
    public TextMeshProUGUI blueRoundsText;
    public TextMeshProUGUI purpleRoundsText;
    public TextMeshProUGUI currentRoundText;
    public TextMeshProUGUI currentScoreText;
    
    public TextMeshProUGUI expandingText;
    
    public List<Image> blueScoreImages = new List<Image>();
    public List<Image> purpleScoreImages = new List<Image>();

    public Color whiteColor;
    public Color blueColor;
    public Color purpleColor;
    
    public string BlueTeamName;
    public string PurpleTeamName;
    
    public void UpdateTeamNames(string blueTeamName, string purpleTeamName)
    {
        BlueTeamName = blueTeamName;
        PurpleTeamName = purpleTeamName;
    }
    
    public void UpdateRoundsText(int blueRoundsWon, int purpleRoundsWon)
    {
        blueRoundsText.text = BlueTeamName + ": " + blueRoundsWon.ToString();
        purpleRoundsText.text = PurpleTeamName + ": " + purpleRoundsWon.ToString();
        
        currentRoundText.text = "Round " + (blueRoundsWon + purpleRoundsWon + 1).ToString();    
    }

    public void UpdateScore(int blueScore, int purpleScore)
    {
        currentScoreText.text = blueScore.ToString() + " - " + purpleScore.ToString();
        UpdateScoreImagesColor(blueScore, purpleScore);
    }
    
    private void UpdateScoreImagesColor(int blueScore, int purpleScore)
    {
        for (int i = 0; i < blueScoreImages.Count; i++)
        {
            if (i < blueScore)
            {
                blueScoreImages[i].color = blueColor;
            }
            else
            {
                blueScoreImages[i].color = whiteColor;
            }
        }
        
        for (int i = 0; i < purpleScoreImages.Count; i++)
        {
            if (i < purpleScore)
            {
                purpleScoreImages[i].color = purpleColor;
            }
            else
            {
                purpleScoreImages[i].color = whiteColor;
            }
        }
    }
}
