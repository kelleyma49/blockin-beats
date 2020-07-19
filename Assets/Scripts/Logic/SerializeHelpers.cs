using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Logic
{
    public class SerializeHelpers
    {
        public static Stream WriteObject<T>(T obj)
        {
            var memStream = new MemoryStream();
            var ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(memStream, obj);
            memStream.Position = 0;
            return memStream;
        }

        public static T ReadObject<T>(Stream stream)
        {
            var ser = new DataContractSerializer(typeof(T));

            // Deserialize the data and read it from 7the instance.
            T deserializedObj = (T)ser.ReadObject(stream);
            return deserializedObj;
        }

        public static T DeepClone<T>(T original)
        {
            using (var memStream = Logic.SerializeHelpers.WriteObject(original))
            {
                var clone = Logic.SerializeHelpers.ReadObject<T>(memStream);
                return clone;
            }
        }
    }
}
