using DocuFlowAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocuFlowAPI.Services
{
    public class ArchivedDocumentsCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ArchivedDocumentsCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // izvrši odmah po startovanju
            await CleanupArchivedDocuments(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRunTime = DateTime.UtcNow.Date.AddHours(12); // 12:00 UTC

                if (now > nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1); // ako je prošlo 12:00, zakazi za sutra
                }

                var delay = nextRunTime - now;

                
                await Task.Delay(delay, stoppingToken);

                await CleanupArchivedDocuments(stoppingToken);
            }
        }


        private async Task CleanupArchivedDocuments(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

                var documentsToDelete = await context.Documents
                    .Where(d => d.Status == DocumentStatus.Archived && d.ArchivedAt <= threeDaysAgo)
                    .ToListAsync(cancellationToken);

                foreach (var doc in documentsToDelete)
                {
                    if (System.IO.File.Exists(doc.FilePath))
                    {
                        System.IO.File.Delete(doc.FilePath);
                    }

                    context.Documents.Remove(doc);
                }

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
