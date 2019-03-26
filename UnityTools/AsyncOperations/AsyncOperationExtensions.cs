﻿using System.Collections;
using System.Collections.Generic;
using UniModule.UnityTools.UniPool.Scripts;
using UniRx;

namespace UniModule.UnityTools.AsyncOperations
{
    public static class AsyncOperationExtensions
    {

        public static IEnumerator WaitAll(this List<IEnumerator> operations) {

            var counter = ClassPool.Spawn<List<int>>();
            counter.Add(0);

            for (var i = 0; i < operations.Count; i++) {
                var index = i;
                Observable.FromCoroutine(x => operations[index]).DoOnCompleted(() => counter[0]++).Subscribe();
            }

            while (counter[0] < operations.Count) {
                yield return null;
            }

            counter.Despawn();
        }

    }
}
