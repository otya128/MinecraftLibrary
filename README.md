MinecraftLibrary
================

MinecraftLibrary for C#
```C#
using otya.Minecraft.Material;
using otya.Minecraft.World;
using otya.Minecraft;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var world = new World(Minecraft.GetSaveFolder(args[0])))
            {
                world.SetBlock(0, 0, 0, Material.Stone);
            }
        }
    }
}
```
