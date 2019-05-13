﻿namespace UniGreenModules.UniContextData.Runtime.Entities
{
    using System;
    using UniModule.UnityTools.Interfaces;
    using UnityEngine;

    public class EntityComponent : MonoBehaviour
    {
        [NonSerialized] private EntityObject _context = new EntityObject();

        public IContext Context => _context;
    }
}