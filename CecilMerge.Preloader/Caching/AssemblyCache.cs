using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using CecilMerge.Puzzle;
using Mono.Cecil;

namespace CecilMerge.Caching
{
    internal class AssemblyCache : IDisposable
    {
        private const string CacheName = "CecilMerge.PluginData.dat";
        private static string CacheFilePath => Path.Combine(Paths.CachePath, CacheName);

        internal Dictionary<string, AssemblyData> Data = new Dictionary<string, AssemblyData>();
        public long LastPatcherWriteTime { get; private set; }

        private long CurrentPatcherWriteTime =>
            File.GetLastWriteTimeUtc(System.Reflection.Assembly.GetExecutingAssembly().Location).ToFileTimeUtc();
        private bool PatcherFileSame =>
            LastPatcherWriteTime == CurrentPatcherWriteTime;

        private static string CleanPluginPath(string path, string directory) => path.Replace(directory, "");
        
        internal void CacheAssemblyInformation(string path)
        {
            LoadCache();
            
            foreach (var file in Directory
                         .GetFiles(Path.GetFullPath(path), "*.dll", SearchOption.AllDirectories))
            {
                // Load the dll for analysis or resolving
                AssemblyDefinition assemblyDefinition;
                try
                {
                    assemblyDefinition = AssemblyDefinition.ReadAssembly(file, TypeLoader.ReaderParameters);
                }
                catch (BadImageFormatException ex)
                {
                    CecilLog.LogWarning("Ignoring file " + CleanPluginPath(file, path) + 
                                        " because it's not a valid DLL!" + ex.Message);
                    continue;
                }
                catch (Exception e)
                {
                    CecilLog.LogError(e.Message);
                    continue;
                }
                
                if (Data.TryGetValue(file, out var assemblyData) && assemblyData.DllFileSame && PatcherFileSame)
                {
                    Merge.Resolve(assemblyData.Merges, assemblyDefinition);
                    CecilLog.LogWarning("Loaded cached dll data '" + assemblyData.SimpleName +
                                     "'. Last known DLL write " +
                                     "time: " + DateTime.FromFileTimeUtc(assemblyData.FileTimestampLastSave).ToLocalTime().ToString("g"));
                    assemblyDefinition.Dispose();
                    continue;
                }

                Data[file] = new AssemblyData
                {
                    IsValidated = true,
                    SimpleName = assemblyDefinition.MainModule.Name,
                    DllFileDir = file,
                    Merges = Merge.Evaluate(assemblyDefinition)
                };
                CecilLog.LogInfo("Loaded and cached dll data '" + Data[file].SimpleName +
                                 "' from file located at '" + CleanPluginPath(file, path) + "'.");
                assemblyDefinition.Dispose();
            }
            
            SaveCache();
        }
        
        private void LoadCache()
        {
            if (!File.Exists(CacheFilePath)) return; // No data to load

            using (var binaryReader = new BinaryReader(File.OpenRead(CacheFilePath)))
            {
                LastPatcherWriteTime = binaryReader.ReadInt64();
                if (LastPatcherWriteTime != CurrentPatcherWriteTime) 
                {
                    CecilLog.LogVerbose("Cache is discarded because CecilMerge has updated!");
                    return; // Data could be malformed for this version
                }
                // We'll throw it out since it's relatively inexpensive to re-run the analysis once.
                
                var quantityToRead = binaryReader.ReadInt32();
                for (var i = 0; i < quantityToRead; i++)
                {
                    var data = new AssemblyData
                    {
                        DllFileDir = binaryReader.ReadString()
                    };
                    data.Load(binaryReader);
                    Data[data.DllFileDir] = data;
                }
            }
        }
        private void SaveCache()
        {
            if (!Directory.Exists(Paths.CachePath))
                Directory.CreateDirectory(Paths.CachePath);
            using (var binaryWriter = new BinaryWriter(File.OpenWrite(CacheFilePath)))
            {
                binaryWriter.Write(CurrentPatcherWriteTime);
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

        internal class AssemblyData : ICecilCacheable
        {
            internal string SimpleName;
            internal string DllFileDir;
            internal long FileTimestampLastSave;
            internal Merge[] Merges;
            
            internal bool DllFileSame => FileTimestampLastSave == FileTimestamp;

            private long FileTimestamp =>
                File.Exists(DllFileDir) ? File.GetLastWriteTimeUtc(DllFileDir).ToFileTimeUtc() : -1;

            public bool IsValidated { get; set; }
            public void Load(BinaryReader binaryReader)
            {
                SimpleName = binaryReader.ReadString();
                FileTimestampLastSave = binaryReader.ReadInt64();

                Merges = new Merge[binaryReader.ReadInt32()];
                for (var i = 0; i < Merges.Length; i++)
                {
                    Merges[i] = new Merge();
                    Merges[i].Load(binaryReader);
                }

                IsValidated = true;
            }
            public void Save(BinaryWriter binaryWriter)
            {
                binaryWriter.Write(SimpleName);
                binaryWriter.Write(FileTimestamp);

                binaryWriter.Write(Merges.Length);
                foreach (var merge in Merges)
                {
                    merge.Save(binaryWriter);
                }
            }
        }
    }
}