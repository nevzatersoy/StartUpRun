using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WorkerScript : MonoBehaviour
{
    // Set in the editor
    public float makeMoneyTime = 1;
    public int moneyGained = 20;
    public float paycheckTime = 3;
    private int paycheck = 500;
    public float promotionTime = 10;

    public GameObject SeniorHRObject;
    public GameObject SeniorObject;
    private GameObject tempWorker;
    private WorkerScript ws;
    private GameObject promoted;

    private bool newlyPromoted = false;
    private int priceSet = -1;

    public bool Junior;
    public bool HR;
    

    //Set by other script
    public CompanyManager manager;
    public int LineNumber;
    public int OrderInLine;

    //Set by this script
    public float makeMoneyTimer = 0;
    public float paycheckTimer = 0;
    private float promotionTimer = 0;

    public Image payImage;


    private void Awake()
    {
        if(HR== true)
        {
            promoted = SeniorHRObject;
        }
        else
        {
            promoted = SeniorObject;
        }
        

    }
    void Update()
    {

        if(priceSet == -1)
        {
            priceSet++;
            if (HR == true)
            {
                priceSet += 2;
            }
            if (Junior == false)
            {
                priceSet += 1;
            }

            paycheck = manager.Prices[priceSet];

        }

        if(makeMoneyTimer >= makeMoneyTime)
        {
            manager.StartCoroutine(manager.AddMoney(moneyGained));
            makeMoneyTimer = 0;
        }

        if(paycheckTimer >=paycheckTime )
        {
            if (newlyPromoted)
            {
                manager.StartCoroutine(manager.PayCheckWorker(Mathf.FloorToInt(paycheck*0.8f), this));
                newlyPromoted = false;
            }
            else
            {
                manager.StartCoroutine(manager.PayCheckWorker(paycheck, this));
            }
            paycheckTimer = 0;
        }

        paycheckTimer += Time.deltaTime;
        makeMoneyTimer += Time.deltaTime;

        payImage.fillAmount = 1 - paycheckTimer / paycheckTime;

        if (Junior == true)
        {
            promotionTimer += Time.deltaTime;
        }

        if(promotionTimer>= promotionTime) //Promote
        {
            promotionTimer = 0;
            //Add new worker
            tempWorker = Instantiate(promoted,transform.position,Quaternion.identity);

            manager.lines[LineNumber][OrderInLine] = tempWorker;

            tempWorker.transform.parent = manager.transform.GetChild(LineNumber).transform;

            tempWorker.transform.DOLocalMoveX(OrderInLine * manager.seperation - 
                (manager.lines[LineNumber].Count - 1) * manager.seperation / 2f, 0.1f, false);
            tempWorker.transform.DOLocalMoveY(0, 0.1f, false);
            tempWorker.transform.DOLocalMoveZ(0, 0.1f, false);

            //Prepare worker
            ws = tempWorker.GetComponent<WorkerScript>();
            ws.manager = manager;
            ws.LineNumber = LineNumber;
            ws.OrderInLine = OrderInLine;
            ws.newlyPromoted = true;
            ws.paycheckTimer = paycheckTimer;
            ws.makeMoneyTimer = makeMoneyTimer;

            if(HR == true)
            {
                manager.HRCount--;
                manager.SeniorHRCount++;
                manager.renewDiscount();
            }

            ws.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = true;
            ws.transform.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = true;
            ws.transform.Find("Canvas").GetComponent<Canvas>().enabled = true;

            ws.enabled = true;


            this.gameObject.transform.DOKill();
            Destroy(this.gameObject);


        }
    }
}
