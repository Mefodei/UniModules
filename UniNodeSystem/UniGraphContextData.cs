﻿using UniModule.UnityTools.Interfaces;
using UniModule.UnityTools.UniPool.Scripts;

namespace UniStateMachine.Nodes
{
    public class UniGraphContextData : IPoolable
    {
        public IContext GraphContext;

        public NodeContextData NodeContextData;

        public void ActivateNode(UniNode node, IContext context, IContext graphContext)
        {
            Release();
            GraphContext = graphContext;
            NodeContextData = ClassPool.Spawn<NodeContextData>();
            NodeContextData.Activate(node,context);
        }

        public void Release()
        {
            GraphContext = null;
            NodeContextData?.Release();
        }
    }
}