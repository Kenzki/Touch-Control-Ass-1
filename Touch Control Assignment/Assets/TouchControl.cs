﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchControl : MonoBehaviour
{

    private float[] timeTouchBegan;
    private bool[] touchDidMove;
    private float tapTimeThreshold = .5f;
    Controlable currently_selected_object;


    Camera cam = new Camera();
    private float drag_distance;
    private float initial_finger_angle;
    private Quaternion initial_object_orientation;
  //private Quaternion initial_camera_orientation;
    private float initial_distance;
    private Vector3 initial_scale;
    private float previousDistance;
    private float scaleSpeed = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        timeTouchBegan = new float[10];
        touchDidMove = new bool[10];
    }

    // Update is called once per frame
    void Update()
    {
        
        //Scaling and Rotating an object
        if (Input.touchCount == 2)
        {
            bool A = Input.GetKey(KeyCode.Space);
            Touch touch1 = Input.touches[0];
            Touch touch2 = Input.touches[1];


            if ((touch1.phase == TouchPhase.Began) || (touch2.phase == TouchPhase.Began))
            {
                
                Vector2 diff = touch2.position - touch1.position;
                initial_finger_angle = Mathf.Atan2(diff.y, diff.x);
                initial_object_orientation = currently_selected_object.transform.rotation;
             // initial_camera_orientation = my_camera.transform.rotation;


                initial_distance = Vector2.Distance(touch1.position, touch2.position);
                initial_scale = currently_selected_object.transform.localScale;
                print(initial_scale);


            }

            if (currently_selected_object)
            {
                Vector2 diff = touch2.position - touch1.position;

                float new_finger_angle = Mathf.Atan2(diff.y, diff.x);
                currently_selected_object.transform.rotation = initial_object_orientation * Quaternion.AngleAxis(Mathf.Rad2Deg * (new_finger_angle - initial_finger_angle), cam.transform.forward);

                currently_selected_object.transform.localScale = (Vector2.Distance(touch1.position, touch2.position) / initial_distance) * initial_scale;
            }


            //Camera Rotation
         /*   if (my_camera)
            {
                Vector2 diff = touch2.position - touch1.position;
                float new_finger_angle = Mathf.Atan2(diff.y, diff.x);
                my_camera.transform.rotation = initial_camera_orientation * Quaternion.AngleAxis(Mathf.Rad2Deg * (new_finger_angle - initial_finger_angle), my_camera.transform.up);
                
            }*/


            //Camera Scaling
            else if (Input.touchCount == 2 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved))
            {
                float distance;
                Vector2 touchcam1 = Input.GetTouch(0).position;
                Vector2 touchcam2 = Input.GetTouch(1).position;

                distance = Vector2.Distance(touchcam1, touchcam2);
                float zoomAmount = (distance - previousDistance) * scaleSpeed * Time.deltaTime;
                cam.transform.Translate(0, 0, zoomAmount);

                previousDistance = distance;
            }


        }
        else
            //dragging an object
            foreach (Touch touch in Input.touches)
            {
                int fingerIndex = touch.fingerId;

                if (touch.phase == TouchPhase.Began)
                {
                    if (currently_selected_object)
                        drag_distance = Vector3.Distance(currently_selected_object.transform.position, cam.transform.position);
                    Debug.Log("Finger #" + fingerIndex.ToString() + " entered!");
                    timeTouchBegan[fingerIndex] = Time.time;
                    touchDidMove[fingerIndex] = false;

                }

                if (touch.phase == TouchPhase.Moved)
                {
                    Debug.Log("Finger #" + fingerIndex.ToString() + " moved!");
                    touchDidMove[fingerIndex] = true;


                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    if (currently_selected_object)
                        currently_selected_object.update_drag_position(ray.GetPoint(drag_distance));
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    float tapTime = Time.time - timeTouchBegan[fingerIndex];
                    Debug.Log("Finger #" + fingerIndex.ToString() + " left. Tap time: " + tapTime.ToString());
                    if (tapTime <= tapTimeThreshold && touchDidMove[fingerIndex] == false)
                    {
                        // Select the object
                        Ray my_ray = cam.ScreenPointToRay(touch.position);
                        Debug.DrawRay(my_ray.origin, 20 * my_ray.direction);

                        RaycastHit info_on_hit;
                        if (Physics.Raycast(my_ray, out info_on_hit))
                        {
                            Controlable my_obj = info_on_hit.transform.GetComponent<Controlable>();
                            if (my_obj)
                            {
                                if (currently_selected_object)
                                {
                                    currently_selected_object.deselect();
                                }
                                my_obj.select();
                                currently_selected_object = my_obj;

                            }

                        }
                        else
                        {
                            if (currently_selected_object)
                            {
                                currently_selected_object.deselect();
                                currently_selected_object = null;
                            }
                        }

                    }

                }
            }


    }

    

}