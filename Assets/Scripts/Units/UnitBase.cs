using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace ZShopping.Units
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public abstract class UnitBase : NetworkBehaviour
    {

        private NetworkVariable<int> netHealth = new NetworkVariable<int>();

        private NetworkVariable<bool> netIsWalking = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [Header("Animation")]
        public Animator animator;
        public GameObject deathPrefab;            // prefab with Rigidbody/colliders for ragdoll
        public float deathForce = 5f;
        public float deathTorque = 15f;

        public int moveRange = 3;
        public int attackRange = 1;

        public int health = 10;

        protected UnityEngine.AI.NavMeshAgent agent;

        protected virtual void Awake()
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent == null)
                agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();

            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                netHealth.Value = health;
                netIsWalking.Value = false;
            }

            netHealth.OnValueChanged += OnHealthChanged;

        }

        private void OnHealthChanged(int oldValue, int newValue)
        {
            health = newValue;

            if (animator != null && newValue <= 0)
                animator.SetTrigger("Die");
            if (newValue <= 0 && IsServer)
            {

                Die();
            }
        }


        protected virtual void Update()
        {
            if (animator != null && agent != null)
            {

                if (IsServer && netIsWalking.Value && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    netIsWalking.Value = false;
                }

                animator.SetBool("Walk", netIsWalking.Value);

            }
        }


        public virtual void MoveTo(Vector3 target)
        {
            if (!IsServer)
            {
                RequestMoveServerRpc(target);
                return;
            }

            netIsWalking.Value = true;
            agent.SetDestination(target);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestMoveServerRpc(Vector3 target)
        {
            MoveTo(target);
        }


        public virtual void Attack(UnitBase target)
        {
            if (!IsServer)
            {
                RequestAttackServerRpc(target.NetworkObjectId);
                return;
            }

            if (animator != null)
                animator.SetTrigger("Attack");

            PlayAttackClientRpc();
            if (Vector3.Distance(transform.position, target.transform.position) <= attackRange)
            {
                target.PlayHitClientRpc();
                target.TakeDamageServerRpc(1);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestAttackServerRpc(ulong targetId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var obj))
            {
                var target = obj.GetComponent<UnitBase>();
                if (target != null)
                    Attack(target);
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(int amount)
        {
            if (!IsServer) return;
            netHealth.Value -= amount;
        }


        protected virtual void Die()
        {

            if (deathPrefab != null)
            {
                GameObject rag = Instantiate(deathPrefab, transform.position, transform.rotation);
                Rigidbody rb = rag.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 upImpulse = Vector3.up * deathForce;
                    Vector3 randomImpulse = Random.insideUnitSphere * deathForce;
                    rb.AddForce(upImpulse + randomImpulse, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * deathTorque, ForceMode.Impulse);
                }


                var netObj = rag.GetComponent<NetworkObject>();
                if (netObj != null && IsServer)
                    netObj.Spawn();
            }


            if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [ClientRpc]
        private void PlayAttackClientRpc()
        {
            if (animator != null)
                animator.SetTrigger("Attack");
        }

        [ClientRpc]
        public void PlayHitClientRpc()
        {
            if (animator != null)
                animator.SetTrigger("Hit");
        }
    }
} 
