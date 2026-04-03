using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

public class canRoll : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.F;
    public float xHor;
    public float zHor;
    public Player player;
    public int StaminaCost = 4;
    public float slidingSpeed = 200f;
    public bool dashing = false;
    public bool blocking = false;
    private int stamina;
    private int basedefense;
    private bool counting = false;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Example());
    }
    IEnumerator Example()
    {
    //    print(Time.time);
        yield return new WaitForSeconds(0.5f);
    //    print(Time.time);

        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Animator>().SetBool("Dashing", false);
        player.GetComponent<Animator>().SetBool("horizontalDashing", false);
    }

    IEnumerator oneSec()
    {
        counting = true;
     //   print("one second has begun" + Time.time);
        yield return new WaitForSeconds(1f);
     //   print("one second complete" + Time.time);
        player.GetComponent<Stamina>().current = player.GetComponent<Stamina>().current - 2;
        counting = false;
    }

    // Update is called once per frame
    void Update()
    {

      //Gets you left/right horizontal parameter
      xHor = player.GetComponent<Animator>().GetFloat("DirX");
      zHor = player.GetComponent<Animator>().GetFloat("DirZ");
      stamina = player.GetComponent<Stamina>().current;

        //Blocking
        if (Input.GetKey(hotKey) && !dashing && stamina >= 1 && player.state != "CASTING")
        {
            Debug.Log("Blocking");
            basedefense = player.GetComponent<Combat>().baseDefense.baseValue;
            blocking = true;
            player.GetComponent<Animator>().SetBool("Blocking", true);
        }
        else if (Input.GetKeyUp(hotKey))
        {
        //    Debug.Log("Blocking Complete");
            blocking = false;
            player.GetComponent<Combat>().baseDefense.baseValue = 1;
            player.GetComponent<Animator>().SetBool("Blocking", false);
        }
        if (blocking == true)
        {
            for (int i = 0; i < player.GetComponent<Stamina>().current; i++)
            {
                if (counting == false){
                    player.GetComponent<Combat>().baseDefense.baseValue = 80;
                    StartCoroutine(oneSec());
                    if (player.GetComponent<Stamina>().current <= 1)
                    {
                        blocking = false;
        //                Debug.Log("Out of stamina - for loop successful, breaking");
                        player.GetComponent<Combat>().baseDefense.baseValue = 1;
                        player.GetComponent<Animator>().SetBool("Blocking", false);
                        break;
                    }
                }
            }
        }



        //All dashes - vertical and horizontal
        if (Input.GetKeyDown(KeyCode.Mouse2) && blocking == false)
        {
            if (player.GetComponent<Stamina>().current >= StaminaCost)
            {
                Debug.Log("Sliding");
                player.GetComponent<Stamina>().current -= StaminaCost;

                Vector3 direction = Vector3.zero;

                if (xHor >= 1f)
                {
                    direction = transform.right;
                    player.GetComponent<Animator>().SetBool("horizontalDashing", true);
                }
                else if (xHor <= -1f)
                {
                    direction = -transform.right;
                    player.GetComponent<Animator>().SetBool("horizontalDashing", true);
                }
                else if (zHor >= 1f)
                {
                    direction = transform.forward;
                    player.GetComponent<Animator>().SetBool("Dashing", true);
                }
                else if (zHor <= -1f)
                {
                    direction = -transform.forward;
                    player.GetComponent<Animator>().SetBool("Dashing", true);
                }

                if (direction != Vector3.zero)
                {
                    player.GetComponent<Rigidbody>().isKinematic = false;
                    player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation; // Lock rotation
                    player.GetComponent<Rigidbody>().velocity = direction * slidingSpeed; // Set velocity directly
                    StartCoroutine(Example());
                }
            }
        }
    }
}
