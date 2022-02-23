using OWML.Common;
using UnityEngine;

namespace CelesteWilds
{
    //Apenas se tiver a roupa completa
    public class Dashing : MonoBehaviour
    {
        PlayerMovementAudio playerMovementAudio;

        public bool allowNormalDashing = true;

        private float maxDashStamina = 1f;
        private float currentDashStamina = 1f;
        private float dashStaminaConsuption = 1f;

        private float defaultDashForce = 8f;
        private float climbDashForce = 7.5f;

        PlayerCharacterController playerCharacterController;
        Transform playerCamera;
        OWRigidbody playerBody;

        Climbing climbing;
        float coyoteTime = 0.75f;
        bool hasUsedClimbingDash = false;
        bool restoreDashOnClimb = false;
        bool wasClimbing = false;

        public IModConsole c;
        public void Start()
        {
            playerCharacterController = Locator.GetPlayerController();
            playerCamera = Locator.GetPlayerCamera().transform;
            playerBody = Locator.GetPlayerBody();
            climbing = gameObject.GetComponent<Climbing>();

            playerMovementAudio = Locator.GetPlayerAudioController().GetComponentInChildren<PlayerMovementAudio>();
        }


        bool wasJumpPressed = false;

        public void Dash(Vector3 direction, float dashForce) 
        {
            playerBody.AddVelocityChange(direction * dashForce);
            playerMovementAudio.OnJump();
        }

        float lastClimbingTime;

        public void Update()
        {
            if (wasClimbing && !climbing.isClimbing)
            {
                lastClimbingTime = Time.time;
                hasUsedClimbingDash = false;
            }

            bool isJumpPressed = OWInput.IsPressed(InputLibrary.jump, InputMode.Character | InputMode.NomaiRemoteCam, 0f);

            if (playerCharacterController.IsGrounded() || (climbing.isClimbing && restoreDashOnClimb)) 
            {
                currentDashStamina = maxDashStamina;
            }
            
            if(isJumpPressed && !wasJumpPressed) 
            {
                if (Time.time - lastClimbingTime <= coyoteTime && !hasUsedClimbingDash) 
                {
                    Dash(playerCamera.forward, climbDashForce);
                    hasUsedClimbingDash = true;
                }
                else if (!playerCharacterController.IsGrounded() && allowNormalDashing)
                {
                    if (currentDashStamina >= dashStaminaConsuption)
                    {
                        Dash(playerCamera.forward, defaultDashForce);
                        currentDashStamina = Mathf.Max(currentDashStamina - dashStaminaConsuption, 0f);
                    }
                }
            }
            ChangePlayerResources.SetBoostChargeFraction(currentDashStamina / maxDashStamina);

            wasJumpPressed = isJumpPressed;
            
            wasClimbing = climbing.isClimbing;
        }
    }
            
}
