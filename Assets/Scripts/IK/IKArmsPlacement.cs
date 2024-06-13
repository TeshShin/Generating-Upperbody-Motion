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

    // ��� IK
    public bool activeHeadIK = true;
    public Vector3 prevheadlook;

    // IK �ӵ� ���� ����
    // �� �ȴ�!!! ���� ��ֹ��� ���� �ӵ��� ������ �� speed�� �����ϸ� �ǰڴµ�?!!
    private Vector3 targetrightPosition;
    [Range(0f,10f)] public float rightReachSpeed = 1f;
    private float rightBaseReachSpeed;

    private Vector3 targetleftPosition;
    [Range(0f, 10f)] public float leftReachSpeed = 1f;
    private float leftBaseReachSpeed;


    [Header("DistanceSpeedCalculator")]
    // selfpos�� ĳ������ ��ġ�� ��Ÿ���� ���� �� ��ũ��Ʈ�� ���� ������Ʈ�� ��ġ��,
    // obstaclepos�� ���� Ÿ���õ� targetobstacle�� ��ġ��
    public Vector3 SelfPos; // ĳ������ ���� ��ġ
    public Vector3 leftObstaclePos; // ���� ��ȣ�ۿ��� ��ü�� ��ġ
    public Vector3 rightObstaclePos; // ���� ��ȣ�ۿ��� ��ü�� ��ġ
    public float previousleftDistance; // ���� �����ӿ����� ������ �Ÿ�
    public float previousrightDistance; // ���� �����ӿ����� ������ �Ÿ�
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
        




        /// ���� �ִ� �ڵ�
        activateIKLeft = leftTarget.activateIK;
        activateIKRight = rightTarget.activateIK;
        /// Head IK ����
        if (activeHeadIK)
        {
            if (safetyRegionLeft.obstacles.Count != 0) // ���� ���°� �켱 (���ÿ� ������ �� ��, �ʰ� ���� �� �Ĵٺ��� ����� �� ����������)
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
        // ���� �߰��� �ڵ� - IK�� ���� ��, �ӵ��� �����ϱ� ���ؼ� ���
        // ������ reaction time�� �����ؼ� �ǵ帱 ���� ������, reaction time�� ���ؼ� �ڿ������� �������� reaction time ���� �����ϰ� �����ؾ� �� �� ����.
        // 
        targetrightPosition = Vector3.MoveTowards(rightTarget.transform.position, rightTarget.target.position, rightReachSpeed * Time.deltaTime);
        targetleftPosition = Vector3.MoveTowards(leftTarget.transform.position, leftTarget.target.position, leftReachSpeed * Time.deltaTime);
        //

        /// �ӵ� ���� ����
        SelfPos = new Vector2(transform.position.x, transform.position.z);

        if (safetyRegionLeft.obstacles.Count != 0) // obstacle�� �߰ߵǾ��� �� ��� ����.
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


        /// ���� ���� ���� �ȴ��� �� ó��
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




        /// �����ִ� ��ֹ����� ��ȣ�ۿ뿡 ���� ����ó��
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
        //    rightReachSpeed = rightspeed * 2.5f + rightBaseReachSpeed; // �ι�° ���� �⺻������ ������ ������
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
                
                if (rightTarget.target != null) // rightTarget == Target Right Hand (���̶�Ű�� ����)
                {
                    if(!alwaysZero)
                    {   // ���� ���� ���� (���� �� ���� ��� �ո� ���� �ִ� ���� ���ֱ� ����.)
                        // 0.21f ���� ������ ��ü�� �������� �� ���� + �չٴ� ���̵� ���ؾ���. �չٴ� ���̴� 0.05������, �� ���̷� ���ؼ� ��ü�� ƨ������ ��쿡 �������ϴٰ� ���ٰ���
                        // ���� �ݺ��ϱ� ������ 0.21f���� ������.
                        if (rightTarget.completeLength + 0.21f >= safetyRegionRight.targetObstacle.distance || rightspeed >= 0.5f)
                        {
                            _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightArmWeight);
                            _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rightArmWeight);

                            _anim.SetIKPosition(AvatarIKGoal.RightHand, targetrightPosition);
                            // ���� �ڵ�
                            //_anim.SetIKPosition(AvatarIKGoal.RightHand, rightTarget.target.position);
                            _anim.SetIKRotation(AvatarIKGoal.RightHand, rightTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetRight.x, rotationOffsetRight.y, rotationOffsetRight.z))); // TEST
                        }
                        else // ���� ���� �ڵ�
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
                            // ���� �Ʒ� �ڵ�
                            //_anim.SetIKPosition(AvatarIKGoal.LeftHand, leftTarget.target.position);
                            _anim.SetIKRotation(AvatarIKGoal.LeftHand, leftTarget.target.rotation * Quaternion.Euler(new Vector3(rotationOffsetLeft.x, rotationOffsetLeft.y, rotationOffsetLeft.z))); // TEST
                        }
                        else // ���� ���� �ڵ�
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
