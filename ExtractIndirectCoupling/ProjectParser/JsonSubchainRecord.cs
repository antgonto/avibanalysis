using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [Serializable()]
    class JsonSubchainRecord
    {
        private static int byteArrayLen = ByteArraySize(new JsonSubchainRecord(new JsonSubchain()));
        private static BufferedStream stream = null;
        private static int offset = 0;
        private static string binaryFile = @"/Users/jnavas/binaryFile";
        public int from;
        public int to;
        public int chlen;
        public int[] chain;
        public bool initial; // 0: true, 1: false
        public bool final;   // 0: true, 1: false

        public static int ByteArrayLen { get => byteArrayLen; set => byteArrayLen = value; }

        public JsonSubchainRecord(JsonSubchain s)
        {
            chain = new int[50];
            from = s.From;
            to = s.To;
            chlen = 0;
            foreach (JsonMethod m in s.Chain)
            {
                chain[chlen++] = m.Id;
            }
            initial = s.Initial;
            final = s.Final;
        }

        public JsonSubchain GetJsonSubchain()
        {
            JsonSubchain s = new JsonSubchain();
            s.From = from;
            s.To = to;
            s.Chain = new JsonMethod[chlen];
            for (int i = 0; i < chlen; i++)
            {
                s.Chain[i] = JsonMethod.MethodsById[chain[i]];
            }
            s.Initial = initial;
            s.Final = final;
            return s;
        }

        private static int ByteArraySize(JsonSubchainRecord s)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, s);

            return ms.ToArray().Length;
        }

        // Convert an JsonSubchainRecord to a byte array
        public static byte[] ObjectToByteArray(JsonSubchainRecord s)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, s);

            return ms.ToArray();
        }

        // Convert a byte array to an JsonSubchainRecord
        public static JsonSubchainRecord ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            JsonSubchainRecord record = (JsonSubchainRecord)binForm.Deserialize(memStream);
            return record;
        }
    }
}
