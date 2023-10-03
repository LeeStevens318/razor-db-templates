using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorDbTemplates;

var builder = WebApplication.CreateBuilder(args);

// Add Razor services
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Add our custom InMemoryFileProvider to services
builder.Services.AddSingleton<InMemoryFileProvider>();

var app = builder.Build();

// Configure RazorViewEngine to use our InMemoryFileProvider
var env = app.Services.GetRequiredService<IWebHostEnvironment>();
var inMemoryFileProvider = app.Services.GetRequiredService<InMemoryFileProvider>();

env.WebRootFileProvider = new CompositeFileProvider(inMemoryFileProvider, env.WebRootFileProvider);

// Map Render Endpoint
app.MapGet("/render", async (InMemoryFileProvider fileProvider, IRazorViewEngine viewEngine, HttpContext context) =>
{
	var templateKey = "/MemoryTemplates/sample.cshtml";
	var templateContent = "@{ var name = (string)Model; }<h1>Hello, @name!</h1>"; // This will be loaded from the DB in real world

	// Add template content to our InMemoryFileProvider
	fileProvider.AddFile(templateKey, templateContent);

	// Razor view engine will look for view we created in our InMemoryFileProvider
	var view = viewEngine.GetView(string.Empty, templateKey, false);
	if (!view.Success)
		return TypedResults.BadRequest("View Not Found");

	await using var output = new StringWriter();
	var viewContext = new ViewContext
	{
		HttpContext = context,
		View = view.View,
		Writer = output,
		ViewData = new ViewDataDictionary<string>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
		{
			Model = "John"
		}
	};

	// Render the view
	await view.View.RenderAsync(viewContext);

	// Return the rendered view
	return Results.Content(output.ToString(), "text/html");
});

app.Run();