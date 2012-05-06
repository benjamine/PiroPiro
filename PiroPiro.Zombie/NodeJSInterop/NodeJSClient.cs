using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace PiroPiro.Zombie.NodeJSInterop
{
    public class NodeJSClient : IDisposable
    {
        public event EventHandler SessionReset;

        private string _SessionID;

        public string SessionID
        {
            get
            {
                if (_SessionID == null)
                {
                    EnsureConnected();
                    _SessionID = Send(new { createSession = true }).ToString();
                }
                return _SessionID;
            }
        }

        public string HostName { get; set; }

        public int Port { get; set; }

        private TcpClient TcpClient;
        private Stream Stream;
        private StreamWriter Writer;
        private StreamReader Reader;

        public class ServerErrorException : Exception
        {
            public ServerErrorException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// Represents a pointer to variable on the server
        /// </summary>
        public class Variable
        {
            public NodeJSClient Client { get; private set; }

            public string Name { get; private set; }

            private static int InstanceCount = 0;

            private string ID;

            private bool _Discarded;

            /// <summary>
            /// Creates a new pointer for a variable on the server
            /// </summary>
            /// <param name="name">optional name</param>
            /// <param name="id">optional id, used for vars created on server, otherwise use null to use autoincrement</param>
            /// <param name="initValue">a javascript expression to initialize this variable value on the server</param>
            internal Variable(NodeJSClient client, string id = null, string name = null, string initExpresion = null)
            {
                InstanceCount++;
                ID = id ?? "_" + InstanceCount;
                Name = name;
                Client = client;
                Client.SessionReset += new EventHandler(Client_SessionReset);
                if (!string.IsNullOrEmpty(initExpresion))
                {
                    client.Execute<bool>(this + " = (" + initExpresion + "); return true;");
                }
            }

            void Client_SessionReset(object sender, EventArgs e)
            {
                // session was lost, this variable cannot be used anymore
                _Discarded = true;
                Client.SessionReset -= new EventHandler(Client_SessionReset);
            }

            public override string ToString()
            {
                if (_Discarded)
                {
                    throw new Exception("Server session for this variable no longer exists: " + Name);
                }
                return "__s['" + Client.SessionID + "'].vars['" + ID + (Name != null ? "_" + Name : "") + "']";
            }
        }

        /// <summary>
        /// Creates a new pointer for a variable on the server
        /// </summary>
        /// <param name="name">optional name</param>
        /// <param name="initValue">a javascript expression to initialize this variable value on the server</param>
        /// <returns></returns>
        public Variable CreateVar(string name = "_", string initExpression = null)
        {
            return new Variable(this, null, name, initExpression);
        }

        /// <summary>
        /// Creates a new pointer for a variable on the server
        /// </summary>
        /// <param name="id">var id reference on the server</param>
        /// <param name="name">optional name</param>
        /// <returns></returns>
        public Variable CreateVarByID(string id, string name = null)
        {
            return new Variable(this, id, name);
        }

        /// <summary>
        /// Checks if this client session exists on server, if not found creates a new one
        /// </summary>
        /// <returns>true if a new session was created </returns>
        public bool CheckSession()
        {
            if (Execute<bool>("return typeof __s['" + SessionID + "'] == 'undefined';"))
            {
                // session lost, reset old id
                _SessionID = null;
                // create a new one
                return SessionID != null;
            }
            return false;
        }

        public NodeJSClient(string hostname, int port)
        {
            HostName = hostname;
            Port = port;
            TcpClient = new TcpClient();
        }

        private void EnsureConnected()
        {
            if (!TcpClient.Connected)
            {
                try
                {
                    TcpClient.Connect(HostName, Port);
                }
                catch
                {
                    // unable to reconnect, try using a new socket
                    try
                    {
                        TcpClient.Close();
                        TcpClient.Client.Close();
                        TcpClient.Client.Dispose();
                    }
                    catch
                    {
                    }
                    TcpClient = new TcpClient();
                    TcpClient.Connect(HostName, Port);

                    if (CheckSession())
                    {
                        // session was lost, all vars should be discarded
                        var handler = SessionReset;
                        if (handler != null)
                        {
                            handler(this, EventArgs.Empty);
                        }
                    }
                }
                Stream = TcpClient.GetStream();
                Reader = new StreamReader(Stream);
                Writer = new StreamWriter(Stream);
            }
        }

        private void Disconnect()
        {
            TcpClient.Close();
        }

        private object Send(object data)
        {
            EnsureConnected();

            var jsonData = JsonConvert.SerializeObject(data);
            Writer.Write(jsonData + "\n\n");// empty line to finish message
            Writer.Flush();

            // read until empty line
            string responseJson = "";
            string line = "-";
            while (!string.IsNullOrEmpty(line))
            {
                line = Reader.ReadLine();
                responseJson += line + "\n";
            }

            var response = JsonConvert.DeserializeXNode(responseJson, "Response").Element("Response");
            if (response.Element("error") != null)
            {
                throw new ServerErrorException(response.Element("error").Value);
            }
            object result = null;
            switch ((response.Element("resultType") + "").ToLowerInvariant())
            {
                case "null":
                    result = null;
                    break;
                case "number":
                    result = (decimal?)response.Element("result");
                    break;
                case "boolean":
                    result = (bool?)response.Element("result");
                    break;
                default:
                    result = (string)response.Element("result");
                    break;
            }
            return result;
        }

        /// <summary>
        /// Executes javascript code on the server and returns the result.
        /// a call to done() on the js code ends the request.
        /// </summary>
        /// <typeparam name="T">expected result type</typeparam>
        /// <param name="js">javascript code to run on the server, must call done(err, result)</param>
        /// <returns></returns>
        public T ExecuteAsync<T>(string js)
        {
            return Execute<T>(js, true);
        }

        /// <summary>
        /// Executes javascript code on the server and returns the result.
        /// </summary>
        /// <typeparam name="T">expected result type</typeparam>
        /// <param name="js">javascript code to run on the server</param>
        /// <param name="async">if true, call done(err, result) on server to end request</param>
        /// <returns></returns>
        public T Execute<T>(string js, bool async = false)
        {
            if (typeof(T) == typeof(Variable[]))
            {
                return (T)(Execute<string>(js).Split(',').Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => CreateVarByID(id)).ToArray() as object);
            }
            else if (typeof(T) == typeof(Variable))
            {
                return (T)(CreateVarByID(Execute<string>(js), "elem") as object);
            }
            else
            {
                return (T)Convert.ChangeType(Send(new
                {
                    sid = SessionID,
                    async = async ? 1 : 0,
                    code = js
                }), typeof(T));
            }
        }

        /// <summary>
        /// Executes javascript code on the server.
        /// a call to done() on the js code ends the request.
        /// </summary>
        /// <param name="js">javascript code to run on the server, must call done(err, result)</param>
        /// <returns></returns>
        public void ExecuteAsync(string js)
        {
            Execute(js, true);
        }

        /// <summary>
        /// Executes javascript code on the server.
        /// </summary>
        /// <param name="js">javascript code to run on the server</param>
        /// <param name="async">if true, call done(err, result) on server to end request</param>
        /// <returns></returns>
        public void Execute(string js, bool async = false)
        {
            Execute<string>(js, async);
        }

        public void Dispose()
        {
            if (TcpClient.Connected)
            {
                TcpClient.Close();
            }
            if (Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
            if (Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }
        }

        /// <summary>
        /// Escapes a value to be used inside a javascript block string representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Escape(string value)
        {
            return value
                .Replace("\t", "\\t")
                .Replace("\"", "\\\"")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        /// <summary>
        /// Escapes a value to be used inside a javascript block string representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Escape(bool value)
        {
            return value ? "true" : "false";
        }

        /// <summary>
        /// Escapes a value to be used inside a javascript block string representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Escape(decimal value)
        {
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
