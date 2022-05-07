using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CyberBuggy.Movement
{
    public class RandomAIInputSetter : MonoBehaviour
    {
        [SerializeField] private PlatformerMovement _platformerMovement;
        [SerializeField] private float _directionChangeRate = 0.5f;
        [SerializeField] private bool _normalizedDirection = false;

        [SerializeField] [Range(0, 1)] private float _fullStopChance = 0.2f;

        private void Start()
        {
            StartCoroutine(Co_ChangeDirection(_directionChangeRate));
        }

        private IEnumerator Co_ChangeDirection(float delayInSeconds)
        {
            while(true)
            {
                var moveChance = Random.Range(0, 100);
                
                if(moveChance >= _fullStopChance * 100)
                {
                    var direction = Random.Range(-1f, 1f);
                    if(_normalizedDirection)
                        direction = Mathf.Sign(direction);
                
                    _platformerMovement.SetWalkInput(new Vector2(direction, 0));
                }
                else
                    _platformerMovement.SetWalkInput(Vector2.zero);
                
                yield return new WaitForSeconds(delayInSeconds);
            }

        }
    }
}
