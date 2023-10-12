using CliWrap;
using DTLib.Logging;

namespace VkAudioDownloader.Helpers;

public class FFMPegHelper
{
    private ContextLogger _logger;
    private readonly IOPath ffmpeg_exe;    
    public FFMPegHelper(ILogger logger, IOPath ffmpegDir)
    {
        _logger = new ContextLogger(nameof(FFMPegHelper), logger);
        ffmpeg_exe=Path.Concat(ffmpegDir,"ffmpeg");
    }
    
    /// <summary>creates fragments list for ffmppeg concat</summary>
    /// <param name="fragmentsDir">there file list.txt will be created</param>
    /// <param name="fragments">audio files in fragmentsDir (with or without dir in path)</param>
    /// <returns>path to list.txt file</returns>
    public IOPath CreateFragmentList(IOPath fragmentsDir, IOPath[] fragments)
    {
        IOPath listFile = Path.Concat(fragmentsDir, "list.txt");
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
    public Task<IOPath[]> ToOpus(IOPath localDir) => 
        ToOpus(Directory.GetFiles(localDir, "*.ts"));

    /// <summary>
    /// converts ts files in to opus
    /// </summary>
    /// <param name="fragments">ts fragment files</param>
    /// <returns>paths to created opus files</returns>
    public async Task<IOPath[]> ToOpus(IOPath[] fragments)
    {
        IOPath[] output = new IOPath[fragments.Length];
        var tasks = new Task<CommandResult>[fragments.Length];
        
        for (var i = 0; i < fragments.Length; i++)
        {
            IOPath tsFile = fragments[i];
            IOPath opusFile = tsFile.Replace(".ts",".opus");
            _logger.LogDebug($"{tsFile} -> {opusFile}");
            var command = Cli.Wrap(ffmpeg_exe.Str).WithArguments(new[]
                {
                    "-i", tsFile.Str, // input
                    "-loglevel", "warning", "-hide_banner", "-nostats", // print only warnings and errors
                    "-map", "0:0", // select first audio track (sometimes there are blank buggy second thack) 
                    "-filter:a", "asetpts=PTS-STARTPTS", // fixes pts
                    "-c", "libopus", "-b:a", "96k", // encoding params
                    opusFile.Str // output
                })
                // ffmpeg prints all log to stderr, because in stdout it ptints encoded file
                .WithStandardErrorPipe(PipeTarget.ToDelegate(StdErrHandle));

            tasks[i] = command.ExecuteAsync();
            output[i] = opusFile;
        }

        await Task.WhenAll(tasks);
        return output;
    }

    protected void StdErrHandle(string msg)
    {
        if(msg.EndsWith("start time for stream 1 is not set in estimate_timings_from_pts"))
            _logger.LogDebug(msg);
        else _logger.LogWarn(msg);
    }
    
    public async Task Concat(IOPath outfile, IOPath fragmentListFile, string codec="libopus")
    {
        _logger.LogDebug($"{fragmentListFile} -> {outfile}");
        var command = Cli.Wrap(ffmpeg_exe.Str).WithArguments(new[]
            {
                "-f", "concat", // mode
                "-i", fragmentListFile.Str, // input list
                "-loglevel", "warning", "-hide_banner", "-nostats", // print only warnings and errors
                "-filter:a", "asetpts=PTS-STARTPTS", // fixes pts
                "-c", codec, "-b:a", "96k", // encoding params
                outfile.Str, "-y" // output override
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