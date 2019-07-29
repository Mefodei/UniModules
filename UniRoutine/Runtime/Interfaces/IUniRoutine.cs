﻿namespace UniTools.UniRoutine.Runtime.Interfaces
{
    using System.Collections;
    using UniGreenModules.UniCore.Runtime.Interfaces;

    public interface IUniRoutine
    {
        IDisposableItem AddRoutine(IEnumerator enumerator, bool moveNextImmediately = true);
        void Update();
    }
}