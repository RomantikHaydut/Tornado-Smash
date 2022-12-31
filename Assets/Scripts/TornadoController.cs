using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoController : MonoBehaviour
{
    // Movement
    [SerializeField] float mouseFollowSpeed = 1.5f;
    private Vector3 targetPos;

    // Attract
    public float activeDistance = 14f; // Make active in this distance
    private List<GameObject> attractableObjectList = new List<GameObject>();
    private List<GameObject> groundObjectList = new List<GameObject>();
    public float attractDistance = 12f;
    public float attractForceMultiplier = 1f;

    //Spin
    public float spinAreaRadius = 5f; // In this radius objects start spinning and stop attract.

    // Other features
    private float groundRadius;
    void Awake()
    {
        FindAttractableObjects();
        groundRadius = groundObjectList[0].transform.localScale.x * 2;

        CheckSpinAreaRadius();
    }

    void Update()
    {
        Movement();
        MakeActiveAttractableObjects();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(AttractGround_Coroutine());
        }
    }

    private void FindAttractableObjects()
    {
        Attractable[] a = GameObject.FindObjectsOfType<Attractable>();
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                groundObjectList.Add(a[i].gameObject);
                a[i].groundCube = true;
            }
            else
            {
                attractableObjectList.Add(a[i].gameObject);
            }
        }
    }
    private void Movement()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        if (hit.transform != null)
        {
            if (hit.transform.tag == "Ground")
            {
                targetPos = hit.point;
            }
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * mouseFollowSpeed);

    }

    private void MakeActiveAttractableObjects() 
    {
        for (int i = 0; i < attractableObjectList.Count; i++)
        {
            Vector3 attractableObjectPosition = new Vector3(attractableObjectList[i].transform.position.x, 0, attractableObjectList[i].transform.position.z);
            Vector3 tornadoPosition = new Vector3(transform.position.x, 0, transform.position.z);
            float distance = Vector3.Distance(attractableObjectPosition, tornadoPosition);
            if (distance < activeDistance)
            {
                Attractable attractable = attractableObjectList[i].GetComponent<Attractable>();
                if (attractable != null)
                {
                    if (!attractable.active)
                        attractable.MakeActive();
                }
            }
        }
    }

    private IEnumerator AttractGround_Coroutine() 
    {
        float radius = groundRadius;
        int groundCount = groundObjectList.Count;
        int attractedGroundCount = 0;
        while (true)
        {
            for (int i = 0; i < groundObjectList.Count; i++)
            {
                Attractable attractable = groundObjectList[i].GetComponent<Attractable>();
                float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(attractable.transform.position.x, 0, attractable.transform.position.z));
                if (distance < radius)
                {
                    if (attractable != null)
                    {
                        if (attractable.canAttract)
                        {
                            attractable.canAttract = false;
                            StartCoroutine(attractable.SpiralSpin_Coroutine());
                            attractedGroundCount++;
                        }
                    }
                }
            }
            if (attractedGroundCount >= groundCount)
                yield break;

            radius *= 2;
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }


    private void CheckSpinAreaRadius()
    {
        if (spinAreaRadius >= attractDistance)
        {
            spinAreaRadius /= 2;
        }
    }


}
