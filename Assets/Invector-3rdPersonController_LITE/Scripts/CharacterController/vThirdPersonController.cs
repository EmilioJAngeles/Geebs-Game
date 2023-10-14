using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Invector.vCharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        [HideInInspector] public UnityEvent onBallCollected = new UnityEvent();
        public GameObject basketballPrefab = null;
        public GameObject digUI = null;
        public GameObject jumpUI = null;
        [SerializeField] private GameObject jumpIncreasedUI = null;
        public GameObject sprintUI = null;
        [SerializeField] private GameObject enterEndUI = null;
        [SerializeField] private GameObject ballCounter = null;
        [SerializeField] private GameObject endGameUI = null;
        [SerializeField] private GameObject finalCamera = null;
        [SerializeField] private Transform geebsSpawnPoint = null;

        private Coroutine showJumpUICoroutine = null;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Basketball")
            {
                Destroy(collision.gameObject);
                onBallCollected.Invoke();
                jumpHeight++;

                if (showJumpUICoroutine != null)
                {
                    StopCoroutine(showJumpUICoroutine);
                    jumpIncreasedUI.SetActive(false);
                }
                showJumpUICoroutine = StartCoroutine(JumpIncreased());
            }
        }

        private IEnumerator JumpIncreased()
        {
            jumpIncreasedUI.SetActive(true);
            yield return new WaitForSeconds(2);
            jumpIncreasedUI.SetActive(false);
            showJumpUICoroutine = null;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == "EmptyRock" || collider.gameObject.tag == "PrizeRock")
            {
                digUI.SetActive(true);
            }

            if (collider.gameObject.tag == "InstructionsPlatform")
            {
                jumpUI.SetActive(true);
            }

            if (collider.gameObject.tag == "EndDoor")
            {
                enterEndUI.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.tag == "EmptyRock" || collider.gameObject.tag == "PrizeRock")
            {
                digUI.SetActive(false);
            }

            if (collider.gameObject.tag == "InstructionsPlatform")
            {
                jumpUI.SetActive(false);
            }

            if (collider.gameObject.tag == "EndDoor")
            {
                enterEndUI.SetActive(false);
            }
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.gameObject.tag == "EmptyRock")
            {
                if (isDigging)
                {
                    isDigging = false;
                    StartCoroutine(WaitAndDestroyDirtMound(collider.gameObject, false));
                }
            }

            if (collider.gameObject.tag == "PrizeRock")
            {
                if (isDigging)
                {
                    isDigging = false;
                    StartCoroutine(WaitAndDestroyDirtMound(collider.gameObject, true));
                }
            }

            if (collider.gameObject.tag == "EndDoor")
            {
                if (Input.GetKey(KeyCode.JoystickButton1))
                {
                    finalCamera.SetActive(true);
                    ballCounter.SetActive(false);
                    endGameUI.SetActive(true);
                    enterEndUI.SetActive(false);

                    this.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator WaitAndDestroyDirtMound(GameObject dirtMound, bool isPrizeRock)
        {
            yield return new WaitForSeconds(0.5f);

            if (isPrizeRock)
            {
                var basketballInstance = Instantiate(basketballPrefab, this.gameObject.transform.position + (this.gameObject.transform.forward * 2), Quaternion.identity);
            }

            Destroy(dirtMound);
            digUI.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button7) && sprintUI.activeInHierarchy)
            {
                sprintUI.SetActive(false);
            }
        }

        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled) return;

            if (inputSmooth == Vector3.zero)
            {
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlLocomotionType()
        {
            if (lockMovement) return;

            if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
            {
                SetControllerMoveSpeed(freeSpeed);
                SetAnimatorMoveSpeed(freeSpeed);
            }
            else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
            {
                isStrafing = true;
                SetControllerMoveSpeed(strafeSpeed);
                SetAnimatorMoveSpeed(strafeSpeed);
            }

            if (!useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlRotationType()
        {
            if (lockRotation) return;

            bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

            if (validInput)
            {
                // calculate input smooth
                inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

                Vector3 dir = (isStrafing && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
                RotateToDirection(dir);
            }
        }

        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (input.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }
        }

        public virtual void Sprint(bool value)
        {
            var sprintConditions = (input.sqrMagnitude > 0.1f && isGrounded &&
                !(isStrafing && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f)));

            if (value && sprintConditions)
            {
                if (input.sqrMagnitude > 0.1f)
                {
                    if (isGrounded && useContinuousSprint)
                    {
                        isSprinting = !isSprinting;
                    }
                    else if (!isSprinting)
                    {
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting)
            {
                isSprinting = false;
            }
        }

        public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }

        public virtual void Jump()
        {
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping = true;

            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", .2f);
        }

        public virtual void Dig()
        {
            if (!isDigging)
            {
                freeSpeed.walkSpeed = freeSpeed.runningSpeed = freeSpeed.sprintSpeed = 0;
                isDigging = true;
                animator.CrossFadeInFixedTime("Dig 2", 0.1f);
                StartCoroutine(DigTimer());
            }

        }

        private IEnumerator DigTimer()
        {
            yield return new WaitForSeconds(1);
            isDigging = false;
            freeSpeed.walkSpeed = 2;
            freeSpeed.runningSpeed = 4;
            freeSpeed.sprintSpeed = 8;
        }
    }
}