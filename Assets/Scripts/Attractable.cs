using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Attractable : MonoBehaviour
{
    private TornadoController tornado;

    private Rigidbody rb;

    public bool canAttract = true;

    public bool groundCube = false;

    public bool spinning = false;

    public bool active = false;

    private float spiralRiseSpeed;

    private float spiralTurnSpeed;

    private float spiralTurnTime;

    private float spiralTurnRadius;

    private float smallSpeed;

    private float smallRadiusSpeed;

    private float timer = 0;

    private Vector3 startScale;


    private void Awake()
    {
        tornado = FindObjectOfType<TornadoController>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        startScale = transform.localScale;
        spiralTurnRadius = tornado.spinAreaRadius;
        GetValues();
    }

    void FixedUpdate()
    {
        if (!groundCube)
        {
            if (!spinning)
            {
                Attract(CalculateDistance());
            }
            else
            {
                SpiralSpin();
            }
        }
    }


    private float CalculateDistance()
    {
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(tornado.transform.position.x, 0, tornado.transform.position.z));
        return distance;
    }

    private void Attract(float distance)
    {
        if (distance < tornado.attractDistance)
        {
            if (distance > tornado.spinAreaRadius)
            {
                Vector3 dir = new Vector3(tornado.transform.position.x - transform.position.x, 0, tornado.transform.position.z - transform.position.z).normalized;
                float force = (tornado.attractDistance / distance) * tornado.attractForceMultiplier;
                rb.AddForce(dir * force);
            }
            else
            {
                //transform.localScale = new Vector3(transform.localScale.x * (1 / tornado.transform.localScale.x), transform.localScale.y * (1 / tornado.transform.localScale.y), transform.localScale.z * (1 / tornado.transform.localScale.z));
                //transform.parent = tornado.transform;
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
                spinning = true;
            }
        }
    }

    private void SpiralSpin()
    {
        transform.RotateAround(tornado.transform.position, Vector3.up, 360 * Time.deltaTime);

        timer += (Time.deltaTime * spiralTurnTime);
        spiralTurnRadius -= Time.deltaTime * spiralTurnTime;

        spiralTurnRadius = Mathf.Clamp(spiralTurnRadius, 0, 100);
        Vector3 dir = (new Vector3(transform.position.x, 0, transform.position.z) - new Vector3(tornado.transform.position.x, 0, tornado.transform.position.z)).normalized;
        Vector3 directedPos = new Vector3(tornado.transform.position.x,transform.position.y, tornado.transform.position.z) +(dir * spiralTurnRadius);
        Vector3 wantedPos = Vector3.Lerp(directedPos, new Vector3(tornado.transform.position.x, transform.position.y, tornado.transform.position.z), timer);
        transform.position = wantedPos;
        transform.position += Vector3.up * Time.deltaTime * spiralRiseSpeed;


        if (transform.localScale != Vector3.zero)
        {
            Vector3 scale = Vector3.Lerp(startScale, Vector3.zero, timer);
            transform.localScale = scale;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public IEnumerator SpiralSpin_Coroutine()
    {
        rb.isKinematic = true;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            Vector3 wantedPos = Vector3.Lerp(transform.position, new Vector3(tornado.transform.position.x, transform.position.y, tornado.transform.position.z), Time.deltaTime * spiralTurnTime);
            transform.position = wantedPos;
         // transform.position += Vector3.up * Time.deltaTime * spiralRiseSpeed;
            transform.RotateAround(tornado.transform.position, Vector3.up, 360 * Time.deltaTime * spiralTurnSpeed);

            if (transform.localScale != Vector3.zero)
            {
                Vector3 scale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * spiralTurnTime);
                transform.localScale = scale;
            }
            else
            {
                gameObject.SetActive(false);
                yield break;
            }

        }
    }

    public IEnumerator GroundSpiralSpin_Coroutine()
    {
        rb.isKinematic = true;
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(tornado.transform.position.x, 0, tornado.transform.position.z));
        while (true)
        {
            yield return null;
            Vector3 wantedPos = Vector3.Lerp(transform.position, new Vector3(tornado.transform.position.x, transform.position.y, tornado.transform.position.z), Time.deltaTime *  spiralTurnTime);
            transform.position = wantedPos;
            transform.position += Vector3.up * Time.deltaTime * spiralRiseSpeed;
            transform.RotateAround(tornado.transform.position, Vector3.up, 360 * Time.deltaTime * spiralTurnSpeed);


            if (transform.localScale.x >= 0.1f)
            {
                transform.localScale -= Vector3.one * Time.deltaTime * smallSpeed * 5f;
            }

            if (Vector3.Distance(transform.position, tornado.transform.position) < 0.3f)
            {
                gameObject.SetActive(false);
                yield break;
            }

        }
    }

    public void MakeActive()
    {
        active = true;
        rb.isKinematic = false;
    }

    private void GetValues()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();

        spiralRiseSpeed = gameManager.spiralRiseSpeed;

        spiralTurnSpeed = gameManager.spiralTurnSpeed;

        spiralTurnTime = 1 / gameManager.spiralTurnTime;

        smallSpeed = gameManager.smallSpeed;

        smallRadiusSpeed = gameManager.smallRadiusSpeed;
    }
}
