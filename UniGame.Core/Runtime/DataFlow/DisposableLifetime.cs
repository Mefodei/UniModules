﻿namespace UniGreenModules.UniGame.Core.Runtime.Common
{
    using System;
    using UniCore.Runtime.DataFlow;
    using UniCore.Runtime.DataFlow.Interfaces;
    using UniCore.Runtime.ObjectPool.Runtime;
    using UniCore.Runtime.ObjectPool.Runtime.Extensions;
    using UniCore.Runtime.ObjectPool.Runtime.Interfaces;

    public class DisposableLifetime : IDisposableLifetime, IPoolable
    {
        private LifeTimeDefinition lifeTimeDefinition;
        private ILifeTime lifeTime;
        private bool isCompleted = true;
        
        public ILifeTime LifeTime => lifeTime;

        public bool IsComplete => isCompleted;

        /// <summary>
        /// restart disposable
        /// </summary>
        public void Initialize()
        {
            lifeTimeDefinition = ClassPool.Spawn<LifeTimeDefinition>();
            lifeTime = lifeTimeDefinition.LifeTime;
            lifeTimeDefinition.Release();
            isCompleted = false;
            
            lifeTime.AddCleanUpAction(Complete);
        }
        
        public void Dispose()
        {
            lifeTimeDefinition?.Terminate();
            this.Despawn();
        }

        public void Release(){}

        public void Complete()
        {
            if (isCompleted)
                return;
            
            isCompleted = true;
            lifeTimeDefinition.Despawn();
            lifeTimeDefinition = null;
            lifeTime = null;
        }
        
        #region lifetime api

        public bool IsTerminated => lifeTime == null || lifeTime.IsTerminated;
        
        public ILifeTime AddCleanUpAction(Action cleanAction) => lifeTime.AddCleanUpAction(cleanAction);

        public ILifeTime AddDispose(IDisposable item) => lifeTime.AddDispose(item);

        public ILifeTime AddRef(object o) => lifeTime.AddRef(o);

        #endregion
    }
}