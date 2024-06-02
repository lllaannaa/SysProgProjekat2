using System.Collections.Generic;

namespace SysProgProjekat2
{
    class Cache
    {
        private readonly Dictionary<string, string[]> _cache = new Dictionary<string, string[]>();
        private readonly object _cacheLock = new object();

        public string[] GetFiles(string keyword)
        {
            lock (_cacheLock)
            {
                _cache.TryGetValue(keyword, out var files);
                return files;
            }
        }

        public void SetFiles(string keyword, string[] files)
        {
            lock (_cacheLock)
            {
                _cache[keyword] = files;
            }
        }
    }
}
