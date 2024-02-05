using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using Mono.Cecil;

namespace CecilMerge
{
    internal class PluginAnalyzer : IDisposable
    {
        private const string CacheName = "CecilMerge.Runtime.PluginData.dat";
        private static string CacheFilePath => Path.Combine(Paths.CachePath, CacheName);
        
        internal Dictionary<string, AssemblyData> Data = new Dictionary<string, AssemblyData>();
        
        internal void Analyze(string path)
        {
            LoadCache();

            Directory
                .GetFiles(Path.GetFullPath(path), "*.dll", SearchOption.AllDirectories)
                .ForEach(filePath => 
                    Analyze(AssemblyDefinition.ReadAssembly(filePath, TypeLoader.ReaderParameters)));
            
            SaveCache();
        }

        private void Analyze(AssemblyDefinition assembly)
        {
            
        }
        private void LoadCache()
        {
            if (!File.Exists(CacheFilePath)) return;

            using (var binaryReader = new BinaryReader(File.OpenRead(CacheFilePath)))
            {
                var quantityToRead = binaryReader.ReadInt32();
                for (var i = 0; i < quantityToRead; i++)
                {
                    var dllFileDir = binaryReader.ReadString();
                    
                    AssemblyData data = default;
                    data.DllFileDir = dllFileDir;
                    data.Load(binaryReader);
                    Data[dllFileDir] = data;
                }
            }
        }
        private void SaveCache()
        {
            if (!Directory.Exists(Paths.CachePath))
                Directory.CreateDirectory(Paths.CachePath);
            using (var binaryWriter = new BinaryWriter(File.OpenWrite(CacheFilePath)))
            {
                binaryWriter.Write(Data.Count);
                foreach (var assemblyData in Data.Values)
                {
                    binaryWriter.Write(assemblyData.DllFileDir);
                    assemblyData.Save(binaryWriter);
                }
            }
        }

        public void Dispose()
        {
            Data.Clear();
            Data = null;
        }

        internal struct AssemblyData
        {
            internal string DllFileDir;
            internal long FileTimestampLastSave;
            internal bool DllFileUpdatedSinceSave => FileTimestampLastSave != FileTimestamp;

            private long FileTimestamp =>
                File.Exists(DllFileDir) ? File.GetLastWriteTimeUtc(DllFileDir).Ticks : -1;

            internal void Load(BinaryReader binaryReader)
            {
                FileTimestampLastSave = binaryReader.ReadInt64();
            }
            internal void Save(BinaryWriter binaryWriter)
            {
                binaryWriter.Write(FileTimestamp);
            }
        }
    }
}