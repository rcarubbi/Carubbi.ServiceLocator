using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Carubbi.ServiceLocator
{

    /// <summary>
    /// Padrão de injeção de dependência
    /// <para>Através desta classe é possível solicitar uma implementação concreta para uma interface sem o uso da palavra chave "new" nem qualquer referencia direta a classe concreta, 
    /// o que permite que um projeto não possua referencia direta ao projeto que possui a classe concreta, 
    /// dando mais flexibilidade ao código.</para>
    /// <para>
    /// Para que isto funcione, a classe concreta é instanciada por reflexão e o tipo é resolvido a partir de um arquivo de configurações que indica qual classe concreta implementa qual interface
    /// </para>
    /// <example>
    ///     <para>&lt;configSections&gt;</para>
    ///         <para>&lt;section name="Implementations" type="System.Configuration.DictionarySectionHandler"/&gt;</para>
    ///      <para>&lt;/configSections&gt;</para>
    ///     <para>&lt;Implementations&gt;</para>
    ///         <para>&lt;add key="IDAOFactory" value="Itau.BSITokenDALFactory.DAOFactory, Itau.BSITokenDALFactory" /&gt;</para>
    ///         <para>&lt;add key="IDbDataParameter" value="System.Data.SqlClient.SqlParameter, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" /&gt;</para>
    ///         <para>&lt;add key="IGeradorPlanilha[RelatorioExecucaoData]" value="Carubbi.Excel.Implementations.GeradorAsposeCells`1[[Itau.ProtocoloFiname.Entities.Relatorio.RelatorioExecucaoData, Itau.ProtocoloFiname.Entities]], Carubbi.Excel" /&gt;</para>
    ///     <para>&lt;/Implementations&gt;</para>
    /// </example>
    /// </summary>
    public static class ImplementationResolver
    {

        /// <summary>
        /// Getter e Setter do Nome do Arquivo de configurações, caso não definido o padrão é Implementations
        /// </summary>
        public static string ImplementationsFileName { get; set; }

        /// <summary>
        /// Invoca o método GetInstance de uma determinada classe T para construir um objeto que implementa o padrão Singleton
        /// </summary>
        /// <typeparam name="T">Tipo da Interface a ser resolvida</typeparam>
        /// <returns>Objeto concreto construido</returns>
        public static T ResolveSingleton<T>()
        {
            try
            {
                var configSection = ((Hashtable)ConfigurationManager.GetSection(ImplementationsFileName ?? "Implementations"));
                var t = Type.GetType(configSection[GetFriendlyName(typeof(T))].ToString());
                var methodGetInstance = t?.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public);
                var impl = (T)methodGetInstance?.Invoke(t, null);
                return impl;
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Resolve uma interface a partir do tipo T chamando seu construtor padrão
        /// </summary>
        /// <typeparam name="T">Tipo da Interface a ser resolvida</typeparam>
        /// <returns>Objeto concreto criado</returns>
        public static T Resolve<T>()
        {
            try
            {
                var configSection = ((Hashtable)ConfigurationManager.GetSection(ImplementationsFileName ?? "Implementations"));
                var t = Type.GetType(configSection[GetFriendlyName(typeof(T))].ToString());
                var impl = (T)Activator.CreateInstance(t);
                return impl;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                return default(T);
            }
        }

        /// <summary>
        /// Resolve uma interface a partir do tipo T chamando um construtor com parâmetros previamente conhecido
        /// </summary>
        /// <typeparam name="T">Tipo da Interface a ser resolvida</typeparam>
        /// <param name="parameters">Lista de Parâmetros</param>
        /// <returns>Objeto concreto criado</returns>
        public static T Resolve<T>(object[] parameters)
        {
            try
            {
                var configSection = ((Hashtable)ConfigurationManager.GetSection(ImplementationsFileName ?? "Implementations"));
                var t = Type.GetType(configSection[GetFriendlyName(typeof(T))].ToString());
                var impl = (T)Activator.CreateInstance(t ?? throw new InvalidOperationException(), parameters);
                return impl;
            }
            catch
            {
                return default(T);
            }
        }

        private static string GetFriendlyName(Type type)
        {
            if (type == typeof(int))
            {
                return "int";
            }

            if (type == typeof(short))
            {
                return "short";
            }

            if (type == typeof(byte))
            {
                return "byte";
            }

            if (type == typeof(bool))
            {
                return "bool";
            }

            if (type == typeof(long))
            {
                return "long";
            }

            if (type == typeof(float))
            {
                return "float";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(decimal))
            {
                return "decimal";
            }

            if (type == typeof(string))
            {
                return "string";
            }

            if (type.IsGenericType)
            {
                return type.Name.Split('`')[0] + "[" + string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName).ToArray()) + "]";
            }

            return type.Name;
        }

        /// <summary>
        /// Resolve uma interface a partir de uma chave alfanumérica chamando um construtor com parâmetros previamente conhecido
        /// </summary>
        /// <param name="key">Chave Alfanumérica</param>
        /// <param name="parameters">Lista de Parâmetros</param>
        /// <returns>Objeto concreto criado</returns>
        public static object Resolve(string key, object[] parameters)
        {
            try
            {
                var configSection = ((Hashtable)ConfigurationManager.GetSection(ImplementationsFileName ?? "Implementations"));
                var t = Type.GetType(configSection[key].ToString());
                object impl = null;
                impl = parameters == null
                    ? Activator.CreateInstance(t ?? throw new InvalidOperationException())
                    : Activator.CreateInstance(t ?? throw new InvalidOperationException(), parameters);
                return impl;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resolve uma interface a partir de uma chave alfanumérica chamando o construtor padrão
        /// </summary>
        /// <param name="key">Chave Alfanumérica</param>
        /// <returns>Objeto concreto criado</returns>
        public static object Resolve(string key)
        {
            return Resolve(key, null);
        }

        /// <summary>
        /// Resolve uma interface a partir de uma chave alfanumérica chamando o construtor padrão
        /// </summary>
        /// <typeparam name="T">Tipo de Retorno</typeparam>
        /// <param name="key">Chave Alfanumérica</param>
        /// <returns>Objeto concreto criado fortemente tipado</returns>
        public static T Resolve<T>(string key)
        {
            return (T)Resolve(key, null);
        }

        /// <summary>
        /// Resolve uma interface a partir de uma chave alfanumérica chamando um construtor com parâmetros previamente conhecido
        /// </summary>
        /// <typeparam name="T">Tipo de Retorno</typeparam>
        /// <param name="key">Chave Alfanumérica</param>
        /// <param name="parameters">Lista de Parâmetros</param>
        /// <returns>Objeto concreto criado fortemente tipado</returns>
        public static T Resolve<T>(string key, object [] parameters)
        {
            return (T)Resolve(key, parameters);
        }

        public static List<T> GetPlugins<T>(string path = null) where T: class, new()
        {
            var plugins = new List<T>();
            var requiredTypes = new[] {typeof(T), typeof(IPlugin)};
            path = path ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var di = new DirectoryInfo(path ?? throw new ArgumentNullException(nameof(path)));

            foreach (var file in di.EnumerateFiles("*.dll"))
            {
                var plugin = Assembly.LoadFrom(file.FullName);
                var types = plugin.GetTypes();
                plugins.AddRange(types.Where(t => requiredTypes.Intersect(t.GetInterfaces()).Count() == 2).Select(t => (T) Activator.CreateInstance(t)));
            }

            return plugins;
        } 
    }
}
