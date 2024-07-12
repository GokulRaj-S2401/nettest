using DASAPI;
using DASAPI.SignalR;
using ZOI.BAL.Services;
using ZOI.BAL.Services.Interface;
using ZOI.BAL.Utilities;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITemplateService, TemplatesService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMvc();
builder.Services.AddSingleton<IDictionary<string, string>>(opts => new Dictionary<string, string>());


builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy", builder =>
    {
        builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((hosts) => true);
    });
});

//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(builder =>
//    {

//        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//        builder.WithOrigins(CommonFunction.GetConnectionString("hubConnection"))
//        .AllowAnyHeader()
//        .AllowAnyMethod()
//        .AllowCredentials();

//    });
//});

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("CORSPolicy");

//app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapHub<CommHub>("/comm");

app.MapControllers();

app.Run();

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services); // calling ConfigureServices method

startup.Configure(app, builder.Environment); // calling Configure method
                                             // Add services to the container.