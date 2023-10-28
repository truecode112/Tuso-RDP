﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Immense.RemoteControl.Shared.Models.Dtos
{
    [DataContract]
    public class DtoWrapper
    {
        [DataMember]
        public byte[] DtoChunk { get; init; } = Array.Empty<byte>();

        [DataMember]
        public DtoType DtoType { get; init; }

        [DataMember]
        public bool IsFirstChunk { get; init; }

        [DataMember]
        public bool IsLastChunk { get; init; }

        [DataMember]
        public string RequestId { get; init; } = string.Empty;

        [DataMember]
        public string InstanceId { get; init; } = string.Empty;

        [DataMember]
        public int SequenceId { get; init; }
    }
}
