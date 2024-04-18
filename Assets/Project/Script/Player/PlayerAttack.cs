using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] InputReader inputReader;

    Animator _animator;

    static readonly int IsAttack = Animator.StringToHash("IsPlayerAttack");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        inputReader.attackEvent += OnAttack;
    }

    private void OnDestroy()
    {
        inputReader.attackEvent -= OnAttack;
    }

    void OnAttack()
    {
        _animator.SetBool(IsAttack,true);
    }

    public void SetAttackFalse()
    {
        _animator.SetBool(IsAttack, false);
    }
}
