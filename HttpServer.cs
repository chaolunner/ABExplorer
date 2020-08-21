using ABExplorer.Extensions;
using ABExplorer.Utilities;
using System.Threading;
using UnityEngine;
using System.Net;
using System.IO;
using System;

namespace ABExplorer
{
    public class HttpServer : IDisposable
    {
        private string _platformName = "";
        private string _abOutPath = "";
        private HttpListener _httpListener;
        private Thread _listenerThread;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var settings = AbExplorerSettings.Instance;
            if (settings.playMode == PlayMode.PackedPlayMode)
            {
                var server = new HttpServer();
                server.Start();
            }
        }
#endif

        private void Start()
        {
            _platformName = PathUtility.GetPlatformName();
            _abOutPath = PathUtility.GetAbOutPath();
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(AbExplorerSettings.Instance.URL);
            _httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _httpListener.Start();

            _listenerThread = new Thread(HttpThread);
            _listenerThread.Start();
        }

        public void Dispose()
        {
            if (_httpListener != null)
            {
                _httpListener.Stop();
                _httpListener = null;
            }

            if (_listenerThread != null)
            {
                _listenerThread.Abort();
                _listenerThread = null;
            }
        }

        private void HttpThread()
        {
            while (_listenerThread.IsAlive)
            {
                var result = _httpListener.BeginGetContext(HttpCallback, _httpListener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        private void HttpCallback(IAsyncResult result)
        {
            var context = _httpListener.EndGetContext(result);

            if (context.Request.QueryString.AllKeys.Length > 0)
            {
                for (int i = 0; i < context.Request.QueryString.AllKeys.Length; i++)
                {
#if UNITY_EDITOR
                    for (int j = 0; j < context.Request.QueryString.AllKeys[i].Length; j++)
                    {
                        Debug.Log(
                            $"Key: {context.Request.QueryString.AllKeys[i]}, Value: {context.Request.QueryString.AllKeys[i][j]}");
                    }
#endif
                }
            }

            if (context.Request.HttpMethod == "GET")
            {
                if (context.Request.Url.LocalPath.StartsWith("//" + _platformName))
                {
                    var startIndex = _platformName.Length + 2;
                    var localPath = context.Request.Url.LocalPath.Substring(startIndex);
                    context.Response.WriteFile(_abOutPath + localPath);
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                var data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
#if UNITY_EDITOR
                Debug.Log(data);
#endif
            }
            else if (context.Request.HttpMethod == "HEAD")
            {
            }

            context.Response.Close();
        }
    }
}