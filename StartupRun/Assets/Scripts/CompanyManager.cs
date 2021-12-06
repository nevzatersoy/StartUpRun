using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class CompanyManager : MonoBehaviour
{
    public int Money = 0;
    private bool moneyLock = false;
    private bool quitLock = false;
    private bool demoteLock = false;
    private Vector3 quitersPos;

    public float seperation = 1.5f;

    public float width = 8f;

    public GameObject JuniorObject;
    public GameObject SeniorObject;
    public GameObject HRObject;
    public GameObject SeniorHRObject;

    public int JuniorPrice;
    public int SeniorPrice;
    public int HRPrice;
    public int SeniorHRPrice;

    public int CollectedMoneyAmount = 50;

    //Spawning workers
    private GameObject[] WorkerObjects = new GameObject[4];
    private string[] WorkerNames = new string[4] { "Junior", "Senior", "HR", "SeniorHR" };
    private int WorkerType;
    private GameObject tempWorkerObject;
    private GameObject replacingWorker;
    private GameObject demotedWorker;
    private WorkerScript tempWorkerScript;
    private WorkerScript tempWorkerScript2;

    private GameObject demoted;

    //Positioning workers
    private int WorkerCount = 0;
    private int LineToUse = 0;
    public List<GameObject>[] lines = new List<GameObject>[3];
    private int count = 0;
    
    private float dist;
    private Vector3 temp;

    public float HRDiscount = 0.05f;
    public float SeniorHRDiscount = 0.1f;
    public int HRCount = 0;
    public int SeniorHRCount = 0;
    private float totalDiscount = 0;

    public int[] Prices = new int[4];

    //Gate Info
    private List<GameObject> gateList;
    private List<int> gatePairs;
    private bool saleBoxesOPEN = false;


    //Finish
    private Vector3 camPos = new Vector3(0, 2, 0.3f);
    private Vector3 camRot = new Vector3(0, 0, 0);
    private Vector3 workerBasePos = new Vector3(0.65f, 0, 6f);
    private Vector3 mainCharPos = new Vector3(0.5f, 0, 4.6f);
    private Quaternion workerRot = Quaternion.Euler(0, 180, 0);
    public float WorkerLength = 2.15f;
    private Transform CameraTransform;
    private Tween CamTween;



    void Start()
    {
        //Fill worker prefabs to an array
        WorkerObjects[0] = JuniorObject;
        WorkerObjects[1] = SeniorObject;
        WorkerObjects[2] = HRObject;
        WorkerObjects[3] = SeniorHRObject;

        //Fill prices to an array
        //int[] Prices = { JuniorPrice, SeniorPrice, HRPrice, SeniorHRPrice };  UNUTMA BUNU SOR
        Prices[0] = JuniorPrice;
        Prices[1] = SeniorPrice;
        Prices[2] = HRPrice;
        Prices[3] = SeniorHRPrice;

        //Prepare worker lines
        for (int i= 0; i < 3; i++)
        {
            lines[i] = new List<GameObject>();
        }

        //Take gate info
        gateList = GetComponent<GateInfo>().gates;
        gatePairs = GetComponent<GateInfo>().pairInfo;

        //Fill gate prices

        for(int i = 0; i < gateList.Count; i++)
        {
            gateList[i].transform.GetChild(0).Find("NormalPrice").Find("Text").GetComponent<Text>().text =
                Prices[System.Array.IndexOf(WorkerNames, gateList[i].tag)].ToString();
        }

    }

    void Update()
    {
        //SaleBoxes Control
        if(HRCount+SeniorHRCount <= 0 && saleBoxesOPEN)
        {
            closeSaleBoxes();
        }
        else if (HRCount+SeniorHRCount > 0 && saleBoxesOPEN == false)
        {
            openSaleBoxes();
        }

        //Company position adjusting
        if (transform.position.x > (width/2f - (lines[0].Count-1)*seperation/2f))
        {
            dist = transform.position.x - (width / 2f - (lines[0].Count - 1) * seperation / 2f);
            for (int i = 0; i < 3; i++)
            {
                temp = transform.GetChild(i).localPosition;
                transform.GetChild(i).localPosition = new Vector3(-dist, temp.y, temp.z);
            }

        }
        else if (transform.position.x < -(width / 2f - (lines[0].Count - 1) * seperation / 2f))
        {
            dist = transform.position.x + (width / 2f - (lines[0].Count - 1) * seperation / 2f);
            for (int i = 0; i < 3; i++)
            {
                temp = transform.GetChild(i).localPosition;
                transform.GetChild(i).localPosition = new Vector3(-dist, temp.y, temp.z);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                temp = transform.GetChild(i).localPosition;
                transform.GetChild(i).localPosition = new Vector3(0, temp.y, temp.z);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        WorkerType = System.Array.IndexOf(WorkerNames, other.gameObject.tag);
        if (other.gameObject.tag == "Money")
        {
            StartCoroutine(AddMoney(CollectedMoneyAmount));
            Destroy(other.gameObject);
        }
        else if (WorkerType!= -1)
        {
            //Disable Collider of this Gate to not touch it again
            other.gameObject.GetComponent< Collider >().enabled = false;
            //Disable the Gate next to it
            gateList[gatePairs[gateList.IndexOf(other.gameObject)]].GetComponent<Collider>().enabled = false;
            
            StartCoroutine(Recruit(other.gameObject.transform.GetChild(0).gameObject));
        }
        else if(other.gameObject.tag == "Finish")
        {
            Tween stopping = DOTween.To(() => gameObject.GetComponent<Movement>().speed, x => gameObject.GetComponent<Movement>().speed = x, 0, 1);
            StartCoroutine(FinishGame(stopping));
        }
    }

    IEnumerator Recruit(GameObject HumanModel)
    {
        yield return new WaitUntil(() => moneyLock == false);
        moneyLock = true;
        
        //If there is not enough money, skip gate
        if(Money < Mathf.FloorToInt(Prices[WorkerType] *(1-totalDiscount)))
        {
            moneyLock = false;
            yield break;
        }

        Money -= Mathf.FloorToInt(Prices[WorkerType] * (1 - totalDiscount));

        Destroy(HumanModel);

        //index of the line -> Line To Use
        LineToUse = WorkerCount / 4;
        //Count of the workers aldready there before this one
        count = lines[LineToUse].Count;

        for (int i = 0; i < count; i++)
        {
            //Adjust positions of old workers on that line
            lines[LineToUse][i].transform.DOLocalMoveX(i * seperation - count*seperation/2f, 1, false);
        }

        //Add new worker
        tempWorkerObject = Instantiate(WorkerObjects[WorkerType],
                                        transform.GetChild(LineToUse).transform.position + (count * seperation / 2 * Vector3.right),
                                        Quaternion.identity);
        lines[LineToUse].Add(tempWorkerObject);

        //Prepare worker
        tempWorkerScript = tempWorkerObject.GetComponent<WorkerScript>();
        tempWorkerScript.manager = this;
        tempWorkerScript.LineNumber = LineToUse;
        tempWorkerScript.OrderInLine = count;
        tempWorkerScript.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
        tempWorkerScript.transform.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = true;
        tempWorkerScript.transform.Find("Canvas").GetComponent<Canvas>().enabled = true;

        tempWorkerScript.enabled = true;

        //Make Child of the Line
        lines[LineToUse][count].transform.parent = transform.GetChild(LineToUse).transform;
        //Adjust position correctly
        lines[LineToUse][count].transform.DOLocalMoveX(count * seperation / 2, 0.5f, false);

        if (tempWorkerScript.HR)
        {
            if (tempWorkerScript.Junior)
            {
                HRCount++;
            }
            else
            {
                SeniorHRCount++;
            }
            renewDiscount();
            
        }

        WorkerCount++;

        moneyLock = false;
    }

    public IEnumerator AddMoney(int amount)
    {
        yield return new WaitUntil(() => moneyLock == false);
        moneyLock = true;
        Money += amount;
        moneyLock = false;
    }

    public IEnumerator PayCheckWorker(int amount, WorkerScript ws)
    {
        yield return new WaitUntil(() => moneyLock == false);
        moneyLock = true;
        if(Money >= Mathf.FloorToInt(amount * (1 - totalDiscount)))
        {
            Money -= Mathf.FloorToInt(amount * (1 - totalDiscount));
            moneyLock = false;
            yield break;
        }
        else
        {
            moneyLock = false;
            if(ws.Junior == true) //Quit
            {
                yield return new WaitUntil(() => quitLock == false);
                quitLock = true;

                if (WorkerCount != ws.LineNumber*4 + ws.OrderInLine + 1) //If the quitter is not the last worker
                {
                    replacingWorker = lines[(WorkerCount - 1) / 4][(WorkerCount % 4) - 1];

                    replacingWorker.GetComponent<WorkerScript>().LineNumber = ws.LineNumber;
                    replacingWorker.GetComponent<WorkerScript>().OrderInLine = ws.OrderInLine;

                    lines[(WorkerCount - 1) / 4].RemoveAt((WorkerCount % 4) - 1);
                    lines[ws.LineNumber][ws.OrderInLine] = replacingWorker;

                    replacingWorker.transform.parent = ws.gameObject.transform.parent;
                    quitersPos = ws.gameObject.transform.localPosition;
                    replacingWorker.transform.DOLocalMove(quitersPos, 0.5f, false);
                }
                else
                {
                    lines[ws.LineNumber].RemoveAt(ws.OrderInLine); //remove from list
                }

                if (ws.HR)
                {
                    HRCount--;
                    renewDiscount();
                    if (HRCount == 0)
                    {

                    }
                }

                WorkerCount--;

                ws.gameObject.transform.DOKill();
                Destroy(ws.gameObject);

                for (int i = 0; i < lines[(WorkerCount - 1) / 4].Count; i++)
                {
                    //Adjust positions of old workers on last line
                    lines[LineToUse][i].transform.DOLocalMoveX(i * seperation - (lines[(WorkerCount - 1) / 4].Count-1) * seperation / 2f, 0.5f, false);
                }

                quitLock = false;

            }
            else                   //Demote
            {
                yield return new WaitUntil(() => demoteLock == false);

                if (ws.HR== true)
                {
                    demoted = WorkerObjects[2];
                }
                else
                {
                    demoted = WorkerObjects[0];
                }

                //Add new worker
                demotedWorker = Instantiate(demoted, ws.transform.position, Quaternion.identity);

                lines[ws.LineNumber][ws.OrderInLine] = demotedWorker;

                demotedWorker.transform.parent = transform.GetChild(ws.LineNumber).transform;

                demoted.transform.DOLocalMoveX(ws.OrderInLine * seperation -
                    (lines[ws.LineNumber].Count - 1) * seperation / 2f, 0.1f, false);
                demotedWorker.transform.DOLocalMoveY(0, 0.1f, false);
                demotedWorker.transform.DOLocalMoveZ(0, 0.1f, false);

                //Prepare worker
                tempWorkerScript2 = demotedWorker.GetComponent<WorkerScript>();
                tempWorkerScript2.manager = ws.manager;
                tempWorkerScript2.LineNumber = ws.LineNumber;
                tempWorkerScript2.OrderInLine = ws.OrderInLine;
                tempWorkerScript2.paycheckTimer = ws.paycheckTimer;
                tempWorkerScript2.makeMoneyTimer = ws.makeMoneyTimer;
                tempWorkerScript2.enabled = true;

                if (ws.HR == true)
                {
                    HRCount++;
                    SeniorHRCount--;
                    renewDiscount();
                }

                tempWorkerScript2.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
                tempWorkerScript2.transform.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = true;
                tempWorkerScript2.transform.Find("Canvas").GetComponent<Canvas>().enabled = true;

                ws.gameObject.transform.DOKill();
                Destroy(ws.gameObject);
            }
        }
    }

    public void renewDiscount()
    {
        totalDiscount = HRCount * HRDiscount + SeniorHRDiscount * SeniorHRCount;

        for(int i = 0; i < gateList.Count; i++)
        {
            if (gateList[i].transform.childCount > 0)
            {
                gateList[i].transform.GetChild(0).Find("SalePrice").Find("Text").GetComponent<Text>().text =
                Mathf.FloorToInt(Prices[System.Array.IndexOf(WorkerNames, gateList[i].tag)]*(1-totalDiscount)).ToString();
            }
        }
    }

    private void openSaleBoxes()
    {
        for (int i = 0; i < gateList.Count; i++)
        {
            if (gateList[i].transform.childCount > 0)
            {
                gateList[i].transform.GetChild(0).Find("SalePrice").GetComponent<Canvas>().enabled = true;
                gateList[i].transform.GetChild(0).Find("NormalPrice").Find("Text").Find("Image").GetComponent<Image>().enabled = true;
            }
        }
        saleBoxesOPEN = true;
    }

    private void closeSaleBoxes()
    {
        for (int i = 0; i < gateList.Count; i++)
        {
            if (gateList[i].transform.childCount > 0)
            {
                gateList[i].transform.GetChild(0).Find("SalePrice").GetComponent<Canvas>().enabled = false;
                gateList[i].transform.GetChild(0).Find("NormalPrice").Find("Text").Find("Image").GetComponent<Image>().enabled = false;
            }
        }
        saleBoxesOPEN = false;
    }

    private IEnumerator FinishGame(Tween stopping)
    {
        //Wait for  player stopping
        yield return stopping.WaitForCompletion();
        gameObject.GetComponent<Movement>().enabled = false;

        //Prepare workers for finish
        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Count; j++)
            {
                lines[i][j].GetComponent<WorkerScript>().enabled = false;
                lines[i][j].GetComponent<Animator>().SetBool("Running", false);
                lines[i][j].transform.Find("Canvas").GetComponent<Canvas>().enabled = false;
            }
        }

        //Stop update
        this.enabled = false;

        //Prepare camera position, rotation
        CameraTransform = transform.Find("Camera");
        CamTween = CameraTransform.DOLocalMove(camPos, 2);
        CameraTransform.DOLocalRotate(camRot, 2);

        yield return CamTween.WaitForCompletion();

        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Count; j++)
            {
                CamTween = CameraTransform.DOLocalMove(camPos, 3f);

                lines[i][j].transform.localRotation = workerRot;
                lines[i][j].transform.localPosition = workerBasePos;
                workerBasePos += Vector3.up * WorkerLength;
                camPos += Vector3.up * WorkerLength;
                mainCharPos += Vector3.up * WorkerLength;

                yield return CamTween.WaitForCompletion();
            }
        }

        gameObject.GetComponent<Animator>().SetBool("Started", false);
        transform.Find("Armature").transform.rotation = workerRot;
        transform.Find("Armature").transform.localPosition = mainCharPos;






    }


}
