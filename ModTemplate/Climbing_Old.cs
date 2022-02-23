using System;
using HarmonyLib;
using OWML.Common;
using UnityEngine;

namespace CelesteWilds
{
    //Apenas se tiver com uma roupa e não estiver em 0G
    public class Climbing_Old : MonoBehaviour
    {
        ScreenPrompt climbingPrompt;
        ScreenPrompt stopClimbingPrompt;

        FirstPersonManipulator firstPersonManipulator;
        Transform playerCamera;
        PlayerBody playerBody;

        CapsuleCollider playerCollider;

        CimblingAttachPointController_Old controller;
        PlayerAttachPoint attachPoint;

        public IModConsole c;

        public event Action OnStopClimbing;
        public event Action OnStartClimbing;

        public const float MaxClimbingStamina = 100f;
        private float currentClimbingStamina = 100f;
        private float climbingStaminaConsuption = 10f;
        private float recoveryTime = 1f;
        private float recoveryStaminaAdition = 25f;
        private float lastClimbingTime = 0f;

        public void Start() 
        {
            playerCamera = Locator.GetPlayerCamera().transform;
            playerBody = Locator.GetPlayerBody().gameObject.GetComponent<PlayerBody>();
            playerCollider = (CapsuleCollider)Locator.GetPlayerCollider();
            firstPersonManipulator = Locator.GetPlayerCamera().GetComponent<FirstPersonManipulator>();

            GameObject attachPointObject = new GameObject("PlayerCimblingAttachPoint");
            attachPointObject.AddComponent<Rigidbody>();
            attachPointObject.AddComponent<OWRigidbody>();
            attachPointObject.AddComponent<FollowTransform>();
            attachPointObject.AddComponent<MatchRigidbody>();

            attachPoint = attachPointObject.AddComponent<PlayerAttachPoint>();

            AccessTools.FieldRefAccess<PlayerAttachPoint, bool>("_lockPlayerTurning")(attachPoint) = false;
            AccessTools.FieldRefAccess<PlayerAttachPoint, bool>("_centerCamera")(attachPoint) = false;
            AccessTools.FieldRefAccess<PlayerAttachPoint, bool>("_matchRotation")(attachPoint) = false;

            controller = attachPointObject.AddComponent<CimblingAttachPointController_Old>();

            Transform followObject = new GameObject("PlayerCimblingFollowObject").transform;
            MatchTransform matchTransf = attachPointObject.AddComponent<MatchTransform>();
            matchTransf._targetTransform = followObject;
            matchTransf._matchPosition = true;
            matchTransf._matchRotation = true;
            matchTransf._matchLocal = false;

            controller.followObject = followObject;

            climbingPrompt = new ScreenPrompt(InputLibrary.interact, "Climb", 0, ScreenPrompt.DisplayState.Normal);
            climbingPrompt.SetVisibility(false);
            stopClimbingPrompt = new ScreenPrompt(InputLibrary.cancel, "Stop Climbing", 0, ScreenPrompt.DisplayState.Normal);
            stopClimbingPrompt.SetVisibility(false);

            Locator.GetPromptManager().AddScreenPrompt(climbingPrompt, PromptPosition.Center, false);
            Locator.GetPromptManager().AddScreenPrompt(stopClimbingPrompt, PromptPosition.Center, false);

            ChangePlayerResources.SetCurrentFuel(currentClimbingStamina);
        }

        private void StartClimbing(Collider wall,Vector3 position)
        {
            OWInput.ChangeInputMode(InputMode.NomaiRemoteCam);
            //controller.followObject.rotation = playerBody.transform.rotation;
            controller.transform.rotation = playerBody.transform.rotation;
            Climb(wall, position);
            attachPoint.AttachPlayer();

            OnStartClimbing?.Invoke();
        }
        private void Climb(Collider wall, Vector3 position) 
        {
            controller.Climb(c, wall, playerCollider.radius, playerCollider.height, position, playerCollider.transform.up);
        }
        private void StopClimbing()
        { 
            OWInput.ChangeInputMode(InputMode.Character);
            controller.StopClimbing();
            attachPoint.DetachPlayer();
            OnStopClimbing?.Invoke();
        }

        bool wasInteractPressed = false;
        bool wasCancelPressed = false;
        bool wasJumpPressed = false;

        public void Update() 
        {
            climbingPrompt.SetVisibility(false);
            stopClimbingPrompt.SetVisibility(false);

            bool isInteractPressed = OWInput.IsPressed(InputLibrary.interact, InputMode.Character | InputMode.NomaiRemoteCam, 0f); 
            bool isCancelPressed = OWInput.IsPressed(InputLibrary.cancel, InputMode.Character | InputMode.NomaiRemoteCam, 0f);
            bool isJumpPressed = OWInput.IsPressed(InputLibrary.jump, InputMode.Character | InputMode.NomaiRemoteCam, 0f);

            if (controller.IsClimbing())
            {
                currentClimbingStamina = Mathf.Clamp(currentClimbingStamina - climbingStaminaConsuption * Time.deltaTime, 0f, MaxClimbingStamina);

                stopClimbingPrompt.SetVisibility(true);
                if ((isCancelPressed && !wasCancelPressed) || (isJumpPressed && !wasJumpPressed) || currentClimbingStamina <= 0f || PlayerState.InZeroG() || PlayerState.IsInsideShip())
                {
                    StopClimbing();
                    lastClimbingTime = Time.time;
                }
            }
            else if(lastClimbingTime + recoveryTime <= Time.time)
            {
                currentClimbingStamina = Mathf.Clamp(currentClimbingStamina + recoveryStaminaAdition * Time.deltaTime, 0f, MaxClimbingStamina);
            }

            float factor = Vector3.Dot(playerBody.transform.up, playerCamera.transform.forward) * 0.5f + 0.5f;
            float maxDistance = Mathf.Lerp(2.5f, 1.5f, factor);
            bool hasFoundWall = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance, OWLayerMask.physicalMask);


            if (!firstPersonManipulator.HasFocusedInteractible() 
                && !PlayerState.IsInsideShip() && !PlayerState.InZeroG() && (!PlayerState.IsAttached() || Utils.PlayerAttachedPoint() == attachPoint.transform))
            {
                if (hasFoundWall)
                {
                    climbingPrompt.SetVisibility(true);
                    climbingPrompt.SetText("Climb on" + hit.collider.transform.name);
                }

                if (hasFoundWall && isInteractPressed && !wasInteractPressed && currentClimbingStamina > 0f)
                {
                    if (!controller.IsClimbing())
                    {
                        Vector3 direction = Vector3.ProjectOnPlane(hit.normal, playerBody.transform.up);
                        StartClimbing(hit.collider, playerBody.transform.position);
                    }
                    else
                    {
                        Climb(hit.collider, playerBody.transform.position);
                    }
                }
            }

            wasInteractPressed = isInteractPressed;
            wasCancelPressed = isCancelPressed;
            wasJumpPressed = isJumpPressed;

            ChangePlayerResources.SetCurrentFuel(currentClimbingStamina);
        }
    }
}
