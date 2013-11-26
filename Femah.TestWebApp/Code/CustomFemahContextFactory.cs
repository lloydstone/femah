using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Femah.Core;

namespace Femah.TestWebApp.Code
{
    public class CustomFemahContextFactory : IFemahContextFactory
    {
        public IFemahContext GenerateContext()
        {
            return new CustomFemahContext(new HttpContextWrapper(HttpContext.Current)) { SomeCustomAttribute = 3 };
        }
    }
}