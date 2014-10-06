using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace otya.Minecraft.Anvil
{
    using otya.Minecraft.NBT;
    public class Anvil : IDisposable
    {
        Stream Input;
        int[] offsets = new int[4096 / 4];
        int[] timestamps = new int[4096 / 4];
        NBT[] nbts = new NBT[4096 / 4];
        HashSet<int> free = new HashSet<int>();
        int[] freeList;
        int freelistendex = 0;
        int X;
        int Z;
        public Anvil(Stream input, int regionx, int regionz)
        {
            X = regionx * 32;
            Z = regionz * 32;
            //free.
            /*
            if (input.Length == 0)
            {
                for (int i = 0; i < 0x2000; i++)
                    input.WriteByte(0);
            }*/
            this.Input = input;
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = this.Input.ReadIntBigEndian();
                if ((offsets[i] & 0xff) != 0)
                {
                    free.Add(offsets[i] >> 8);
                    int sec = offsets[i] & 0xff;
                    for (int j = 2; j <= sec; j++)
                    {
                        free.Add((offsets[i] >> 8) + j);
                    }
                }
            }
            freelistendex = offsets.Length - free.Count;
            freeList = new int[freelistendex];
            freelistendex = 0;
            for (int i = 2; i < offsets.Length; i++)
            {
                if (!free.Contains(i))
                    freeList[freelistendex++] = i;
            }
            freelistendex = 0;
            for (int i = 0; i < timestamps.Length; i++)
                timestamps[i] = this.Input.ReadIntBigEndian();
        }
        public int AllocChunk()
        {
            return freeList[freelistendex++];
        }
        List<T> CreateList<T>(int cap, T val)
        {
            var lis = new List<T>(cap);
            for (int i = 0; i < cap; i++)
            {
                lis.Add(val);
            }
            return lis;
        }
        public NBT CreateLevel(int x, int z)
        {
            var tc = new TagCompound("Level", new List<Tag>
                {
                    new TagByte("LightPopulated", 0),
                    new TagByte("TerrainPopulated", 1),
                    new TagByte("V", 1),
                    new TagInt("xPos", x + X),
                    new TagInt("zPos", z + Z),
                    new TagLong("InhabitedTime", 0),
                    new TagLong("LastUpdate", 0),
                    new TagByteArray("Biomes", CreateList<byte>(256, 0)),
                    new TagList("Entities", TagType.Compound),
                    new TagList("TileEntities", TagType.Compound),
                    new TagList("Sections", TagType.Compound),
                    new TagIntArray("HeightMap", CreateList<int>(256, 0))
                });
            var n = new NBT();
            n.Root = new TagCompound("");
            n.Root.AddTag(tc);
            return n;
        }
        TagCompound GenerateSection(int y)
        {
            var BlockLight = CreateList<byte>(2048, 0);
            var Data = CreateList<byte>(2048, 0);
            var Blocks = CreateList<byte>(4096, 0);
            var SkyLight = CreateList<byte>(2048, 0);
            var chunk = new TagCompound();
            chunk.AddTag(new TagInt("Y", y));
            chunk.AddTag(new TagByteArray("BlockLight", BlockLight));
            chunk.AddTag(new TagByteArray("Data", Data));
            chunk.AddTag(new TagByteArray("Blocks", Blocks));
            chunk.AddTag(new TagByteArray("SkyLight", SkyLight));
            return chunk;
        }
        public void SetBlock(int x, int y, int z, byte block, byte data)
        {
            int cx = x / 16, cy = y / 16, cz = z / 16;
            int uses = GetOffset(cx, cz) & 0xff;
            int k = cx + cz * 32;
            int i = GetIndex(cx, cz);

            if (uses == 0 || i == 0)
            {
                i = AllocChunk();
                Debug.WriteLine("Allocate Chunk{0}", i);
                offsets[k] = (i << 8) | 0x01;
            }
            var nbt = nbts[k];
            //Unloaded Chunk
            if (nbt == null)
            {
                //Load
                nbts[k] = nbt = Read(cx, cz);
                Debug.WriteLine("Loading Chunk(x,z) {0},{1}", cx, cz);
                //Ungenerated Chunk
                if (nbt == null)
                {
                    //Generate
                    Debug.WriteLine("Generating Chunk(x,z) {0},{1}", cx, cz);
                    nbts[k] = nbt = CreateLevel(cx, cz);
                    offsets[k] = (i << 8) | 0x01;
                }
            }
            var level = ((TagCompound)nbt.Root["Level"]);
            if (!level.Tags.ContainsKey("Sections"))
            {
                level.AddTag(new TagList("Sections", TagType.Compound));
            }
            var mogi = (TagList)level["Sections"];
            //Debug.WriteLine("Loading Chunk(y) {0}", cy);
            //チャンクが生成されていない!!!
            if (mogi.Tags.Count <= cy)
            {
                for (int j = mogi.Tags.Count; j <= cy; j++)
                {
                    Debug.WriteLine("Generating Chunk(y) {0}", j);
                    mogi.Tags.Add(GenerateSection(j));
                }
            }
            var chunk = (TagCompound)mogi.Tags[cy];
            var blocks = (TagByteArray)chunk.Tags["Blocks"];
            var dt = (TagByteArray)chunk.Tags["Data"];
            blocks.Item[(x & 15) | ((z & 15) << 4) | ((y & 15) << 8)] = block;
            var dtindex = ((x & 15) >> 1) | ((z & 15) << 3/*4*/) | ((y & 15) << 7);
            if (X < 0)
            {
                var v = (byte)(0x0F << (byte)((x & 15) & 1) * 4);
                dt.Item[dtindex] &= v;//^
                dt.Item[dtindex] = (byte)((byte)dt.Item[dtindex] | (byte)((byte)data << (byte)((x & 15) & 1) * 4));
            }
            else
            {
                var v = (byte)(0x0F << (byte)((x & 1) ^ 1) * 4);
                dt.Item[dtindex] &= v;//^
                dt.Item[dtindex] = (byte)((byte)dt.Item[dtindex] | (byte)((byte)data << (byte)((x + 1 & 1) ^ 1) * 4));
            }
        }
        /// <summary>
        /// Save to input file stream
        /// </summary>
        public void Write()
        {
            string nam = Path.GetTempFileName();
            Debug.WriteLine("Write to {0}", (object)nam);
            var fs = new FileStream(nam, FileMode.OpenOrCreate);
            Write(fs);
            fs.Close();
            Input.Close();
            Debug.WriteLine("Copy to {0}", (object)((FileStream)Input).Name);
            File.Delete(((FileStream)Input).Name);
            File.Move(nam, ((FileStream)Input).Name);
            //            File.Copy(nam, ((FileStream)Input).Name, true);
            //          File.Delete(nam);
        }
        public void Write(Stream stream)
        {
            int i;
            for (i = 0; i < offsets.Length; i++)
                stream.Write(offsets[i]);
            for (i = 0; i < timestamps.Length; i++)
                stream.Write(timestamps[i]);
            var offsets2 = new int[4096 / 4];
            DeflateStream com = null;
            int off = 0x200;
            for (int m = 0; m < offsets.Length; m++)
            {
                i = offsets[m] >> 8;
                var use = offsets[m] & 0xff;
                offsets2[m] = 0;
                if (use == 0)
                {
                    continue;
                }
                offsets2[m] |= off;
                if (nbts[m] == null)
                {
                    CopyTo(Input, stream, i);
                }
                else
                {
                    long pos = stream.Position;
                    stream.Write((int)0);//dmmy
                    stream.WriteByte(0x02);
                    //Write Header
                    stream.WriteByte(0x78);
                    stream.WriteByte(0x9C);
                    com = new DeflateStream(stream, CompressionMode.Compress, true);
                    nbts[m].Write(com);
                    com.Close();// Flush();
                    long siz = stream.Position - (pos + 4);
                    stream.Position = pos;
                    stream.Write((int)siz);
                    stream.Position = pos + siz + 4;
                }
                int j = ((((int)stream.Position + 4096) / 4096) * 4096) - (int)stream.Position;
                for (int k = 0; k < j; k++)//padding
                    stream.WriteByte(0);
                offsets2[m] |= (((int)stream.Position >> 4) - off) >> 8;
                off = (int)stream.Position >> 4;
            }
            stream.Position = 0;
            for (i = 0; i < offsets.Length; i++)
                stream.Write(offsets2[i]);
            stream.Flush();
        }
        public NBT Read(int x, int z)
        {
            int off = GetOffset(x, z);

            return ReadP(off >> 8, x + z * 32);
        }
        byte[] buffer = new byte[4096];
        public void CopyTo(Stream input, Stream to, int pos)
        {
            input.Position = pos * 4096;
            int length = input.ReadIntBigEndian() + 1;
            if (length > buffer.Length)
                buffer = new byte[length];
            input.Read(buffer, 0, length);
            input.Position = pos * 4096;
            to.Write(length - 1);
            to.Write(buffer, 0, length);
        }
        public NBT ReadP(int pos, int m)
        {
            if (pos == 0) return null;
            if (nbts[m] != null)
            {
                return
                    nbts[m];
            }
            Input.Position = pos * 4096;
            int length = Input.ReadIntBigEndian() - 1;
            int type = Input.ReadByte();
            Stream decomStream;
            if (type == 2)
            {
                Input.ReadByte();
                Input.ReadByte();
                length -= 2;
                decomStream = new DeflateStream(Input, CompressionMode.Decompress, true);
                //Skip Header
            }
            else if (type == 1)
            {
                decomStream = new GZipStream(Input, CompressionMode.Decompress, true);
            }
            else
            {
                return null;
            }
            var nbt = new NBT(decomStream);
            nbt.Read();
            decomStream.Close();
            nbts[m] = nbt;
            return nbt;
        }
        /// <summary>
        /// Close Input Stream
        /// </summary>
        public void Dispose()
        {
            this.Input.Close();
        }
        private int GetOffset(int x, int z)
        {
            return offsets[x + z * 32];
        }
        private int GetIndex(int x, int z)
        {
            return GetOffset(x, z) >> 8;
        }
    }
}
