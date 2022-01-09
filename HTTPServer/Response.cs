using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }
        StatusCode statusCode;
        List<string> headerLines = new List<string>();
        public Response(StatusCode code, string contentType, string redirectionPath, string content = "")
        {
            this.statusCode = code;

            //Add headlines (Content-Type, Content-Length,Date, [location if there is redirection])
            headerLines.Add("Content-Type:" + contentType);
            headerLines.Add("Content-Length:" + content.Length);
            headerLines.Add("Date:" + DateTime.Now.ToString());
            
            if (redirectionPath != null)
                headerLines.Add("Location:" + redirectionPath);

            //Add the status code
            responseString = GetStatusLine(code);

            //Add the header lines
            foreach (string line in headerLines)
            {
                responseString += line + "\r\n";
            }

            //Add the content
            responseString += "\r\n";
            responseString += content;

        }

        private string GetStatusLine(StatusCode code)
        {

            // TODO: Create the response status line and return it
            string statusLine = string.Format("{0} {1} {2}\r\n", "", Configuration.ServerHTTPVersion, (int)code, code.ToString());

            return statusLine;
        }
    }
}
