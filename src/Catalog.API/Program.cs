var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.AddDefaultAuthentication();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(CatalogApi.AdminOnlyPolicyName, policy => policy.RequireRole(CatalogApi.AdminRoleName));
});

var withApiVersioning = builder.Services.AddApiVersioning(options =>
{
    // Include "api-supported-versions" and "api-deprecated-versions" headers in all responses
    options.ReportApiVersions = true;
});

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages();

app.MapCatalogApi();

app.UseDefaultOpenApi();
app.Run();
