using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> gates;
    public List<int> pairInfo;

    private GameObject[] seniors;
    private GameObject[] seniorHRs;
    private GameObject[] juniors;
    private GameObject[] HRs;

    private float tempZ;


    void Awake()
    {
        seniors = GameObject.FindGameObjectsWithTag("Senior");
        juniors = GameObject.FindGameObjectsWithTag("Junior");
        seniorHRs = GameObject.FindGameObjectsWithTag("SeniorHR");
        HRs = GameObject.FindGameObjectsWithTag("HR");

        gates.AddRange(seniors);
        gates.AddRange(seniorHRs);
        gates.AddRange(juniors);
        gates.AddRange(HRs);

        for(int i = 0; i < gates.Count; i++)
        {
            tempZ = gates[i].transform.position.z;

            for(int j = 0; j < gates.Count; j++)
            {
                if(j!=i && (Mathf.Abs(gates[j].transform.position.z- tempZ) < 1))
                {
                    pairInfo.Add(j);
                }
            }
        }
    }

}
