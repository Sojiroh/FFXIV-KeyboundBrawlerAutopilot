using System;
using Glasswork;

namespace AutoKeyboundBrawler.Windows;

internal readonly record struct ThemeChoice(string Id, string Name, Func<Look> Dark, Func<Look> Light);
