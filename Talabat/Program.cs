using Contracts;
using Entities.ErrorModel;
using Entities.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Repository;
using Repository.Data.Identity;
using Talabat;
using Talabat.ActionFilters;
using Talabat.Extensions;
using Talabat.Middlewares;


var builder = WebApplication.CreateBuilder(args);
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureRepositoryBase();
builder.Services.ConfigureRepositoryBasket();
builder.Services.ConfigureTokenService();
builder.Services.ConfigureResponseCacheService();
builder.Services.ConfigureUnitOfWork();
builder.Services.ConfigureOrderService();
builder.Services.COnfigureOrderCashing();
builder.Services.ConfigureStripeService();
builder.Services.ConfiguraSqlContext(builder.Configuration);
builder.Services.ConfigureIdentityDbContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddAuthentication();
builder.Services.ConfigureRedisConnection(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
/// configure errors to return errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    /// that is default return badRequest 
    /// and i use InvalidModelStateResponseFactory to return shape of response to custome on it
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState.Where(M => M.Value.Errors.Count() > 0)
                .SelectMany(m => m.Value.Errors).Select(e => e.ErrorMessage).ToArray();
        var validationErrorResponse = new ApiValidationErrorResponse()
        {
            Errors = errors
        };
        return new BadRequestObjectResult(validationErrorResponse);
    };
});
// Add Stripe Infrastructure
/*builder.Services.AddStripeInfrastructure(builder.Configuration);*/
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});
builder.Services.AddControllers(config =>
{
    config.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
}).AddApplicationPart(typeof(Talabat.Presentation.AssemblyReference).Assembly);


var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerManager>();
//app.ConfigureExeptionHandler(logger);


var scope = app.Services.CreateScope();
var service = scope.ServiceProvider;

try
{
    var context = service.GetRequiredService<RepositoryContext>();
    await context.Database.MigrateAsync();

    var Identitycontext = service.GetRequiredService<AppIdentityDbContext>();
    await Identitycontext.Database.MigrateAsync();

    await StoreContextSeed.SeedAsync(context, logger);

    // seeding for user manager
    var userManager = service.GetRequiredService<UserManager<AppUser>>();
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager);  
}
catch (Exception ex)
{
	throw ex;
}
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseHsts();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
