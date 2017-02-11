using Discord;
using Discord.Commands;
using Hourai.Model;
using System.Threading.Tasks;

namespace Hourai.Preconditions {

  public enum ModuleType : long {
    Standard = 1 << 0,
    Admin = 1 << 1,
    Feeds = 1 << 2
  }

  public class RequireModuleAttribute : RequireContextAttribute {

    public ModuleType Module { get; }

    public RequireModuleAttribute(ModuleType module) : base(ContextType.Guild) {
      Module = module;
    }

    public override async Task<PreconditionResult> CheckPermissions(
        ICommandContext context,
        CommandInfo commandInfo,
        IDependencyMap dependencies) {
      var baseCheck = await base.CheckPermissions(context, commandInfo, dependencies);
      if (!baseCheck.IsSuccess)
          return baseCheck;
      var guild = dependencies.Get<DatabaseService>().Context.GetGuild(context.Guild);
      if (guild.IsModuleEnabled(Module))
          return PreconditionResult.FromSuccess();
      return PreconditionResult.FromError($"Module \"{commandInfo.Module.Name}\" is not enabled.");
    }
  }

}
