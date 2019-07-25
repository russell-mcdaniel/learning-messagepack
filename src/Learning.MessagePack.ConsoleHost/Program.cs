using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Learning.MessagePack.ConsoleHost.Models;
using MessagePack;
using Newtonsoft.Json.Linq;

namespace Learning.MessagePack.ConsoleHost
{
    class Program
    {
        private static void Main()
        {
            var message = CreateMessage();
            var serialized = SerializeMessage(message);
            var deserialized = DeserializeMessage(serialized);
            var json = JToken.Parse(deserialized).ToString();

            Console.WriteLine(json);
            Console.WriteLine();
            Console.WriteLine($"Serialized/Compressed Length:  {serialized.Length}");
            Console.WriteLine($"Deserialized Length (JSON):    {deserialized.Length}");
        }

        private static ThingMessage CreateMessage()
        {
            var entities = new List<ThingEntity>();
            entities.Add(new ThingEntity() { EntityType = "Order.Header", EntityKey = Guid.NewGuid(), EntityDateTime = DateTimeOffset.Now });
            entities.Add(new ThingEntity() { EntityType = "Order.Detail", EntityKey = Guid.NewGuid(), EntityDateTime = DateTimeOffset.Now });
            entities.Add(new ThingEntity() { EntityType = "Order.Detail", EntityKey = Guid.NewGuid(), EntityDateTime = DateTimeOffset.Now });

            var message = new ThingMessage();
            message.MessageType = "Order.Create";
            message.MessageKey = Guid.NewGuid();
            message.CreatedDateTime = DateTimeOffset.Now;
            message.Entities = entities;

            return message;
        }

        private static byte[] SerializeMessage(object o)
        {
            var bytes = MessagePackSerializer.Serialize(o);                 // ~421 bytes (baseline)
            var bytesLz4 = LZ4MessagePackSerializer.Serialize(o);           // ~306 bytes (-27%)
            var bytesNetComp = SerializeMessageWithNetCompression(o);       // ~247 bytes (-41%)

            return bytesNetComp;
        }

        private static byte[] SerializeMessageWithNetCompression(object o)
        {
            using (var stream = new MemoryStream())
            using (var compressor = new DeflateStream(stream, CompressionLevel.Fastest))
            {
                MessagePackSerializer.Serialize(compressor, o);
                compressor.Close();

                return stream.ToArray();
            }
        }

        private static string DeserializeMessage(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            using (var outputStream = new MemoryStream())
            using (var compressor = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                // Decompress to memory stream first to avoid problems with MessagePack
                // deserialization on large payloads.
                compressor.CopyTo(outputStream);

                // ToJson() accepts a MemoryStream, but an exception is thrown in this project. Converting
                // to a byte array first succeeds. More research is required to understand why this occurs.
                var bytes = outputStream.ToArray();

                return MessagePackSerializer.ToJson(bytes);
            }
        }

        private static string DeserializeMessageHasError(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            using (var outputStream = new MemoryStream())
            using (var compressor = new DeflateStream(inputStream, CompressionMode.Decompress))
            {
                // Decompress to memory stream first to avoid problems with MessagePack
                // deserialization on large payloads.
                compressor.CopyTo(outputStream);

                // MessagePack.FormatterNotRegisteredException
                // HResult = 0x80131500
                // Message = System.IO.MemoryStream is not registered in this resolver.resolver:StandardResolver
                return MessagePackSerializer.ToJson(outputStream);
            }
        }
    }
}
