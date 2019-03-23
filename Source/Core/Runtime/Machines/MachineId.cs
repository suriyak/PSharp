﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Threading;

using Microsoft.PSharp.Runtime;

namespace Microsoft.PSharp
{
    /// <summary>
    /// Unique machine id.
    /// </summary>
    [DataContract]
    public sealed class MachineId : IEquatable<MachineId>, IComparable<MachineId>
    {
        /// <summary>
        /// The runtime that executes the machine with this id.
        /// </summary>
        public IMachineRuntime Runtime { get; private set; }

        /// <summary>
        /// Unique id, when <see cref="NameValue"/> is empty.
        /// </summary>
        [DataMember]
        public readonly ulong Value;

        /// <summary>
        /// Unique id, when non-empty.
        /// </summary>
        [DataMember]
        public readonly string NameValue;

        /// <summary>
        /// Type of the machine with this id.
        /// </summary>
        [DataMember]
        public readonly string Type;

        /// <summary>
        /// Name of the machine used for logging.
        /// </summary>
        [DataMember]
        public readonly string Name;

        /// <summary>
        /// Generation of the runtime that created this machine id.
        /// </summary>
        [DataMember]
        public readonly ulong Generation;

        /// <summary>
        /// Endpoint.
        /// </summary>
        [DataMember]
        public readonly string Endpoint;

        /// <summary>
        /// True if <see cref="NameValue"/> is used as the unique id, else false.
        /// </summary>
        public bool IsNameUsedForHashing => NameValue.Length > 0;

        /// <summary>
        /// Creates a new machine id.
        /// </summary>
        /// <param name="type">Machine type</param>
        /// <param name="friendlyName">Friendly machine name</param>
        /// <param name="runtime">The runtime that executes the machine with this id.</param>
        /// <param name="useNameForHashing">Use friendly name as the id</param>
        internal MachineId(Type type, string friendlyName, BaseRuntime runtime, bool useNameForHashing = false)
        {
            this.Runtime = runtime;
            this.Endpoint = string.Empty;

            if (useNameForHashing)
            {
                this.Value = 0;
                this.NameValue = friendlyName;
                this.Runtime.Assert(!string.IsNullOrEmpty(friendlyName), "Input friendlyName cannot be null when used as Id");
            }
            else
            {
                // Atomically increments and safely wraps into an unsigned long.
                this.Value = (ulong)Interlocked.Increment(ref runtime.MachineIdCounter) - 1;
                this.NameValue = string.Empty;

                // Checks for overflow.
                this.Runtime.Assert(Value != ulong.MaxValue, "Detected MachineId overflow.");
            }

            this.Generation = runtime.Configuration.RuntimeGeneration;

            this.Type = type.FullName;
            if (IsNameUsedForHashing)
            {
                Name = this.NameValue;
            }
            else 
            {
                this.Name = string.Format("{0}({1})", string.IsNullOrEmpty(friendlyName) ? Type : friendlyName, Value);
            }
        }

        /// <summary>
        /// Bind the machine id.
        /// </summary>
        /// <param name="runtime">The runtime that executes the machine with this id.</param>
        internal void Bind(BaseRuntime runtime)
        {
            this.Runtime = runtime;
        }
        
        /// <summary>
        /// Determines whether the specified System.Object is equal
        /// to the current System.Object.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj is MachineId mid)
            {
                // Use same machanism for hashing.
                if (this.IsNameUsedForHashing != mid.IsNameUsedForHashing)
                {
                    return false;
                }

                return this.IsNameUsedForHashing ?
                    this.NameValue.Equals(mid.NameValue) && this.Generation == mid.Generation :
                    this.Value == mid.Value && this.Generation == mid.Generation;
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (this.IsNameUsedForHashing ? this.NameValue.GetHashCode() : this.Value.GetHashCode());
            hash = hash * 23 + this.Generation.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a string that represents the current machine id.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() => this.Name;

        /// <summary>
        /// Indicates whether the specified <see cref="MachineId"/> is equal
        /// to the current <see cref="MachineId"/>.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(MachineId other) => this.Equals((object)other);

        /// <summary>
        /// Compares the specified <see cref="MachineId"/> with the current
        /// <see cref="MachineId"/> for ordering or sorting purposes.
        /// </summary>
        public int CompareTo(MachineId other) => string.Compare(this.Name, other?.Name);

        bool IEquatable<MachineId>.Equals(MachineId other) => this.Equals(other);

        int IComparable<MachineId>.CompareTo(MachineId other) => string.Compare(this.Name, other?.Name);
    }
}
