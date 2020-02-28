﻿#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace MayaLauncher
{
    /// <summary>
    /// A very simple Named Pipe Server implementation that makes it 
    /// easy to pass string messages between two applications.
    /// </summary>
    public class NamedPipeManager 
    {
        public string NamedPipeName = Guid.NewGuid().ToString();
        public event Action<string> ReceiveString;

        private const string EXIT_STRING = "__EXIT__";
        private bool _isRunning = false;
        public Thread PipeServerThread;
        
        public NamedPipeManager(string name)
        {
            NamedPipeName = name;
        }
            
        /// <summary>
        /// Starts a new Pipe server on a new thread
        /// </summary>
        public void StartServer()
        {
            PipeServerThread = new Thread(StartPipeServerThread)
            {
                Name = "PipeManager_" + "1", // StringUtils.NewStringId(),
                IsBackground = true
            };
           
            PipeServerThread.Start(NamedPipeName);
        }

        private void StartPipeServerThread(object pipeName)
        {
            _isRunning = true;

            while (true)
            {
                string text;
                using (var server = new NamedPipeServerStream(pipeName as string))
                {
                    server.WaitForConnection();

                    using (StreamReader reader = new StreamReader(server))
                    {
                        text = reader.ReadToEnd();
                    }
                }

                if (text == EXIT_STRING) break;

                OnReceiveString(text);

                if (_isRunning == false) break;
            }
        }

        /// <summary>
        /// Called when data is received.
        /// </summary>
        /// <param name="text"></param>
        protected virtual void OnReceiveString(string text) => ReceiveString?.Invoke(text);
        

        /// <summary>
        /// Shuts down the pipe server...
        ///
        /// Called from a different thread but writes a message
        /// to the pipe to shut itself down.
        /// </summary>
        public void StopServer()
        {
            _isRunning = false;
            Write(EXIT_STRING);            
        }

        public void WaitForThreadShutDown(int ms)
        {
            if (PipeServerThread != null && PipeServerThread.IsAlive)
            {
                for (int i = 0; i < ms/100; i++)
                {
                    if (!PipeServerThread.IsAlive)
                        break;

                    Thread.Sleep(100);
                }

                if (PipeServerThread.IsAlive)
                {
                    //mmApp.LogLocal("PipeManagerThread failed to shut down.");
                    PipeServerThread.Abort();
                    Thread.Sleep(100);
                    //mmApp.LogLocal("PipeManagerThread failed. Abort done.");
                }
            }
        }

        /// <summary>
        /// Write a client message to the pipe
        /// </summary>
        /// <param name="text"></param>
        /// <param name="connectTimeout"></param>
        public bool Write(string text, int connectTimeout = 300)
        {
            using (var client = new NamedPipeClientStream(NamedPipeName))
            {
                try
                {
                    client.Connect(connectTimeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                    return false;

                using (StreamWriter writer = new StreamWriter(client))
                {
                    writer.Write(text);
                    writer.Flush();
                }
            }

            return true;
        }

    }
}
