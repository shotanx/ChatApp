using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Threading;

namespace WebSocketServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                WriteRequestParam(context); // es metodi mogvianebit davamatet. Console logshi saintereso rameebs achvenebs (tumca sachiro araa)
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("WebSocket Connected");

                    await ReceiveMessage(webSocket, async (result, buffer) =>
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            Console.WriteLine("Message Received");
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("Received Close message");
                        }
                    });
                }
                else
                {
                    Console.WriteLine("Hello from the 2nd request delegate.");
                    await next();
                }
            });

            //es ubralod savarjishod damatebit ragac metodi. next() ar chirdeba imitom rom amis shemdeg arc araferia.
            app.Run(async context =>
            {
                Console.WriteLine("Hello from the 3rd request delegate. es mxolod serveris consoleshi chans.");
                await context.Response.WriteAsync("Hello from the 3rd request delegate. amas achvenebs browsershi localhost:5000-ze");
            });
        }

        //metodi romelic amobechdavs request header-s. HttpContext-idan sxvadasxva properties amovbechdavt.
        public void WriteRequestParam(HttpContext context) // es konteqsti zemotastan arafer shuashia
        {
            Console.WriteLine("Request Method: " + context.Request.Method);
            Console.WriteLine("Request Protocol: " + context.Request.Protocol);

            if (context.Request.Headers != null)
            {
                foreach (var h in context.Request.Headers)
                {
                    Console.WriteLine("--> " + h.Key + " : " + h.Value);
                }
            }
        }

        private async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
