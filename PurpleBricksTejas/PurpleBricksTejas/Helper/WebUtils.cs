using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Reflection;
using System.Web.Mvc;

namespace SNS.CodeLibrary.SharedCode
{
    /// <summary>
    /// Utils class, contains web specific utility methods
    /// </summary>
    public class WebUtils
    {

        /// <summary>
        /// Checks for leading '[' or '{' to determine if given string is JSON 
        /// </summary>
        public static bool IsJSON(string Text)
        {
            if(string.IsNullOrWhiteSpace(Text))
                return false;

            return Text.Trim().Substring(0, 1).IndexOfAny(new char[] { '[', '{' }) > -1;
        }

        /// <summary>
        /// Removes leading closing braces inserted by infrastructure as part of JSON hijacking protection.
        /// Returns valid JSON if it exists, else returns null.
        /// </summary>
        public static string CheckGetProtectedJSON(string Text)
        {
            if(string.IsNullOrWhiteSpace(Text))
                return null;

            int index = Text.IndexOf("[");
            if(index == -1)
                index = Text.IndexOf("{");

            if(index == -1)
                return null;

            string text = Text.Substring(index);
            if(!IsJSON(text))
                return null;

            return text;
        }

        /// <summary>
        /// Returns current Url not including page, without trailing "/".
        /// </summary>
        /// <param name="IncludeAppDirectory">Whether or not to include the current url's application path. False would return base url.</param>
        public static string GetCurrentUrl(bool IncludeAppDirectory)
        {
            string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            if(baseUrl.EndsWith("/"))
                baseUrl = baseUrl.Remove(baseUrl.Length - 1, 1);

            string urlAppPath = HttpContext.Current.Request.ApplicationPath;
            if(urlAppPath.EndsWith("/"))
                urlAppPath = urlAppPath.Remove(urlAppPath.Length - 1, 1);

            if(IncludeAppDirectory)
                return baseUrl + "/" + urlAppPath;
            else
                return baseUrl;
        }

        /// <summary>
        /// Returns the page Title text, useful for HTML error message extraction
        /// </summary>
        public static string ExtractHtmlTitleText(string HtmlText)
        {
            if(String.IsNullOrWhiteSpace(HtmlText))
                return string.Empty;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("/html[1]/title[1]");
            if(titleNode == null)
                titleNode = doc.DocumentNode.SelectSingleNode("/html[1]/head[1]/title[1]");

            string title = string.Empty;
            if(titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
                title = titleNode.InnerText;

            return title;
        }

        /// <summary>
        /// Concatenates and returns the title and body 
        /// </summary>
        public static string ExtractHtmlTitleBodyText(string HtmlText)
        {
            if(String.IsNullOrWhiteSpace(HtmlText))
                return string.Empty;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            string title = string.Empty;
            string body = string.Empty;

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("/html[1]/title[1]");
            if(titleNode == null)
                titleNode = doc.DocumentNode.SelectSingleNode("/html[1]/head[1]/title[1]");

            if(titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
                title = titleNode.InnerText;

            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html[1]/body[1]");
            if(bodyNode != null && !string.IsNullOrWhiteSpace(bodyNode.InnerText))
                body = bodyNode.InnerText;

            return string.Format("{0}<p>{1}</p>", title, body);
        }

        /// <summary>
        /// Returns web response even if exception thrown e.g. 304 (not modified) http status causes exception. Works around this DotNet quirk.
        /// Throws exception if response is null, or if ThrowHttpError is TRUE and status code is 4xx or 5xx.
        /// </summary>
        public static HttpWebResponse GetHttpWebResponseWithoutException(HttpWebRequest Request, bool ThrowHttpError = false)
        {
            if(Request == null)
                throw new ArgumentNullException("Request");

            HttpWebResponse response;
            try
            {
                response = Request.GetResponse() as HttpWebResponse;
                if(response == null)
                    throw new ApplicationException("Null response from remote server.");

                return response;
            }
            catch(WebException wex)
            {
                if(wex.Status == WebExceptionStatus.ProtocolError)
                {
                    response = wex.Response as HttpWebResponse;
                    if(response == null)
                        throw new ApplicationException("Null response with error from remote server.");

                    int statusCode = (int)response.StatusCode;
                    if(ThrowHttpError && statusCode >= 400 && statusCode < 600)
                        throw wex;

                    return response;
                }
                else
                {
                    throw wex;
                }
            }
        }

        public static string NameValueToString(NameValueCollection coll)
        {
            string res = "";
            foreach (string key in coll.AllKeys)
            {
                if (String.IsNullOrWhiteSpace(key)) continue;
                if (res.Length != 0)
                {
                    res += "&";
                }
                res += Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(coll[key]);
            }
            return res;
        }

        public static WebRequest BuildPostRequest(string url, NameValueCollection queryParams, int? timeout = null)
        {
            WebRequest wr = WebRequest.Create(url);
            HttpWebRequest wr2 = wr as HttpWebRequest;
            if (wr2 != null && timeout != null)
            {
                wr2.Timeout = timeout.Value; 
                wr2.ReadWriteTimeout = timeout.Value;
            }
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";

            if (queryParams != null && queryParams.Count > 0)
            {
                Stream rs = wr.GetRequestStream();
                string data = NameValueToString(queryParams);
                if (!String.IsNullOrWhiteSpace(data))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    rs.Write(bytes, 0, bytes.Length);
                }
            }
            return wr;
        }

        public static string GetRootFolder()
        {
            string res = "";
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath("~");
            }
            else
            {
                res = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                if (File.Exists(res))
                {
                    res = Path.GetDirectoryName(res);
                }
                if (res != null && res.ToLower().EndsWith("\\bin"))
                {
                    res = res.Substring(0, res.Length - 4);
                }
            }
            return res;
        }

        /// <summary>
        /// Returns string list of model errors in passed in model state.
        /// </summary>
        public static List<string> GetModelStateErrors(ModelStateDictionary AppModelState)
        {
            List<string> modelErrorList = new List<string>();
            string errorMsg;
            foreach(ModelState modelState in AppModelState.Values)
            {
                foreach(ModelError error in modelState.Errors)
                {
                    errorMsg = error.ErrorMessage;
                    if(string.IsNullOrWhiteSpace(errorMsg) && error.Exception != null)
                        errorMsg = error.Exception.InnerException != null ? error.Exception.InnerException.Message : error.Exception.Message;

                    modelErrorList.Add(errorMsg);
                }
            }

            return modelErrorList;
        }

        /// <summary>
        /// Returns string list of model errors in passed in model as HTML unordered list.
        /// </summary>
        public static string GetModelStateErrorsHTML(ModelStateDictionary AppModelState)
        {
            string ul = "<ul>";
            List<string> modelErrorList = GetModelStateErrors(AppModelState);
            foreach(string error in modelErrorList)
                ul += "<li>" + error + "</li>";

            ul += "</ul>";

            return ul;
        }

        public static IEnumerable<SelectListItem> TriStateSelectList(bool? value, string yesText = null, string noText = null, string nullText = null)
        {
            List<SelectListItem> res = new List<SelectListItem>();
            res.Add(new SelectListItem()
            {
                Value = "",
                Text = nullText ?? "",
                Selected = (value == null)
            });
            res.Add(new SelectListItem()
            {
                Value = "Y",
                Text = yesText ?? "Yes",
                Selected = (value == true)
            });
            res.Add(new SelectListItem()
            {
                Value = "N",
                Text = noText ?? "No",
                Selected = (value == false)
            });
            return res;
        }

        /// <summary>
        /// Returns the passed in razor View as a html string
        /// </summary>
        public static string RenderMvcViewToString(ControllerContext Context,
                                                    string ViewPath,
                                                    object Model = null,
                                                    bool Partial = false,
                                                    string MasterViewName = null)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;


            if(Partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(Context, ViewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(Context, ViewPath, MasterViewName);

            if(viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            IView view = viewEngineResult.View;
            Context.Controller.ViewData.Model = Model;

            string result = null;

            using(StringWriter sw = new StringWriter())
            {
                ViewContext ctx = new ViewContext(Context, 
                                            view,
                                            Context.Controller.ViewData,
                                            Context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        public static string ConvertPartialViewToString(Controller controller, PartialViewResult partialView)
        {
            using (var sw = new StringWriter())
            {
                partialView.View = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialView.ViewName).View;

                var vc = new ViewContext(controller.ControllerContext, partialView.View, partialView.ViewData, partialView.TempData, sw);
                partialView.View.Render(vc, sw);

                var partialViewString = sw.GetStringBuilder().ToString();

                return partialViewString;

            }
        }

    }//end class

} //end name

