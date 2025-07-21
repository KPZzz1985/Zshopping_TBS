using UnityEngine;
using System.Collections.Generic;
using ZShopping.Units;
using UnityEngine.UI;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class TurnManager : NetworkBehaviour
{
    [Header("Line of Sight")]
    public LayerMask obstacleLayerMask;
    [Header("UI Elements")]
    public Text turnText;
    public Text timerText;
    public Button endTurnButton;

    [Header("Highlights")]
    public HighlightManager highlightManager;

    [Tooltip("Grid tile size for move range calculation")]
    public float tileSize = 1f;

    [Tooltip("Turn duration in seconds")]
    public float turnTime = 60f;

    [Header("Units")]
    public List<UnitBase> player1Units;
    public List<UnitBase> player2Units;


    private List<UnitBase> currentTeamUnits;
    private int currentTeamIndex = 0; // 0 = player1, 1 = player2
    private int currentUnitIndex = 0;
    private bool usedMove;
    private bool usedAttack;
    private UnitBase currentUnit;
    private float timer;
    private bool infiniteMovementEnabled = false;
    private bool allowInput = false;

    private NetworkVariable<int> netCurrentTeam = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<ulong> netCurrentUnitId = new NetworkVariable<ulong>(0UL, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> netTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private int CountAlive(List<UnitBase> list)
    {
        int count = 0;
        foreach (var u in list)
            if (u != null && u.health > 0)
                count++;
        return count;
    }

    private void GameOver(string result)
    {
        if (highlightManager != null)
            highlightManager.ClearHighlights();
        if (turnText != null)
            turnText.text = result;
        if (timerText != null)
            timerText.gameObject.SetActive(false);
        if (endTurnButton != null)
            endTurnButton.gameObject.SetActive(false);
        enabled = false;
    }

    private void GrantInfiniteMovement()
    {
        infiniteMovementEnabled = true;
        Debug.Log("Infinite movement enabled");
    }

    void Awake()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();
            endTurnButton.onClick.AddListener(EndTurn);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {

            currentTeamUnits = new List<UnitBase>(player1Units);
            currentTeamIndex = 0;
            currentUnitIndex = 0;
            timer = turnTime;
            netTimer.Value = timer;
            netCurrentTeam.Value = currentTeamIndex;
            StartUnitTurn();
        }
        else
        {

            netCurrentTeam.OnValueChanged += OnClientTeamChanged;
            netCurrentUnitId.OnValueChanged += OnClientUnitChanged;

            OnClientTeamChanged(-1, netCurrentTeam.Value);
            OnClientUnitChanged(0, netCurrentUnitId.Value);

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(netTimer.Value).ToString();
        }
    }

    void Update()
    {

        if (!IsServer)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(netTimer.Value).ToString();
        }
        else
        {

            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                netTimer.Value = timer;
            }
            else
            {

                currentUnitIndex = currentTeamUnits.Count;
                StartUnitTurn();
            }
        }


        if (highlightManager != null && currentUnit != null && !usedAttack)
        {
            var enemies = player1Units.Contains(currentUnit) ? player2Units : player1Units;
            highlightManager.ShowAttackHighlights(currentUnit, enemies);

            int blockMask = obstacleLayerMask;
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.health <= 0) continue;
                float dist = Vector3.Distance(currentUnit.transform.position, enemy.transform.position);
                if (dist <= currentUnit.attackRange)
                {
                    Vector3 origin = currentUnit.transform.position + Vector3.up * 1f;
                    Vector3 target = enemy.transform.position + Vector3.up * 1f;
                    Vector3 dir = target - origin;
                    bool blocked = Physics.Raycast(origin, dir.normalized, dir.magnitude, blockMask);
                    Debug.DrawLine(origin, target, blocked ? Color.red : Color.green);
                }
            }
        }


        if (IsServer && currentUnit != null && usedMove && !usedAttack)
        {
            var agent = currentUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                bool foundTarget = false;
                var enemies = player1Units.Contains(currentUnit) ? player2Units : player1Units;
                foreach (var enemy in enemies)
                {
                    if (enemy == null || enemy.health <= 0) continue;
                    float dist = Vector3.Distance(currentUnit.transform.position, enemy.transform.position);
                    if (dist <= currentUnit.attackRange)
                    {

                        Vector3 origin = currentUnit.transform.position + Vector3.up * 1f;
                        Vector3 target = enemy.transform.position + Vector3.up * 1f;
                        Vector3 dir = target - origin;
                        if (!Physics.Raycast(origin, dir.normalized, dir.magnitude, obstacleLayerMask))
                        {
                            foundTarget = true;
                            break;
                        }
                    }
                }
                if (!foundTarget)
                {
                    Debug.Log("[Turn] No targets available, ending unit turn");
                    EndTurn();
                    return;
                }
            }
        }
        

        if (allowInput && Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                UnitBase hitUnit = hit.collider.GetComponent<UnitBase>();

                if (hitUnit != null && IsEnemyUnit(hitUnit))
                {

                    Vector3 origin = currentUnit.transform.position + Vector3.up * 0.5f;
                    Vector3 targetPos = hitUnit.transform.position + Vector3.up * 0.5f;
                    Vector3 dir = targetPos - origin;
                    if (Physics.Raycast(origin, dir.normalized, dir.magnitude, obstacleLayerMask))
                    {
                        Debug.Log("[Turn] Attack blocked by obstacle");
                        return;
                    }
                    float dist = Vector3.Distance(currentUnit.transform.position, hitUnit.transform.position);
                    if (dist <= currentUnit.attackRange)
                    {
                        Debug.Log($"[Turn] {currentUnit.name} requested attack on {hitUnit.name}");
                        AttackServerRpc(hitUnit.NetworkObjectId);
                    }
                }

                else
                {
                    Vector3 target = hit.point;
                    float dist = Vector3.Distance(currentUnit.transform.position, target);
                    float maxMove = infiniteMovementEnabled ? float.MaxValue : currentUnit.moveRange * tileSize;
                    if (dist <= maxMove)
                    {
                        Debug.Log($"[Turn] {currentUnit.name} requested move to {target}");
                        MoveServerRpc(target);
                    }
                }
            }
        }
    }


    private void StartUnitTurn()
    {

        int alive1 = CountAlive(player1Units);
        int alive2 = CountAlive(player2Units);
        if (alive1 == 0 || alive2 == 0)
        {
            string result = alive1 == 0 && alive2 == 0 ? "Draw" : (alive2 == 0 ? "Player 1 wins!" : "Player 2 wins!");
            GameOver(result);
            return;
        }

        currentTeamUnits.RemoveAll(u => u == null || u.health <= 0);

        if (currentUnitIndex >= currentTeamUnits.Count)
        {
            currentTeamIndex = 1 - currentTeamIndex;
            currentTeamUnits = (currentTeamIndex == 0)
                ? new List<UnitBase>(player1Units)
                : new List<UnitBase>(player2Units);

            currentTeamUnits.RemoveAll(u => u == null || u.health <= 0);
            currentUnitIndex = 0;
            timer = turnTime;
        }

        if (currentUnitIndex < currentTeamUnits.Count)
        {
            currentUnit = currentTeamUnits[currentUnitIndex];
            usedMove = false;
            usedAttack = false;

            allowInput = (currentTeamIndex == 0);

            netCurrentTeam.Value = currentTeamIndex;
            netCurrentUnitId.Value = currentUnit.NetworkObjectId;

            netTimer.Value = timer;
            Debug.Log($"Player {currentTeamIndex + 1}: {currentUnit.name}");
            if (turnText != null)
                turnText.text = $"Player {currentTeamIndex + 1} - {currentUnit.name}";
            if (timerText != null)
                timerText.text = timer.ToString("F0");
            if (highlightManager != null)
            {
                highlightManager.ClearHighlights();
                highlightManager.ShowUnitHighlight(currentUnit);
                if (!infiniteMovementEnabled)
                    highlightManager.ShowMoveHighlights(currentUnit);
            }
        }
    }

    void EndTurn()
    {
        if (highlightManager != null)
            highlightManager.ClearHighlights();

        currentUnitIndex++;

        StartUnitTurn();
    }
    
    bool IsEnemyUnit(UnitBase u)
    {
        return player1Units.Contains(currentUnit) ? player2Units.Contains(u)
                                                   : player1Units.Contains(u);
    }
    

    private void OnClientTeamChanged(int oldTeam, int newTeam)
    {

        if (turnText != null)
            turnText.text = $"Player {newTeam + 1}";

        bool myTeam = (IsServer && newTeam == 0) || (!IsServer && newTeam == 1);
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();
            endTurnButton.interactable = myTeam;
            if (myTeam)
            {
                if (IsServer)
                    endTurnButton.onClick.AddListener(() => EndTurn());
                else
                    endTurnButton.onClick.AddListener(() => EndTurnServerRpc());
            }
        }
        allowInput = myTeam;
    }
    private void OnClientUnitChanged(ulong oldId, ulong newId)
    {

        if (highlightManager != null && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newId, out var obj))
        {
            var unit = obj.GetComponent<UnitBase>();
            currentUnit = unit; // set local reference so HandleInput works
            usedMove = false;
            usedAttack = false;
            highlightManager.ClearHighlights();
            highlightManager.ShowUnitHighlight(unit);
            highlightManager.ShowMoveHighlights(unit);

            var clientEnemies = player1Units.Contains(unit) ? player2Units : player1Units;
            highlightManager.ShowAttackHighlights(unit, clientEnemies);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndTurnServerRpc()
    {
        EndTurn();
    }


    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc(ulong targetId)
    {

        if (!IsServer || currentUnit == null || usedAttack) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var obj))
        {
            var target = obj.GetComponent<UnitBase>();
            if (target != null)
            {
                currentUnit.Attack(target);
                usedAttack = true;

                DelayedEndTurn().Forget();
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(Vector3 position)
    {

        if (!IsServer || currentUnit == null || usedMove) return;
        currentUnit.MoveTo(position);
        usedMove = true;
    }


    private async UniTaskVoid DelayedEndTurn()
    {

        await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
        EndTurn();
    }
} 
