using Arc4u.Diagnostics;
using System;
using System.Collections.Generic;
using System.Web;

namespace Arc4u.Windows.Extension
{
    public static class UriExtensions
    {
        public static Dictionary<String, Object> ExtractParametersFromQuery(this Uri uri)
        {
            var dict = new Dictionary<String, Object>();
            try
            {
                var nvc = HttpUtility.ParseQueryString(uri.Query);
                foreach (var k in nvc.AllKeys)
                {
                    dict.Add(k, nvc[k]);
                }
            }
            catch (Exception ex)
            {
                Logger.Technical.From(typeof(UriExtensions)).Exception(ex).Log();
            }

            return dict;
        }
    }
}
