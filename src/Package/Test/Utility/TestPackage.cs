﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Test.Utility
{
    internal sealed class TestRPackage : RPackage
    {
        TestServiceProvider _serviceProvider = new TestServiceProvider();

        public void Init()
        {
            base.Initialize();
        }

        public void Close()
        {
            base.Dispose(true);
        }

        protected override object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public static IEnumerable<string> PackageMefAssemblies { get; } = new string[]
        {
            "Microsoft.VisualStudio.R.Package.Test.dll",
            "Microsoft.VisualStudio.R.Package.dll"
        };

        public static IEnumerable<string> TestMefAssemblies { get; } = new string[]
        {
            "Microsoft.VisualStudio.R.Package.Test.dll",
        };
    }
}