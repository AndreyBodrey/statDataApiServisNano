using Microsoft.Data.Sqlite;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Runtime.InteropServices;
using System.Net;

// добавить checkBox igmp

namespace statServer
{
	class progamm
	{
		static string hostt = "http://192.168.0.112:8000";
		static DataBase? data;
		static RequestActions? req;
		static void Main(string[] args)		
		{

			var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    System.Console.WriteLine(ip.ToString());
					if ( ip.ToString() != "127.0.0.1") hostt = "http://" + ip.ToString() + ":8080";
                }
            }

			
				[DllImport("/home/drey/test/sock/socCpp1_lib/libigmpwork.so", CharSet = CharSet.Auto)]
				static extern int  istartWork();
				[DllImport("/home/drey/test/sock/socCpp1_lib/libigmpwork.so", CharSet = CharSet.Auto)]
				static extern int  istopWork();

			data = new DataBase("dataBase.db2");
			req = new RequestActions();		

			

			var builder = WebApplication.CreateBuilder(args);
			var app = builder.Build();

			app.Run(async (context) =>
			{
    			context.Response.ContentType = "text/html; charset=utf-8";

   				if (context.Request.Path == "/request")
    				{
						var form = context.Request.Form;

						string str = form["dateStart"];
						if(str != null) req.dateFrom = str;
						else req.dateFrom = "none";

						str = form["dateStop"];
						if (str != null)  req.dateTo = str;
						else req.dateTo = "none";

						str = form["scrambled"];
						if (str != null)  req.scrambled = true;
						else req.scrambled = false;

						str = form["igmpFlag"];
						if (str != null)  req.igmp = true;
						else req.igmp = false;

						str = form["late"];
						if (str != null) req.late = true;
						else req.late = false;

						str = form["checkSumm"];
						if (str != null) req.checkSumm = true;
						else req.checkSumm = false;

						str = form["allErrors"];
						if (str != null) req.allErrors = true;
						else req.allErrors = false;

						string tabl = "<table width=\"100%\" cellspacing=\"0\" cellpadding=\"4\" border=\"1\"><tr tr style=\"height: 50px;\"><td width=\"25%\"align=\"center\"><a href=\"/data\">выборка данных</a></td><td width=\"25%\" align=\"center\"><a href=\"/files\">FILES</a></td><td width=\"25%\"align=\"center\"><a href=\"/ctrl\">CONTROL</a></td><td width=\"25%\" align=\"center\"><a href=\"/abaut\">HELP</a></td></tr></table>";
						await context.Response.WriteAsync(tabl);
					
//----------DEBUG HERE-------------------------------------
						req.printAll();
						string? ret = req.getSQL();
						await context.Response.WriteAsync("<br>" + ret + "<br>");

						List<DataItem> qselectedData = data.readData(ret);
						await context.Response.WriteAsync(DataItem.TableStart());
						foreach( var item in qselectedData)
						{
							await context.Response.WriteAsync(item.ToTableRow());
						}
						await context.Response.WriteAsync(DataItem.TableOver());

//--------------------------------------------------------


    			}
    			else if (context.Request.Path == "/startwork")
				{
					istartWork();
					await context.Response.SendFileAsync("htmlFiles/control.html");
				}
				else if (context.Request.Path == "/stopwork")
				{
					istopWork();
					await context.Response.SendFileAsync("htmlFiles/control.html");
				}
				else if (context.Request.Path == "/ctrl")
				{
					await context.Response.SendFileAsync("htmlFiles/control.html");
				}
				else if (context.Request.Path == "/files")
				{
					await context.Response.WriteAsync("<!DOCTYPE html><html><head><meta charset=\"utf-8\"> <title>FILES</title></head><body><h1> <a href=\"/\">FILES NOT READY</a></h1></body></html>");
				}
				else if (context.Request.Path == "/abaut")
				{
					await context.Response.WriteAsync("<!DOCTYPE html><html><head><meta charset=\"utf-8\"> <title>HELP</title></head><body><h1> <a href=\"/\">HELP NOT READY</a></h1></body></html>");
				}
				else
    			{
        			await context.Response.SendFileAsync("htmlFiles/index.html");
    			}
				
			});
				app.MapGet("/2", () => "Hello mazafaka!");
				app.Run(hostt);
		}
	}
}

//<!DOCTYPE html><html><head><meta charset=\"utf-8\"> <title>HELP</title></head><body>
//</body></html>