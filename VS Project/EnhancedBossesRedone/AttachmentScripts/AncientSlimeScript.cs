using EnhancedBossesRedone.Bosses;
using EnhancedBossesRedone.Data;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedBossesRedone.AttachmentScripts
{
    public class AncientSlimeScript : MonoBehaviour
    {
        /**
         * Need to get a connection to Bonemass. If none found, slime will just skip and stay still.
         */
        public void Awake()
        {
            nview = gameObject.GetComponentInChildren<ZNetView>();
            if (!nview.IsValid() || !nview.IsOwner())
            {
                return;
            }

            bonemass = TryGetBonemass();
            if (KillIfBonemassNotPresent())
            {
                return;
            }

            initialDistance = GetDistanceFromBonemass();
            heightToReach = Random.Range(6f, 10f);
            raiseSpeed = Random.Range(1f, 2f);
            moveSpeed = Random.Range(2f, 3f);
            direction = directions[Random.Range(0, 2)];
        }

        /**
         * Update motion and skip if Bonemass has been killed off.
         */
        public void Update()
        {
            if (!nview!.IsValid() || !nview!.IsOwner())
            {
                return;
            }

            if (TimeIsUp())
            {
                return;
            }

            MoveToNextPosition();

            if (ReadyForRefresh())
            {
                if (KillIfBonemassNotPresent())
                {
                    return;
                }
                CalcMovementVector();
            }
        }

        /**
         * Checks if time has elapsed.
         */
        public bool TimeIsUp()
        {
            totalTime += Time.deltaTime;
            timer += Time.deltaTime;
            return (totalTime > ConfigManager.BonemassAncientSlimeLifetime!.Value);
        }

        /**
         * Check if we need to refresh the calculations.
         */
        public bool ReadyForRefresh()
        {
            if (timer < updateRate)
            {
                return false;
            }

            timer -= updateRate;
            return true;
        }

        /**
         * Try to get Bonemass to apply damage calculations.
         */
        public Bonemass? TryGetBonemass()
        {
            if (bonemass != null)
            {
                return bonemass;
            }

            List<Character> characters = Character.GetAllCharacters();
            foreach (Character character in characters)
            {
                Bonemass boss;
                if ((boss = character.GetComponent<Bonemass>()) != null)
                {
                    return boss;
                }
            }
            return null;
        }

        /**
         * If we can't find bonemass, end slime.
         */
        public bool KillIfBonemassNotPresent()
        {
            if (bonemass == null)
            {
                Character character = gameObject.GetComponentInChildren<Character>();
                if (character != null)
                {
                    character.SetHealth(0.0f);
                }
                return true;
            }
            return false;
        }

        public void MoveToNextPosition()
        {
            if (moveDirCalculated)
            {
                gameObject.transform.position += (raiseSpeed * moveUp + moveSpeed * moveDirection) * Time.deltaTime;
            }
        }

        /**
         * Finds where Bonemass is.
         */
        public Vector3 GetBonemassPosition()
        {
            return bonemass!.transform.position;
        }

        /**
         * Finds where the current blob is.
         */
        public Vector3 GetSlimePosition()
        {
            return gameObject.transform.position;
        }

        /**
         * Retrieves distance from Bonemass
         */
        public float GetDistanceFromBonemass()
        {
            return (GetBonemassPosition() - GetSlimePosition()).magnitude;
        }

        /**
         * Gets the direction to Bonemass.
         */
        public Vector2 CalcDirectionToBonemass()
        {
            return (GetBonemassPosition() - GetSlimePosition()).normalized;
        }

        /**
         * Returns the XZ components of direction to Bonemass.
         */
        public Vector3 CalcDirectionToBonemassXZ()
        {
            Vector3 direction = GetBonemassPosition() - GetSlimePosition();
            return new Vector3(direction.x, 0, direction.z).normalized;
        }

        /**
         * Gets how high the object is off of the ground.
         */
        public float GetSlimeHeight()
        {
            Vector3 position = GetSlimePosition();
            return position.y - position.ConvertToWorldHeight().y;
        }

        /**
         * Calculates movement vector.
         */
        private void CalcMovementVector()
        {
            Vector3 circleVec = direction * Vector3.Cross(CalcDirectionToBonemass(), Vector3.up).normalized;
            Vector3 moveVec = GetDistanceFromBonemass() > initialDistance ? CalcDirectionToBonemassXZ() : Vector3.zero;
            moveDirection = (circleVec + moveVec).normalized;
            moveUp = GetSlimeHeight() > heightToReach ? Vector3.zero : Vector3.up;
            moveDirCalculated = true;
        }


        public Bonemass? bonemass;
        public ZNetView? nview;
        public float heightToReach;
        public float initialDistance;
        public float raiseSpeed;
        public float moveSpeed;
        public int direction;
        public Vector3 moveDirection;
        public Vector3 moveUp;
        public GameObject? particleContainer;

        private float totalTime;
        private float timer;
        private float updateRate = 1;
        private bool moveDirCalculated;


        public List<int> directions = new List<int>() { -1, 1 };
    }
}
