using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace otya.Minecraft.NBT
{
    public enum TagType : byte
    {
        End,
        Byte,
        Short,
        Int,
        Long,
        Float,
        Double,
        ByteArray,
        String,
        List,
        Compound,
        IntArray,
    }
    internal static class ExMethod
    {
        internal static short ReadShortBigEndian(this Stream stream)
        {
            int namesizeh = stream.ReadByte();
            int namesizel = stream.ReadByte();
            if (namesizeh == -1 || namesizel == -1) return -1;
            return (short)(namesizeh << 8 | namesizel);
        }
        internal static int ReadIntBigEndian(this Stream stream)
        {
            byte[] Int = new byte[sizeof(int)];
            if (stream.Read(Int, 0, Int.Length) == -1) return -1;
            int result = 0;
            int shift = sizeof(int) * 8 - 8;
            foreach (var i in Int)
            {
                result |= i << shift;
                shift -= 8;
            }
            return result;
        }
        internal static long ReadLongBigEndian(this Stream stream)
        {
            byte[] Int = new byte[sizeof(long)];
            if (stream.Read(Int, 0, Int.Length) == -1) return -1;
            long result = 0;
            int shift = sizeof(long) * 8 - 8;
            foreach (var i in Int)
            {
                result = result | (long)((int)i << (int)shift);
                shift -= 8;
            }
            return (long)result;
        }
        internal static float ReadFloat(this Stream stream)
        {
            byte[] value = new byte[sizeof(float)];
            stream.Read(value, 0, sizeof(float)); if (BitConverter.IsLittleEndian) Array.Reverse(value);
            return BitConverter.ToSingle(value, 0);
        }
        internal static double ReadDouble(this Stream stream)
        {
            byte[] value = new byte[sizeof(double)];
            stream.Read(value, 0, sizeof(double)); if (BitConverter.IsLittleEndian) Array.Reverse(value);
            return BitConverter.ToDouble(value, 0);
            //using(BinaryReader br = new BinaryReader(stream))
            //return br.ReadDouble();
        }
        internal static void Write(this Stream stream, int w)
        {
            byte[] ww = BitConverter.GetBytes(w); if (BitConverter.IsLittleEndian) Array.Reverse(ww);
            stream.Write(ww, 0, ww.Length);
            //stream.WriteByte((byte)(w >> 24));
            //stream.WriteByte((byte)(w >> 16));
            //stream.WriteByte((byte)(w >> 8));
            //stream.WriteByte((byte)w);
        }
        internal static void Write(this Stream stream, long w)
        {
            byte[] ww = BitConverter.GetBytes(w); if (BitConverter.IsLittleEndian) Array.Reverse(ww);
            stream.Write(ww, 0, ww.Length);
            //stream.WriteByte((byte)(w >> 56));
            //stream.WriteByte((byte)(w >> 48));
            //stream.WriteByte((byte)(w >> 40));
            //stream.WriteByte((byte)(w >> 32));
            //stream.WriteByte((byte)(w >> 24));
            //stream.WriteByte((byte)(w >> 16));
            //stream.WriteByte((byte)(w >> 8));
            //stream.WriteByte((byte)w);
        }
        internal static void Write(this Stream stream, short w)
        {
            byte[] ww = BitConverter.GetBytes(w); if (BitConverter.IsLittleEndian) Array.Reverse(ww);
            stream.Write(ww, 0, ww.Length);
            //stream.WriteByte((byte)(w >> 8));
            //stream.WriteByte((byte)w);
        }
        internal static void Write(this Stream stream, byte w)
        {
            stream.WriteByte(w);
        }
        internal static void Write(this Stream stream, float w)
        {
            byte[] ww = BitConverter.GetBytes(w); if (BitConverter.IsLittleEndian) Array.Reverse(ww);
            stream.Write(ww, 0, ww.Length);
        }
        internal static void Write(this Stream stream, double w)
        {
            byte[] ww = BitConverter.GetBytes(w); if (BitConverter.IsLittleEndian) Array.Reverse(ww);
            stream.Write(ww, 0, ww.Length);
        }
        internal static void Write(this Stream stream, string w)
        {
            byte[] ww = Encoding.UTF8.GetBytes(w); //if (!BitConverter.IsLittleEndian) ww.Reverse();
            stream.Write(ww, 0, ww.Length);
        }
    }
    public class IllegalNBTException : Exception
    {
        public IllegalNBTException(string msg) : base(msg) { }
    }
    public interface Tag
    {
        TagType Type { get; }
        string Name { get; }
        void Read(Stream stream, bool nameread = true);
        void Write(Stream stream, bool namewrite = true);
    }
    public class TagEnd : Tag
    {
        public TagType Type
        {
            get
            {
                return TagType.End;
            }
        }
        public string Name { get; protected set; }
        public virtual void Read(Stream stream, bool nameread = true) { }
        public virtual void Write(Stream stream, bool namewrite = true) { }
    }
    public abstract class TagBase : Tag
    {
        public abstract TagType Type { get; }
        public string Name { get; protected set; }
        public virtual void Read(Stream stream, bool nameread = true)
        {
            if (!nameread) return;
            int namesize = stream.ReadShortBigEndian();
            if (namesize == -1) throw new IllegalNBTException(NBT.SmallStreamMessage);
            byte[] name = new byte[namesize];
            stream.Read(name, 0, namesize);
            this.Name = Encoding.UTF8.GetString(name);
            //stream.read
        }
        public virtual void Write(Stream stream, bool namewrite = true)
        {
            if (namewrite)
            {
                stream.Write((byte)this.Type);
                if (this.Name == null) { stream.Write((byte)0); stream.Write((byte)0); return; }
                stream.Write((short)this.Name.Length);
                stream.Write(this.Name);
            }
        }
        public virtual string NBTToString()
        {
            return Type.ToString();
        }
    }
    public class TagCompound : TagBase
    {
        public override TagType Type
        {
            get
            {
                return TagType.Compound;
            }
        }
        public TagCompound()
        {
            //this.reader = new NBT();
        }
        public TagCompound(string name)
        {
            this.Name = name;
            //this.reader = new NBT();
        }
        public TagCompound(string name, List<Tag> value)
        {
            this.Name = name;
            foreach (var i in value)
            {
                this.Tags[i.Name] = i;
            }
            //this.reader = new NBT();
            //this.reader.tags.AddRange(value);
        }
        public Tag this[string key]
        {
            get
            {
                return tags[key];
            }
            set
            {
                tags[key] = value;
            }
        }
        //public List<Tag> Tags { get { return this.reader.tags; } }
        public Dictionary<string, Tag> Tags
        {
            get { return tags; }
        }
        public void AddTag(Tag tag)
        {
            this.tags.Add(tag.Name, tag);
        }
        private Dictionary<string, Tag> tags = new Dictionary<string, Tag>();
        //public NBT reader;
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            int read = stream.ReadByte();
            while (read != -1)
            {
                Tag tag = NBT.ReadTag((TagType)read, stream);
                if (tag is TagEnd) return;
                this.tags.Add(tag.Name, tag);
                read = stream.ReadByte();
            }
            //reader = new NBT(stream);
            //reader.Read();
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            //if (namewrite) //why?
            base.Write(stream, namewrite);
            //reader.Write(stream);
            foreach (var i in tags)
            {
                i.Value.Write(stream, true);
            }
            stream.Write((byte)0);
        }
    }
    public class TagList : TagBase
    {
        internal TagList()
        {
        }
        public TagList(TagType ListType)
        {
            this.ListTagType = ListType;
        }
        public TagList(string name, TagType ListType)
            : this(name, ListType, new List<Tag>())
        {
        }
        public TagList(string name, TagType ListType, List<Tag> list)
        {
            this.Name = name;
            this.ListTagType = ListType;
            this.Tags = list;//this.ListSize = ListSize;
            //this.reader = new NBTListReader(null, this);
        }
        public override TagType Type
        {
            get
            {
                return TagType.List;
            }
        }
        //public NBT reader;
        public TagType ListTagType { get; set; }
        //public int ListSize {get;protected set; }

        public List<Tag> Tags { get; protected set; }//{get{return this.reader.tags;}}
        public override void Read(Stream stream, bool nameread = true)
        {
            if (nameread) base.Read(stream);
            int tagtype = stream.ReadByte();
            if (tagtype == -1) throw new IllegalNBTException(NBT.SmallStreamMessage);
            this.ListTagType = (TagType)tagtype;
            int ListSize = stream.ReadIntBigEndian();
            Tags = new List<Tag>(ListSize);
            //this.ListSize = stream.ReadIntBigEndian();
            if (ListSize == -1) throw new IllegalNBTException(NBT.SmallStreamMessage);

            for (int i = 0; i < ListSize; i++)
            {
                Tag tag = NBT.ReadTag(this.ListTagType, stream, false);
                if (tag.Type == TagType.End) return;
                //this.Root = tag;
                this.Tags.Add(tag);
            }
            //reader = new NBTListReader(stream, this);
            //reader.Read();
        }
        public override void Write(Stream stream, bool namewrite = true)
        {
            if (ListTagType == 0)
            {
                if (Tags.Count != 0)
                    ListTagType = Tags[Tags.Count - 1].Type;
            }
            //this.ListSize = Tags.Count;//ListSizeを実際のサイズに
            base.Write(stream, namewrite);
            //if (namewrite)//why?
            stream.Write((byte)ListTagType);
            stream.Write((int)Tags.Count);

            foreach (var i in Tags)
            {
                i.Write(stream, false);
            }
            //stream.WriteByte((byte)TagType.End);
            //reader.Write(stream);
        }/*
        public class NBTListReader : NBT
        {
            protected TagList list;
            public NBTListReader(Stream stream, TagList l)
                : base(stream)
            {
                this.list = l;
            }
            public override void Read()
            {
                for (int i = 0; i < list.ListSize;i++ )
                {
                    Tag tag = ReadTag(list.ListTagType, stream, false);
                    if (tag is TagEnd) return;
                    this.Root = tag;
                    this.tags.Add(tag);
                }
            }
            public override void Write(Stream str)
            {
                writestream = str;
                foreach (var i in tags)
                {
                    i.Write(writestream, false);
                }
            }
        }*/
    }
    public class NBT : IDisposable//, IEnumerable<Tag>
    {
        protected Stream stream;
        protected Stream writestream;
        /// <summary>
        /// Root Tag
        /// </summary>
        public TagCompound Root
        {
            get;
            set;
        }/*
        public Dictionary<string, Tag> Tags
        {
            get { return tagdic; }
        }*/
        /// <summary>
        /// Tag List
        /// </summary>
        //protected List<Tag> tags = new List<Tag>();
        private Dictionary<string, Tag> tagdic = null;
        /*public Tag this[int v]
        {
            get
            {
                return tags[v];
            }
            set
            {
                if (tagdic != null && value.Name != null) tagdic[value.Name] = value;
                tags[v] = value;
            }
        }*/
        public Tag this[string v]
        {
            get
            {/*
                if (tagdic == null)
                {
                    tagdic = new Dictionary<string, Tag>(tags.Count);
                    foreach (var i in tags)
                    {
                        if (i.Name != null) tagdic.Add(i.Name, i);
                    }
                }*/
                return tagdic[v];
            }
            set
            {/*
                if (tagdic == null)
                {
                    tagdic = new Dictionary<string, Tag>(tags.Count);
                    foreach (var i in tags)
                    {
                        if (i.Name != null) tagdic.Add(i.Name, i);
                    }
                }*/
                tagdic[v] = value;
            }
        }
        /// <summary>
        /// Create NBT of Stream
        /// </summary>
        /// <param name="stream">Read Stream</param>
        public NBT(Stream stream)
        {
            this.stream = stream;
        }
        public NBT()
        {
        }
        /// <summary>
        /// Read Stream Dispose
        /// </summary>
        public void Dispose()
        {
            this.stream.Dispose();
        }
        /// <summary>
        /// Create NBT of File
        /// call Read()
        /// </summary>
        /// <param name="FilePath">FilePath</param>
        public NBT(string FilePath)
        {
            FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            if (file.ReadByte() == 0x1F && file.ReadByte() == 0x8B)
            {
                file.Seek(0, SeekOrigin.Begin);//戻す
                this.stream = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
            }
            else
            {
                file.Seek(0, SeekOrigin.Begin);//戻す
                this.stream = file;
            }
        }
        public static void WriteTag(Tag write, Stream stream, bool namewrite = true)
        {
            write.Write(stream, namewrite);
        }
        public static Tag ReadTag(TagType read, Stream stream, bool nameread = true)
        {
            Tag tag = null;
            switch ((TagType)read)
            {
                case TagType.End:
                    tag = new TagEnd();
                    break;
                case TagType.Byte:
                    tag = new TagByte();
                    tag.Read(stream, nameread);
                    break;
                case TagType.Short:
                    tag = new TagShort();
                    tag.Read(stream, nameread);
                    break;
                case TagType.Int:
                    tag = new TagInt();
                    tag.Read(stream, nameread);
                    break;
                case TagType.Long:
                    tag = new TagLong();
                    tag.Read(stream, nameread);
                    break;
                case TagType.Float:
                    tag = new TagFloat();
                    tag.Read(stream, nameread);
                    break;
                case TagType.Double:
                    tag = new TagDouble();
                    tag.Read(stream, nameread);
                    break;
                case TagType.ByteArray:
                    tag = new TagByteArray();
                    tag.Read(stream, nameread);
                    break;
                case TagType.String:
                    tag = new TagString();
                    tag.Read(stream, nameread);
                    break;
                case TagType.List:
                    tag = new TagList();
                    tag.Read(stream, nameread);
                    break;
                case TagType.Compound:
                    tag = new TagCompound();
                    tag.Read(stream, nameread);
                    break;
                case TagType.IntArray:
                    tag = new TagIntArray();
                    tag.Read(stream, nameread);
                    break;
                default:
                    throw new IllegalNBTException("Unknown NBT Tag ID " + read + " " + read.ToString("X") + " " + char.ToString((char)read));
            }
            return tag;
        }
        /// <summary>
        /// Read NBT
        /// </summary>
        public virtual void Read()
        {
            int read = stream.ReadByte();
            this.Root = new TagCompound();
            this.Root.Read(stream);
            /*
            int read = stream.ReadByte();
            while (read != -1)
            {
                Tag tag = ReadTag((TagType)read, stream);
                if (tag is TagEnd) return;
                //this.Root = tag;
                this.tagdic.Add(tag.Name, tag);
                read = stream.ReadByte();
            }*/
        }
        /// <summary>
        /// Write Compression NBT File
        /// </summary>
        /// <param name="FilePath">FilePath</param>
        public virtual void WriteGZipFile(string FilePath)
        {
            using (StreamWriter str = new StreamWriter(FilePath))
            {
                using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(str.BaseStream, System.IO.Compression.CompressionMode.Compress, true))
                {
                    this.Write(gzip);
                }
            }
        }
        /// <summary>
        /// Write Non Compression NBT File
        /// </summary>
        /// <param name="FilePath">FilePath</param>
        public virtual void WriteFile(string FilePath)
        {
            using (StreamWriter str = new StreamWriter(FilePath))
                this.Write(str.BaseStream);
        }/*
        public void Write(Stream str)
        {
            this.Write(new Stream(str));
        }*/
        /// <summary>
        /// Write NBT to Stream
        /// </summary>
        /// <param name="str">Stream</param>
        public virtual void Write(Stream str)
        {
            this.Root.Write(str);
            /*
            writestream = str;
            foreach (var i in tagdic)
            {
                i.Value.Write(writestream, true);
            }*/
        }
        static internal readonly string SmallStreamMessage = "Stream is Small";
    }
}
