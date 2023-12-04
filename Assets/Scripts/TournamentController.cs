using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.MLAgents.Policies;
using UnityEngine;

public class TournamentController : MonoBehaviour
{
    public string BlueTeamName;
    public string PurpleTeamName;

    int ballSpawnSide;

    VolleyballSettings volleyballSettings;

    public VolleyballAgent blueAgent;
    public VolleyballAgent purpleAgent;

    public List<VolleyballAgent> AgentsList = new List<VolleyballAgent>();
    List<Renderer> RenderersList = new List<Renderer>();

    Rigidbody blueAgentRb;
    Rigidbody purpleAgentRb;

    public GameObject ball;
    Rigidbody ballRb;

    public GameObject blueGoal;
    public GameObject purpleGoal;

    Renderer blueGoalRenderer;

    Renderer purpleGoalRenderer;

    Team lastHitter;

    private int resetTimer;
    public int MaxEnvironmentSteps;

    private int blueScore = 0;
    private int purpleScore = 0;
    private int blueRoundsWon = 0;
    private int purpleRoundsWon = 0;
    
    private int overallScore = 0;
    private int currentRound = 0;

    public TournamentAgentsSO tournamentAgentsSO;

    public TournamentUI tournamentUI;
    
    public GameObject blueConfetti;
    public GameObject purpleConfetti;
    public GameObject blueCamera;
    public GameObject purpleCamera;

    public GameObject eyesBlue;
    public GameObject eyesPurple;

    private bool hasWinner;
    public IEnumerator NextScoreCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ResetScene();
    }
    public IEnumerator NextRoundCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ResetScene();
    }
    
    public IEnumerator WinnerCoroutine()
    {
        yield return new WaitForSeconds(1f);
        ResetScene();
    }
    
    void Start()
    {
        SetupAlumniAgents(BlueTeamName, PurpleTeamName);
        
        
        blueConfetti.SetActive(false);
        purpleConfetti.SetActive(false);
        blueCamera.SetActive(false);
        purpleCamera.SetActive(false);
        tournamentUI.constantUIParent.SetActive(true);
        
        // Used to control agent & ball starting positions
        blueAgentRb = blueAgent.GetComponent<Rigidbody>();
        purpleAgentRb = purpleAgent.GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        
        blueAgent.enabled = false;
        purpleAgent.enabled = false;
        
        ballRb.isKinematic = true;
        
        // Starting ball spawn side
        // -1 = spawn blue side, 1 = spawn purple side
        var spawnSideList = new List<int> { -1, 1 };
        ballSpawnSide = spawnSideList[Random.Range(0, 2)];

        // Render ground to visualise which agent scored
        blueGoalRenderer = blueGoal.GetComponent<Renderer>();
        purpleGoalRenderer = purpleGoal.GetComponent<Renderer>();
        RenderersList.Add(blueGoalRenderer);
        RenderersList.Add(purpleGoalRenderer);

        volleyballSettings = FindObjectOfType<VolleyballSettings>();

        SetRound(currentRound);
        tournamentUI.UpdateRoundsText(blueRoundsWon, purpleRoundsWon);
        tournamentUI.UpdateScore(blueScore, purpleScore);

        StartCoroutine(StartMatchCoroutine());
        // ResetScene();
    }
    private void SetupAlumniAgents(string blueTeamName, string purpleTeamName)
    {
        // Find on the tournamentAgentsSO the agents name that matches the blueTeamName and purpleTeamName
        // and set the agents on the tournament controller
        foreach (var agent in tournamentAgentsSO.agentsDataList)
        {
            if (agent.teamName == blueTeamName)
            {
                blueAgent.GetComponent<BehaviorParameters>().Model = agent.agentModel;
                blueAgent.transform.localScale = agent.prefab.transform.localScale;
            }
            else if (agent.teamName == purpleTeamName)
            {
                purpleAgent.GetComponent<BehaviorParameters>().Model = agent.agentModel;
                purpleAgent.transform.localScale = agent.prefab.transform.localScale;
            }
        }
        
        tournamentUI.UpdateTeamNames(blueTeamName, purpleTeamName);
        
    }
    private IEnumerator StartMatchCoroutine()
    {
        blueCamera.SetActive(true);
        tournamentUI.expandingText.gameObject.SetActive(true);
        tournamentUI.constantUIParent.transform.localPosition = new Vector3(0, 200, 0);

        tournamentUI.expandingText.text = BlueTeamName;
        tournamentUI.expandingText.transform.DOScale(1, 0.5f).From(0);
        
        yield return new WaitForSeconds(2f);
        
        eyesBlue.transform.DOScaleY(0.25f, 0.5f);
        tournamentUI.expandingText.text = "VS";
        tournamentUI.expandingText.transform.DOScale(1, 0.5f).From(0);
        
        yield return new WaitForSeconds(1f);
        
        tournamentUI.expandingText.text = PurpleTeamName;
        tournamentUI.expandingText.transform.DOScale(1, 0.5f).From(0);
            
        eyesPurple.transform.DOScaleY(0.25f, 0.5f);

        blueCamera.SetActive(false);
        purpleCamera.SetActive(true);
        yield return new WaitForSeconds(2f);
        purpleCamera.SetActive(false);
        ballRb.isKinematic = false;
        blueAgent.enabled = true;
        purpleAgent.enabled = true;
        tournamentUI.expandingText.gameObject.SetActive(false);
        tournamentUI.constantUIParent.transform.DOLocalMoveY(0, 1);
        
        eyesBlue.transform.localScale = Vector3.one;
        eyesPurple.transform.localScale = Vector3.one;
        
        ResetScene();
    }


    /// <summary>
    /// Tracks which agent last had control of the ball
    /// </summary>
    public void UpdateLastHitter(Team team)
    {
        lastHitter = team;
    }
    /// <summary>
    /// Resolves scenarios when ball enters a trigger and assigns rewards.
    /// Example reward functions are shown below.
    /// To enable Self-Play: Set either Purple or Blue Agent's Team ID to 1.
    /// </summary>
    public void ResolveEvent(Event triggerEvent)
    {
        
        if (hasWinner)
            return;
        
        switch (triggerEvent)
        {
            case Event.HitOutOfBounds:
                ResetScene();
                break;

            case Event.HitBlueGoal:
                // blue wins
                blueScore += 1;
                overallScore += 1;
                // turn floor blue
                StartCoroutine(GoalScoredSwapGroundMaterial(volleyballSettings.blueGoalMaterial, RenderersList, .5f));

                // end episode
                ResetScene();
                tournamentUI.UpdateScore(blueScore, purpleScore);
                break;

            case Event.HitPurpleGoal:
                // purple wins
                purpleScore += 1;
                overallScore += 1;

                // turn floor purple
                StartCoroutine(GoalScoredSwapGroundMaterial(volleyballSettings.purpleGoalMaterial, RenderersList, .5f));

                // end episode
                ResetScene();
                tournamentUI.UpdateScore(blueScore, purpleScore);
                break;
        }

        if(blueScore == 3 || purpleScore == 3)
        {
            if(blueScore > purpleScore)
            {
                blueRoundsWon += 1;
            }
            else
            {
                purpleRoundsWon += 1;
            }
            currentRound += 1;
            
            if (currentRound == 3 || (blueRoundsWon > purpleRoundsWon + 1 || purpleRoundsWon > blueRoundsWon + 1 ))
            {
                SetWinner(blueRoundsWon > purpleRoundsWon ? Team.Blue : Team.Purple);
            }
            else
            {
                SetRound(currentRound);
                tournamentUI.UpdateScore(blueScore, purpleScore);
                tournamentUI.UpdateRoundsText(blueRoundsWon, purpleRoundsWon);

                ResetScene();
            }
        }
    }

    public void SetWinner(Team team)
    {
        Debug.Log(team.ToString() + " is the winner");
        
        if (team == Team.Blue)
        {
            blueConfetti.SetActive(true);
            purpleAgentRb.isKinematic = true;
        }
        else
        {
            purpleConfetti.SetActive(true);
            blueAgentRb.isKinematic = true;
        }
        tournamentUI.expandingText.text = team == Team.Blue ? BlueTeamName + " Wins!" : PurpleTeamName + " Wins!";
        tournamentUI.expandingText.gameObject.SetActive(true);
        tournamentUI.expandingText.transform.DOScale(1, 0.5f).From(0);
        tournamentUI.constantUIParent.SetActive(false);
        
        hasWinner = true;
    }
    /// <summary>
    /// Changes the color of the ground for a moment.
    /// </summary>
    /// <returns>The Enumerator to be used in a Coroutine.</returns>
    /// <param name="mat">The material to be swapped.</param>
    /// <param name="time">The time the material will remain.</param>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, List<Renderer> rendererList, float time)
    {
        foreach (var renderer in rendererList)
        {
            renderer.material = mat;
        }

        yield return new WaitForSeconds(time); // wait for 2 sec

        foreach (var renderer in rendererList)
        {
            renderer.material = volleyballSettings.defaultMaterial;
        }
    }

    /// <summary>
    /// Called every step. Control max env steps.
    /// </summary>
    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            blueAgent.EpisodeInterrupted();
            purpleAgent.EpisodeInterrupted();
            ResetScene();
        }
    }

    public IEnumerator ResetSceneCoroutine(Team scoringTeam)
    {
        yield return new WaitForSeconds(1f);
        ResetScene();
    }

    /// <summary>
    /// Reset agent and ball spawn conditions.
    /// </summary>
    public void ResetScene()
    {
        resetTimer = 0;

        lastHitter = Team.Default; // reset last hitter

        foreach (var agent in AgentsList)
        {
            // randomise starting positions and rotations
            var randomPosX = Random.Range(-2f, 2f);
            var randomPosZ = Random.Range(-2f, 2f);
            var randomPosY = Random.Range(0.5f, 3.75f); // depends on jump height
            var randomRot = Random.Range(-45f, 45f);

            agent.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
            agent.transform.eulerAngles = new Vector3(0, randomRot, 0);

            agent.GetComponent<Rigidbody>().velocity = default(Vector3);
        }

        // reset ball to starting conditions
        ResetBall();
    }
    
    public void SetRound(int round)
    {
        blueScore = 0;
        purpleScore = 0;
        overallScore = 0;
        
        if(round == 0)
        {
            ball.transform.localScale = new Vector3(5,5,5);
            ball.GetComponent<Rigidbody>().mass = 3;
        }
        else if(round == 1)
        {
            ball.transform.localScale = new Vector3(4,4,4);
            ball.GetComponent<Rigidbody>().mass = 2;
        }
        else if(round == 2)
        {
            ball.transform.localScale = new Vector3(7,7,7);
            ball.GetComponent<Rigidbody>().mass = 5;
        }
    }

    /// <summary>
    /// Reset ball spawn conditions
    /// </summary>
    void ResetBall()
    {
        var randomPosX = Random.Range(-2f, 2f);
        var randomPosZ = Random.Range(6f, 10f);
        var randomPosY = Random.Range(6f, 8f);

        // alternate ball spawn side
        // -1 = spawn blue side, 1 = spawn purple side
        ballSpawnSide = -1 * ballSpawnSide;

        if (ballSpawnSide == -1)
        {
            ball.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
        }
        else if (ballSpawnSide == 1)
        {
            ball.transform.localPosition = new Vector3(randomPosX, randomPosY, -1 * randomPosZ);
        }

        ballRb.angularVelocity = Vector3.zero;
        ballRb.velocity = Vector3.zero;
    }
}
