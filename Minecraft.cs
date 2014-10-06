using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace otya.Minecraft
{
    public static class Minecraft
    {
        public static readonly string MinecraftDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        public static readonly string SaveDir = Path.Combine(MinecraftDir, "saves");
        public static string GetSaveFolder(string worldname)
        {
            return Path.Combine(SaveDir, worldname);
        }
    }
}
