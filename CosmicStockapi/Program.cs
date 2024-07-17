using System.Text;
using CosmicStockapi.Auto;
using CosmicStockapi.ErrorHandling;
using CosmicStockapi.Extension;
using CosmicStockapi.Middleware;
using Data;
using Data.Classes;
using Data.Interfaces;
using Data.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddSingleton<ICacheService,CacheService>();
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Services.AddScoped<IApplicationUser,ApplicationUser>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<ICreateEntity,CreateEntity>();
builder.Services.AddScoped<IPostRating,PostRating>();
builder.Services.AddScoped<IOrderRepository,OrderRepository>();
builder.Services.AddScoped<IShoppingCartRepo,ShoppingCartRepo>();
builder.Services.AddScoped<IPhotoService,PhotoService>();
builder.Services.AddTransient<IEmailSender,EmailSender>();
builder.Services.Configure<CloudinaryPictures>(builder.Configuration.GetSection("CloudinaryPhotos"));
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));
builder.Services.AddIdentity<AppUser,IdentityRole>(opt =>
            {
                opt.SignIn.RequireConfirmedEmail = false;
                opt.User.RequireUniqueEmail = true;
                opt.Password.ToString();
                opt.Password.RequireUppercase = false;
                opt.Password.RequiredUniqueChars = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();   
builder.Services.Configure<DataProtectionTokenProviderOptions>(o=>{
    o.TokenLifespan = TimeSpan.FromHours(3);
});     
builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<IConnectionMultiplexer>(r =>
            {
                var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"),true);
                return ConnectionMultiplexer.Connect(configuration);
            });
builder.Services.AddAuthentication(options =>
{
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => 
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidAudience = builder.Configuration["JWT:VaildAudience"],
                    ValidIssuer = builder.Configuration["JWT:VaildIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                };
            });
builder.Services.AddAuthorization(o=>
{ 
    o.AddPolicy("AdminCEO", x =>
    {
        x.RequireAssertion(x =>x.User.IsInRole(StaticInfo.AdminRole));
    });
});               
builder.Services.AddHttpContextAccessor();  
builder.Services.AddControllers();
builder.Services.AddCors(options =>{
    options.AddPolicy("CosmicStock",policy=>{
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerAuthInformation();
builder.Services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = ActionContext =>
                {
                    var errors = ActionContext.ModelState
                    .Where(e =>e.Value.Errors.Count > 0)
                    .SelectMany(e => e.Value.Errors)
                    .Select(e => e.ErrorMessage).ToList();
                    var errorResponse = new ApiValidationResponse
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleWare>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}
app.UseHttpsRedirection();
app.UseRouting(); 
app.UseCors("CosmicStock");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
