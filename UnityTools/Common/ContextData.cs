﻿using System;
using System.Collections.Generic;
using Assets.Modules.UnityToolsModule.Tools.UnityTools.DataFlow;
using Assets.Tools.UnityTools.Interfaces;
using Assets.Tools.UnityTools.ObjectPool.Scripts;

namespace Assets.Tools.UnityTools.Common
{
    public class ContextData
    {
        /// <summary>
        /// registered conmponents
        /// </summary>
        private Dictionary<Type, object> _contextValues = new Dictionary<Type, object>();

        public virtual TData Get<TData>()
        {
            if (!_contextValues.TryGetValue(typeof(TData), out var value))
            {
                return default(TData);
            }

            var valueData = value as DataValue<TData>;
            return valueData.Value;

        }

        public bool Remove<TData>()
        {
            var type = typeof(TData);
            if (_contextValues.TryGetValue(type, out var value))
            {
                var typeValue = (DataValue<TData>)value;
                var removed = _contextValues.Remove(type);

                typeValue.Dispose();
                return removed;
            }

            return false;
        }

        public void Add<TData>(TData data)
        {
            object value = null;
            DataValue<TData> dataValue = null;
            var type = typeof(TData);

            if (_contextValues.TryGetValue(type, out value))
            {
                dataValue = value as DataValue<TData>;
                dataValue.SetValue(data);
                return;
            }

            dataValue = ClassPool.Spawn<DataValue<TData>>();
            dataValue.SetValue(data);
            _contextValues[type] = dataValue;

        }

        public void Release()
        {

            foreach (var contextValue in _contextValues)
            {
                var disposable = contextValue.Value as IDisposable;
                disposable?.Dispose();
            }
            _contextValues.Clear();

        }

    }
}