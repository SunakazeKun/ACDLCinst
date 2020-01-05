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
        const int SLOT_CATALOG_OFFSET = 0x0072DA;

        static bool IsValidSaveFile(byte[] data)
        {
            if (data == null || data.Length != SAVE_FILE_SIZE)
                return false;
            return ToUInt32(data, 0x48) >> 8 == 0x525555;
        }

        static bool IsDlcItem(int id)
        {
            if (id == 1100) // sporty wall
                return true;
            if (1102 <= id && id <= 1103) // golden wallpaper to creepy wallpaper
                return true;
            if (1300 <= id && id <= 1304) // hopscotch floor to creepy carpet
                return true;
            if (1784 <= id && id <= 1785) // ghost umbrella to maple umbrella
                return true;
            if (1884 <= id && id <= 1896) // shamrock hat to white police cap
                return true;
            if (1964 <= id && id <= 1975) // red Pikmin to red horned hat
                return true;
            if (id == 2100) // ladder shades
                return true;
            if (3600 <= id && id <= 3613) // top to shopping cart
                return true;
            if (3615 <= id && id <= 3621) // Chihuahua model to festive wreath
                return true;
            if (3623 <= id && id <= 3627) // Wii locker to Nintendo DS lite
                return true;
            if (id == 3632) // fedora chair
                return true;
            if (id == 3634) // Kapp'n model
                return true;
            if (3641 <= id && id <= 3644) // tteok plate to shaved ice lamp
                return true;
            if (3646 <= id && id <= 3647) // flower bouquet to Cupid bench
                return true;
            if (3651 <= id && id <= 3657) // Snowman vanity to sweets player
                return true;
            if (3659 <= id && id <= 3676) // golden bed to creepy statue
                return true;
            return false;
        }

        static bool IsInvalidItem(int id)
        {
            if (45 <= id && id <= 49)
                return true;
            if (62 <= id && id <= 64)
                return true;
            if (67 <= id && id <= 69)
                return true;
            if (id == 79)
                return true;
            if (95 <= id && id <= 100)
                return true;
            if (id == 209)
                return true;
            if (218 <= id && id <= 219)
                return true;
            if (247 <= id && id <= 249)
                return true;
            if (262 <= id && id <= 299)
                return true;
            if (378 <= id && id <= 399)
                return true;
            if (468 <= id && id <= 499)
                return true;
            if (518 <= id && id <= 599)
                return true;
            if (664 <= id && id <= 699)
                return true;
            if (767 <= id && id <= 799)
                return true;
            if (969 <= id && id <= 999)
                return true;
            if (1091 <= id && id <= 1199)
                return true;
            if (1289 <= id && id <= 1349)
                return true;
            if (1696 <= id && id <= 1699)
                return true;
            if (1734 <= id && id <= 1799)
                return true;
            if (1951 <= id && id <= 1999)
                return true;
            if (2047 <= id && id <= 2199)
                return true;
            if (2234 <= id && id <= 2299)
                return true;
            if (2300 <= id && id <= 2427)
                return true;
            if (3218 <= id && id <= 3590)
                return true;
            if (3600 <= id && id <= 3849)
                return true;
            if (3908 <= id && id <= 3919)
                return true;
            if (3989 <= id && id <= 3999)
                return true;
            if (4054 <= id && id <= 4094)
                return true;
            return false;
        }

        static void PatchItems(byte[] data)
        {
            byte[] dlcs = DecompressGZip(Resource.dlcItems_GZ);

            for (int i = 0; i < dlcs.Length; i++)
                data[DLC_DATA_OFFSET + i] = dlcs[i];
        }

        static void PatchCatalogue(byte[] data, int slotOffset)
        {
            if (ToUInt32(data, slotOffset) == 0xE4A45761)
                return;

            for (int i = 0; i < 4096; i++)
            {
                int bitfieldByte = slotOffset + SLOT_CATALOG_OFFSET + i / 8;
                int bitfieldBit = i % 8;

                if (IsDlcItem(i))
                    data[bitfieldByte] |= (byte)(1 << (bitfieldBit));
                else if (IsInvalidItem(i))
                    data[bitfieldByte] &= (byte)~(1 << (bitfieldBit));
            }
            
            PutUInt32(data, slotOffset, Crc32.Calculate(data, slotOffset + 4, SLOT_SIZE));
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
