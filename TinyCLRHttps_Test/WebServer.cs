using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace TinyCLRHttps_Test
{
  internal class WebServer
  {
    static X509Certificate certificate = null;
    Thread web_thread;
    public bool DebugLog = true;

    /// <summary>
    /// Constructor of WebServer
    /// </summary>
    /// <param name="certificate">certificate needed when using HTTPS webserver</param>
    public WebServer(X509Certificate cert)
    {
      certificate = cert;
    }

    public void Start()
    {
      web_thread = new Thread(WebThread);
      web_thread.Start();
    }

    public void Restart()
    {
      web_thread.Abort();
      web_thread = new Thread(WebThread);
      web_thread.Start();
    }

    void WebThread()
    {
      HttpListener listener = new HttpListener("https");
      //HttpListener listener = new HttpListener("http"); // that works fine
      try
      {
        listener.HttpsCert = certificate;
        listener.Start();
        DebugWrite("Startet listening . . . ");

        while (true)
        {
          // Get Context, Request / Response
          HttpListenerContext context;
          try
          {
            context = listener.GetContext(); // Never finishes that
          }
          catch (Exception e)
          {
            DebugWrite(e.ToString()); // Never gets here
            continue;
          }

          HttpListenerRequest req = context.Request;
          DebugWrite("Request Method : " + req.HttpMethod);

          switch (req.HttpMethod)
          {
            case "GET":
              getHandler(context, req);
              continue;
            case "POST":
              //response = postHandler(context, req);
              break;
            default:
              break;
          }

          HttpListenerResponse response = context.Response;
          response.Close();
          GC.Collect();
        }
      }
      catch (Exception ex)
      {
        DebugWrite("WebServer Crashed");
        listener.Stop();
        DebugWrite(ex.ToString());
      }
      DebugWrite("WebServer stopped");
      return;
    }

    private void getHandler(HttpListenerContext context, HttpListenerRequest req)
    {
      HttpListenerResponse response = context.Response;
      DebugWrite("Response to GET");
      byte[] buffer;
      Stream output = response.OutputStream;
      buffer = Encoding.UTF8.GetBytes("<!doctype html>\r\n<html lang=\"en\">\r\n<body>Hello World</body>\r\n</html>\r\n");
      response.ContentLength64 = buffer.Length;
      output.Write(buffer, 0, buffer.Length);
      response.Close();

      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    private void DebugWrite(string str)
    {
      if (DebugLog)
        Debug.WriteLine(DateTime.Now + " WebServer : " + str);
    }

    public ThreadState State()
    {
      return web_thread.ThreadState;
    }

    public bool isAlive()
    {
      return web_thread.IsAlive;
    }
  }
}
