using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Mono.Cecil;

namespace CecilMerge
{
    internal class AssemblyCache : IDisposable
    {
        private const string CacheName = "CecilMerge.Runtime.PluginData.dat";
        private static string CacheFilePath => Path.Combine(Paths.CachePath, CacheName);

        internal Dictionary<string, AssemblyData> Data = new Dictionary<string, AssemblyData>();
        public long LastPatcherWriteTime { get; private set; }

        private long CurrentPatcherWriteTime =>
            File.GetLastWriteTimeUtc(System.Reflection.Assembly.GetExecutingAssembly().Location).ToFileTimeUtc();
        private bool PatcherFileSame =>
            LastPatcherWriteTime == CurrentPatcherWriteTime;

        private static string CleanPluginPath(string path, string directory) => path.Replace(directory, "");
        
        internal IEnumerable<KeyValuePair<AssemblyDefinition, AssemblyData>> CacheAssemblyInformation(string path)
        {
            LoadCache();
            
            foreach (var file in Directory
                         .GetFiles(Path.GetFullPath(path), "*.dll", SearchOption.AllDirectories))
            {
                if (Data.TryGetValue(file, out var assemblyData) && assemblyData.DllFileSame && PatcherFileSame)
                {
                    CecilLog.LogWarning("Cached dll data '" + assemblyData.SimpleName +
                                     "'. Last known DLL write " +
                                     "time: " + DateTime.FromFileTimeUtc(assemblyData.FileTimestampLastSave).ToLocalTime().ToString("g"));
                    continue;
                }
                
                // Analyze the dll now
                AssemblyDefinition assembly;
                try
                {
                    assembly = AssemblyDefinition.ReadAssembly(file, TypeLoader.ReaderParameters);
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
                
                assemblyData = new AssemblyData
                {
                    SimpleName = assembly.MainModule.Name,
                    DllFileDir = file
                };

                yield return new KeyValuePair<AssemblyDefinition, AssemblyData>(assembly, assemblyData);
                    
            
                Data[file] = assemblyData;
                    
                CecilLog.LogInfo("Cached dll data '" + assemblyData.SimpleName +
                                 "' from file located at '" + CleanPluginPath(file, path) + "'.");
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

        internal class AssemblyData
        {
            internal string SimpleName;
            internal string DllFileDir;
            internal long FileTimestampLastSave;
            internal bool DllFileSame => FileTimestampLastSave == FileTimestamp;

            private long FileTimestamp =>
                File.Exists(DllFileDir) ? File.GetLastWriteTimeUtc(DllFileDir).ToFileTimeUtc() : -1;

            internal void Load(BinaryReader binaryReader)
            {
                SimpleName = binaryReader.ReadString();
                FileTimestampLastSave = binaryReader.ReadInt64();
            }
            internal void Save(BinaryWriter binaryWriter)
            {
                binaryWriter.Write(SimpleName);
                binaryWriter.Write(FileTimestamp);
            }
        }
    }
}