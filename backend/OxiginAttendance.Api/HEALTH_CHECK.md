# Health Check Configuration
# Add these configurations to your backend Program.cs for health checks

# Install health check packages if needed:
# dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
# dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore

# Add to Program.cs:
# builder.Services.AddHealthChecks()
#     .AddDbContext<ApplicationDbContext>();

# app.MapHealthChecks("/health");

# Docker health check usage:
# HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
#   CMD curl -f http://localhost:80/health || exit 1