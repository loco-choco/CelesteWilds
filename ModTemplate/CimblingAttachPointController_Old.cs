using OWML.Common;
using UnityEngine;

namespace CelesteWilds
{
    //Isso será o controle de uma esfera
    //Ai o player ira tentar seguir essa esfera
    public class CimblingAttachPointController_Old : MonoBehaviour
    {
        public Transform followObject;

        //public float radius;
        //public float height;

        //public float upperRayDeflectiveness = 0.5f;
        //public float downerRayDeflectiveness;
        Vector3 centerPoint;
        Vector3 direction;
        float upmostDistance;
        float downmostDistance;

        Transform transformBeingClimbed;
        OWRigidbody rigidbodyBeingClimbed;

        MatchRigidbody matchRigidbody;

        Collider playerCollider;

        bool isClimbing;
        float climbingSpeed = 4f;

        public void Start()
        {
            playerCollider = Locator.GetPlayerCollider();

            matchRigidbody = gameObject.GetComponent<MatchRigidbody>();
            isClimbing = false;
        }

        public bool IsClimbing() => isClimbing;
        public bool Climb(IModConsole c, Collider colliderToClimb, float radiusToKeep, float playerHeight, Vector3 initialPostion, Vector3 upDirection)
        {
            if (colliderToClimb == null)
            {
                Debug.Log("Given Collider To Climb is null");
                return false;
            }
            Vector3 closestPosition = initialPostion;
            Vector3 normal = Vector3.zero;

            Vector3 capsuleCenter = closestPosition + normal * radiusToKeep;

            var hitsWhenGoingDown = Physics.CapsuleCastAll(capsuleCenter + upDirection * playerHeight / 2f, capsuleCenter - upDirection * playerHeight / 2f
                , radiusToKeep, -upDirection, playerHeight * 1.5f, OWLayerMask.physicalMask);

            RaycastHit downHit = default;

            bool stopSearch = false;
            for (int i = 0; i < hitsWhenGoingDown.Length && !stopSearch; i++)
            {
                if (hitsWhenGoingDown[i].collider != playerCollider)
                {
                    downHit = hitsWhenGoingDown[i];
                    stopSearch = true;
                }
            }

            downmostDistance = stopSearch ? downHit.distance : playerHeight * 1.5f;

            var hitsWhenGoingUp = Physics.CapsuleCastAll(capsuleCenter + upDirection * playerHeight / 2f, capsuleCenter - upDirection * playerHeight / 2f
                , radiusToKeep, upDirection, playerHeight * 1.5f, OWLayerMask.physicalMask);

            RaycastHit upHit = default;

            stopSearch = false;
            for (int i = 0; i < hitsWhenGoingUp.Length && !stopSearch; i++)
            {
                if (hitsWhenGoingUp[i].collider != playerCollider)
                {
                    upHit = hitsWhenGoingUp[i];
                    stopSearch = true;
                }
            }

            upmostDistance = stopSearch ? upHit.distance : playerHeight * 1.5f;

            c.WriteLine(hitsWhenGoingUp.Length + " " + upmostDistance.ToString() + " " + hitsWhenGoingDown.Length.ToString() + " " + downmostDistance.ToString());

            rigidbodyBeingClimbed = colliderToClimb.attachedRigidbody.GetAttachedOWRigidbody();
            transformBeingClimbed = colliderToClimb.transform;

            matchRigidbody.targetRigidbody = rigidbodyBeingClimbed._rigidbody;

            followObject.parent = transformBeingClimbed;
            followObject.position = initialPostion;
            transform.position = initialPostion;

            centerPoint = transformBeingClimbed.InverseTransformPoint(initialPostion);
            direction = transformBeingClimbed.InverseTransformDirection(upDirection).normalized;
            
            isClimbing = true;

            //radius = radiusToKeep;
            //height = playerHeight;
            currentPosition = 0f;
            return true;
        }
        public void StopClimbing()
        {
            rigidbodyBeingClimbed = null;
            transformBeingClimbed = null;
            followObject.parent = null;
            matchRigidbody.targetRigidbody = null;
            isClimbing = false;
        }

        float currentPosition;
        public void FixedUpdate()
        {
            if (!isClimbing)
                return;

            Vector2 movementInput = OWInput.GetAxisValue(InputLibrary.moveXZ, InputMode.Character | InputMode.NomaiRemoteCam);

            currentPosition += movementInput.y * climbingSpeed * Time.fixedDeltaTime;
            currentPosition = Mathf.Clamp(currentPosition, -downmostDistance, upmostDistance);

            Vector3 newPositionForPoint = centerPoint + direction * currentPosition;
            followObject.localPosition = newPositionForPoint;//.position += transform.up * movementInput.y * climbingSpeed * Time.fixedDeltaTime;//newPositionForPoint;
        }

        //public void LateUpdate() 
        //{
        //    if (!isClimbing)
        //        return;

        //    AttachToSurface();
        //}

        //public void AttachToSurface() 
        //{
        //    var hitsFromUpperPart = Physics.RaycastAll(transform.position + transform.up * height / 2f, transform.forward + transform.up * upperRayDeflectiveness
        //       , radius * 1.5f, OWLayerMask.physicalMask);

        //    RaycastHit hitFromUpperPart = default;

        //    bool stopUpperSearch = false;
        //    for (int i = 0; i < hitsFromUpperPart.Length && !stopUpperSearch; i++)
        //    {
        //        if (hitsFromUpperPart[i].collider != playerCollider)
        //        {
        //            hitFromUpperPart = hitsFromUpperPart[i];
        //            stopUpperSearch = true;
        //        }
        //    }

        //    var hitsFromDownerPart = Physics.RaycastAll(transform.position - transform.up * height / 2f, transform.forward + transform.up * downerRayDeflectiveness
        //       , radius * 1.5f, OWLayerMask.physicalMask);

        //    RaycastHit hitFromDownerPart = default;

        //    bool stopDownerSearch = false;
        //    for (int i = 0; i < hitsFromDownerPart.Length && !stopDownerSearch; i++)
        //    {
        //        if (hitsFromDownerPart[i].collider != playerCollider)
        //        {
        //            hitFromDownerPart = hitsFromDownerPart[i];
        //            stopDownerSearch = true;
        //        }
        //    }

        //    Vector3 normal = (hitFromUpperPart.normal + hitFromDownerPart.normal) / 2;
        //    normal = normal.normalized;

        //    var hitsFromNormal = Physics.RaycastAll(transform.position , -normal
        //       , radius * 1.5f, OWLayerMask.physicalMask);

        //    RaycastHit hitFromNormal = default;

        //    bool stopNormalSearch = false;
        //    for (int i = 0; i < hitsFromNormal.Length && !stopNormalSearch; i++)
        //    {
        //        if (hitsFromNormal[i].collider != playerCollider)
        //        {
        //            hitFromNormal = hitsFromNormal[i];
        //            stopNormalSearch = true;
        //        }
        //    }

        //    transform.position = hitFromNormal.point + normal * 1.5f;
        //}
    }
}
