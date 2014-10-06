using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace otya.Minecraft.World
{
    using otya.Minecraft.Anvil;
    using otya.Minecraft.Material;
    public struct Coord
    {
        public int X;
        public int Z;
    }
    public class World : IDisposable
    {
        Dictionary<Coord, Anvil> Anvils;
        string RegionDirectory;
        public World(string directory)
        {
            RegionDirectory = Path.Combine(directory, "region");
            if (!Directory.Exists(RegionDirectory))
            {
                throw new ArgumentException(RegionDirectory + " is not exists");
            }
            Anvils = new Dictionary<Coord, Anvil>();
        }
        public void SetBlock(int x, int y, int z, Material block)
        {
            SetBlock(x, y, z, block.ID, 0);
        }
        public void SetBlock(int x, int y, int z, Material block, byte data)
        {
            SetBlock(x, y, z, block.ID, data);
        }
        public void SetBlock(int x, int y, int z, byte block, byte data)
        {
            int ax = (int)((x >> 4) / 32d);
            int az = (int)((z >> 4) / 32d);
            int indx = x < 0 ? -ax - 1 : ax;
            int indz = z < 0 ? -az - 1 : az;
            ax = x < 0 ? 15 + ax : ax;
            az = z < 0 ? 15 + az/*-az - 1*/ : az;
            Coord Anvil = new Coord { X = indx, Z = indz };
            Anvil mca;
            if (!Anvils.ContainsKey(Anvil))
            {
                Debug.WriteLine("LoadAnvil r.{0}.{1}.mca", indx, indz);
                string file = Path.Combine(RegionDirectory, "r." + indx + '.' + indz + ".mca");
                Anvils.Add(Anvil, mca = new Anvil(new FileStream(file, FileMode.OpenOrCreate), indx, indz));
            }
            else
            {
                mca = Anvils[Anvil];
            }
            if (x < 0) x = 512 + x;
            if (z < 0) z = 512 + z;
            mca.SetBlock(x % 512, y, z % 512, block, data);
        }

        public void UnloadChunk(Coord coord)
        {
            var mca = Anvils[coord];
            Debug.WriteLine("Unloading Chunk {0},{1}", coord.X, coord.Z);
            throw new NotImplementedException();
        }
        public void UnloadAnvil(Coord coord)
        {
            var mca = Anvils[coord];
            Debug.WriteLine("Unloading r.{0}.{1}.mca", coord.X, coord.Z);
            mca.Write();
            Anvils.Remove(coord);
        }
        public void DisposeAsync()
        {
            Task[] allthread = new Task[Anvils.Count];
            int j = 0;
            foreach (var i in Anvils)
            {
                var value = i.Value;
                allthread[j++] = Task.Run(() =>
                {
                    value.Write();
                    value.Dispose();
                });
            }
            Task.WaitAll(allthread);
        }
        public void Dispose()
        {
            foreach (var i in Anvils)
            {
                var value = i.Value;
                value.Write();
                value.Dispose();
            }
        }
    }
}
