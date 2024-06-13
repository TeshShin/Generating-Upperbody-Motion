/****************************************************
 * File: IKArmsPlacement.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 27/01/2021
   * Project: Generating Upper-Body Motion for Real-Time Characters
   * Last update: 30/06/2022
*****************************************************/

// TODO: CHECK THIS SCRIPT WHEN WE HAVE THE WHOLE METHOD

using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class IKArmsPlacement : MonoBehaviour
{
    #region Read-only & Static Fields

    protected Animator _anim;

    #endregion

    #region Instance Fields

    [Header("Arms Placement - Basic")]
    public bool enableArmsIK = false;

    [Header("Arms Placement - Options")]
    [Range(0f, 1f)] public float rightArmWeight;
    [Range(0f, 1f)] public float leftArmWeight;
    public Vector3 rotationOffsetRight;
    public Vector3 rotationOffsetLeft;
    [Range(0f, 1f)] public float rightArmReturnWeight;
    [Range(0f, 1f)] public float leftArmReturnWeight;

    [Header("Head Placement - Options")]
    [Range(0f, 1f)] public float headWeight;

    public TargetIK leftTarget;
    public TargetIK rightTarget;

    public bool activateIKLeft;
    public bool activateIKRight;

    // TEST
    public SafetyRegionLeft safetyRegionLeft;
    public SafetyRegionRight safetyRegionRight;
    public bool alwaysZero;
    private bool leftcheck = false;
    private bool rightcheck = false;

    // 헤드 IK
    public bool activeHeadIK = true;
    public Vector3 prevheadlook;

    // IK 속도 조절 실험
    // 오 된다!!! 이제 장애물과 나의 속도를 가지고 이 speed를 조정하면 되겠는데?!!
    private Vector3 targetrightPosition;
    [Range(0f,10f)] public float rightReachSpeed = 1f;
    private float rightBaseReachSpeed;

    private Vector3 targetleftPosition;
    [Range(0f, 10f)] public float leftReachSpeed = 1f;
    private float leftBaseReachSpeed;


    [Header("DistanceSpeedCalculator")]
    // selfpos는 캐릭터의 위치를 나타내는 현재 이 스크립트를 가진 오브젝트의 위치로,
    // obstaclepos는 현재 타게팅된 targetobstacle의 위치로
    public Vector3 SelfPos; // 캐릭터의 이전 위치
    public Vector3 leftObstaclePos; // 좌측 상호작용할 물체의 위치
    public Vector3 rightObstaclePos; // 우측 상호작용할 물체의 위치
    public float previousleftDistance; // 이전 프레임에서의 좌측과 거리
    public float previousrightDistance; // 이전 프레임에서의 우측과 거리
    [Range(0f, 10f)] public float leftspeed;
    [Range(0f, 10f)] public float rightspeed;
    #endregion

    #region Unity Methods

    void Start()
    {
        rightBaseReachSpeed = rightReachSpeed;
        leftBaseReachSpeed = leftReachSpeed;
        // Getting components from Inspector
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        




        /// 원래 있던 코드
        activateIKLeft = leftTarget.activateIK;
        activateIKRight = rightTarget.activateIK;
        /// Head IK 관련
        if (activeHeadIK)
        {
            if (safetyRegionLeft.obstacles.Count != 0) // 왼쪽 보는거 우선 (동시에 왔지만 그 중, 늦게 들어온 걸 쳐다보는 방식은 좀 복잡해질듯)
            {
                if (safetyRegionLeft.prevObstacle != safetyRegionLeft.nowObstacle)
                {
                    // todo
                }
                //headWeight = Mathf.Lerp(headWeight, 1, Time.deltaTime);
                //leftcheck = true;
                if (rightcheck)
                {
                    headWeight = Mathf.Lerp(headWeight, 0, Time.deltaTime * 2.5f);
                    if (headWeight <= 0.3f)
                    {
                        rightcheck = false;
                    }
                }
                else
                {
                    headWeight = Mathf.Lerp(headWeight, 1, Time.deltaTime * 2.5f);
                    leftcheck = true;
                }
            }
            else if (safetyRegionRight.obstacles.Count != 0)
            {
                if (leftcheck)
                {
                    headWeight = Mathf.Lerp(headWeight, 0, Time.deltaTime * 2.5f);
                    if (headWeight <= 0.3f)
                    {
                        leftcheck = false;
                    }
                }
                else
                {
                    headWeight = Mathf.Lerp(headWeight, 1, Time.deltaTime * 2.5f);
                    rightcheck = true;
                }

            }
            else
            {
                headWeight = Mathf.Lerp(headWeight, 0, Time.deltaTime * 2.5f);
            }
        }
        
    }
    private void FixedUpdate()
    {
        // 내가 추가한 코드 - IK를 따를 때, 속도를 조절하기 위해서 사용
        // 구현된 reaction time을 조정해서 건드릴 수는 있지만, reaction time을 통해서 자연스럽게 나오려면 reaction time 값을 세밀하게 조정해야 할 것 같음.
        // 
        targetrightPosition = Vector3.MoveTowards(rightTarget.transform.position, rightTarget.target.position, rightReachSpeed * Time.deltaTime);
        targetleftPosition = Vector3.MoveTowards(leftTarget.transform.position, leftTarget.target.position, leftReachSpeed * Time.deltaTime);
        //

        /// 속도 측정 관련
        SelfPos = new Vector2(transform.position.x, transform.position.z);

        if (safetyRegionLeft.obstacles.Count != 0) // obstacle이 발견되었을 때 계산 시작.
        {
            leftObstaclePos = new Vector2(safetyRegionLeft.targetObstacle.location.x, safetyRegionLeft.targetObstacle.location.z);
            if(previousleftDistance == 0f)
            {
                previousleftDistance = Vector2.Distance(SelfPos, leftObstaclePos);
            }
            float currentleftDistance = Vector2.Distance(SelfPos, leftObstaclePos);
            float leftdistancechange = previousleftDistance - currentleftDistance;

            leftspeed = leftdistancechange / Time.deltaTime;

            previousleftDistance = currentleftDistance;
        }
        else
        {
            leftspeed = 0f;
            previousleftDistance = 0f;
        }

        if (safetyRegionRight.obstacles.Count != 0)
        {
            rightObstaclePos = new Vector2(safetyRegionRight.targetObstacle.location.x, safetyRegionRight.targetObstacle.location.z);
            if (previousrightDistance == 0f)
            {
                previousrightDistance = Vector2.Distance(SelfPos, rightObstaclePos);
            }
            float currentrightDistance = Vector2.Distance(SelfPos, rightObstaclePos);
            float rightdistancechange = previousrightDistance - currentrightDistance;

            rightspeed = rightdistancechange / Time.deltaTime;

            previousrightDistance = currentrightDistance;
        }
        else
        {
            rightspeed = 0f;
            previousrightDistance = 0f;
        }


        /// 손이 닿을 때와 안닿을 때 처리
        if(safetyRegionRight.obstacles.Count != 0)
        {
            if (rightspeed >= 0.5f)
            {
                rightArmReturnWeight = 0.7f;
                rightReachSpeed = rightspeed * 2f + rightBaseReachSpeed; 
            }
            else if (rightTarget.completeLength + 0.21f >= safetyRegionRight.targetObstacle.distance)
            {
                rightArmReturnWeight = 0.7f;
                rightReachSpeed = rightBaseReachSpeed + 0.5f; 
            }
            else
            {
                rightArmReturnWeight = Mathf.Lerp(rightArmReturnWeight, 0f, Time.deltaTime * 2f);
                rightReachSpeed = rightBaseReachSpeed;
            }
        }
        else
        {
            rightArmReturnWeight = Mathf.Lerp(rightArmReturnWeight, 0f, Time.deltaTime * 2f);
            rightReachSpeed = rightBaseReachSpeed;
        }

        if (safetyRegionLeft.obstacles.Count != 0)
        {
            if (leftspeed >= 0.5f)
            {
                leftArmReturnWeight = 0.7f;
                leftReachSpeed = leftspeed * 2f + leftBaseReachSpeed;
            }
            else if (leftTarget.completeLength + 0.21f >= safetyRegionLeft.targetObstacle.distance)
            {
                leftArmReturnWeight = 0.7f;
                leftReachSpeed = leftBaseReachSpeed + 0.5f;
            }
            else
            {
                leftArmReturnWeight = Mathf.Lerp(leftArmReturnWeight, 0f, Time.deltaTime * 2f);
                leftReachSpeed = leftBaseReachSpeed;
            }
        }
        else
        {
            leftArmReturnWeight = Mathf.Lerp(leftArmReturnWeight, 0f, Time.deltaTime * 2f);
            leftReachSpeed = leftBaseReachSpeed;
        }




        /// 멈춰있는 장애물과의 상호작용에 대한 예외처리
        if (safetyRegionRight.obstacles.Count != 0)
        {
            if (safetyRegionRight.nowObstacle.gameObject.CompareTag("Static Obstacle"))
            {
                rightReachSpeed = 3f;
            }
        }

        if (safetyRegionLeft.obstacles.Count != 0)
        {
            if (safetyRegionLeft.nowObstacle.gameObject.CompareTag("Static Obstacle"))
            {
                leftReachSpeed = 3f;
            }
        }
        
        

        /// 
        //if (!(rightTarget.completeLength + 0.21f >= safetyRegionRight.targetObstacle.distance) && !(rightspeed >= 1f))
        //{
        //    rightArmReturnWeight = Mathf.Lerp(rightArmReturnWeight, 0f, Time.deltaTime * 2.5f);
        //    rightReachSpeed = rightBaseReachSpeed;
        //}
        //else
        //{
        //    rightArmReturnWeight = 0.5f;
        //    rightReachSpeed = rightspeed * 2.5f + rightBaseReachSpeed; // 두번째 항은 기본적으로 설정한 값으로
        //}
        //if (!(leftTarget.completeLength + 0.21f >= safetyRegionLeft.targetObstacle.distance) && !(leftspeed >= 1f))
        //{
        //    leftArmReturnWeight = Mathf.Lerp(leftArmReturnWeight, 0f, Time.deltaTime * 2.5f);
        //    leftReachSpeed = leftBaseReachSpeed;
        //}
        //else
        //{
        //    leftArmReturnWeight = 0.5f;   
        //    leftReachSpeed = leftspeed * 2.5f + leftBaseReachSpeed;
        //}
    }

    void OnAnimatorIK()
    {
        if (_anim)
        {
            // If the IK is active, set the position and rotation directly to the target 
            if (enableArmsIK)
            {
                if (activeHeadIK)
                {
                    if (safetyRegionLeft.obstacles.Count != 0)
                    {
                        //animator.SetLookAtWeight(headWeight);
                        //StartCoroutine(Lerp());
                        _anim.SetLookAtPosition(safetyRegionLeft.targetObstacle.location);
                        prevheadlook = safetyRegionLeft.targetObstacle.location;
                    }
                    //Set the look target position, if one has been assigned, just the first it encounters
                    else if (safetyRegionRight.obstacles.Count != 0)
                    {
                        _anim.SetLookAtPosition(safetyRegionRight.targetObstacle.location);
                        prevheadlook = safetyRegionRight.targetObstacle.location;
                    }
                    _anim.SetLookAtWeight(headWeight);
                    _anim.SetLookAtPosition(prevheadlook);
                }

                // Set the right hand target position and rotation, if one has been assigned
                
                if (rightTarget.target != null) // rightTarget == Target Right Hand (하이라키에 있음)
                {
                    if(!alwaysZero)
                    {   // 조건 새로 만듬 (닿을 수 없는 대상에 손만 뻗고 있는 것을 없애기 위함.)
                        // 0.21f 더한 이유가 물체와 닿을려면 팔 길이 + 손바닥 길이도 더해야함. 손바닥 길이는 0.05이지만, 손 길이로 인해서 물체가 튕겨지는 경우에 닿을려하다가 말다가를
                        // 무한 반복하기 때문에 0.21f까지 더해줌.
                        if (rightTarget.completeLength + 0.21f >= safetyRegionRight.targetObstacle.distance || rightspeed >= 0.5f)
                        {
                            _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightArmWeight);
                            _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightArmWeight);

                            _anim.SetIKPosition(AvatarIKGoal.RightHand, targetrightPosition);
                            // 원래 코드
                            //_anim.SetIKPosition(AvatarIKGoal.RightHand, rightTarget.target.position);
                            _anim.SetIKRotation(AvatarIKGoal.RightHand, rightTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetRight.x, rotationOffsetRight.y, rotationOffsetRight.z))); // TEST
                        }
                        else // 원래 없던 코드
                        {
                            _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightArmReturnWeight);
                            _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightArmReturnWeight);

                            _anim.SetIKPosition(AvatarIKGoal.RightHand, targetrightPosition);
                            _anim.SetIKRotation(AvatarIKGoal.RightHand, rightTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetRight.x, rotationOffsetRight.y, rotationOffsetRight.z))); // TEST
                        }

                    }
                    else
                    {
                        _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
                        _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);

                        _anim.SetIKPosition(AvatarIKGoal.RightHand, rightTarget.target.position);
                        _anim.SetIKRotation(AvatarIKGoal.RightHand, rightTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetRight.x, rotationOffsetRight.y, rotationOffsetRight.z))); // TEST
                    }

                }
                

                // Set the left hand target position and rotation, if one has been assigned
                if (leftTarget.target != null)
                {
                    if(!alwaysZero)
                    {
                        if (leftTarget.completeLength + 0.21f >= safetyRegionLeft.targetObstacle.distance || leftspeed >= 0.5f)
                        {
                            _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftArmWeight);
                            _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftArmWeight);

                            _anim.SetIKPosition(AvatarIKGoal.LeftHand, targetleftPosition);
                            // 원래 아래 코드
                            //_anim.SetIKPosition(AvatarIKGoal.LeftHand, leftTarget.target.position);
                            _anim.SetIKRotation(AvatarIKGoal.LeftHand, leftTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetLeft.x, rotationOffsetLeft.y, rotationOffsetLeft.z))); // TEST
                        }
                        else // 원래 없던 코드
                        {
                            _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftArmReturnWeight);
                            _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftArmReturnWeight);
                            _anim.SetIKPosition(AvatarIKGoal.LeftHand, targetleftPosition);
                            _anim.SetIKRotation(AvatarIKGoal.LeftHand, leftTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetLeft.x, rotationOffsetLeft.y, rotationOffsetLeft.z))); // TEST
                        }

                    }
                    else
                    {
                        _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                        _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
                        _anim.SetIKPosition(AvatarIKGoal.LeftHand, leftTarget.target.position);
                        _anim.SetIKRotation(AvatarIKGoal.LeftHand, leftTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetLeft.x, rotationOffsetLeft.y, rotationOffsetLeft.z))); // TEST
                    }
                }
                
            }
            // If the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                // _anim.SetLookAtWeight(0);
            }
        }
    }

    #endregion
}
