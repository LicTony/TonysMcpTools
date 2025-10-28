using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TonysMcpTools.Utiles
{
    public static class Util
    {

        /// <summary>
        /// Retorna variable de entorno del tipo string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string? GetString(string key, string? defaultValue = null)
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }



        /// <summary>
        /// Retorna variable de entorno del tipo string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetStringNotNull(string key, string defaultValue = "")
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }


        /// <summary>
        /// Retorna variable de entorno del tipo entero
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int GetInt(string key, int defaultValue = 0)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Retorna variable de entorno del tipo boleano
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }


    }
}
