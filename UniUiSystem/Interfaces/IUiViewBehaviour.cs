using UniModule.UnityTools.Interfaces;
using UniModule.UnityTools.UniPool.Scripts;
using UnityEngine;

namespace UniUiSystem {
    
    public interface IUiViewBehaviour : IPoolable
    {
        bool IsActive { get; }

        IContext Context { get; }
        
        RectTransform RectTransform { get; }
        
        void UpdateView();
        
        void SetState(bool active);
        
    }
}