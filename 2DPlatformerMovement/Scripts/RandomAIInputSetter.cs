using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CyberBuggy.Movement
{
    public class RandomAIInputSetter : MonoBehaviour
    {
        [SerializeField] private PlatformerMovement _platformerMovement;
        [SerializeField] private Vector2 _directionChangeRange = new Vector2(0.1f, 4f);

        [SerializeField] private Vector2 _jumpIntervalRange = new Vector2(0.25f, 0.5f);
        [SerializeField] private Vector2 _jumpDurationRange = new Vector2(0.05f, 0.3f);
        [SerializeField] private bool _normalizedDirection = false;

        [SerializeField] [Range(0, 1)] private float _fullStopChance = 0.2f;

        private void Start()
        {
            StartCoroutine(Co_ChangeDirection(_directionChangeRange));
            StartCoroutine(Co_Jump(_jumpIntervalRange, _jumpDurationRange));
        }

        private IEnumerator Co_ChangeDirection(Vector2 delayRangeInSeconds)
        {
            while (true)
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
                
                yield return new WaitForSeconds(Random.Range(delayRangeInSeconds.x, delayRangeInSeconds.y));
            }

        }
        private IEnumerator Co_Jump(Vector2 delayRangeInSeconds, Vector2 durationRangeInSeconds)
        {
            while (true)
            {
                _platformerMovement.SetJumpInput(false);
                yield return new WaitForSeconds(Random.Range(delayRangeInSeconds.x, delayRangeInSeconds.y));
                _platformerMovement.SetJumpInput(true);
                yield return new WaitForSeconds(Random.Range(durationRangeInSeconds.x, durationRangeInSeconds.y));
            }
        }
    }
}
