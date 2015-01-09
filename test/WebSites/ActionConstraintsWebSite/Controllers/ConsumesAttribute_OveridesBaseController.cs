// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ActionConstraintsWebSite
{
    [Consumes("application/json")]
    public class ConsumesAttribute_OverridesBaseController : Controller
    {
        [Consumes("text/json")]
        public Dummy CreateDummy([FromBody] DummyClass_Json dummy)
        {
            // should be picked if request content type is application/xml and not application/json.
            dummy.SampleString = "ConsumesAttribute_OverridesBaseController_text/json";
            return dummy;
        }

        public virtual IActionResult CreateDummy([FromBody] Dummy dummy)
        {
            // should be picked if request content type is application/json.
            dummy.SampleString = "ConsumesAttribute_OverridesBaseController_application/json";
            return new ObjectResult(dummy);
        }
    }
}