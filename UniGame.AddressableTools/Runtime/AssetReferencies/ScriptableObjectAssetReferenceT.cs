﻿using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniGreenModules.UniGame.AddressableTools.Runtime.AssetReferencies
{
    public class ScriptableObjectAssetReferenceT<T> : AssetReferenceT<T>
        where T : ScriptableObject
    {
        public ScriptableObjectAssetReferenceT(string guid) : base(guid)
        {
        }
    }
}