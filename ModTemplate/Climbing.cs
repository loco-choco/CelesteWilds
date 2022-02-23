using OWML.Common;
using UnityEngine;

namespace CelesteWilds
{
    //Apenas se tiver com uma roupa e não estiver em 0G
    public class Climbing : MonoBehaviour
    {
        PlayerAudioController playerAudioController;

        ScreenPrompt centerClimbingPrompt;
        ScreenPrompt sideClimbingPrompt;
        ScreenPrompt stopClimbingPrompt;
        NotificationData startedClimbingNotification;
        NotificationData stopedClimbingNotification;

        FirstPersonManipulator firstPersonManipulator;
        Transform playerCamera;

        OWRigidbody owrigidbody;
        CapsuleCollider capsuleCollider;

        MatchRigidbody matchRigidbody;

        public IModConsole c;

        public const float MaxClimbingStamina = 100f;
        private float currentClimbingStamina = 100f;
        private float climbingStaminaConsuption = 4f;
        private float recoveryTime = 1f;
        private float recoveryStaminaAdition = 25f;
        private float lastClimbingTime = 0f;

        private float climbingVerticalSpeed = 4f;
        private float climbingHorizontalSpeed = 2f;
        public bool isClimbing = false;

        public void Start() 
        {
            playerCamera = Locator.GetPlayerCamera().transform;
            firstPersonManipulator = playerCamera.GetComponent<FirstPersonManipulator>();
            owrigidbody = gameObject.GetComponent<OWRigidbody>();
            capsuleCollider = (CapsuleCollider)Locator.GetPlayerCollider();
            playerAudioController = Locator.GetPlayerAudioController();

            matchRigidbody = gameObject.GetComponent<MatchRigidbody>();

            centerClimbingPrompt = new ScreenPrompt(InputLibrary.interact, "Climb", 0, ScreenPrompt.DisplayState.Normal);
            centerClimbingPrompt.SetVisibility(false);
            sideClimbingPrompt = new ScreenPrompt(InputLibrary.interact, "Climb", 0, ScreenPrompt.DisplayState.Normal);
            centerClimbingPrompt.SetVisibility(false);
            stopClimbingPrompt = new ScreenPrompt(InputLibrary.cancel, "Stop Climbing", 0, ScreenPrompt.DisplayState.Normal);
            stopClimbingPrompt.SetVisibility(false);

            Locator.GetPromptManager().AddScreenPrompt(centerClimbingPrompt, PromptPosition.Center, false);
            Locator.GetPromptManager().AddScreenPrompt(sideClimbingPrompt, PromptPosition.UpperRight, false);
            Locator.GetPromptManager().AddScreenPrompt(stopClimbingPrompt, PromptPosition.UpperRight, false);

            startedClimbingNotification = new NotificationData(NotificationTarget.Player, "STARTED CLIMBING");
            stopedClimbingNotification = new NotificationData(NotificationTarget.Player, "STOPED CLIMBING");

            ChangePlayerResources.SetCurrentFuel(currentClimbingStamina);
        }

        private void StartClimbing(Collider wall, SurfaceType surfaceType)
        {
            Climb(wall);
            playerAudioController.PlayOneShotInternal(PlayerMovementAudio.GetLandingAudioType(surfaceType));
            startedClimbingNotification.displayMessage = TextTranslation.Translate_UI((int)UIPrompts.STARTED_CLIMB);
            NotificationManager.SharedInstance.PostNotification(startedClimbingNotification);
            isClimbing = true;
        }
        private void Climb(Collider wall) 
        {
            matchRigidbody.targetRigidbody = wall.attachedRigidbody;
        }
        public void StopClimbing()
        {
            stopedClimbingNotification.displayMessage = TextTranslation.Translate_UI((int)UIPrompts.STOPED_CLIMB);
            NotificationManager.SharedInstance.PostNotification(stopedClimbingNotification);
            matchRigidbody.targetRigidbody = null;
            isClimbing = false;
        }

        bool wasCancelPressed = false;
        bool wasJumpPressed = false;

        public void ClimbUpdate()
        {
            Vector2 movementInput = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character);
            float upMovementInput = OWInput.GetValue(InputLibrary.thrustUp, InputMode.Character);
            float downMovementInput = OWInput.GetValue(InputLibrary.thrustDown, InputMode.Character);

            if (Locator.GetPlayerController().IsGrounded()) 
            {
                if (upMovementInput > 0f)
                    Locator.GetPlayerController().MakeUngrounded();
                else if(downMovementInput > 0f)
                    StopClimbing();
            }
            Vector3 horizontalInput = transform.forward * movementInput.y + transform.right * movementInput.x;
            Vector3 verticalInput = transform.up * (upMovementInput - downMovementInput);
            Vector3 expectedFinalPosition = transform.position + (verticalInput * climbingVerticalSpeed + horizontalInput * climbingHorizontalSpeed) * Time.fixedDeltaTime;
            owrigidbody._rigidbody.MovePosition(expectedFinalPosition);
        }

        public bool CheckForExistingWalls(Rigidbody currentRigidbody,Vector3 climbingDirection, Vector3 centerOffset, float checkHeight, float checkRadius,bool useSphere, out bool wasCurrentRigidbodyFound, out Rigidbody closestRigidbody) 
        {
            Vector3 upPoint = transform.position + centerOffset + climbingDirection * checkHeight / 2f;
            Vector3 downPoint = transform.position + centerOffset - climbingDirection * checkHeight / 2f;
            Collider[] possibleWalls;
            if(useSphere)
                possibleWalls = Physics.OverlapSphere(transform.position + centerOffset, checkRadius, OWLayerMask.physicalMask);
            else
                possibleWalls = Physics.OverlapCapsule(upPoint, downPoint, checkRadius, OWLayerMask.physicalMask);


            bool foundAWall = false;
            wasCurrentRigidbodyFound = false;
            closestRigidbody = null;

            for (int i = 0; i < possibleWalls.Length && (!foundAWall || !wasCurrentRigidbodyFound); i++) 
            {
                if (possibleWalls[i].transform != transform)
                {
                    if (!foundAWall)
                    {
                        closestRigidbody = possibleWalls[i].attachedRigidbody;
                        foundAWall = true;
                    }

                    if (!wasCurrentRigidbodyFound && possibleWalls[i].attachedRigidbody == currentRigidbody)
                        wasCurrentRigidbodyFound = true;
                }
            }
            return foundAWall;
        }

        public void FixedUpdate() 
        {
            if (isClimbing)
            {
                bool useSphere = PlayerState.InZeroG();
                float radius = useSphere ? capsuleCollider.height * 1.5f : capsuleCollider.radius * 2.25f;
                float offsetSize = useSphere ? 0f : (capsuleCollider.height + capsuleCollider.radius * 1.5f) * 0.25f;

                if (CheckForExistingWalls(matchRigidbody.targetRigidbody, transform.up, transform.up * offsetSize, capsuleCollider.height , radius, useSphere, out bool wasCurrentRigidbFound, out Rigidbody closestRigidb))
                {
                    if(!wasCurrentRigidbFound)
                        matchRigidbody.targetRigidbody = closestRigidb;

                    ClimbUpdate();
                }
                else 
                {
                    StopClimbing();
                }
            }
        }

        public void Update() 
        {
            centerClimbingPrompt.SetVisibility(false);
            centerClimbingPrompt.SetText(TextTranslation.Translate_UI((int)UIPrompts.CLIMB));
            sideClimbingPrompt.SetVisibility(false);
            sideClimbingPrompt.SetText(TextTranslation.Translate_UI((int)UIPrompts.CLIMB));
            stopClimbingPrompt.SetVisibility(false);
            stopClimbingPrompt.SetText(TextTranslation.Translate_UI((int)UIPrompts.STOP_CLIMB));

            bool isInteractPressed = OWInput.IsPressed(InputLibrary.interact, InputMode.Character | InputMode.NomaiRemoteCam, 0f); 
            bool isCancelPressed = OWInput.IsPressed(InputLibrary.cancel, InputMode.Character | InputMode.NomaiRemoteCam, 0f);
            bool isJumpPressed = OWInput.IsPressed(InputLibrary.jump, InputMode.Character | InputMode.NomaiRemoteCam, 0f);

            if (isClimbing)
            {
                currentClimbingStamina = Mathf.Clamp(currentClimbingStamina - climbingStaminaConsuption * Time.deltaTime, 0f, MaxClimbingStamina);

                stopClimbingPrompt.SetVisibility(true);
                if (PlayerState.IsAttached() || (isCancelPressed && !wasCancelPressed) || (isJumpPressed && !wasJumpPressed) || currentClimbingStamina <= 0f)
                {
                    StopClimbing();
                    lastClimbingTime = Time.time;
                }
            }
            else if(lastClimbingTime + recoveryTime <= Time.time)
            {
                currentClimbingStamina = Mathf.Clamp(currentClimbingStamina + recoveryStaminaAdition * Time.deltaTime, 0f, MaxClimbingStamina);
            }

            float factor = 0.5f + Vector3.Dot(transform.up, playerCamera.transform.forward) * 0.5f;
            float maxDistance = Mathf.Lerp(capsuleCollider.radius * 1.75f, capsuleCollider.radius * 2f, factor);
            bool hasFoundWall = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance, OWLayerMask.physicalMask);


            if (!firstPersonManipulator.HasFocusedInteractible() && !PlayerState.IsAttached())
            {
                if (hasFoundWall)
                {
                    if (!isClimbing)
                        centerClimbingPrompt.SetVisibility(true);

                    else
                        sideClimbingPrompt.SetVisibility(true);

                }

                if (hasFoundWall && isInteractPressed && currentClimbingStamina > 0f)
                {
                    if (!isClimbing)
                        StartClimbing(hit.collider, Locator.GetSurfaceManager().GetHitSurfaceType(hit));

                    else
                        Climb(hit.collider);
                }
            }
            wasCancelPressed = isCancelPressed;
            wasJumpPressed = isJumpPressed;

            ChangePlayerResources.SetCurrentFuel(currentClimbingStamina);
        }
    }
}
