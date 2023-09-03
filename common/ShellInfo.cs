using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Common
{

    [Serializable]
    public class ShellInfo
    {
        public SymmetricEncryptType sysEncType;
        public AsymmetricEncryptType asysEncType;
        public CompressType compType;
        public int oriSize;
        public int keySize;
        public string asPriKey;
        public byte[] encKey;
        public byte[] iv;

        public byte[] Serialize()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                this.Serialize(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public static ShellInfo Deserialize(byte[] data)
        {
            ShellInfo deserializedShellInfo;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                deserializedShellInfo = memoryStream.DeSerialize<ShellInfo>();
                return deserializedShellInfo;
            }
        }

    }
    public static class Serializer
    {
        public static byte[] ToByteArray<T>(this T graph)
        {
            using (var ms = new MemoryStream())
            {
                graph.Serialize(ms);

                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(this byte[] serialized)
        {
            using (var ms = new MemoryStream(serialized))
            {
                return ms.DeSerialize<T>();
            }
        }

        public static void Serialize<T>(this T graph, Stream target)
        {
            // create the formatter:
            IFormatter formatter = new BinaryFormatter();

            // set the binder to the custom binder:
            formatter.Binder = TypeOnlyBinder.Default;

            // serialize the object into the stream:
            formatter.Serialize(target, graph);

        }

        public static T DeSerialize<T>(this Stream source)
        {
            // create the formatter:
            IFormatter formatter = new BinaryFormatter();

            // set the binder to the custom binder:
            formatter.Binder = TypeOnlyBinder.Default;

            // serialize the object into the stream:
            return (T)formatter.Deserialize(source);
        }




        /// <summary>
        /// removes assembly name from type resolution
        /// </summary>
        public class TypeOnlyBinder : SerializationBinder
        {
            private static SerializationBinder defaultBinder = new BinaryFormatter().Binder;



            public override Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName.Equals("NA"))
                    return Type.GetType(typeName);
                else
                    return defaultBinder.BindToType(assemblyName, typeName);

            }

            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                // but null out the assembly name
                assemblyName = "NA";
                typeName = serializedType.FullName;

            }

            private static object locker = new object();
            private static TypeOnlyBinder _default = null;

            public static TypeOnlyBinder Default
            {
                get
                {
                    lock (locker)
                    {
                        if (_default == null)
                            _default = new TypeOnlyBinder();
                    }
                    return _default;
                }
            }
        }


    }

}
