﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace HandyControlDemo.Data
{
    internal class GlobalData
    {
        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
            {
                try
                {
                    var json = File.ReadAllText(AppConfig.SavePath);
                    Config = JsonConvert.DeserializeObject<AppConfig>(json);
                }
                catch
                {
                    Config = new AppConfig();
                }
            }
            else
            {
                Config = new AppConfig();
            }
        }

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(AppConfig.SavePath, json);
        }

        public static AppConfig Config { get; set; }
    }
}