using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public float speed = 4;

    public float width = 9f;

    private float lastXPos = -1;
    private bool started = false;
    private Vector3 tempPosVector;


    Animator animator;
    // Update is called once per frame
    void Awake()
    {
        animator = gameObject.GetComponent<Animator>();
    }
    void Update()
    {
        if (started)
        {
            transform.position += Vector3.forward * speed * Time.deltaTime;

            if (lastXPos != -1)
            {

                tempPosVector = transform.position + Vector3.right * Time.deltaTime * (Input.mousePosition.x - lastXPos);

                if (tempPosVector.x > width / 2)
                {
                    tempPosVector = new Vector3(width / 2, tempPosVector.y, tempPosVector.z);
                }
                else if (tempPosVector.x < -width / 2)
                {
                    tempPosVector = new Vector3(-width / 2, tempPosVector.y, tempPosVector.z);
                }

                transform.position = tempPosVector;

            }
            lastXPos = Input.mousePosition.x;

            
        }
        else if (Input.GetMouseButtonUp(0))
        {
            started = true;
            animator.SetBool("Started",true);
        }
    }

    
}
