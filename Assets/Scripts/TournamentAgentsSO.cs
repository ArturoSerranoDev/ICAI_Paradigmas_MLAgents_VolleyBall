using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

[CreateAssetMenu(fileName = "TournamentAgentsSO", menuName = "ScriptableObjects/TournamentAgentsSO", order = 1)]
public class TournamentAgentsSO : ScriptableObject
{
    public List<VolleyballAgentData> agentsDataList = new List<VolleyballAgentData>();
}

[Serializable]
public class VolleyballAgentData
{
    public string teamName;
    public GameObject prefab;
    public NNModel agentModel;
    
    public VolleyballAgentData(string teamName, GameObject prefab)
    {
        this.teamName = teamName;
        this.prefab = prefab;
    }
}
