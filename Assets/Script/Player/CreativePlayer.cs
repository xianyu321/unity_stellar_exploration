using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativePlayer : Player
{
    [Header("创造模式参数")]
    public bool isJumpDown = false;
    public bool isFlying = false;
    public float FlyingSpeedHoist = 2.5f;
    public float CreativePlayerUpSpeed = 10f;
    public float CreativePlayerDownSpeed = -8f;
    private float CreativePlayerYSpeed = 0f;
    private float lastSpacePressTime;//上一次空格按下时间
    private float timeBetweenDoubleClicks = 0.4f;//双击空格间隔时间
    private float lastHandoffFlyTime;//上一次飞行切换时间
    private float timeHandoffDoubleClicks = 0.8f;//切换飞行的间隔时间

    protected override void JumpDown()
    {
        if (Input.GetButtonDown("Jump"))
        {
            isJumpDown = true;
            if (isGrounded)
            {
                jumpRequest = true;
            }
            if (Time.time - lastSpacePressTime < timeBetweenDoubleClicks)
            {
                if(Time.time - lastHandoffFlyTime > timeHandoffDoubleClicks){
                    lastHandoffFlyTime = Time.time;
                    verticalMomentum = 0;
                    isFlying = !isFlying;
                }
            }
            lastSpacePressTime = Time.time;
        }
        if (Input.GetButtonUp("Jump")){
            isJumpDown = false;
        }
    }

    protected override void YMove(){
        if (isFlying){
            if(isGrounded){
                isFlying = false;
                return;
            }
            if(isJumpDown && isSquating){
                return;
            }
            CreativePlayerYSpeed = 0;
            if (isJumpDown){
                CreativePlayerYSpeed = CreativePlayerUpSpeed;
            }
            if (isSquating){
                CreativePlayerYSpeed = CreativePlayerDownSpeed;
            }
            velocity += Vector3.up * CreativePlayerYSpeed * Time.fixedDeltaTime;
        }else{
            base.YMove();
        }
    }

    protected override void XZMove(){
        if(isFlying){
            if (isSprinting)
            {
                velocity = (transform.forward * vertical * frontSprintSpeed + transform.right * horizontal * horizontalSprintSeppd)
                * Time.fixedDeltaTime;
            }
            else
            {
                if (vertical > 0)
                {
                    velocity = (transform.forward * vertical * frontWalkSpeed + transform.right * horizontal * horizontalWalkSpeed)
                    * Time.fixedDeltaTime ;
                }
                else
                {
                    velocity = (transform.forward * vertical * backWalkSpeed + transform.right * horizontal * horizontalWalkSpeed)
                    * Time.fixedDeltaTime;
                }
            }
            velocity *= FlyingSpeedHoist;
        }else{
            base.XZMove();
        }
    }
}
