// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using RazorRenderer;

Renderer.Initialize();

var model = new EmailViewModel
{
    UserName = "User",
    SenderName = "Sender",
    UserData1 = 1,
    UserData2 = 2
};

string view = await Renderer.RenderViewAsync("EmailTemplate", model);

Console.WriteLine(view);
