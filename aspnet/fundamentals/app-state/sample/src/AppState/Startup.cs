using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Framework.Logging;
using AppState.Model;
using Microsoft.AspNet.Hosting;

namespace AppState
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCaching();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
            });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            app.UseIISPlatformHandler();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // don't count favicon requests
            app.Map("/favicon.ico", ignore => { });

            // example middleware that does not reference session at all and is configured before app.UseSession()
            app.Map("/untracked", subApp =>
            {
                subApp.Run(async context =>
                {
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("Requested at: " + DateTime.Now.ToLongTimeString() + "<br>");
                    await context.Response.WriteAsync("This part of the application isn't referencing Session...<br><a href=\"/\">Return</a>");
                    await context.Response.WriteAsync("</body></html>");
                });
            });

            app.UseSession();

            // establish session
            app.Map("/session", subApp =>
            {
                subApp.Run(async context =>
                {
                    RequestEntryCollection collection = GetOrCreateEntries(context);
                    collection.Add(context.Request.PathBase + context.Request.Path);
                    SaveEntries(context, collection);
                    context.Session.SetString("StartTime", DateTime.Now.ToLongTimeString());

                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync($"Counting: You have made {collection.TotalCount()} requests to this application.<br><a href=\"/\">Return</a>");
                    await context.Response.WriteAsync("</body></html>");

                });
            });

            // main catchall middleware
            app.Run(async context =>
            {
                RequestEntryCollection collection = GetOrCreateEntries(context);

                await context.Response.WriteAsync("<html><body>");
                if (collection.TotalCount() == 0)
                {
                    await context.Response.WriteAsync("Your session has not been established.<br>");
                    await context.Response.WriteAsync(DateTime.Now.ToLongTimeString() + "<br>");
                    await context.Response.WriteAsync("<a href=\"/session\">Establish session</a>.<br>");
                }
                else
                {
                    collection.Add(context.Request.PathBase + context.Request.Path);
                    await context.Response.WriteAsync("Session Established At: " + context.Session.GetString("StartTime") + "<br>");
                    foreach (var entry in collection.Entries)
                    {
                        await context.Response.WriteAsync("Request: " + entry.Path + " was requested " + entry.Count + " times.<br />");
                    }
                    SaveEntries(context, collection);

                    await context.Response.WriteAsync("Your session was located, you've visited the site this many times: " + collection.TotalCount() + "<br />");
                }
                await context.Response.WriteAsync("<a href=\"/untracked\">Visit untracked part of application</a>.<br>");
                await context.Response.WriteAsync("</body></html>");
            });
        }

        private RequestEntryCollection GetOrCreateEntries(HttpContext context)
        {
            RequestEntryCollection collection = null;
            byte[] requestEntriesBytes = context.Session.Get("RequestEntries");
            if (requestEntriesBytes != null)
            {
                using (MemoryStream stream = new MemoryStream(requestEntriesBytes))
                {
                    var formatter = new BinaryFormatter();
                    collection = formatter.Deserialize(stream) as RequestEntryCollection;
                }
            }
            if (collection == null)
            {
                collection = new RequestEntryCollection();
            }
            return collection;
        }

        private void SaveEntries(HttpContext context, RequestEntryCollection collection)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, collection);
                context.Session.Set("RequestEntries", stream.ToArray());
            }
        }
    }
}
