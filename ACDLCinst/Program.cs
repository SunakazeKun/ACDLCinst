using System.IO;
using System.IO.Compression;

namespace ACDLCinst
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;
            string path = args[0];
            byte[] data = File.ReadAllBytes(path);

            if (!IsValidSaveFile(data))
                return;

            PatchItems(data);
            PatchCatalogue(data, SLOT_1_OFFSET);
            PatchCatalogue(data, SLOT_2_OFFSET);
            PatchCatalogue(data, SLOT_3_OFFSET);
            PatchCatalogue(data, SLOT_4_OFFSET);
            File.WriteAllBytes(path, data);
        }

        #region Save data patching
        const int SAVE_FILE_SIZE = 0x40F340;
        const int DLC_DATA_OFFSET = 0x20F320;
        const int SLOT_1_OFFSET = 0x001140;
        const int SLOT_2_OFFSET = 0x009800;
        const int SLOT_3_OFFSET = 0x011EC0;
        const int SLOT_4_OFFSET = 0x01A580;
        const int SLOT_SIZE = 0x00759C;
        
        static bool IsValidSaveFile(byte[] data)
        {
            if (data == null || data.Length != SAVE_FILE_SIZE)
                return false;
            return ToUInt32(data, 0x48) >> 8 == 0x525555;
        }

        static void PatchItems(byte[] data)
        {
            byte[] dlcs = DecompressGZip(Resource.dlcItems_GZ);

            for (int i = 0; i < dlcs.Length; i++)
                data[DLC_DATA_OFFSET + i] = dlcs[i];
        }

        static void PatchCatalogue(byte[] data, int slot)
        {
            if (ToUInt32(data, slot) == 0xE4A45761)
                return;

            data[slot + 0x7363] |= 0xD0; // Wallpapers
            data[slot + 0x737C] |= 0xF0; // Floors pt 1
            data[slot + 0x737D] |= 0x01; // Floors pt 2
            data[slot + 0x73B9] |= 0x03; // Umbrellas
            data[slot + 0x73C5] |= 0xF0; // Hats pt 1
            data[slot + 0x73C6] |= 0xFD; // Hats pt 2
            data[slot + 0x73C7] |= 0x01; // Hats pt 3
            data[slot + 0x73CF] |= 0xF0; // Hats pt 4
            data[slot + 0x73D0] |= 0xFF; // Hats pt 5
            data[slot + 0x73E0] |= 0x10; // Glasses
            data[slot + 0x749C] = 0xFF; // Furniture pt 1
            data[slot + 0x749D] |= 0xBF; // Furniture pt 2
            data[slot + 0x749E] |= 0xBF; // Furniture pt 3
            data[slot + 0x749F] |= 0x0F; // Furniture pt 4
            data[slot + 0x74A0] |= 0x05; // Furniture pt 5
            data[slot + 0x74A1] |= 0xDE; // Furniture pt 6
            data[slot + 0x74A2] |= 0xF8; // Furniture pt 7
            data[slot + 0x74A3] |= 0xFB; // Furniture pt 8
            data[slot + 0x74A4] = 0xFF; // Furniture pt 9
            data[slot + 0x74A5] |= 0x1F; // Furniture pt 10
            
            PutUInt32(data, slot, Crc32.Calculate(data, slot + 4, SLOT_SIZE));
        }
        #endregion

        #region Helper functions
        static byte[] DecompressGZip(byte[] compressed)
        {
            int bufSize = 4096;
            byte[] buf = new byte[bufSize];

            using (GZipStream s = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress))
            using (MemoryStream m = new MemoryStream())
            {
                int count;

                do
                {
                    count = s.Read(buf, 0, bufSize);

                    if (count > 0)
                        m.Write(buf, 0, count);
                }
                while (count > 0);

                return m.ToArray();
            }
        }

        static uint ToUInt32(byte[] data, int offset)
        {
            uint ret = (uint)data[offset] << 24;
            ret |= (uint)data[offset + 1] << 16;
            ret |= (uint)data[offset + 2] << 8;
            ret |= data[offset + 3];
            return ret;
        }

        static void PutUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }
        #endregion
    }
}
