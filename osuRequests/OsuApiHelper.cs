﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpOsu;
using CSharpOsu.Module;

namespace osuRequests
{
    class OsuApiHelper
    {
        OsuClient m_client;

        public OsuApiHelper(string apiKey)
        {
            m_client = new OsuClient(apiKey);
            
        }

        public async Task<string[]> GetBeatmap(string url)
        {
            string[] parts = url.Split("/");

            int id = Convert.ToInt32(parts[parts.Length - 1]);
            bool isSet = parts[parts.Length - 2] == "s" ? true : false;

            OsuBeatmap[] maps = m_client.GetBeatmap(id, isSet);

            if (maps.Length == 0)
            {
                return new string[] { "404", "404" };
            }

            OsuBeatmap map = maps[0];

            string resp = "";
            string respTwitch = "";

            if (isSet)
            {
                resp = $"[{url} {map.artist} - {map.title}]";
                respTwitch = $"{map.artist} - {map.title}";
            }
            else
            {
                resp = $"[{url} {map.artist} - {map.title} [{map.version}]] by {map.creator} ({Math.Round(Convert.ToDouble(map.difficultyrating), 2)}*, CS{map.diff_size}, AR{map.diff_approach}, HP{map.diff_drain}, {map.bpm} BPM)";
                respTwitch = $"{map.artist} - {map.title} [{map.version}] by {map.creator} ({Math.Round(Convert.ToDouble(map.difficultyrating), 2)}*, CS{map.diff_size}, AR{map.diff_approach}, HP{map.diff_drain}, {map.bpm} BPM)";
            }

            await Task.FromResult(0);

            return new string[] { resp, respTwitch };
        }
    }
}