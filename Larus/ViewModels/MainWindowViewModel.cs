using Larus.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Larus.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        searchResults = this.WhenAnyValue(x => x.Pattern, x => x.Dir, x => x.Files, (p, d, f) => (p, d, f))
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Where((term) => !(string.IsNullOrWhiteSpace(term.Item1)
                               || string.IsNullOrWhiteSpace(term.Item2)
                               || string.IsNullOrWhiteSpace(term.Item3)))
            .SelectMany(SearchFiles)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.SearchResults);

        resultsEnabled = this.WhenAnyValue(x => x.SearchResults)
            .Select(x => x is not null)
            .ToProperty(this, x => x.ResultsEnabled);
    }

    private async Task<IEnumerable<FileResults>> SearchFiles((string, string, string) term, CancellationToken token)
    {
        // TODO find .gitignore to eliminate unwanted files
        var (pattern, dir, files) = term;
        return await Observable.Start(() =>
        {
            var result = new List<FileResults>();
            var relevantFiles = Directory.GetFiles(dir, files, SearchOption.AllDirectories).Where(filename => !filename.Contains("node_modules"));

            foreach (var file in relevantFiles)
            {
                FileResults? curr = null;
                foreach (var (line, n) in File.ReadLines(file).Select((l, i) => (l, i)))
                {
                    if (line.Any(ch => char.IsControl(ch) && ch != '\n' && ch != '\r' && ch != '\t'))
                    {
                        // probably binary
                        break;
                    }

                    if (line.Contains(pattern))
                    {
                        if (curr is null)
                        {
                            curr = new FileResults { Filepath = file, Results = new List<FileResult>() };
                        }

                        curr.Results.Add(new FileResult { Match = line, LineNo = n });
                    }
                }

                if (curr is not null)
                {
                    result.Add(curr);
                }
            }

            return result;
        }, RxApp.TaskpoolScheduler);
    }

    private string _pattern = "";
    public string Pattern
    {
        get => _pattern;
        set => this.RaiseAndSetIfChanged(ref _pattern, value);
    }

    private string _files = "";
    public string Files {
        get => _files;
        set => this.RaiseAndSetIfChanged(ref _files, value);
    }

    private string _directory = "";
    public string Dir
    {
        get => _directory;
        set => this.RaiseAndSetIfChanged(ref _directory, value);
    }

    private readonly ObservableAsPropertyHelper<IEnumerable<FileResults>> searchResults;
    public IEnumerable<FileResults> SearchResults => searchResults.Value;

    private readonly ObservableAsPropertyHelper<bool> resultsEnabled;
    public bool ResultsEnabled => resultsEnabled.Value;
}
