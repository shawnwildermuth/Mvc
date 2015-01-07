using System;
using Microsoft.AspNet.Mvc;

namespace LoggingWebSite
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DummyActionFilterAttribute : ActionFilterAttribute
    {
        
    }
}