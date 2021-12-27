using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;
        string contenttype = "text/html";
        public Server(int portNumber, string redirectionFilePath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            this.LoadRedirectionRules(redirectionFilePath);
            //TODO: initialize this.serverSocket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, portNumber));
        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(100);
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket clientSocket = this.serverSocket.Accept();
                Console.WriteLine("New Client accepted ; {0}", clientSocket.RemoteEndPoint);
                Thread newthread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newthread.Start(clientSocket);

            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            Socket clientSock = (Socket)obj;

            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            clientSock.ReceiveTimeout = 0;

            // TODO: receive requests in while true until remote client closes the socket.
            int receivedLength;
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] data = new byte[1024];
                    receivedLength = clientSock.Receive(data);

                    string req = Encoding.ASCII.GetString(data);



                    // todo: break the while loop if receivedlen==0
                    if (receivedLength == 0)
                    {
                        Console.WriteLine("client: {0} ended the connection", clientSock.RemoteEndPoint);
                        break;
                    }
                    // TODO: Create a Request object using received request string
                    Request request = new Request(req);


                    Console.WriteLine("client: {0} sent request: {1}", req);
                    // TODO: Call HandleRequest Method that returns the response
                    Response response = HandleRequest(request);

                    // TODO: Send Response back to client
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response.ResponseString);
                    clientSock.Send(responseBytes);

                }
                catch (Exception ex)
                {
                    // TODO: log exception using Logger class
                    Logger.LogException(ex);
                }
                finally
                {
                    // TODO: close client socket
                    clientSock.Close();
                }

            }


        }

        Response HandleRequest(Request request)
        {
            //throw new NotImplementedException();
            string content = "";
            try
            {
                //TODO: check for bad request 
                if (!request.ParseRequest())
                {
                    content = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                    return new Response(StatusCode.BadRequest, contenttype, content, null);
                }


                //TODO: map the relativeURI in request to get the physical path of the resource.
                //TODO: check for redirect
                if (Configuration.RedirectionRules.ContainsKey(request.relativeURI))
                {
                    content = LoadDefaultPage(Configuration.RedirectionDefaultPageName);
                    return new Response(StatusCode.Redirect, contenttype, content, Configuration.RedirectionRules[request.relativeURI]);
                }
                //TODO: check file exists
                if (request.relativeURI == string.Empty)
                {
                    if (File.Exists(Path.Combine(Configuration.RootPath, Configuration.MainPage)))
                    {
                        content = LoadDefaultPage(Configuration.MainPage);
                        return new Response(StatusCode.OK, contenttype, content, null);
                    }
                }
                //TODO: read the physical file
                // Create OK response
                else if (request.relativeURI != string.Empty)
                {
                    if (File.Exists(Path.Combine(Configuration.RootPath, request.relativeURI)))
                    {
                        content = LoadDefaultPage(request.relativeURI);
                        return new Response(StatusCode.OK, contenttype, content, null);
                    }
                    else
                    {
                        content = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                        return new Response(StatusCode.NotFound, contenttype, content, null);
                    }

                }

            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                // TODO: in case of exception, return Internal Server Error. 
                //content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                return new Response(StatusCode.InternalServerError, contenttype, content, null);
            }

            return new Response(StatusCode.InternalServerError, contenttype, content, null);
        }

        // private string GetRedirectionPagePathIFExist(string relativePath)
        // {
        //     // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
        //     if (File.Exists(Configuration.RootPath + "/" + Configuration.RedirectionRules[relativePath]))
        //         return Configuration.RedirectionRules[relativePath];
        //     else
        //         return string.Empty;
        // }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            if (!File.Exists(filePath))
            {
                Logger.LogException(new FileNotFoundException("cannot find the file", filePath));
                return string.Empty;
            }
            // else read file and return its content
            return File.ReadAllText(filePath);

        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                //  using the filepath parametr read the redirection rules from file 
                // and add them to Configuration.RedirectionRules
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        Configuration.RedirectionRules.Add(parts[0], parts[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
