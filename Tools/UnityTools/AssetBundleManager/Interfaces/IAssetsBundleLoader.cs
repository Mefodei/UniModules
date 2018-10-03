﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tools.UnityTools.AssetBundleManager.Interfaces {

    public interface IAssetsBundleLoader {

        AssetBundleManifest AssetBundleManifest { get; }

        IAssetBundleRequest GetAssetBundleRequest(string assetBundleName,AssetBundleSourceType sourceType,bool loadDependencies = true);

        List<string> GetBundleDependencies(string assetBundleName);

    }

}