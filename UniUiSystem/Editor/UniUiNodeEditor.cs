﻿using System;
using System.Collections.Generic;
using Modules.UniTools.UniUiSystem.Interfaces;
using SubModules.Scripts.UniStateMachine.NodeEditor;
using UniNodeSystemEditor;
using UniStateMachine;
using UniStateMachine.Nodes;
using UnityEditor;
using UnityEngine;
using UniUiSystem;

namespace UniTools.UniUiSystem
{
    using UniRx;

    [CustomNodeEditor(typeof(UniUiNode))]
    public class UniUiNodeEditor : UniNodeEditor
    {
        
        private UiModule _moduleView;
        
        public static Type UniPortType = typeof(UniPortValue);

        private static List<IInteractionTrigger> _buttons = new List<IInteractionTrigger>();

        public override void OnBodyGUI()
        {           
            var uiNode = target as UniUiNode;
            _moduleView = uiNode.viewReference.editorAsset as UiModule;

            base.OnBodyGUI();

            var isChanged = DrawUiNode(uiNode);
            if (isChanged)
            {
                UpdateUiData(uiNode,uiNode.viewReference.editorAsset as UiModule);
            }
            
            EditorUtility.SetDirty(uiNode.graph.gameObject);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public override void UpdateData(UniGraphNode node)
        {
            
            var uiNode = node as UniUiNode;
            var view = uiNode.viewReference.editorAsset as UiModule;
            if (!Validate(view))
            {
                UpdateUiData(uiNode,view);
            }
            
            base.UpdateData(node);
            
        }

        public bool DrawUiNode(UniUiNode node)
        {
            var oldView = _moduleView;
            var uiView = node.viewReference.editorAsset;

            var isChanged = uiView != oldView;

            if (GUILayout.Button("UPDATE"))
            {
                isChanged = true;
            }

            return isChanged;
        }

        private void UpdateUiData(UniUiNode node,UiModule uiView)
        {
            if (!uiView)
            {
                return;
            }
            
            CollectUiData(uiView);

            uiView.Initialize(Unit.Default);

            PrefabUtility.SavePrefabAsset(uiView.gameObject);
            
        }

        private bool Validate(UiModule view)
        {
            if (!view)
                return true;
            
            var triggers = view.Triggers.Items;

            for (int i = 0; i < triggers.Count; i++)
            {
                var trigger = triggers[i];
                if (string.IsNullOrEmpty(trigger.ItemName))
                    return false;
            }
            return true;
        }

        private void CollectUiData(UiModule screen)
        {
            CollectSlots(screen);
            CollectTriggers(screen);
        }

        public void CollectSlots(UiModule module)
        {
            module.Slots.Release();
            CollectItems<IUiModuleSlot>(module.gameObject, module.AddSlot);
        }

        public void CollectTriggers(UiModule module)
        {
            module.Triggers.Release();
            CollectItems<InteractionTrigger>(module.gameObject, x =>
            {
                x.ApplyName(x.name);
                module.AddTrigger(x);
            });
        }

        private void CollectItems<TData>(GameObject target, Action<TData> action)
        {
            var items = new List<TData>();
            target.GetComponentsInChildren<TData>(true, items);

            foreach (var slot in items)
            {
                action(slot);
            }
        }
    }
}