using UnityEngine.AI;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] float walkingSpeed = 3f;
    [SerializeField] float runningSpeed = 6f;

    [SerializeField] LayerMask Ground, Player;

    //Walking Around
    [SerializeField] Vector3 walkPoint;
    [SerializeField] float walkPointRange;

    //Atack
    [SerializeField] float timeBetweenAttackes;
    

    //State
    [SerializeField] float sightRange, attackRange;

    bool playerInSightRange, playerInAttackRange;
    bool _alreadyAttacked;
    NavMeshAgent _agent;
    Transform _player;
    bool _walkPointSet;
    Animator _animator;

    static readonly int IsWalking = Animator.StringToHash("IsWalking");
    static readonly int IsRunning = Animator.StringToHash("IsRunning");
    static readonly int IsAttack = Animator.StringToHash("IsAttack");

    private void Awake()
    {
        _player = GameObject.Find("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, Player);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, Player);

        if (!playerInSightRange && !playerInAttackRange) 
        {
            _animator.SetBool(IsRunning, false);
            WalkAround();
        }
        else if (playerInSightRange && !playerInAttackRange) 
        {
            _animator.SetBool(IsWalking, false);
            ChasePlayer();
        } 
        else if (playerInSightRange && playerInAttackRange) 
        {
            _animator.SetBool(IsWalking, false);
            _animator.SetBool(IsRunning, false);
            Attack();
        } 
        else
        {
            _animator.SetBool(IsWalking, false);
            _animator.SetBool(IsRunning, false);
            _agent.speed = walkingSpeed;
        }
    }

    void WalkAround()
    {
        if (!_walkPointSet) SearchWalkPoint();

        if(_walkPointSet) _agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reach
        if(distanceToWalkPoint.magnitude < 1f)
        {
            _walkPointSet = false;
        }

        _agent.speed = walkingSpeed;

        _animator.SetBool(IsWalking, true);
    }

    private void SearchWalkPoint()
    {
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        float randomZ = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX,transform.position.y,transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, Ground))
        {
            _walkPointSet =true;
        }
    }

    void ChasePlayer()
    {
        _agent.SetDestination(_player.position);

        _agent.speed = runningSpeed;

        _animator.SetBool(IsRunning, true);
    }

    void Attack()
    {
        _agent.SetDestination(transform.position);
        transform.LookAt(_player);

        if(!_alreadyAttacked)
        {
            //TODO: attack code goes here
            _animator.SetTrigger(IsAttack);
            _agent.speed = 0f;
            //
            _alreadyAttacked = true;
            Invoke(nameof(ResetAttackFlag), timeBetweenAttackes);
        }
        else
        {
            _animator.SetBool(IsAttack,false);
        }
    }

    public void ResetAttackFlag()
    {
        _alreadyAttacked = false;
    }
}
