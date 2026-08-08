using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace PerfBench
{
    [MemoryDiagnoser]
    public class PaletteSessionBenchmark
    {
        private List<string> _filePaths;
        private string _filePathSync;
        private string _filePathAsync;

        [GlobalSetup]
        public void Setup()
        {
            _filePaths = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                _filePaths.Add($"/path/to/some/fake/file/palette_{i}.act");
            }
            _filePathSync = Path.GetTempFileName();
            _filePathAsync = Path.GetTempFileName();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (File.Exists(_filePathSync)) File.Delete(_filePathSync);
            if (File.Exists(_filePathAsync)) File.Delete(_filePathAsync);
        }

        [Benchmark(Baseline = true)]
        public void SyncSave()
        {
            var json = JsonSerializer.Serialize(_filePaths);
            File.WriteAllText(_filePathSync, json);
        }

        [Benchmark]
        public async Task AsyncSaveText()
        {
            var json = JsonSerializer.Serialize(_filePaths);
            await File.WriteAllTextAsync(_filePathAsync, json);
        }

        [Benchmark]
        public async Task AsyncSerializeStream()
        {
            using var fs = new FileStream(_filePathAsync, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await JsonSerializer.SerializeAsync(fs, _filePaths);
        }
    }
}
