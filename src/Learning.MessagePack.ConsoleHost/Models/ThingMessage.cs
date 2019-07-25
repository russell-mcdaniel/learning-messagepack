using System;
using System.Collections.Generic;
using MessagePack;

namespace Learning.MessagePack.ConsoleHost.Models
{
    [MessagePackObject]
    public class ThingMessage
    {
        public ThingMessage()
        {
            Entities = new List<ThingEntity>();
        }

        [Key("MessageType")]
        public string MessageType { get; set; }

        [Key("MessageKey")]
        public Guid MessageKey { get; set; }

        [Key("CreatedDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        [Key("Entities")]
        public List<ThingEntity> Entities { get; set; }
    }
}
