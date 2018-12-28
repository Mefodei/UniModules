﻿using System.Collections.Generic;
using Assets.Modules.UnityToolsModule.Tools.UnityTools.DataFlow;
using Assets.Tools.UnityTools.ObjectPool.Scripts;

namespace Assets.Tools.UnityTools.Interfaces
{

    public interface IContextData<TContext> : IPoolable
    {
        IReadOnlyCollection<TContext> Contexts { get; }

        TData Get<TData>(TContext context);

        void RemoveContext(TContext context);
        void Remove<TData>(TContext context);
        void AddValue<TData>(TContext context, TData value);
        bool HasContext(TContext context);

    }
}