using System;
using CliWrap;
using DTLib.Filesystem;
using DTLib.Extensions;
using DTLib.Logging.New;

namespace VkAudioDownloader;

public class FFMPegHelper
{
    private LoggerContext _logger;
    private readonly string ffmpeg;    
    public FFMPegHelper(ILogger logger, string ffmpegDir)
    {
        _logger = new LoggerContext(logger, nameof(FFMPegHelper));
        ffmpeg=Path.Concat(ffmpegDir,"ffmpeg");
    }
    
    /// <summary>creates fragments list for ffmppeg concat</summary>
    /// <param name="fragmentsDir">there file list.txt will be created</param>
    /// <param name="fragments">audio files in fragmentsDir (with or without dir in path)</param>
    /// <returns></returns>
    public string CreateFragmentList(string fragmentsDir, string[] fragments)
    {
        string listFile = Path.Concat(fragmentsDir, "list.txt");
        using var playlistFile = File.OpenWrite(listFile);
        for (var i = 0; i < fragments.Length; i++)
        {
            var clearFileName = fragments[i].AsSpan().AfterLast(Path.Sep);
            playlistFile.Write($"file '{clearFileName}'\n".ToBytes(StringConverter.UTF8));
        }

        return listFile;
    }
    
    
    /// <summary>converts ts files in directory to opus</summary>
    /// <param name="localDir">directory with ts fragment files</param>
    /// <returns>paths to created opus files</returns>
    public Task<string[]> ToOpus(string localDir) => 
        ToOpus(Directory.GetFiles(localDir, "*.ts"));

    /// <summary>
    /// converts ts files in to opus
    /// </summary>
    /// <param name="fragments">ts fragment files</param>
    /// <returns>paths to created opus files</returns>
    public async Task<string[]> ToOpus(string[] fragments)
    {
        string[] output = new string[fragments.Length];
        var tasks = new Task<CommandResult>[fragments.Length];
        
        for (var i = 0; i < fragments.Length; i++)
        {
            string tsFile = fragments[i];
            string opusFile = tsFile.Replace(".ts",".opus");
            _logger.LogDebug($"{tsFile} -> {opusFile}");
            var command = Cli.Wrap(ffmpeg).WithArguments(new[]
                {
                    "-i", tsFile, // input
                    "-loglevel", "warning", "-hide_banner", "-nostats", // print only warnings and errors
                    "-map", "0:0", // select first audio track (sometimes there are blank buggy second thack) 
                    "-filter:a", "asetpts=PTS-STARTPTS", // fixes pts
                    "-c", "libopus", "-b:a", "96k", // encoding params
                    opusFile // output
                })
                // ffmpeg prints all log to stderr, because in stdout it ptints encoded file
                .WithStandardErrorPipe(PipeTarget.ToDelegate(
                    msg => _logger.LogWarn(msg)));

            tasks[i] = command.ExecuteAsync();
            output[i] = opusFile;
        }

        await Task.WhenAll(tasks);
        return output;
    }

    public async Task Concat(string outfile, string fragmentListFile, string codec="libopus")
    {
        _logger.LogDebug($"{fragmentListFile} -> {outfile}");
        var command = Cli.Wrap(ffmpeg).WithArguments(new[]
            {
                "-f", "concat", // mode
                "-i", fragmentListFile, // input list
                "-loglevel", "warning", "-hide_banner", "-nostats", // print only warnings and errors
                "-filter:a", "asetpts=PTS-STARTPTS", // fixes pts
                "-c", codec, "-b:a", "96k", // encoding params
                outfile, "-y" // output override
            })
            // ffmpeg prints all log to stderr, because in stdout it ptints encoded file
            .WithStandardErrorPipe(PipeTarget.ToDelegate(
                msg => _logger.LogWarn(msg)))
            .WithValidation(CommandResultValidation.None);
        
        var rezult =await command.ExecuteAsync();
        // log time
        if (rezult.ExitCode != 0)
        {
            _logger.LogError($"command failed with code {rezult.ExitCode}");
            throw new Exception($"command: {command} failed");
        }
    }
}