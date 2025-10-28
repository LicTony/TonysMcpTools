using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TonysMcpTools.Utiles
{
    public static class DpapiHelper
    {
        
        /// <summary>
        /// Cifra un texto usando DPAPI
        /// </summary>
        /// <param name="textoPlano">El texto a cifrar</param>
        /// <param name="usarMaquina">Si es true, cualquier usuario de la máquina puede descifrar. 
        /// Si es false (default), solo el usuario actual puede descifrar</param>
        /// <returns>El texto cifrado en Base64</returns>
        public static string Cifrar(string textoPlano, byte[] _entropy, bool usarMaquina = false)
        {
            if (string.IsNullOrEmpty(textoPlano))
                return string.Empty;

            try
            {
                // Convertir el texto a bytes
                byte[] datosPlanos = Encoding.UTF8.GetBytes(textoPlano);

                // Elegir el scope (usuario actual o máquina)
                DataProtectionScope scope = usarMaquina
                    ? DataProtectionScope.LocalMachine
                    : DataProtectionScope.CurrentUser;

                // Cifrar usando DPAPI
                byte[] datosCifrados = ProtectedData.Protect(datosPlanos, _entropy, scope);

                // Retornar como string Base64 para fácil almacenamiento
                return Convert.ToBase64String(datosCifrados);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("Error al cifrar los datos con DPAPI", ex);
            }
        }

        /// <summary>
        /// Descifra un texto previamente cifrado con DPAPI
        /// </summary>
        /// <param name="textoCifrado">El texto cifrado en Base64</param>
        /// <param name="usarMaquina">Debe coincidir con el valor usado al cifrar</param>
        /// <returns>El texto original descifrado</returns>
        public static string Descifrar(string textoCifrado, byte[] _entropy, bool usarMaquina = false)
        {
            if (string.IsNullOrEmpty(textoCifrado))
                return string.Empty;

            try
            {
                // Convertir desde Base64 a bytes
                byte[] datosCifrados = Convert.FromBase64String(textoCifrado);

                // Elegir el scope (debe ser el mismo que al cifrar)
                DataProtectionScope scope = usarMaquina
                    ? DataProtectionScope.LocalMachine
                    : DataProtectionScope.CurrentUser;

                // Descifrar usando DPAPI
                byte[] datosDescifrados = ProtectedData.Unprotect(datosCifrados, _entropy, scope);

                // Convertir los bytes de vuelta a string
                return Encoding.UTF8.GetString(datosDescifrados);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("Error al descifrar los datos. Puede que hayan sido cifrados por otro usuario.", ex);
            }
            catch (FormatException ex)
            {
                throw new FormatException("El formato de los datos cifrados no es válido", ex);
            }
        }


        /// <summary>
        /// Descifra un texto previamente cifrado con DPAPI de forma segura (atrapando excepciones)
        /// </summary>
        /// <param name="textoCifrado">El texto cifrado en Base64</param>
        /// <param name="usarMaquina">Debe coincidir con el valor usado al cifrar</param>
        /// <returns>El texto original descifrado</returns>
        public static string DescifrarSeguro(string textoCifrado,byte[] _entropy, bool usarMaquina = false)
        {
            try
            {
                return Descifrar(textoCifrado, _entropy, usarMaquina);
            }
            catch
            {
                return string.Empty;
            }
        }

    }

}
