﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Tools.UnityTools.ObjectPool.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace XNode {
    [Serializable]
    public class NodePort
    {
        [ReadOnlyValue]
        [SerializeField]
        private ulong _id;

        public ulong Id
        {
            get
            {
                if (_id == 0)
                {
                    UpdateId();
                }

                return _id;
            }
        }

        public void UpdateId()
        {
            _id = node.graph.GetId();
        }

        public enum IO { Input, Output }
        
        public int ConnectionCount { get { return connections.Count; } }
        /// <summary> Return the first non-null connection </summary>
        public NodePort Connection {
            get {
                for (var i = 0; i < connections.Count; i++) {
                    if (connections[i] != null) return connections[i].Port;
                }
                return null;
            }
        }

        public IO direction { get { return _direction; } }
        public Node.ConnectionType connectionType { get { return _connectionType; } }

        /// <summary> Is this port connected to anytihng? </summary>
        public bool IsConnected { get { return connections.Count != 0; } }
        public bool IsInput { get { return direction == IO.Input; } }
        public bool IsOutput { get { return direction == IO.Output; } }

        public string fieldName { get { return _fieldName; } }

        public Node node
        {
            get
            {
                return _node;
            }
            set
            {
                if (_node == value)
                    return;
                _node = value;
                _id = _node.graph.GetId();
            }
        }
        
        public bool IsDynamic { get { return _dynamic; } }
        public bool IsStatic { get { return !_dynamic; } }
        public Type ValueType {
            get {
                if (valueType == null && !string.IsNullOrEmpty(_typeQualifiedName)) valueType = Type.GetType(_typeQualifiedName, false);
                return valueType;
            }
            set {
                valueType = value;
                if (value != null) _typeQualifiedName = value.AssemblyQualifiedName;
            }
        }
        private Type valueType;

        [SerializeField] private string _fieldName;
        [SerializeField] private Node _node;
        [SerializeField] private string _typeQualifiedName;
        [SerializeField] private List<PortConnection> connections = new List<PortConnection>();
        [SerializeField] private IO _direction;
        [SerializeField] private Node.ConnectionType _connectionType;
        [SerializeField] private bool _dynamic;

        /// <summary> Construct a static targetless nodeport. Used as a template. </summary>
        public NodePort(FieldInfo fieldInfo) {
            _fieldName = fieldInfo.Name;
            ValueType = fieldInfo.FieldType;
            _dynamic = false;
            var attribs = fieldInfo.GetCustomAttributes(false);
            for (var i = 0; i < attribs.Length; i++) {
                if (attribs[i] is Node.InputAttribute) {
                    _direction = IO.Input;
                    _connectionType = (attribs[i] as Node.InputAttribute).connectionType;
                } else if (attribs[i] is Node.OutputAttribute) {
                    _direction = IO.Output;
                    _connectionType = (attribs[i] as Node.OutputAttribute).connectionType;
                }
            }
        }

        /// <summary> Copy a nodePort but assign it to another node. </summary>
        public NodePort(NodePort nodePort, Node node) {
            _fieldName = nodePort._fieldName;
            ValueType = nodePort.valueType;
            _direction = nodePort.direction;
            _dynamic = nodePort._dynamic;
            _connectionType = nodePort._connectionType;
            _node = node;
        }

        /// <summary> Construct a dynamic port. Dynamic ports are not forgotten on reimport, and is ideal for runtime-created ports. </summary>
        public NodePort(string fieldName, Type type, IO direction, Node.ConnectionType connectionType, Node node) {
            _fieldName = fieldName;
            this.ValueType = type;
            _direction = direction;
            _node = node;
            _dynamic = true;
            _connectionType = connectionType;
        }

        /// <summary> Checks all connections for invalid references, and removes them. </summary>
        public void VerifyConnections() {
            for (var i = connections.Count - 1; i >= 0; i--) {
                if (connections[i].node != null &&
                    !string.IsNullOrEmpty(connections[i].fieldName) &&
                    connections[i].node.GetPort(connections[i].fieldName) != null)
                    continue;
                connections.RemoveAt(i);
            }
        }

        /// <summary> Return the output value of this node through its parent nodes GetValue override method. </summary>
        /// <returns> <see cref="Node.GetValue(NodePort)"/> </returns>
        public object GetOutputValue() {
            if (direction == IO.Input) return null;
            return node.GetValue(this);
        }

        /// <summary> Return the output value of the first connected port. Returns null if none found or invalid.</summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public object GetInputValue() {
            var connectedPort = Connection;
            if (connectedPort == null) return null;
            return connectedPort.GetOutputValue();
        }

        /// <summary> Return the output values of all connected ports. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public object[] GetInputValues() {
            var objs = new object[ConnectionCount];
            for (var i = 0; i < ConnectionCount; i++) {
                var connectedPort = connections[i].Port;
                if (connectedPort == null) { // if we happen to find a null port, remove it and look again
                    connections.RemoveAt(i);
                    i--;
                    continue;
                }
                objs[i] = connectedPort.GetOutputValue();
            }
            return objs;
        }

        /// <summary> Return the output value of the first connected port. Returns null if none found or invalid. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public T GetInputValue<T>() {
            var obj = GetInputValue();
            return obj is T ? (T) obj : default(T);
        }

        /// <summary> Return the output values of all connected ports. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public T[] GetInputValues<T>() {
            var objs = GetInputValues();
            var ts = new T[objs.Length];
            for (var i = 0; i < objs.Length; i++) {
                if (objs[i] is T) ts[i] = (T) objs[i];
            }
            return ts;
        }

        /// <summary> Return true if port is connected and has a valid input. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public bool TryGetInputValue<T>(out T value) {
            var obj = GetInputValue();
            if (obj is T) {
                value = (T) obj;
                return true;
            } else {
                value = default(T);
                return false;
            }
        }

        /// <summary> Return the sum of all inputs. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public float GetInputSum(float fallback) {
            var objs = GetInputValues();
            if (objs.Length == 0) return fallback;
            float result = 0;
            for (var i = 0; i < objs.Length; i++) {
                if (objs[i] is float) result += (float) objs[i];
            }
            return result;
        }

        /// <summary> Return the sum of all inputs. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public int GetInputSum(int fallback) {
            var objs = GetInputValues();
            if (objs.Length == 0) return fallback;
            var result = 0;
            for (var i = 0; i < objs.Length; i++) {
                if (objs[i] is int) result += (int) objs[i];
            }
            return result;
        }

        /// <summary> Connect this <see cref="NodePort"/> to another </summary>
        /// <param name="port">The <see cref="NodePort"/> to connect to</param>
        public void Connect(NodePort port) {
            if (connections == null) connections = new List<PortConnection>();
            if (port == null) { Debug.LogWarning("Cannot connect to null port"); return; }
            if (port == this) { Debug.LogWarning("Cannot connect port to self."); return; }
            if (IsConnectedTo(port)) { Debug.LogWarning("Port already connected. "); return; }
            if (direction == port.direction) { Debug.LogWarning("Cannot connect two " + (direction == IO.Input ? "input" : "output") + " connections"); return; }
            if (port.connectionType == Node.ConnectionType.Override && port.ConnectionCount != 0) { port.ClearConnections(); }
            if (connectionType == Node.ConnectionType.Override && ConnectionCount != 0) { ClearConnections(); }
            connections.Add(new PortConnection(port));
            if (port.connections == null) port.connections = new List<PortConnection>();
            if (!port.IsConnectedTo(this)) port.connections.Add(new PortConnection(this));
            node.OnCreateConnection(this, port);
            port.node.OnCreateConnection(this, port);
        }

        public List<NodePort> GetConnections() {
            var result = ClassPool.Spawn<List<NodePort>>();
            for (var i = 0; i < connections.Count; i++) {
                var port = GetConnection(i);
                if (port != null) result.Add(port);
            }
            return result;
        }

        public NodePort GetConnection(int i) {
            //If the connection is broken for some reason, remove it.
            if (connections[i].node == null || string.IsNullOrEmpty(connections[i].fieldName)) {
                connections.RemoveAt(i);
                return null;
            }
            var port = connections[i].node.GetPort(connections[i].fieldName);
            if (port == null) {
                connections.RemoveAt(i);
                return null;
            }
            return port;
        }

        /// <summary> Get index of the connection connecting this and specified ports </summary>
        public int GetConnectionIndex(NodePort port) {
            for (var i = 0; i < ConnectionCount; i++) {
                if (connections[i].Port == port) return i;
            }
            return -1;
        }

        public bool IsConnectedTo(NodePort port) {
            for (var i = 0; i < connections.Count; i++) {
                if (connections[i].Port == port) return true;
            }
            return false;
        }

        /// <summary> Disconnect this port from another port </summary>
        public void Disconnect(NodePort port) {
            // Remove this ports connection to the other
            for (var i = connections.Count - 1; i >= 0; i--) {
                if (connections[i].Port == port) {
                    connections.RemoveAt(i);
                }
            }
            if (port != null) {
                // Remove the other ports connection to this port
                for (var i = 0; i < port.connections.Count; i++) {
                    if (port.connections[i].Port == this) {
                        port.connections.RemoveAt(i);
                    }
                }
            }
            // Trigger OnRemoveConnection
            node.OnRemoveConnection(this);
            if (port != null) port.node.OnRemoveConnection(port);
        }

        /// <summary> Disconnect this port from another port </summary>
        public void Disconnect(int i) {
            // Remove the other ports connection to this port
            var otherPort = connections[i].Port;
            if (otherPort != null) {
                for (var k = 0; k < otherPort.connections.Count; k++) {
                    if (otherPort.connections[k].Port == this) {
                        otherPort.connections.RemoveAt(i);
                    }
                }
            }
            // Remove this ports connection to the other
            connections.RemoveAt(i);

            // Trigger OnRemoveConnection
            node.OnRemoveConnection(this);
            if (otherPort != null) otherPort.node.OnRemoveConnection(otherPort);
        }

        public void ClearConnections() {
            while (connections.Count > 0) {
                Disconnect(connections[0].Port);
            }
        }

        /// <summary> Get reroute points for a given connection. This is used for organization </summary>
        public List<Vector2> GetReroutePoints(int index) {
            return connections[index].reroutePoints;
        }

        /// <summary> Swap connections with another node </summary>
        public void SwapConnections(NodePort targetPort) {
            var aConnectionCount = connections.Count;
            var bConnectionCount = targetPort.connections.Count;

            var portConnections = new List<NodePort>();
            var targetPortConnections = new List<NodePort>();

            // Cache port connections
            for (var i = 0; i < aConnectionCount; i++)
                portConnections.Add(connections[i].Port);

            // Cache target port connections
            for (var i = 0; i < bConnectionCount; i++)
                targetPortConnections.Add(targetPort.connections[i].Port);

            ClearConnections();
            targetPort.ClearConnections();

            // Add port connections to targetPort
            for (var i = 0; i < portConnections.Count; i++)
                targetPort.Connect(portConnections[i]);

            // Add target port connections to this one
            for (var i = 0; i < targetPortConnections.Count; i++)
                Connect(targetPortConnections[i]);

        }

        /// <summary> Copy all connections pointing to a node and add them to this one </summary>
        public void AddConnections(NodePort targetPort) {
            var connectionCount = targetPort.ConnectionCount;
            for (var i = 0; i < connectionCount; i++) {
                var connection = targetPort.connections[i];
                var otherPort = connection.Port;
                Connect(otherPort);
            }
        }

        /// <summary> Move all connections pointing to this node, to another node </summary>
        public void MoveConnections(NodePort targetPort) {
            var connectionCount = connections.Count;

            // Add connections to target port
            for (var i = 0; i < connectionCount; i++) {
                var connection = targetPort.connections[i];
                var otherPort = connection.Port;
                Connect(otherPort);
            }
            ClearConnections();
        }

        /// <summary> Swap connected nodes from the old list with nodes from the new list </summary>
        public void Redirect(List<Node> oldNodes, List<Node> newNodes) {
            foreach (var connection in connections) {
                var index = oldNodes.IndexOf(connection.node);
                if (index >= 0) connection.node = newNodes[index];
            }
        }

        [Serializable]
        private class PortConnection {
            [SerializeField] public string fieldName;
            [SerializeField] public Node node;
            public NodePort Port { get { return port != null ? port : port = GetPort(); } }

            [NonSerialized] private NodePort port;
            /// <summary> Extra connection path points for organization </summary>
            [SerializeField] public List<Vector2> reroutePoints = new List<Vector2>();

            public PortConnection(NodePort port) {
                this.port = port;
                node = port.node;
                fieldName = port.fieldName;
            }

            /// <summary> Returns the port that this <see cref="PortConnection"/> points to </summary>
            private NodePort GetPort() {
                if (node == null || string.IsNullOrEmpty(fieldName)) return null;
                return node.GetPort(fieldName);
            }
        }
    }
}