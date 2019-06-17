﻿namespace UniGreenModules.GBG.UI.Runtime
{
    using System.Collections;
    using Interfaces;
    using UniCore.Runtime.Extension;
    using UniCore.Runtime.Interfaces;
    using UniCore.Runtime.Views;
    using UnityEngine;
    using UniTools.UniRoutine.Runtime;

    public class UiView<TModel> : ComponentViewModel<TModel>, IUiView<TModel>
    {
        private IDisposableItem _updateDisposable;

        public RectTransform RectTransform  => transform as RectTransform;

        public void UpdateView()
        {
            //is update already scheduled?
            if (_updateDisposable != null && _updateDisposable?.IsDisposed == false)
                return;

            //release dispose items
            _updateDisposable?.Dispose();

            //check validation step
            var validationResult = Validate();
            if (!validationResult)
                return;

            //schedule single ui update at next EndOfFrame call
            _updateDisposable = OnScheduledUpdate().
                RunWithSubRoutines(RoutineType.EndOfFrame);
        }

        #region private methods

        protected virtual bool Validate() => isActiveAndEnabled;

        protected override void OnRelease()
        {
            _updateDisposable.Cancel();
        }

        private IEnumerator OnScheduledUpdate()
        {
            OnUpdateView();
            yield break;
        }

        protected virtual void OnUpdateView(){}
        
        protected void OnDestroy() => Release();

        protected virtual void OnEnable() => UpdateView();
        
        #endregion

    }
}