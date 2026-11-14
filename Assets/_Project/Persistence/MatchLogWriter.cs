using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Keshipin.Core;
using UnityEngine;

namespace Keshipin.Persistence
{
    public sealed class MatchLogWriter : MonoBehaviour
    {
        readonly List<string> lines = new();

        public void ResetLog()
        {
            lines.Clear();
            lines.Add($"# KeshipinWars match {DateTime.UtcNow:O}");
        }

        public void Append(MatchEvent ev)
        {
            lines.Add(ev.ToString());
        }

        public string WriteToDisk(string matchId)
        {
            var dir = Path.Combine(Application.persistentDataPath, "MatchLogs");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{matchId}.log");
            File.WriteAllLines(path, lines, Encoding.UTF8);
            return path;
        }

        public string FinalStateHash(MatchState state)
        {
            var sb = new StringBuilder();
            sb.Append(state.Phase).Append('|').Append(state.Round);
            foreach (var p in state.Players)
            {
                sb.Append('|').Append(p.Id.Value).Append(':').Append(p.Stocks).Append(':').Append(p.Points);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hash = 2166136261u;
            foreach (var b in bytes)
            {
                hash ^= b;
                hash *= 16777619u;
            }

            return hash.ToString("X8");
        }
    }
}
