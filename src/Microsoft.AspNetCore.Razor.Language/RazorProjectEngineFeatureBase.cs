// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorProjectEngineFeatureBase : IRazorProjectEngineFeature
    {
        private RazorProjectEngine _engine;

        public RazorProjectEngine Engine
        {
            get { return _engine; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _engine = value;
                OnInitialized();
            }
        }

        protected TFeature GetRequiredFeature<TFeature>() where TFeature : IRazorProjectEngineFeature
        {
            if (Engine == null)
            {
                throw new InvalidOperationException(Resources.FormatFeatureMustBeInitialized(nameof(Engine)));
            }

            var feature = Engine.Features.OfType<TFeature>().FirstOrDefault();
            ThrowForMissingFeatureDependency<TFeature>(feature);

            return feature;
        }

        protected void ThrowForMissingFeatureDependency<TEngineDependency>(TEngineDependency value)
        {
            if (value == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatFeatureDependencyMissing(
                        GetType().Name,
                        typeof(TEngineDependency).Name,
                        typeof(RazorProjectEngine).Name));
            }
        }

        protected virtual void OnInitialized()
        {
        }
    }
}
