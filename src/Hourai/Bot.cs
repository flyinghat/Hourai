using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Hourai {

  public class BotCounters {

    public SimpleCounter Reconnects { get; }

    public BotCounters() {
      Reconnects = new SimpleCounter();
    }

  }

  public class Bot {

    static void Main() => new Bot().Run().GetAwaiter().GetResult();

    public static ISelfUser User { get; private set; }
    public static IUser Owner { get; private set; }

    public DateTime StartTime { get; private set; }
    public TimeSpan Uptime => DateTime.Now - StartTime;

    DiscordShardedClient Client { get; set; }
    ErrorService ErrorService { get; set; }
    CommandService CommandService { get; set; }

    static TaskCompletionSource<object> ExitSource { get; set; }
    bool _initialized;

    public static event Func<Task> RegularTasks {
      add { _regularTasks.Add(Check.NotNull(value));
      Log.Info($"Regular Tasks: {_regularTasks.Count}");}
      remove { _regularTasks.Remove(value); }
    }

    static List<Func<Task>> _regularTasks;

    public Bot() {
      ExitSource = new TaskCompletionSource<object>();

      _regularTasks = new List<Func<Task>>();

      Config.Load();
    }

    public static void Exit() {
      Log.Info("Bot exit has registered. Will exit on next cycle.");
      ExitSource.SetResult(new object());
    }

    async Task Initialize() {
      if (_initialized)
        return;
      StartTime = DateTime.Now;
      Log.Info("Initializing...");
      Client = new DiscordShardedClient(Config.DiscordConfig);
      CommandService = new CommandService(new CommandServiceConfig() {
        DefaultRunMode = RunMode.Sync
      });
      var map = new DependencyMap();
      map.Add(this);
      map.Add(Client);

      map.Add(new CounterSet(new ActivatorFactory<SimpleCounter>()));
      map.Add(new BotCounters());
      map.Add(new LogSet());

      map.Add(ErrorService = new ErrorService());
      map.Add(new LogService(map));

      //Log.Info($"Database: {Config.DbFilename}");

      var entryAssembly = Assembly.GetEntryAssembly();
      await CommandService.AddModulesAsync(entryAssembly);

      var serviceType = typeof(IService);
      var services = from type in entryAssembly.GetTypes()
                     where serviceType.IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract
                     select type;

      Log.Info("Loading services...");
      foreach(var service in services) {
        AddService(service, map);
      }
      Log.Info("Services loaded!");

      _initialized = true;
    }

    object AddService(Type type, IDependencyMap map) {
      object obj;
      if (map.TryGet(type, out obj))
        return obj;

      var typeInfo = type.GetTypeInfo();
      Log.Info($"Loading Service {type.Name}...");
      var constructor = typeInfo.DeclaredConstructors.Where(x => !x.IsStatic).First();
      var parameters = constructor.GetParameters();
      var properties = typeInfo.DeclaredProperties.Where(p => p.CanWrite);

      object[] args = new object[parameters.Length];

      for (int i = 0; i < parameters.Length; i++)
      {
        var paramType = parameters[i].ParameterType;
        Log.Info($"Found {type.Name} dependency => {paramType.Name}...");
        object arg;
        if (paramType == typeof(CommandService)) {
          arg = CommandService;
        } else if (map == null || !map.TryGet(paramType, out arg)) {
          if (paramType == typeof(IDependencyMap))
            arg = map;
          else
            arg = AddService(paramType, map);
        }
        args[i] = arg;
      }

      try
      {
        obj = constructor.Invoke(args);
        var add = typeof(IDependencyMap).GetMethod("Add");
        var method = add.MakeGenericMethod(type);
        method.Invoke(map, new [] {obj});
      }
      catch (Exception ex)
      {
        throw new Exception($"Failed to create \"{type.FullName}\"", ex);
      }

      foreach(var property in properties)
      {
        var propType = property.PropertyType;
        Log.Info($"Found {type.Name} dependency => {propType.Name}...");
        object arg = null;
        if (propType == typeof(CommandService)) {
          arg = CommandService;
        } else if (map == null || !map.TryGet(propType, out arg)) {
          if (propType  == typeof(IDependencyMap))
            arg = map;
          else if (!property.IsDefined(typeof(NotServiceAttribute)))
            arg = AddService(propType, map);
        }
        if (arg != null)
          property.SetValue(obj, arg, null);
      }
      return obj;
    }

    async Task MainLoop() {
      while (!ExitSource.Task.IsCompleted) {
        Log.Info("Starting regular tasks...");
        var tasks = Task.WhenAll(_regularTasks.Select(t => t()));
        if (Client.CurrentUser != null)
          await Client.SetGameAsync(Config.Version);
        Log.Info("Waiting ...");
        await Task.WhenAny(Task.Delay(60000), ExitSource.Task);
      }
    }

    async Task Run() {
      Log.Info($"Starting...");
      await Initialize();
      Log.Info("Logging into Discord...");
      await Client.LoginAsync(TokenType.Bot, Config.Token, false);
      Log.Info("Starting Discord Client...");
      await Client.StartAsync();
      User = Client.CurrentUser;
      Log.Info($"Logged in as {User.ToIDString()}");

      Owner = (await Client.GetApplicationInfoAsync()).Owner;
      Log.Info($"Owner: {Owner.Username} ({Owner.Id})");
      try {
        while (!ExitSource.Task.IsCompleted) {
          try {
            await MainLoop();
          } catch (Exception error) {
            Log.Error(error);
            ErrorService.RegisterException(error);
          }
        }
      } finally {
        Log.Info("Logging out...");
        await Client.LogoutAsync();
        Log.Info("Stopping Discord client...");
        await Client.StopAsync();
      }
    }

  }

}