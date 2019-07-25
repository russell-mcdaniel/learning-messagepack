using System;
using MessagePack;

namespace Learning.MessagePack.ConsoleHost.Models
{
    [MessagePackObject]
    public class ThingEntity
    {
        [Key("EntityType")]
        public string EntityType { get; set; }

        [Key("EntityKey")]
        public Guid EntityKey { get; set; }

        [Key("EntityDateTime")]
        public DateTimeOffset EntityDateTime { get; set; }
    }
}
