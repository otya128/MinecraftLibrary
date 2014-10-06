using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace otya.Minecraft.NBT
{
    public class TagByte : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Byte;
            }
        }
        public TagByte() { }
        public TagByte(string name, byte value) { this.Name = name; this.Item = value; }
        public byte Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            int by = stream.ReadByte();
            if (by == -1) throw new IllegalNBTException(NBT.SmallStreamMessage);
            this.Item = (byte)by;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item.ToString();
        }
    }
    public class TagShort : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Short;
            }
        }
        public TagShort() { }
        public TagShort(string name, short value) { this.Name = name; this.Item = value; }
        public short Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            short by = stream.ReadShortBigEndian();
            this.Item = (short)by;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item.ToString();
        }
    }
    public class TagInt : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Int;
            }
        }
        public TagInt() { }
        public TagInt(string name, int value) { this.Name = name; this.Item = value; }
        public int Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            int by = stream.ReadIntBigEndian();
            this.Item = by;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item.ToString();
        }
    }
    public class TagLong : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Long;
            }
        }
        public TagLong() { }
        public TagLong(string name, long value) { this.Name = name; this.Item = value; }
        public long Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            long by = stream.ReadLongBigEndian();
            this.Item = by;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item.ToString();
        }
    }
    public class TagFloat : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Float;
            }
        }
        public TagFloat() { }
        public TagFloat(string name, float value) { this.Name = name; this.Item = value; }
        public float Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            float by = stream.ReadFloat();
            this.Item = by;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item.ToString();
        }
    }
    public class TagDouble : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Double;
            }
        }
        public TagDouble() { }
        public TagDouble(string name, double value) { this.Name = name; this.Item = value; }
        public double Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            double by = stream.ReadDouble();
            this.Item = by;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item.ToString();
        }
    }
    public class TagByteArray : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.ByteArray;
            }
        }
        public TagByteArray() { }
        public TagByteArray(string name, List<byte> value) { this.Name = name; this.Item = value; }
        public List<byte> Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            int by = stream.ReadIntBigEndian();
            byte[] bytes = new byte[by];
            stream.Read(bytes, 0, by);
            Item = new List<byte>(bytes);
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write((int)this.Item.Count);
            foreach (var i in Item)
                stream.Write(i);
        }
        public override string NBTToString()
        {
            StringBuilder sb = new StringBuilder(this.Item.Count * 3);
            int length = this.Item.Count;
            for (int i = 0; i < length; i++)
            {
                sb.Append(Item[i].ToString("X2"));
                if (i <= length - 2) sb.Append(',');
            }
            return sb.ToString();
        }
    }
    public class TagIntArray : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.IntArray;
            }
        }
        public TagIntArray() { }
        public TagIntArray(string name, List<int> value) { this.Name = name; this.Item = value; }
        public List<int> Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            int by = stream.ReadIntBigEndian();
            Item = new List<int>(by);
            for (int i = 0; i < by; i++)
            {
                Item.Add(stream.ReadIntBigEndian());
            }
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            stream.Write((int)this.Item.Count);
            foreach (var i in Item)
                stream.Write(i);
        }
        public override string NBTToString()
        {
            StringBuilder sb = new StringBuilder();
            int length = this.Item.Count;
            for (int i = 0; i < length; i++)
            {
                sb.Append(Item[i].ToString());
                if (i <= length - 2) sb.Append(',');
            }
            return sb.ToString();
        }
    }
    public class TagString : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.String;
            }
        }
        public TagString() { }
        public TagString(string name, string value) { this.Name = name; this.Item = value; }
        public string Item { get; set; }
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            string name = this.Name;
            if (nameread) base.Read(stream);
            this.Item = this.Name;
            this.Name = name;
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            base.Write(stream, namewrite);
            if (Item == null)
            {
                stream.Write((short)0);
                return;
            }
            stream.Write((short)Encoding.UTF8.GetBytes(Item).Length);
            stream.Write(Item);
        }
        public override string NBTToString()
        {
            return this.Item;
        }
    }
}
