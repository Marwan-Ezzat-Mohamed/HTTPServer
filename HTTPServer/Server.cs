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
            //call this.LoadRedirectionRules passing redirectionMatrixPath to it
            this.LoadRedirectionRules(redirectionFilePath);
            //initialize this.serverSocket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, portNumber));
        }

        public void StartServer()
        {
            // Listen to connections, with large backlog.
            serverSocket.Listen(1000);
            // Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //accept connections and start thread for each accepted connection.
                Socket clientSocket = this.serverSocket.Accept();
                Console.WriteLine("New Client accepted ; {0}", clientSocket.RemoteEndPoint);
                Thread newthread = new Thread(new ParameterizedThreadStart(HandleConnection));
                newthread.Start(clientSocket);

            }
        }

        public void HandleConnection(object obj)
        {
            // Create client socket 
            Socket clientSock = (Socket)obj;

            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            clientSock.ReceiveTimeout = 0;
            // receive requests in while true until remote client closes the socket.


            while (true)
            {
                try
                {
                    // Receive request
                    byte[] data = new byte[1024];
                    int receivedLength = clientSock.Receive(data);

                    // break the while loop if receivedlen==0
                    if (receivedLength == 0)
                    {
                        Console.WriteLine("client: {0} ended the connection", clientSock.RemoteEndPoint);
                        break;
                    }

                    string req = Encoding.ASCII.GetString(data);
                    // Create a Request object using received request string
                    Request request = new Request(req);

                    // Call HandleRequest Method that returns the response
                    Response response = HandleRequest(request);


                    Console.WriteLine("req {0}" + req);

                    //Console.WriteLine("Received: {0} from Client: {1}" + response.ResponseString);

                    // Send Response back to client
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response.ResponseString);
                    clientSock.Send(responseBytes);


                    //clientSock.Send(responseBytes, 0, receivedLength, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            // close client socket
            clientSock.Close();

        }

        Response HandleRequest(Request request)
        {

            string content = string.Empty;
            try
            {
                //check for bad request 
                if (!request.ParseRequest())
                {
                    content = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                    return new Response(StatusCode.BadRequest, contenttype, content, string.Empty);
                }

                switch (request.method.ToString())
                {
                    case "GET":
                        return handleGetMethod(request);
                    case "POST":
                        return handlePostMethod(request);
                    case "HEAD":
                        return handleHeadMethod(request);
                    default:
                        return new Response(StatusCode.InternalServerError, contenttype, null, content); ;
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // in case of exception, return Internal Server Error. 
                return new Response(StatusCode.InternalServerError, contenttype, null, content);
            }

        }

        public Response handlePostMethod(Request request)
        {
            string postURL = "";
            string content = string.Empty;
            try
            {
                if (request.relativeURI.Equals(postURL))
                {
                    content = request.body;
                    writeToRequestsFile(content);
                    return new Response(StatusCode.OK, contenttype, null, content);
                }
                else
                {
                    return new Response(StatusCode.NotFound, contenttype, null, content);
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // in case of exception, return Internal Server Error. 
                content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                return new Response(StatusCode.InternalServerError, contenttype, null, content);
            }

        }

        public Response handleHeadMethod(Request request)
        {

            try
            {
                string redirectionPagePath = GetRedirectionPagePathIFExist(request.relativeURI);
                if (redirectionPagePath != string.Empty)
                {
                    return new Response(StatusCode.Redirect, "text/html", redirectionPagePath, LoadDefaultPage(Configuration.RedirectionDefaultPageName));
                }

                if (request.relativeURI == string.Empty)
                {
                    return new Response(StatusCode.OK, contenttype, null, string.Empty);
                }
                //read the physical file
                // Create OK response
                else
                {
                    string requestedFilePath = Path.Combine(Configuration.RootPath, request.relativeURI);
                    if (File.Exists(requestedFilePath))
                    {
                        return new Response(StatusCode.OK, contenttype, null, string.Empty);
                    }
                    else
                    {
                        return new Response(StatusCode.NotFound, contenttype, null, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // in case of exception, return Internal Server Error.
                return new Response(StatusCode.InternalServerError, contenttype, null, string.Empty);
            }

        }
        public Response handleGetMethod(Request request)
        {
            string content = string.Empty;
            try
            {
                //throw new Exception("123");
                string RedirectionPagePath = GetRedirectionPagePathIFExist(request.relativeURI);
                if (RedirectionPagePath != string.Empty)
                {
                    return new Response(StatusCode.Redirect, "text/html", RedirectionPagePath, LoadDefaultPage(Configuration.RedirectionDefaultPageName));
                }

                if (request.relativeURI == string.Empty)
                {
                    content = LoadDefaultPage(Configuration.MainPage);
                    return new Response(StatusCode.OK, contenttype, null, content);
                }
                else
                {
                    string requestedFilePath = Path.Combine(Configuration.RootPath, request.relativeURI);
                    if (File.Exists(requestedFilePath))
                    {
                        content = LoadDefaultPage(request.relativeURI);
                        return new Response(StatusCode.OK, contenttype, null, content);
                    }
                    else
                    {
                        content = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                        return new Response(StatusCode.NotFound, contenttype, null, content);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                return new Response(StatusCode.InternalServerError, contenttype, null, content);
            }

        }


        //utils
        private void writeToRequestsFile(string content)
        {
            //read the file
            string path = Configuration.RootPath + "/" + Configuration.RequestBodyFileName;
            string file = File.ReadAllText(path);
            //append the content to the html file
            file = file.Replace("<body>", "<body>" + "<h1>" + content + "</h1>" + "<br>");
            //write the file
            File.WriteAllText(path, file);
        }
        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            // check if filepath not exist log exception using Logger class and return empty string
            if (!File.Exists(filePath))
            {
                Logger.LogException(new FileNotFoundException("cannot find the file", filePath));
                return string.Empty;
            }
            // else read file and return its content
            return File.ReadAllText(filePath);
        }


        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty
            if (Configuration.RedirectionRules.ContainsKey(relativePath))
            {
                return Configuration.RedirectionRules[relativePath];
            }
            return string.Empty;
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
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}
