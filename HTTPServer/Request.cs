using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string[] requestLines;
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        HTTPVersion httpVersion;
        string requestString;
        string[] contentLines;

        public Request(string requestString)
        {
            this.requestString = requestString;
        }
        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            throw new NotImplementedException();

            //TODO: parse the receivedRequest using the \r\n delimeter  

            // check that there is atleast 3 lines: Request line, Host Header, Blank line (usually 4 lines with the last empty line for empty content)

            // Parse Request line

            // Validate blank line exists

            // Load header lines into HeaderLines dictionary
            if (ParseRequestLine() || LoadHeaderLines() || ValidateBlankLine())
            {
                return true;
            }
            else
                return false;
        }

        private bool ParseRequestLine()
        {
            contentLines = requestString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (contentLines.Length < 4)
                return false;
            else
            {
                 requestLines = contentLines[0].Split(' ');
                if (requestLines.Length != 3) return false;
                switch (requestLines[0])
                {
                    case "GET":
                        method = RequestMethod.GET;
                        break;
                    case "POST":
                        method = RequestMethod.POST;
                        break;
                    default:
                        method = RequestMethod.HEAD;
                        break;
                }
                if (!ValidateIsURI(requestLines[1]))
                    return false;
                    relativeURI = requestLines[1].Remove(0, 1);
                switch (requestLines[2])
                {
                    case "HTTP/1.1":
                        httpVersion = HTTPVersion.HTTP11;
                        break;
                    case "HTTP/1.0":
                        httpVersion = HTTPVersion.HTTP10;
                        break;
                    default:
                        httpVersion = HTTPVersion.HTTP09;
                        break;
                }
            }
            return true;

           // throw new NotImplementedException();
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines()
        {
            string[] array = new string[] { ":" };
            headerLines = new Dictionary<string, string>();
            for(int i=1;i<contentLines[i].Length;i++)
            {
                string[] array2= contentLines[i].Split(array,StringSplitOptions.RemoveEmptyEntries);
                headerLines.Add(array2[0], array2[1]);

            }
            return array.Length > 1;
            //throw new NotImplementedException();
        }

        private bool ValidateBlankLine()
        {
            return requestString.EndsWith("\r\n\r\n");
            //throw new NotImplementedException();
        }

    }
}
