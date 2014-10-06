using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace otya.Minecraft.Material
{
    public class Material
    {
        static public Material Stone = new Material(1, "minecraft:stone");
        public byte ID
        {
            get;
            protected set;
        }
        [Obsolete]
        internal Material(byte ID)
        {
            this.ID = ID;
            this.Name = "";
        }
        internal Material(byte ID, string name)
        {
            this.ID = ID;
            this.Name = name;
        }
        public string Name
        {
            get;
            protected set;
        }
    }
}
