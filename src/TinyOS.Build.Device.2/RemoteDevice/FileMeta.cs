using System;
using System.Linq;

namespace TinyOS.Build.Device
{
    public class FileMeta : IEquatable<FileMeta>
    {
        public string LocalPath { get; set; } = string.Empty;
        public string RemotePath { get; set; } = string.Empty;
        public byte[] Hash { get; set; } = new byte[0];

        public override bool Equals(object obj)
            => obj is FileMeta other && Equals(other);

        public bool Equals(FileMeta other)
            => Hash.SequenceEqual(other.Hash);

        public override int GetHashCode()
            => Hash.GetHashCode();
    }
}