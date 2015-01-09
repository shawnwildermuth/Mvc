// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ActionConstraintsWebSite
{
    [Consumes("application/xml")]
    public class ConsumesAttribute_OverridesController : ConsumesAttribute_OverridesBaseController
    {
        public override IActionResult CreateDummy([FromBody] Dummy dummy)
        {
            // should be picked if request content type is text/json.
            dummy.SampleString = "ConsumesAttribute_OverridesController_application/xml";
            return new JsonResult(dummy);
        }
    }
}