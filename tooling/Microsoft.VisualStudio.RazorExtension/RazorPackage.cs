﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.RazorExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(RazorPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(Microsoft.VisualStudio.RazorExtension.RazorInfo.RazorInfoToolWindow))]
    public sealed class RazorPackage : Package
    {
        public const string PackageGuidString = "13b72f58-279e-49e0-a56d-296be02f0805";

        private const string CSharpPackageIdString = "13c3bbb4-f18f-4111-9f54-a0fb010d9194";

        protected override void Initialize()
        {
            base.Initialize();

            // We need to force the CSharp package to load. That's responsible for the initialization
            // of the remote host client.
            var shell = GetService(typeof(SVsShell)) as IVsShell;
            if (shell == null)
            {
                return;
            }

            IVsPackage package = null;
            var packageGuid = new Guid(CSharpPackageIdString);
            shell.LoadPackage(ref packageGuid, out package);

            Microsoft.VisualStudio.RazorExtension.RazorInfo.RazorInfoToolWindowCommand.Initialize(this);
        }
    }
}
