using System;
using System.IO;

// https://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
public class CollectablePun {
    public string resourceName;
    public Int32 playerViewId;

    public static object Deserialize(byte[] data){
        CollectablePun result = new CollectablePun();

        using (MemoryStream m = new MemoryStream(data)) {
            using (BinaryReader reader = new BinaryReader(m)) {
                result.resourceName = reader.ReadString();
                result.playerViewId = reader.ReadInt32();
            }
        }
      return result;
    }

  public static byte[] Serialize(object customType){
        CollectablePun c = (CollectablePun)customType;
        // UnityEngine.Debug.Log(c.resourceName);

        using (MemoryStream m = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(m)) {
                writer.Write(c.resourceName);
                writer.Write(c.playerViewId);
            }

            return m.ToArray();
        }
    }
}