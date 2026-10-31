using Balenthiran.WhosAccountable.Abstractions;
using Balenthiran.WhosAccountable.Services.Datasets;
using Balenthiran.WhosAccountable.Services.Scoring;

namespace Balenthiran.WhosAccountable.WebApi;

public static class ServiceRegistration
{
    public static void AddBackendServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatasetLoader, DatasetLoader>();
        services.AddSingleton<IScoreEngine, ScoreEngine>();

        // Scored once, here, and held for the life of the process (no database, nothing to refresh).
        services.AddSingleton(sp =>
        {
            var configured = configuration["Dataset:Path"] ?? "Data/cohort-v1.json";
            var path = Path.IsPathRooted(configured) ? configured : Path.Combine(AppContext.BaseDirectory, configured);
            var dataset = sp.GetRequiredService<IDatasetLoader>().Load(File.ReadAllText(path));
            return new ScoreboardStore(dataset, sp.GetRequiredService<IScoreEngine>());
        });
    }
}
