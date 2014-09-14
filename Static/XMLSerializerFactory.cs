using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;

namespace RelayServer.Static
{
    public class XmlSerializerFactory
    {
        private XmlSerializerFactory() { }
        private static Hashtable serializers = new Hashtable();
        public static XmlSerializer GetSerializer(Type t)
        {
            XmlSerializer xs = null;
            lock (serializers.SyncRoot)
            {
                xs = serializers[t] as XmlSerializer;
                if (xs == null)
                {
                    xs = new XmlSerializer(t);
                    serializers.Add(t, xs);
                }
            }
            return xs;
        }
    }
}
