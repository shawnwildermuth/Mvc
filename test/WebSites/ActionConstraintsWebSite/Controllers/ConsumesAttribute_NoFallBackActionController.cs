// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ActionConstraintsWebSite
{
    [Route("ConsumesAttribute_AmbiguousActions/[action]")]
    public class ConsumesAttribute_NoFallBackActionController : Controller
    {
        [Consumes("application/json", "text/json")]
        public Dummy CreateDummy([FromBody] DummyClass_Json jsonInput)
        {
            return jsonInput;
        }

        [Consumes("application/xml")]
        public Dummy CreateDummy([FromBody] DummyClass_Xml xmlInput)
        {
            return xmlInput;
        }
    }
}