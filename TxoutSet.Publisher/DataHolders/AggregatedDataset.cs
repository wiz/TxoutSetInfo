﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using TxoutSet.Common;

namespace TxoutSet.Publisher.DataHolders
{
    public class AggregatedDataset
    {
        public Dictionary<string, Dataset> Sets { get; set; } = new Dictionary<string, Dataset>();

        public void Add(string source, TxoutSetInfo set)
        {
            var setHash = set.ToJsonStr().ToSHA256();
            if (Sets.ContainsKey(setHash))
            {
                Sets[setHash].AddSource(source);
            }
            else
            {
                Sets.Add(setHash, new Dataset(source, set));
            }
        }

        public void Clear()
        {
            Sets.Clear();
        }

        internal void Tweetout(Zonfig _zonfig)
        {
            Auth.SetUserCredentials(_zonfig.ConsumerKey, _zonfig.ConsumerSecret, _zonfig.UserAccessToken, _zonfig.UserAccessSecret);

            foreach (var val in Sets.Values)
                tweetResult(val.ToJson(), val.Consensus);
        }

        private void tweetResult(string tweetText, List<string> consensus)
        {
            var res = Tweet.PublishTweet(tweetText);

            var consensusTweet = String.Join(", ", consensus);
            Tweet.PublishTweetInReplyTo(consensusTweet, res.Id);
        }
    }


    public static class UtilEx
    {
        public static string ToSHA256(this string str)
        {
            return BitConverter.ToString(Encoding.UTF8.GetBytes(str).ToSHA256()).Replace("-", "");
        }

        public static byte[] ToSHA256(this byte[] bytes)
        {
            using (var hash = new SHA256Managed())
            {
                return hash.ComputeHash(bytes);
            }
        }

        public static string ToJsonStr(this TxoutSetInfo set)
        {
            var res = JsonConvert.SerializeObject(set, Formatting.Indented);
            res = res.Replace("-1", "xXx");

            var tweet = res.Replace("{", "").Replace("}", "").Replace("  ", "").Trim();
            return tweet;
        }
    }
}
