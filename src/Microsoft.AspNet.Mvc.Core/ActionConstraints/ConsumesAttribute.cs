// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Specifies the allowed content types which can be used to help in selecting the action based on request's
    /// content-type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ConsumesAttribute : Attribute, IResourceFilter, IConsumesActionConstraint
    {
        /// <summary>
        /// Creates a new instance of <see cref="ConsumesAttribute"/>.
        /// </summary>
        public ConsumesAttribute(string contentType, params string[] otherContentTypes)
        {
            ContentTypes = GetContentTypes(contentType, otherContentTypes);
        }

        public int Order { get; } = Int32.MaxValue - 10;

        public IList<MediaTypeHeaderValue> ContentTypes { get; set; }

        public void OnResourceExecuting([NotNull] ResourceExecutingContext context)
        {
            MediaTypeHeaderValue requestContentType = null;
            MediaTypeHeaderValue.TryParse(context.HttpContext.Request.ContentType, out requestContentType);
            if (context.ActionDescriptor.FilterDescriptors.Last().Filter == this)
            {
                // Only execute if this is the last filter before calling the action.
                // This is because we want to avoid running filters which were inherited when there was already a filter
                // on the action. This ensures that we only run the filter which is closest to the action.
                if (requestContentType != null &&
                    !ContentTypes.Any(contentType => contentType.IsSubsetOf(requestContentType)))
                {
                    context.Result = new UnsupportedMediaTypeResult();
                }
            }
        }

        public void OnResourceExecuted([NotNull] ResourceExecutedContext context)
        {
        }

        private bool IsApplicable(ActionConstraintContext context)
        {
            return context.CurrentCandidate.Action.FilterDescriptors.Last(filter => filter.Filter is ConsumesAttribute).Filter == this;
        }

        public bool Accept(ActionConstraintContext context)
        {
            // First check to see if this is applicble
            if (!IsApplicable(context))
            {
                // since this constraint is not applicable it should return true.
                return true;
            }

            MediaTypeHeaderValue requestContentType = null;
            MediaTypeHeaderValue.TryParse(context.RouteContext.HttpContext.Request.ContentType, out requestContentType);

            // If the request content type is null we need to act like pass through.
            // In case there is a single candidate with a constraint it should be selected.
            // If there are multiple actions with consumes action constraints this should result in ambiguous exception
            // unless there is another action without a consumes constraint.
            if (requestContentType == null)
            {
                var isActionWithoutConsumeConstraintPresent = context.Candidates.Any(
                    candidate => candidate.Constraints == null ||
                    !candidate.Constraints.Any(constraint => constraint is IConsumesActionConstraint));

                return !isActionWithoutConsumeConstraintPresent;
            }

            if (ContentTypes.Any(c => c.IsSubsetOf(requestContentType)))
            {
                return true;
            }

            var firstCandidate = context.Candidates[0];
            if (firstCandidate != context.CurrentCandidate)
            {
                // If the current candidate has reached here, its not a match.
                return false;
            }

            foreach (var candidate in context.Candidates)
            {
                if (candidate == firstCandidate)
                {
                    continue;
                }

                var tempContext = new ActionConstraintContext()
                {
                    Candidates = context.Candidates,
                    RouteContext = context.RouteContext,
                    CurrentCandidate = candidate
                };

                if (candidate.Constraints == null || candidate.Constraints.Count() == 0 ||
                    candidate.Constraints.Any(constraint => constraint is IConsumesActionConstraint &&
                                                            constraint.Accept(tempContext)))
                {
                    // There is someone later in the chain which can handle the request.
                    // end the process here.
                    return false;
                }
            }

            // There is no one later in the chain that can handle this content type return a false positive so that
            // later we can detect and return a 415.
            return true;
        }

        private List<MediaTypeHeaderValue> GetContentTypes(string firstArg, string[] args)
        {
            var contentTypes = new List<MediaTypeHeaderValue>();
            contentTypes.Add(MediaTypeHeaderValue.Parse(firstArg));
            foreach (var item in args)
            {
                var contentType = MediaTypeHeaderValue.Parse(item);
                contentTypes.Add(contentType);
            }

            return contentTypes;
        }
    }
}