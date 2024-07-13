using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Randomizer.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RandomizerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"))
);

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".ips"] = "application/octet-stream";
provider.Mappings[".bps"] = "application/octet-stream";
provider.Mappings[".rdc"] = "application/octet-stream";
provider.Mappings[".lua"] = "text/x-lua";

app.UseStaticFiles(new StaticFileOptions {
    ContentTypeProvider = provider
});

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
