using EnhancedBossesRedone.Bosses;
using EnhancedBossesRedone.Data;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedBossesRedone.AttachmentScripts
{
    public class MoveForThrow : MonoBehaviour
    {
        /**
         * Need to get a connection to Elder. If none found, slime will just skip and stay still.
         */
        public void Awake()
        {
            nview = gameObject.GetComponentInChildren<ZNetView>();
            rb = gameObject.GetComponentInChildren<Rigidbody>();
            sync = gameObject.GetComponentInChildren<ZSyncTransform>();
            log = gameObject.GetComponentInChildren<TreeLog>();
            impact = gameObject.GetComponentInChildren<ImpactEffect>();

            rb!.isKinematic = false;
            rb!.useGravity = false;
            sync!.m_useGravity = false;
            sync!.m_isKinematicBody = false;

            elder = TryGetElder();
            if (elder == null)
            {
                DestroyCompletely();
            }

            character = elder!.GetComponent<Character>();

            initialGroundPos = gameObject.transform.position.ConvertToWorldHeight();
            initialScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            
            if (nview.IsValid() && nview.IsOwner())
            {
                randomHeight = UnityEngine.Random.Range(8.0f, 13.6f);
                randomRaiseSpeed = UnityEngine.Random.Range(9.0f, 14.0f);
                randomMoveSpeed = UnityEngine.Random.Range(6.0f, 14.0f);

                Object.Instantiate(ZNetScene.instance.GetPrefab("vfx_RockDestroyed_large"), initialGroundPos, Quaternion.identity);
                Object.Instantiate(ZNetScene.instance.GetPrefab("sfx_gdking_rock_destroyed"), initialGroundPos, Quaternion.identity);
                tornado = Object.Instantiate(ZNetScene.instance.GetPrefab("eb_leaftornado"), initialGroundPos, Quaternion.identity);
                tornadoParticles = tornado.GetComponentsInChildren<ParticleSystem>();
                particlesPlaying = true;
            }
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

            if (thrown)
            {
                Destroy(this);
            }

            if (elder == null || character == null || totalTime > destroyTime)
            {
                DestroyCompletely();
            }

            if (totalTime < attractTime)
            {
                LaunchLog();
            }

            if (totalTime > attractTime)
            {
                Vector3 characterPos = character!.GetCenterPoint() + new Vector3(0, 5, 0);
                Vector3 direction = (characterPos - gameObject.transform.position).normalized;
                float distance = Vector3.Distance(characterPos, gameObject.transform.position);
                DetectLogWhenClose(distance);
                MoveLog(direction);
            }

            ThrowIfCloseEnough();

            totalTime += Time.deltaTime;
        }

        private void LaunchLog()
        {
            if (gameObject.transform.position.y - initialGroundPos.y < randomHeight)
            {
                gameObject.transform.position += randomRaiseSpeed * Vector3.up * Time.deltaTime;
            }
        }

        private void DetectLogWhenClose(float distance)
        {
            if (distance < 5.0f * initialScale.magnitude)
            {
                closeEnough = true;
            }
        }

        private void MoveLog(Vector3 direction)
        {
            gameObject.transform.position += randomMoveSpeed * direction * Time.deltaTime;
           
            if (tornado != null)
            {
                tornado!.transform.position = gameObject.transform.position.ConvertToWorldHeight();
            }
        }

        private void StopTornado()
        {
            if (tornado != null && tornadoParticles != null && particlesPlaying)
            {
                foreach (ParticleSystem particle in tornadoParticles)
                {
                    particle.Stop();
                }
                particlesPlaying = false;
            }
            tornadoStopped = true;
        }

        private void ThrowIfCloseEnough()
        {
            if (!closeEnough)
            {
                return;
            }

            Character target = character!.GetBaseAI().GetTargetCreature();
            if (target == null)
            {
                DestroyCompletely();
            }

            rb!.useGravity = true;
            sync!.m_useGravity = true;
            Vector3 targetPosition = (target!.transform.position + Helpers.InsideAnnulusXZ(0.0f, 5.0f)).ConvertToWorldHeight();
            Vector3 direction = (targetPosition - gameObject.transform.position) + 3 * Vector3.up;

            Instantiate(ZNetScene.instance.GetPrefab("fx_sledge_demolisher_hit"), gameObject.transform.position, Quaternion.identity);
            rb!.linearVelocity = 1.6f * direction;
            impact!.m_maxVelocity = 1.6f * direction.magnitude + 10;

            StopTornado();
            thrown = true;
        }

        private void DestroyCompletely(bool heal = false)
        {
            StopTornado();
            log!.m_dropWhenDestroyed = new DropTable();
            log!.m_subLogPrefab = null;
            log!.gameObject.SetActive(false);
            log!.m_nview.Destroy();
        }

        public void OnDestroy()
        {
            if (!tornadoStopped)
            {
                StopTornado();
            }
        }


        /**
         * Try to get Elder.
         */
        public Elder? TryGetElder()
        {
            if (elder != null)
            {
                return elder;
            }

            List<Character> characters = Character.GetAllCharacters();
            foreach (Character character in characters)
            {
                Elder boss;
                if ((boss = character.GetComponent<Elder>()) != null)
                {
                    return boss;
                }
            }
            return null;
        }

        


        public Elder? elder;
        public ZNetView? nview;
        public Rigidbody? rb;
        public ZSyncTransform? sync;
        public TreeLog? log;
        public Character? character;
        public ImpactEffect? impact;

        public Vector3 initialGroundPos;
        public Vector3 initialScale;

        public GameObject? tornado;
        public ParticleSystem[]? tornadoParticles;


        private float totalTime;
        private float attractTime = 1.2f;
        private float destroyTime = 10.0f;
        private bool closeEnough;
        private bool thrown;
        private bool particlesPlaying;
        private bool tornadoStopped;
        private float randomHeight;
        private float randomRaiseSpeed;
        private float randomMoveSpeed;
    }
}
