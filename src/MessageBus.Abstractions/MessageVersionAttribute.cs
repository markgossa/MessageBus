using System;

namespace MessageBus.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MessageVersionAttribute : Attribute
    {
        public int Version { get; set; }

        public MessageVersionAttribute(int version)
        {
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), 
                    "Version must be an integer greater than 0");
            }

            Version = version;
        }
    }
}
