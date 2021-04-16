using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using Serilog.Formatting.Display;
using Serilog.Formatting;
using System.IO;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace SerilogTest
{
	class Program
	{
		static void Main(string[] args)
		{
            var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message}{NewLine}{Exception}";
            Serilog.Formatting.Display.MessageTemplateTextFormatter tf = new Serilog.Formatting.Display.MessageTemplateTextFormatter(outputTemplate, CultureInfo.InvariantCulture);


            Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.CustomSink(tf)
				.WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
				.CreateLogger();

			Log.Information("Hello, Serilog!");

			int a = 10, b = 0;
			try
			{
				Log.Debug("Dividing {A} by {B}", a, b);
				Console.WriteLine(a / b);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Something went wrong");
			}
			finally
			{
				Log.CloseAndFlush();
			}

			Console.ReadLine();
		}
	}

    public class CustomSink : ILogEventSink
    {
        private readonly ITextFormatter _formatter;
        private readonly Action<string>[] _handlers;

        public CustomSink(ITextFormatter formatter, params Action<string>[] handlers)
        {
            _formatter = formatter;
            _handlers = handlers;
        }

        public void Emit(LogEvent logEvent)
        {
            var buffer = new StringWriter(new StringBuilder(256));
            _formatter.Format(logEvent, buffer);
            string message = buffer.ToString();

            Console.WriteLine(message);
            //foreach (Action<string> handler in _handlers)
            //    handler(message);
        }
    }


    public static class MySinkExtensions
    {
        public static LoggerConfiguration CustomSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  ITextFormatter formatter = null, params Action<string>[] handlers)
        {
            return loggerConfiguration.Sink(new CustomSink(formatter, handlers));
        }


        public static ILogger Here(this ILogger logger,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            return logger
                .ForContext("MemberName", memberName)
                .ForContext("FilePath", sourceFilePath)
                .ForContext("LineNumber", sourceLineNumber);
        }

    }
}