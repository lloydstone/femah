using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Femah.Core;

namespace Femah.TestWebApp.Code
{
    public class CustomFemahContext : FemahContext
    {
        public CustomFemahContext(HttpContextBase context)
            : base(context)
        {
        }
        public int SomeCustomAttribute { get; set; }
    }
}