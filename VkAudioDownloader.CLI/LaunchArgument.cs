using System;
using System.Text;
using DTLib.Extensions;

namespace VkAudioDownloader.CLI;

public class LaunchArgument
{
    public string[] Aliases;
    public string Description;
    public string? ParamName;
    public Action? Handler;
    public Action<string>? HandlerWithArg;
    
    private LaunchArgument(string[] aliases, string description)
    {
        Aliases = aliases;
        Description = description;
    }
    
    public LaunchArgument(string[] aliases, string description, Action handler) 
        : this(aliases, description) => Handler = handler;

    public LaunchArgument(string[] aliases, string description, Action<string> handler, string paramName)
        : this(aliases, description)
    {
        HandlerWithArg = handler;
        ParamName = paramName;
    }

    public StringBuilder AppendHelpInfo(StringBuilder b)
    {
        b.Append(Aliases[0]);
        for (int i = 1; i < Aliases.Length; i++)
            b.Append(", ").Append(Aliases[i]);
        if (!String.IsNullOrEmpty(ParamName))
            b.Append(" [").Append(ParamName).Append(']');
        b.Append(" - ").Append(Description);
        return b;
    }

    public override string ToString() => 
        $"{{{{{Aliases.MergeToString(", ")}}}, Handler: {Handler is null}, HandlerWithArg: {HandlerWithArg is null}}}";
}