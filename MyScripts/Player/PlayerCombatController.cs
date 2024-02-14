using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Keybinds")]
    private KeyCode atkKey = KeyCode.Mouse0;
    private KeyCode defKey = KeyCode.Mouse1;
    private KeyCode abKeyOne = KeyCode.Alpha1;
    private KeyCode abKeyTwo = KeyCode.Alpha2;
    private KeyCode abKeyThree = KeyCode.Alpha3;
    private KeyCode abKeyFour = KeyCode.Alpha4;
    private KeyCode abKeyFive = KeyCode.Alpha5;
    private KeyCode abKeySpecial = KeyCode.R;

    [Header("Cooldowns")]
    public float abCooldownOne;
    public float abCooldownTwo;
    public float abCooldownThree;
    public float abCooldownFour;
    public float abCooldownFive;
    public float abCooldownSpecial;
    public float atkTimer;

    [Header("States")]
    public bool isBlockingAvailable;
    public bool isBlocking;
    public bool isWeakAtkAvailable;
    public bool isChargingStrongAtkAvailable;
    public bool isChargingStrongAtk;
    public bool isAbilityAvailable;

    public void Update()
    {
        DoDef();
        DoAtk();
        CheckCooldown(abKeyOne, abCooldownOne, 1);
        CheckCooldown(abKeyTwo, abCooldownTwo, 2);
        CheckCooldown(abKeyThree, abCooldownThree, 3);
        CheckCooldown(abKeyFour, abCooldownFour, 4);
        CheckCooldown(abKeyFive, abCooldownFive, 5);
        CheckCooldown(abKeySpecial, abCooldownSpecial, 6);

        RunCooldowns();
    }

    private void DoAtk()
    {
        if (Input.GetKey(atkKey) && isChargingStrongAtkAvailable)
        {
            // Disable blocking and abilities 
            isBlockingAvailable = false;
            isAbilityAvailable = false;

            atkTimer += Time.deltaTime;

            if (atkTimer > 0.3f && !isChargingStrongAtk)
            {
                isChargingStrongAtk = true;
            }

            if (atkTimer >= 2.0f) 
            {
                // Fires Strong Attack!
                Debug.Log("Strong Attack!");
                isChargingStrongAtk = false;
                atkTimer = 0.0f;
            }
        }
        
        if(Input.GetKeyUp(atkKey) && isWeakAtkAvailable)
        {
            isChargingStrongAtk = false;

            if (atkTimer != 0.0f && atkTimer <= 0.2f)  
            {
                // Fires weak attack!
                Debug.Log("Weak Attack!");
            }
            atkTimer = 0.0f;

            // Activate blocking and abilities 
            isBlockingAvailable = true;
            isAbilityAvailable = true;
        }
    }

    private void DoDef()
    {
        if (Input.GetKey(defKey) && isBlockingAvailable && !isBlocking)
        {
            // Starts blocking!
            isBlocking = true;
            Debug.Log("Blocking");

            // Deaktivate weak atk, strong atk and abilities
            isWeakAtkAvailable = false;
            isChargingStrongAtkAvailable = false;
            isAbilityAvailable = false;
        }
        if (Input.GetKeyUp(defKey))
        {
            // Ends blocking!
            isBlocking = false;
            Debug.Log("Blocking ended");

            // Aktivate weak atk, strong atk and abilities
            isWeakAtkAvailable = true;
            isChargingStrongAtkAvailable = true;
            isAbilityAvailable = true;
        }
    }

    private void CheckCooldown(KeyCode key, float abCooldown,int abSlot)
    {
        if (Input.GetKeyDown(key) && abCooldown <= 0.0f && isAbilityAvailable)
        {
            // Disable
            //isWeakAtkAvailable = false;
            //isChargingStrongAtkAvailable = false;
            //isAbilityAvailable = false;
            //isBlockingAvailable = false;

            // If animation of ability ended activate atk, block and abilities

            switch (abSlot)
            {
                case 1:
                    abCooldownOne = 5.0f;
                    // Use ability 1
                    Debug.Log("1");
                    break;

                case 2:
                    abCooldownTwo = 5.0f;
                    // Use ability 2
                    Debug.Log("2");
                    break;

                case 3:
                    abCooldownThree = 5.0f;
                    // Use ability 3
                    Debug.Log("3");
                    break;

                case 4:
                    abCooldownFour = 5.0f;
                    // Use ability 4
                    Debug.Log("4");
                    break;

                case 5:
                    abCooldownFive = 5.0f;
                    // Use ability 5
                    Debug.Log("5");
                    break;

                case 6:
                    abCooldownSpecial = 10.0f;
                    // Use ability special
                    Debug.Log("R");
                    break;

                default:
                    break;
            }
        }
    }

    private void RunCooldowns()
    {
        if(abCooldownOne > 0.0f)   {abCooldownOne -= Time.deltaTime;}
        if(abCooldownTwo > 0.0f)   {abCooldownTwo -= Time.deltaTime;}
        if(abCooldownThree > 0.0f) {abCooldownThree -= Time.deltaTime;}
        if(abCooldownFour > 0.0f)  {abCooldownFour -= Time.deltaTime;}
        if(abCooldownFive > 0.0f)  {abCooldownFive -= Time.deltaTime;}
        if(abCooldownSpecial > 0.0f)   {abCooldownSpecial -= Time.deltaTime;}

        // Set overall abilties cooldown
        if (abCooldownOne > 1.0f && abCooldownTwo > 1.0f && abCooldownThree > 1.0f && abCooldownFour > 1.0f && abCooldownFive > 1.0f && abCooldownSpecial > 6.0f)
        {
            isAbilityAvailable = true;
        }
    }
}
